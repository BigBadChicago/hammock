using System;

namespace Hammock.Extras.Serialization
{
    public class XmlSerializer : SerializerBase
    {
        public override T Deserialize<T>(string content)
        {
            return (T) Deserialize(content, typeof (T));
        }

        public override object Deserialize(string content, Type type)
        {
            var root = type.Name.ToLowerInvariant();
            
            return DeserializeXmlWithRoot(content, type, root);
        }

        public override string Serialize(object instance, Type type)
        {
            var root = type.Name.ToLowerInvariant();
            
            return SerializeXmlWithRoot(instance, type, root);
        }

        public override string ContentType
        {
            get { return "text/xml"; }
        }
    }
}