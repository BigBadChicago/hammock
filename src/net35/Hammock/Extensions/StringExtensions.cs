using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
#if SILVERLIGHT && !WindowsPhone
using System.Windows.Browser;
#endif

#if WindowsPhone
using System.Web;
#endif

#if !SILVERLIGHT
using System.Web;
#endif

namespace Hammock.Extensions
{
    internal static class StringExtensions
    {
        public static bool IsNullOrBlank(this string value)
        {
            return String.IsNullOrEmpty(value) ||
                   (!String.IsNullOrEmpty(value) && value.Trim() == String.Empty);
        }

        public static bool EqualsIgnoreCase(this string left, string right)
        {
            return String.Compare(left, right, StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        public static bool EqualsAny(this string input, params string[] args)
        {
            return args.Aggregate(false, (current, arg) => current | input.Equals(arg));
        }

        public static string FormatWith(this string format, params object[] args)
        {
            return String.Format(format, args);
        }

        public static string FormatWithInvariantCulture(this string format, params object[] args)
        {
            return String.Format(CultureInfo.InvariantCulture, format, args);
        }

        public static string Then(this string input, string value)
        {
            return String.Concat(input, value);
        }

        public static string UrlEncode(this string value)
        {
            // This is more correct than HttpUtility; 
            // it escapes spaces as %20, not +
            return Uri.EscapeDataString(value);
        }

        public static string UrlDecode(this string value)
        {
            return Uri.UnescapeDataString(value);
        }

        public static Uri AsUri(this string value)
        {
            return new Uri(value);
        }

        public static string ToBase64String(this byte[] input)
        {
            return Convert.ToBase64String(input);
        }

        public static byte[] GetBytes(this string input)
        {
            return Encoding.UTF8.GetBytes(input);
        }

        public static string PercentEncode(this string s)
        {
            var bytes = s.GetBytes();
            var sb = new StringBuilder();
            foreach (var b in bytes)
            {
                sb.Append(string.Format("%{0:X}", b));
            }
            return sb.ToString();
        }

        public static IDictionary<string, string> ParseQueryString(this string query)
        {
            return ParseQueryString(query, Encoding.UTF8);
        }

        // From http://anonsvn.mono-project.com/viewvc/trunk/mcs/class/System.Web/System.Web/HttpUtility.cs
        public static IDictionary<string, string> ParseQueryString(this string query, Encoding encoding)
        {
            var result = new Dictionary<string, string>();
            if (query.Length == 0)
                return result;

            var decoded = HttpUtility.HtmlDecode(query);
            var decodedLength = decoded.Length;
            var namePos = 0;
            var first = true;
            while (namePos <= decodedLength)
            {
                int valuePos = -1, valueEnd = -1;
                for (var q = namePos; q < decodedLength; q++)
                {
                    if (valuePos == -1 && decoded[q] == '=')
                    {
                        valuePos = q + 1;
                    }
                    else if (decoded[q] == '&')
                    {
                        valueEnd = q;
                        break;
                    }
                }

                if (first)
                {
                    first = false;
                    if (decoded[namePos] == '?')
                        namePos++;
                }

                string name;
                if (valuePos == -1)
                {
                    name = null;
                    valuePos = namePos;
                }
                else
                {
                    name = UrlDecode(decoded.Substring(namePos, valuePos - namePos - 1));
                }
                if (valueEnd < 0)
                {
                    namePos = -1;
                    valueEnd = decoded.Length;
                }
                else
                {
                    namePos = valueEnd + 1;
                }
                string value = UrlDecode(decoded.Substring(valuePos, valueEnd - valuePos));

                if (name != null)
                    result.Add(name, value);
                if (namePos == -1)
                    break;
            }
            return result;
        }

        private const RegexOptions Options =
#if !SILVERLIGHT
            RegexOptions.Compiled | RegexOptions.IgnoreCase;
#else
            RegexOptions.IgnoreCase;
#endif
    }
}