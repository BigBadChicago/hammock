#if !Smartphone

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;

namespace Hammock.Serialization
{
    public class HammockDataContractJsonSerializer : Utf8Serializer, ISerializer, IDeserializer
    {
        private readonly Dictionary<RuntimeTypeHandle, DataContractJsonSerializer> _serializers =
            new Dictionary<RuntimeTypeHandle, DataContractJsonSerializer>();

        #region ISerializer Members

        public string Serialize(object instance, Type type)
        {
            string content;
            using (var stream = new MemoryStream())
            {
                var serializer = CacheOrGetSerializerFor(type);
                serializer.WriteObject(stream, instance);

                content = ContentEncoding.GetString(stream.ToArray());
            }
            return content;
        }

        public virtual string ContentType
        {
            get { return "application/json"; }
        }

        #endregion

        #region IDeserializer Members

        public virtual object Deserialize(string content, Type type)
        {
            object instance;
            using (var stream = new MemoryStream(ContentEncoding.GetBytes(content)))
            {
                var serializer = CacheOrGetSerializerFor(type);
                instance = serializer.ReadObject(stream);
            }
            return instance;
        }

        public virtual T Deserialize<T>(string content)
        {
            var type = typeof (T);
            T instance;
            using (var stream = new MemoryStream(ContentEncoding.GetBytes(content)))
            {
                var serializer = CacheOrGetSerializerFor(type);
                instance = (T) serializer.ReadObject(stream);
            }
            return instance;
        }

        #endregion

        private DataContractJsonSerializer CacheOrGetSerializerFor(Type type)
        {
            var handle = type.TypeHandle;
            if (_serializers.ContainsKey(handle))
            {
                return _serializers[handle];
            }

            var serializer = new DataContractJsonSerializer(type);
            _serializers.Add(handle, serializer);

            return serializer;
        }
    }
}

#endif