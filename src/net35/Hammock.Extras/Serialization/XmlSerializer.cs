using System;

namespace Hammock.Extras.Serialization
{
    public class XmlSerializer : SerializerBase
    {
        public override T Deserialize<T>(RestResponse<T> response)
        {
            var root = typeof (T).Name.ToLowerInvariant();

            return (T)DeserializeXmlWithRoot(response.Content, typeof(T), root);
        }

        public override object Deserialize(RestResponse response, Type type)
        {
            var root = type.Name.ToLowerInvariant();

            return DeserializeXmlWithRoot(response.Content, type, root);
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