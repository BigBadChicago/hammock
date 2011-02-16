using System;

namespace Hammock.Serialization
{
    public interface IDeserializer
    {
        object Deserialize(RestResponse response, Type type);
        T Deserialize<T>(RestResponse<T> response);
    }
}