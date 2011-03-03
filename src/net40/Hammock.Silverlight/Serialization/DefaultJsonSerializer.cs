using System;
#if NET40
using System.Dynamic;
#endif

namespace Hammock.Serialization
{
    public class DefaultJsonSerializer : IDeserializer
    {
        public object Deserialize(RestResponse response, Type type)
        {
            var result = JsonParser.Deserialize(response.Content, type);
            return result;
        }

        public T Deserialize<T>(RestResponse<T> response)
        {
            var result = JsonParser.Deserialize<T>(response.Content);
            return result;
        }

#if NET40
        public dynamic DeserializeDynamic<T>(RestResponse<T> response) where T : DynamicObject
        {
            var result = JsonParser.Deserialize(response.Content);
            return result;
        }
#endif
    }
}
