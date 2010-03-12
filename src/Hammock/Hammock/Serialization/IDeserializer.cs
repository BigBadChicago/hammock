using System;

namespace Hammock.Serialization
{
    public interface IDeserializer
    {
        object Deserialize(string content, Type type);
        T Deserialize<T>(string content);
    }
}