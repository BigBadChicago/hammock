using System.Collections.Generic;
using System.Linq;
using Hammock.Model;
using Hammock.Web.Mocks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hammock.Extensions
{
    public static class JsonExtensions
    {
        public static JProperty FindSingleChildProperty(this JToken startToken, string propertyName)
        {
            JProperty ret = null;
            var props = from JProperty p
                            in startToken.Children().OfType<JProperty>()
                        where p.Name.ToLower() == propertyName
                        select p;

            if (!props.Any())
            {
                foreach (var token in startToken.Children())
                {
                    ret = FindSingleChildProperty(token, propertyName);
                    if (ret != null)
                    {
                        break;
                    }
                }
            }
            else
            {
                ret = props.ToArray()[0];
            }
            return ret;
        }

        public static JObject FindSingleChildObject(this JToken startToken, string objectName)
        {
            JObject ret = null;
            var props = from JObject o
                            in startToken.Children().OfType<JObject>()
                        where o["Name"].Value<string>() == objectName
                        select o;

            if (!props.Any())
            {
                foreach (var token in startToken.Children())
                {
                    ret = FindSingleChildObject(token, objectName);
                    if (ret != null)
                    {
                        break;
                    }
                }
            }
            else
            {
                ret = props.ToArray()[0];
            }
            return ret;
        }

        public static string ToJson(this IMockable instance)
        {
            var json = JsonConvert.SerializeObject(instance);

            return json;
        }

        public static string ToJson(this IMockable instance, JsonSerializerSettings settings)
        {
            var json = JsonConvert.SerializeObject(instance, Formatting.Indented, settings);

            return json;
        }
        
        public static string ToJson(this IEnumerable<IMockable> collection)
        {
            var json = JsonConvert.SerializeObject(collection);

            return json;
        }

        public static string ToJson(this IEnumerable<IMockable> collection, JsonSerializerSettings settings)
        {
            var json = JsonConvert.SerializeObject(collection, Formatting.Indented, settings);

            return json;
        }
    }
}