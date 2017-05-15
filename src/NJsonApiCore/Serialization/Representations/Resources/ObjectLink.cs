using Newtonsoft.Json;
using NJsonApi.Serialization.Converters;
using System;
using System.Collections.Generic;

namespace NJsonApi.Serialization.Representations
{
    [JsonConverter(typeof(SerializationAwareConverter))]
    public class ObjectLink : ILink
    {
        public string Href { get; set; }
        public String Name { get; private set; }
        public IDictionary<String, Object> Meta { get; private set; }

        public ObjectLink(String name, IDictionary<String, Object> meta, Uri href)
        {
            this.Name = name;
            this.Meta = meta;
            this.Href = href.AbsoluteUri;
        }


        public void Serialize(JsonWriter writer)
        {
            if (Meta != null && Meta.Count > 0)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("href");
                writer.WriteValue(Href);

                //Meta
                writer.WritePropertyName("meta");
                writer.WriteStartObject();
                foreach (var metaKeyValue in Meta)
                {
                    writer.WritePropertyName(metaKeyValue.Key);
                    writer.WriteValue(metaKeyValue.Value);
                }
                writer.WriteEndObject();


                writer.WriteEndObject();
            }
            else
            {
                writer.WriteValue(Href);
            }
            
        }

        public override string ToString() => Href;
    }
}