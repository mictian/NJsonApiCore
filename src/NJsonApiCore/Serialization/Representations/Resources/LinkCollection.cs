using NJsonApi.Serialization.Converters;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using NJsonApi.Serialization.Representations;

namespace NJsonApiCore.Serialization.Representations.Resources
{
    [JsonConverter(typeof(SerializationAwareConverter))]
    public class LinkCollection : ISerializationAware
    {
        protected Dictionary<string, ILink> Links { get; set; }

        public void Add(ILink link) => Links.Add(link.Name, link);

        public IDictionary<string, ILink> GetLinks()
        {
            return this.Links;
        }

        public void AddOnce(ILink link)
        {
            if (Links.ContainsKey(link.Name))
            {
                Links.Remove(link.Name);
            }
            Add(link);
        }

        public LinkCollection()
        {
            Links = new Dictionary<string, ILink>();
        }
        
        public void AddRange(LinkCollection range)
        {
            foreach (var linkKeyValue in range.GetLinks())
            {
                Links.Add(linkKeyValue.Key, linkKeyValue.Value);
            }
        }

        public void Serialize(JsonWriter writer)
        {
            writer.WriteStartObject();
            var serializer = new JsonSerializer();

            foreach (var link in Links)
            {
                writer.WritePropertyName(link.Key);
                serializer.Serialize(writer, link.Value);
            }
            writer.WriteEndObject();
        }
    }
}
