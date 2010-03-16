using System.Collections.Generic;
using System.Reflection;

namespace Hammock.Extensions
{
    internal static class ReflectionExtensions
    {
        public static IEnumerable<T> GetCustomAttributes<T>(this PropertyInfo info, bool inherit)
            where T : class
        {
            var attributes = info.GetCustomAttributes(typeof (T), inherit);
            return attributes.ToEnumerable<T>();
        }

        public static bool HasCustomAttribute<T>(this PropertyInfo info, bool inherit)
            where T : class
        {
            var attributes = info.GetCustomAttributes(typeof (T), inherit);
            return attributes.Length > 0;
        }

        public static object GetValue(this object instance, string property)
        {
            var info = instance.GetType().GetProperty(property);
            var value = info.GetValue(instance, null);
            return value;
        }

        public static void SetValue(this object instance, string property, object value)
        {
            var info = instance.GetType().GetProperty(property);
            info.SetValue(instance, value, null);
        }
    }
}