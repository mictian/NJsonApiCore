using NJsonApi.Serialization.Representations;
using NJsonApiCore.Serialization.Representations.Resources;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace NJsonApi
{
    public interface IResourceMapping
    {
        Func<object, object> IdGetter { get; set; }
        Action<object, string> IdSetter { get; set; }
        Type ResourceRepresentationType { get; set; }
        Type Controller { get; }
        string ResourceType { get; set; }
        List<IRelationshipMapping> Relationships { get; set; }
        /// <summary>
        /// List of extra/custom links that can be register for a give resource
        /// </summary>
        LinkCollection Links { get; set; }

        /// <summary>
        /// List of extra/custom links that apply for resource but only at the top level document
        /// </summary>
        LinkCollection TopLevelLinks { get; set; }
        Dictionary<string, Func<object, object>> PropertyGetters { get; set; }
        Dictionary<string, Action<object, object>> PropertySetters { get; }
        Dictionary<string, Expression<Action<object, object>>> PropertySettersExpressions { get; }

        bool ValidateIncludedRelationshipPaths(string[] includedPaths);

        Dictionary<string, object> GetAttributes(object objectGraph);

        Dictionary<string, object> GetValuesFromAttributes(Dictionary<string, object> attributes);
    }
}