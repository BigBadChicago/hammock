using System;
using Newtonsoft.Json;

namespace Hammock.Extras.Serialization
{
    public class JsonSerializer : SerializerBase
    {
        public JsonSerializer()
        {
            
        }

        public JsonSerializer(JsonSerializerSettings settings) : base(settings)
        {
            
        }

        public override T Deserialize<T>(RestResponse<T> response)
        {
            return DeserializeJson<T>(response.Content);
        }

        public override object Deserialize(RestResponse response, Type type)
        {
            return DeserializeJson(response.Content, type);
        }

        public override string Serialize(object instance, Type type)
        {
            return SerializeJson(instance, type);
        }

        public override string ContentType
        {
            get { return "application/json"; }
        }
    }
}