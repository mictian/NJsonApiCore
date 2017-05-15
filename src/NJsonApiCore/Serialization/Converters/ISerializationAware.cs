using Newtonsoft.Json;

namespace NJsonApi.Serialization.Converters
{
    public interface ISerializationAware
    {
        void Serialize(JsonWriter writer);
    }
}