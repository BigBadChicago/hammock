using System.Text.RegularExpressions;

namespace Hammock.Web.Extensions
{
    internal static class RegexExtensions
    {
        public static bool Matches(this string input, string pattern)
        {
            return Regex.IsMatch(input, pattern);
        }
    }
}