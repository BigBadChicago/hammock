using System.Xml.Linq;
using System.Xml.Serialization;

namespace Hammock.Web.Extensions
{
    internal static class SerializationExtensions
    {
        public static XDocument ToXml<T>(this T instance)
        {
            var document = new XDocument();
            var serializer = new XmlSerializer(typeof (T));

            using (var writer = document.CreateWriter())
            {
                serializer.Serialize(writer, instance);
                writer.Flush();
                return document;
            }
        }

        public static XDocument ToXml<T>(this T instance, params XNamespace[] namespaces)
        {
            var document = new XDocument();
            var serializer = new XmlSerializer(typeof (T));

            using (var writer = document.CreateWriter())
            {
                serializer.Serialize(writer, instance);
                writer.Flush();
                return document;
            }
        }

        public static T FromXml<T>(this XDocument source) where T : class
        {
            var serializer = new XmlSerializer(typeof (T));
            var result = default(T);

            if (source.Root != null)
            {
                using (var reader = source.Root.CreateReader())
                {
                    result = serializer.Deserialize(reader) as T;
                }
            }

            return result;
        }
    }
}