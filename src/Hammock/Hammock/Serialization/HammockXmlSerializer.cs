using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Hammock.Serialization
{
    public class HammockXmlSerializer : Utf8Serializer, ISerializer, IDeserializer
    {
        private readonly Dictionary<RuntimeTypeHandle, XmlSerializer> _serializers =
           new Dictionary<RuntimeTypeHandle, XmlSerializer>();

        private readonly XmlWriterSettings _settings;
        private readonly XmlSerializerNamespaces _namespaces;

        public HammockXmlSerializer(XmlWriterSettings settings)
        {
            _settings = settings;
        }

        public HammockXmlSerializer(XmlWriterSettings settings, XmlSerializerNamespaces namespaces) : this(settings)
        {
            _namespaces = namespaces;
        }

        #region ISerializer Methods

        public virtual string Serialize(object instance, Type type)
        {
            string result;
            using (var stream = new MemoryStream())
            {
                using (var writer = XmlWriter.Create(stream, _settings))
                {
                    if (writer != null)
                    {
                        var serializer = CacheOrGetSerializerFor(type);

                        if(_namespaces != null)
                        {
                            serializer.Serialize(writer, instance, _namespaces);
                        }
                        else
                        {
                            serializer.Serialize(writer, instance);
                        }
                    }
                }

#if !Smartphone
                result = ContentEncoding.GetString(stream.ToArray());
#else
                result = ContentEncoding.GetString(stream.ToArray(), 0, (int)stream.Length);
#endif
            }
            return result;
        }

        #endregion

        public virtual string ContentType
        {
            get { return "application/xml"; }
        }

        #region IDeserializer Methods

        public virtual object Deserialize(string content, Type type)
        {
            object instance;
            var serializer = CacheOrGetSerializerFor(type);
            using(var reader = new StringReader(content))
            {
                instance = serializer.Deserialize(reader);    
            }
            return instance;
        }

        public virtual T Deserialize<T>(string content)
        {
            T instance;
            var serializer = CacheOrGetSerializerFor(typeof(T));
            using (var reader = new StringReader(content))
            {
                instance = (T) serializer.Deserialize(reader);
            }
            return instance;
        }

        #endregion

        private XmlSerializer CacheOrGetSerializerFor(Type type)
        {
            var handle = type.TypeHandle;
            if (_serializers.ContainsKey(handle))
            {
                return _serializers[handle];
            }

            var serializer = new XmlSerializer(type);
            _serializers.Add(handle, serializer);

            return serializer;
        }
    }
}


