using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonApi.Serialization.Representations;
using NJsonApi.Serialization.Representations.Resources;
using NJsonApiCore.Serialization.Representations.Resources;
using System.Collections.Generic;
using System.Linq;

namespace NJsonApi.Serialization.Documents
{
    /// <summary>
    /// JSON-API Document.
    /// This class is used as the result of transformation, has the JSON-API schema and therefore is the one used in the final serialization into a string JSON.
    /// IMPORTANT: This class represent the top level response structure, if you are looking for inner data values <see cref="IResourceRepresentation"/> interface and its implentations
    /// </summary>
    public class CompoundDocument
    {
        public CompoundDocument()
        {
            this.JsonApi = new JsonApi();
        }

        [JsonProperty(PropertyName = "data", NullValueHandling = NullValueHandling.Ignore)]
        public IResourceRepresentation Data { get; set; }

        [JsonProperty(PropertyName = "links", NullValueHandling = NullValueHandling.Ignore)]
        public LinkCollection Links { get; set; }

        [JsonProperty(PropertyName = "included", NullValueHandling = NullValueHandling.Ignore)]
        public List<SingleResource> Included { get; set; }

        [JsonProperty(PropertyName = "errors", NullValueHandling = NullValueHandling.Ignore)]
        public List<Error> Errors { get; set; }

        [JsonProperty(PropertyName = "meta", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object> Meta { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JToken> UnmappedAttributes { get; set; }

        [JsonProperty(PropertyName = "jsonapi", NullValueHandling = NullValueHandling.Ignore)]
        public JsonApi JsonApi { get; set; }
    }
}