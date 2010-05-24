using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Hammock.Attributes;
using Hammock.Attributes.Validation;
using Hammock.Web;

namespace Hammock.Extensions
{
    internal static class WebExtensions
    {
        public static void ParseNamedAttributes<T>(this IWebQueryInfo info,
                                                   IEnumerable<PropertyInfo> properties,
                                                   IDictionary<string, string> transforms,
                                                   IDictionary<string, string> collection) 
            where T : Attribute, INamedAttribute
        {
            foreach (var property in properties)
            {
                var attributes = property.GetCustomAttributes<T>(true);

                foreach (var attribute in attributes)
                {
                    var value = transforms.ContainsKey(property.Name)
                                    ? transforms[property.Name]
                                    : property.GetValue(info, null);

                    if (value == null)
                    {
                        continue;
                    }

                    var header = value.ToString();
                    if (!header.IsNullOrBlank())
                    {
                        collection.Add(attribute.Name, header);
                    }
                }
            }
        }

        public static Uri UriMinusQuery(this Uri uri, out WebParameterCollection parameters)
        {
            var sb = new StringBuilder();

            parameters = new WebParameterCollection();
            var query = uri.Query.ParseQueryString();
            foreach(var key in query.Keys)
            {
                parameters.Add(key, query[key].UrlDecode());
            }

            var port = uri.Scheme.Equals("http") && uri.Port != 80 || 
                       uri.Scheme.Equals("https") && uri.Port != 443 ? 
                       ":" + uri.Port : "";

            sb.Append(uri.Scheme)
                .Append("://")
                .Append(uri.Host)
                .Append(port)
                .Append(uri.AbsolutePath);

            return new Uri(sb.ToString());
        }

        public static string ToBasicAuthorizationHeader(string username, string password)
        {
            var token = "{0}:{1}".FormatWith(username, password).GetBytes().ToBase64String();
            return "Basic {0}".FormatWith(token);
        }

        public static void ParseValidationAttributes(this IWebQueryInfo info,
                                                     IEnumerable<PropertyInfo> properties,
                                                     IDictionary<string, string> collection) 
        {
            foreach (var property in properties)
            {
                var attributes = property.GetCustomAttributes<ValidationAttribute>(true);

                foreach (var attribute in attributes)
                {
                    // Support multiple transforms
                    var value = collection.ContainsKey(property.Name)
                                    ? collection[property.Name]
                                    : property.GetValue(info, null);
                    
                    var transformed = attribute.TransformValue(property, value);
                    if (transformed.IsNullOrBlank())
                    {
                        continue;
                    }

                    if(collection.ContainsKey(property.Name))
                    {
                        collection[property.Name] = transformed;
                    }
                    else
                    {
                        collection.Add(property.Name, transformed);
                    }
                }
            }
        }
    }
}