using Newtonsoft.Json;
using NJsonApi.Serialization.Converters;

namespace NJsonApi.Serialization.Representations
{
    public interface ILink : ISerializationAware
    {
        string Href { get; set; }

        string Name { get; }
    }
}