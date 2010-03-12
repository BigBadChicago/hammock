using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace Hammock.Serialization
{
    public class HammockJavaScriptSerializer : Utf8Serializer, ISerializer, IDeserializer
    {
        private readonly JavaScriptSerializer _serializer;

        public HammockJavaScriptSerializer(JavaScriptTypeResolver resolver)
        {
            _serializer = new JavaScriptSerializer(resolver);
        }

        public HammockJavaScriptSerializer(JavaScriptTypeResolver resolver, IEnumerable<JavaScriptConverter> converters)
        {
            _serializer = new JavaScriptSerializer(resolver);
            _serializer.RegisterConverters(converters);
        }

        public HammockJavaScriptSerializer(IEnumerable<JavaScriptConverter> converters)
        {
            _serializer = new JavaScriptSerializer();
            _serializer.RegisterConverters(converters);
        }

        public string Serialize(object instance, Type type)
        {
            return _serializer.Serialize(instance);
        }

        public string ContentType
        {
            get { return "application/json"; }
        }

        public object Deserialize(string content, Type type)
        {
            return _serializer.DeserializeObject(content);
        }

        public T Deserialize<T>(string content)
        {
            return _serializer.Deserialize<T>(content);
        }
    }
}
