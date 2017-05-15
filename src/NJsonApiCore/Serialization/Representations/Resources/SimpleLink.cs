using Newtonsoft.Json;
using NJsonApi.Serialization.Converters;
using System;

namespace NJsonApi.Serialization.Representations
{
    [JsonConverter(typeof(SerializationAwareConverter))]
    public class SimpleLink : ILink
    {
        public string Name { get; private set; }
        public string Href { get; set; }


        public SimpleLink(string name)
        {
            this.Name = name;
        }

        public SimpleLink(string name, Uri href) : this(name)
        {
            this.Href = href.AbsoluteUri;
        }

        public void Serialize(JsonWriter writer) => writer.WriteValue(Href);

        public override string ToString() => Href;
    }
}