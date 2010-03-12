using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hammock.Authentication;
using Hammock.Web;
using Hammock.Web.Attributes;
using Hammock.Web.Query;

namespace Hammock.Extensions
{
    internal static class WebExtensions
    {
        public static Uri Uri(this string url)
        {
            return new Uri(url);
        }

        public static WebParameterCollection ParseParameters(this IWebQueryInfo info)
        {
            var parameters = new Dictionary<string, string>();
            var properties = info.GetType().GetProperties();

            info.ParseAttributes<ParameterAttribute>(properties, parameters);

            var collection = new WebParameterCollection();
            parameters.ForEach(p => collection.Add(new WebParameter(p.Key, p.Value)));

            return collection;
        }

        public static void ParseAttributes<T>(this IWebQueryInfo info,
                                              IEnumerable<PropertyInfo> properties,
                                              IDictionary<string, string> collection)
            where T : Attribute, INamedAttribute
        {
            foreach (var property in properties)
            {
                var attributes = property.GetCustomAttributes<T>(true);

                foreach (var attribute in attributes)
                {
                    var value = property.GetValue(info, null);

                    if (value == null)
                    {
                        continue;
                    }

                    if (property.HasCustomAttribute<BooleanToIntegerAttribute>(true))
                    {
                        value = ((bool)value) ? "1" : "0";
                    }

                    var dateFormatAttribute = property
                        .GetCustomAttributes<DateTimeFormatAttribute>(true)
                        .SingleOrDefault();

                    if (dateFormatAttribute != null)
                    {
                        value = ((DateTime)value).ToString(dateFormatAttribute.Format);
                    }

                    var header = value.ToString();

                    if (!header.IsNullOrBlank())
                    {
                        collection.Add(attribute.Name, header);
                    }
                }
            }
        }

        public static string ToAuthorizationHeader(this BasicAuthCredentials credentials)
        {
            return ToAuthorizationHeader(credentials.Username, credentials.Password);
        }

        public static string ToAuthorizationHeader(string username, string password)
        {
            var token = "{0}:{1}".FormatWith(username, password).GetBytes().ToBase64String();
            return "Basic {0}".FormatWith(token);
        }
    }
}