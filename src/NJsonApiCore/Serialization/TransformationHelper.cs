using NJsonApi.Exceptions;
using NJsonApi.Infrastructure;
using NJsonApi.Serialization.Representations;
using NJsonApi.Serialization.Representations.Relationships;
using NJsonApi.Serialization.Representations.Resources;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NJsonApi.Utils;
using NJsonApiCore.Serialization.Representations.Resources;

namespace NJsonApi.Serialization
{
    public class TransformationHelper
    {
        private const string MetaCountAttribute = "count";
        private readonly IConfiguration configuration;
        private readonly ILinkBuilder linkBuilder;

        public TransformationHelper(IConfiguration configuration, ILinkBuilder linkBuilder)
        {
            this.configuration = configuration;
            this.linkBuilder = linkBuilder;
        }

        /// <summary>
        /// Given an action result and the generated list of resources generated the best resource to properly representthe final result.
        /// This method is needed as internally the transformation is done with a List of object due to the result can be a list of entities or one single entity
        /// </summary>
        /// <param name="resource">Original Action result</param>
        /// <param name="representationList">Computed resources list (this is a list as an internal decision)</param>
        /// <returns>ResourceCollection or SingleResource</returns>
        public IResourceRepresentation ChooseProperResourceRepresentation(object resource, IEnumerable<SingleResource> representationList)
        {
            return resource is IEnumerable ?
                (IResourceRepresentation)new ResourceCollection(representationList) :
                representationList.Single();
        }

        public List<SingleResource> CreateIncludedRepresentations(List<object> primaryResourceList, IResourceMapping resourceMapping, Context context)
        {
            var includedList = new List<SingleResource>();

            var primaryResourceIdentifiers = primaryResourceList.Select(x =>
            {
                var id = new SingleResourceIdentifier
                {
                    Id = resourceMapping.IdGetter(x).ToString(),
                    Type = resourceMapping.ResourceType
                };

                return id;
            });

            var alreadyVisitedObjects = new HashSet<SingleResourceIdentifier>(primaryResourceIdentifiers, new SingleResourceIdentifierComparer());

            foreach (var resource in primaryResourceList)
            {
                includedList.AddRange(
                    AppendIncludedRepresentationRecursive(
                        resource,
                        resourceMapping,
                        alreadyVisitedObjects,
                        context));
            }

            if (includedList.Any())
            {
                return includedList;
            }
            return null;
        }

        private List<SingleResource> AppendIncludedRepresentationRecursive(
            object resource,
            IResourceMapping resourceMapping,
            HashSet<SingleResourceIdentifier> alreadyVisitedObjects,
            Context context,
            string parentRelationshipPath = "")
        {
            var includedResources = new List<SingleResource>();

            foreach (var relationship in resourceMapping.Relationships)
            {
                if (relationship.InclusionRule == ResourceInclusionRules.ForceOmit)
                {
                    continue;
                }

                var relatedResources = UnifyObjectsToList(relationship.RelatedResource(resource));
                string relationshipPath = BuildRelationshipPath(parentRelationshipPath, relationship);

                if (!context.IncludedResources.Any(x => x.Contains(relationshipPath)))
                {
                    continue;
                }

                foreach (var relatedResource in relatedResources)
                {
                    var relatedResourceId = new SingleResourceIdentifier
                    {
                        Id = relationship.ResourceMapping.IdGetter(relatedResource).ToString(),
                        Type = relationship.ResourceMapping.ResourceType
                    };

                    if (alreadyVisitedObjects.Contains(relatedResourceId))
                    {
                        continue;
                    }

                    alreadyVisitedObjects.Add(relatedResourceId);
                    includedResources.Add(
                        CreateResourceRepresentation(relatedResource, relationship.ResourceMapping, context));

                    includedResources.AddRange(
                        AppendIncludedRepresentationRecursive(relatedResource, relationship.ResourceMapping, alreadyVisitedObjects, context, relationshipPath));
                }
            }

            return includedResources;
        }

        private string BuildRelationshipPath(string parentRelationshipPath, IRelationshipMapping relationship)
        {
            if (string.IsNullOrEmpty(parentRelationshipPath))
            {
                return relationship.RelationshipName;
            }
            else
            {
                return $"{parentRelationshipPath}.{relationship.RelationshipName}";
            }
        }

        /// <summary>
        /// Given if an object generates a List of objects with it or cast the input parameter into a such a list if it is possible
        /// </summary>
        /// <param name="nestedObject">Object to cast or wrap</param>
        /// <returns>List of object</returns>
        public List<object> UnifyObjectsToList(object nestedObject)
        {
            var list = new List<object>();
            if (nestedObject != null)
            {
                if (nestedObject is IEnumerable<object>)
                    list.AddRange((IEnumerable<object>)nestedObject);
                else
                    list.Add(nestedObject);
            }

            return list;
        }

        /// <summary>
        /// Validates that the passed in type is valid to be JSON-API serialized
        /// </summary>
        /// <exception cref="NotSupportedException">If the type if not serializable</exception>
        /// <param name="innerObjectType">Type of the action result that will be serialized</param>
        public void VerifyTypeSupport(Type innerObjectType)
        {
            //TODO: What the aim of this IMetaDataWrapper??
            if (typeof(IMetaDataWrapper).IsAssignableFrom(innerObjectType))
            {
                throw new NotSupportedException(string.Format("Error while serializing type {0}. IEnumerable<MetaDataWrapper<>> is not supported.", innerObjectType));
            }

            if (typeof(IEnumerable).IsAssignableFrom(innerObjectType) && !innerObjectType.GetTypeInfo().IsGenericType)
            {
                throw new NotSupportedException(string.Format("Error while serializing type {0}. Non generic IEnumerable are not supported.", innerObjectType));
            }
        }

        /// <summary>
        /// In case the action result is a <see cref="IMetaDataWrapper"/> wrap it
        /// </summary>
        /// <param name="objectGraph">Action result to check</param>
        /// <returns>Inner object in case of IMetaDaraWrapper or just the input parameter</returns>
        public object UnwrapResourceObject(object objectGraph)
        {
            if (!(objectGraph is IMetaDataWrapper))
            {
                return objectGraph;
            }
            var metadataWrapper = objectGraph as IMetaDataWrapper;
            return metadataWrapper.GetValue();
        }

        /// <summary>
        /// Given an object to be serialized, extract from it its metadata.
        /// In order to return custom metadata the object must implement <see cref="IMetaDataWrapper"/> interface
        /// </summary>
        /// <param name="objectGraph">Object from which metadata is extracted</param>
        /// <returns>Metadata to be serialized</returns>
        public Dictionary<string, object> GetMetadata(object objectGraph)
        {
            if (objectGraph is IMetaDataWrapper)
            {
                var metadataWrapper = objectGraph as IMetaDataWrapper;
                return metadataWrapper.MetaData;
            }
            return null;
        }

        /// <summary>
        /// Given a single DTO object (obtained from an action result) transform it/generate the corresponding Resource
        /// </summary>
        /// <param name="objectGraph">Single Action result</param>
        /// <param name="resourceMapping">ResourceMapping used to extract from the object the data needed to create the Resource</param>
        /// <param name="context">Serialization context</param>
        /// <returns>New Resource</returns>
        public SingleResource CreateResourceRepresentation(
            object objectGraph,
            IResourceMapping resourceMapping,
            Context context)
        {
            var result = new SingleResource();

            result.Id = resourceMapping.IdGetter(objectGraph).ToString();
            result.Type = resourceMapping.ResourceType;
            result.Attributes = resourceMapping.GetAttributes(objectGraph);
            
            result.Links = new LinkCollection();
            result.Links.Add(linkBuilder.FindResourceSelfLink(context, result.Id, resourceMapping));

            if (resourceMapping.Relationships.Any())
            {
                result.Relationships = CreateRelationships(objectGraph, result.Id, resourceMapping, context);
            }

            return result;
        }

        public Dictionary<string, Relationship> CreateRelationships(object objectGraph, string parentId, IResourceMapping resourceMapping, Context context)
        {
            var relationships = new Dictionary<string, Relationship>();

            foreach (var linkMapping in resourceMapping.Relationships)
            {
                var relationshipName = linkMapping.RelationshipName;
                var rel = new Relationship();
                var relLinks = new RelationshipLinks();

                relLinks.Self = linkBuilder.RelationshipSelfLink(context, parentId, resourceMapping, linkMapping);
                relLinks.Related = linkBuilder.RelationshipRelatedLink(context, parentId, resourceMapping, linkMapping);

                if (!linkMapping.IsCollection)
                {
                    string relatedId = null;
                    object relatedInstance = null;
                    if (linkMapping.RelatedResource != null)
                    {
                        relatedInstance = linkMapping.RelatedResource(objectGraph);
                        if (relatedInstance != null)
                            relatedId = linkMapping.ResourceMapping.IdGetter(relatedInstance).ToString();
                    }
                    if (linkMapping.RelatedResourceId != null && relatedId == null)
                    {
                        var id = linkMapping.RelatedResourceId(objectGraph);
                        if (id != null)
                            relatedId = id.ToString();
                    }

                    if (linkMapping.InclusionRule != ResourceInclusionRules.ForceOmit)
                    {
                        // Generating resource linkage for to-one relationships
                        if (relatedInstance != null)
                            rel.Data = new SingleResourceIdentifier
                            {
                                Id = relatedId,
                                Type = configuration.GetMapping(relatedInstance.GetType()).ResourceType // This allows polymorphic (subtyped) resources to be fully represented
                            };
                        else if (relatedId == null || linkMapping.InclusionRule == ResourceInclusionRules.ForceInclude)
                            rel.Data = new NullResourceIdentifier(); // two-state null case, see NullResourceIdentifier summary
                    }
                }
                else
                {
                    IEnumerable relatedInstance = null;
                    if (linkMapping.RelatedResource != null)
                        relatedInstance = (IEnumerable)linkMapping.RelatedResource(objectGraph);

                    // Generating resource linkage for to-many relationships
                    if (linkMapping.InclusionRule == ResourceInclusionRules.ForceInclude && relatedInstance == null)
                        rel.Data = new MultipleResourceIdentifiers();
                    if (linkMapping.InclusionRule != ResourceInclusionRules.ForceOmit && relatedInstance != null)
                    {
                        var idGetter = linkMapping.ResourceMapping.IdGetter;
                        var identifiers = relatedInstance
                            .Cast<object>()
                            .Select(o => new SingleResourceIdentifier
                            {
                                Id = idGetter(o).ToString(),
                                Type = configuration.GetMapping(o.GetType()).ResourceType // This allows polymorphic (subtyped) resources to be fully represented
                            });
                        rel.Data = new MultipleResourceIdentifiers(identifiers);
                    }

                    // If data is present, count meta attribute is added for convenience
                    if (rel.Data != null)
                        rel.Meta = new Dictionary<string, string> { { MetaCountAttribute, ((MultipleResourceIdentifiers)rel.Data).Count.ToString() } };
                }

                if (relLinks.Self != null || relLinks.Related != null)
                {
                    rel.Links = relLinks;
                }

                if (rel.Data != null || rel.Links != null)
                {
                    relationships.Add(relationshipName, rel);
                }
            }
            return relationships.Any() ? relationships : null;
        }

        /// <summary>
        /// Validates that the passed in type is register to be serialized (handle by the JSON-API converter)
        /// </summary>
        /// <exception cref="MissingMappingException">Exception thrown when the type is not regiter in the configuration</exception>
        /// <param name="type">Type to ask for</param>
        /// <param name="config">Configuration containing register types and its metadata</param>
        public void AssureAllMappingsRegistered(Type type, IConfiguration config)
        {
            if (!config.IsResourceRegistered(type))
            {
                throw new MissingMappingException(type);
            }
        }

        public LinkCollection GetTopLevelLinks(Uri requestUri, IResourceMapping resourceMapping)
        {
            var topLevel = new LinkCollection();
            topLevel.Add(new SimpleLink("self", requestUri));

            topLevel.AddRange(resourceMapping.TopLevelLinks);

            return topLevel;
        }
    }
}