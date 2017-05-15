using NJsonApi.Serialization;
using NJsonApi.Serialization.Representations;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Routing.Template;

namespace NJsonApi.Web.MVCCore.Serialization
{
    public class LinkBuilder : ILinkBuilder
    {
        private readonly IApiDescriptionGroupCollectionProvider descriptionProvider;

        public LinkBuilder(IApiDescriptionGroupCollectionProvider descriptionProvider)
        {
            this.descriptionProvider = descriptionProvider;
        }

        public ILink FindResourceSelfLink(Context context, string resourceId, IResourceMapping resourceMapping)
        {
            var actions = descriptionProvider.From(resourceMapping.Controller).Items;

            //It is assumed that a GET method to retrieve a resource by Id is implemetned and verified by the ConfigurationBuilder
            //See ConfigurationBuilder class HasOnlyOneGetMethod method
            var action = actions.Single(a =>
                a.HttpMethod == "GET" &&
                a.ParameterDescriptions.Count(p => p.Name == "id") == 1);

            var values = new Dictionary<string, object>();
            values.Add("id", resourceId);

            return CreateLink(context, action, values);
        }

        public ILink RelationshipRelatedLink(Context context, string resourceId, IResourceMapping resourceMapping, IRelationshipMapping linkMapping)
        {
            var selfLink = FindResourceSelfLink(context, resourceId, resourceMapping).Href;
            var completeLink = $"{selfLink}/{linkMapping.RelationshipName}";
            return new SimpleLink("related", new Uri(completeLink));
        }

        public ILink RelationshipSelfLink(Context context, string resourceId, IResourceMapping resourceMapping, IRelationshipMapping linkMapping)
        {
            var selfLink = FindResourceSelfLink(context, resourceId, resourceMapping).Href;
            var completeLink = $"{selfLink}/relationships/{linkMapping.RelationshipName}";
            return new SimpleLink("self", new Uri(completeLink));
        }

        // TODO replace with UrlHelper method once RC2 has been released

        /// <summary>
        /// Creates a new Link fr
        /// </summary>
        /// <param name="context"></param>
        /// <param name="action"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        private SimpleLink CreateLink(Context context, ApiDescription action, Dictionary<string, object> values)
        {
            var template = TemplateParser.Parse(action.RelativePath);
            var result = action.RelativePath.ToLowerInvariant();

            //for each parameter in the tempate relative path of the action, its values is extracted from the passed in values dictionary
            foreach (var parameter in template.Parameters)
            {
                var value = values[parameter.Name];
                result = result.Replace(parameter.ToPlaceholder(), value.ToString());
            }

            return new SimpleLink("self", new Uri(context.BaseUri, result));
        }
    }
}