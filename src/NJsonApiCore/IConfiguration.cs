using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace NJsonApi
{
    public interface IConfiguration
    {
        string DefaultJsonApiMediaType { get; }

        void AddMapping(IResourceMapping resourceMapping);

        /// <summary>
        /// Given a type (generally a DTO Model) returns the generated <see cref="IResourceMapping"/> created when JSON-API was configured
        /// </summary>
        /// <param name="type">Action result used as a key</param>
        /// <returns>ResourceMapping for the give type</returns>
        IResourceMapping GetMapping(Type type);

        IResourceMapping GetMapping(object objectGraph);

        JsonSerializer GetJsonSerializer();

        bool IsResourceRegistered(Type resource);

        bool IsControllerRegistered(Type controller);

        bool ValidateIncludedRelationshipPaths(string[] includedPaths, object objectGraph);

        bool ValidateAcceptHeader(string acceptsHeaders);

        string[] FindRelationshipPathsToInclude(string includeQueryParameter);

        JsonSerializerSettings GetJsonSerializerSettings();
    }
}