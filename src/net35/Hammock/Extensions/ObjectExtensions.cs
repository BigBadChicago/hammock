using System;
using System.Collections.Generic;
using System.Linq;

namespace Hammock.Extensions
{
    internal static class ObjectExtensions
    {
        public static bool Implements(this object instance, Type interfaceType)
        {
            return interfaceType.IsGenericTypeDefinition
                       ? instance.ImplementsGeneric(interfaceType)
                       : interfaceType.IsAssignableFrom(instance.GetType());
        }

        private static bool ImplementsGeneric(this Type type, Type targetType)
        {
            var interfaceTypes = type.GetInterfaces();
            foreach (var interfaceType in interfaceTypes)
            {
                if (!interfaceType.IsGenericType)
                {
                    continue;
                }

                if (interfaceType.GetGenericTypeDefinition() == targetType)
                {
                    return true;
                }
            }

            var baseType = type.BaseType;
            if (baseType == null)
            {
                return false;
            }

            return baseType.IsGenericType
                       ? baseType.GetGenericTypeDefinition() == targetType
                       : baseType.ImplementsGeneric(targetType);
        }

        private static bool ImplementsGeneric(this object instance, Type targetType)
        {
            return instance.GetType().ImplementsGeneric(targetType);
        }

        public static Type GetDeclaredTypeForGeneric(this object instance, Type interfaceType)
        {
            return instance.GetType().GetDeclaredTypeForGeneric(interfaceType);
        }

        public static Type GetDeclaredTypeForGeneric<T>(this object instance)
        {
            var interfaceType = typeof(T);
            return instance.GetDeclaredTypeForGeneric(interfaceType);
        }

        public static Type GetDeclaredTypeForGeneric<T>(this object instance, T interfaceType)
        {
            return instance.GetDeclaredTypeForGeneric(typeof(T));
        }

        public static Type GetDeclaredTypeForGeneric<T>(this Type type)
        {
            var interfaceType = typeof(T);
            return type.GetDeclaredTypeForGeneric(interfaceType);
        }

        public static Type GetDeclaredTypeForGeneric<T>(this Type type, T interfaceType)
        {
            return type.GetDeclaredTypeForGeneric(typeof(T));
        }

        public static Type GetDeclaredTypeForGeneric(this Type baseType, Type interfaceType)
        {
            var type = default(Type);

            if (baseType.ImplementsGeneric(interfaceType))
            {
#if NETCF
                var generic = baseType.GetInterfaces()
                    .Single(i => i.FullName.Equals(interfaceType.FullName));
#else
                var generic = baseType.GetInterface(interfaceType.FullName, true);
#endif
                if (generic.IsGenericType)
                {
                    var args = generic.GetGenericArguments();
                    if (args.Length == 1)
                    {
                        type = args[0];
                    }
                }
            }

            return type;
        }

        public static IEnumerable<Type> GetDeclaredTypesForGeneric(this object instance, Type interfaceType)
        {
            return instance.GetType().GetDeclaredTypesForGeneric(interfaceType);
        }

        public static IEnumerable<Type> GetDeclaredTypesForGeneric<T>(this object instance)
        {
            var interfaceType = typeof(T);
            return instance.GetType().GetDeclaredTypesForGeneric(interfaceType);
        }

        public static IEnumerable<Type> GetDeclaredTypesForGeneric<T>(this object instance, T interfaceType)
        {
            return instance.GetDeclaredTypesForGeneric<T>();
        }

        public static IEnumerable<Type> GetDeclaredTypesForGeneric<T>(this Type type)
        {
            var interfaceType = typeof(T);
            return type.GetDeclaredTypesForGeneric(interfaceType);
        }

        public static IEnumerable<Type> GetDeclaredTypesForGeneric<T>(this Type type, T interfaceType)
        {
            return type.GetDeclaredTypesForGeneric(typeof(T));
        }

        public static IEnumerable<Type> GetDeclaredTypesForGeneric(this Type type, Type interfaceType)
        {
            foreach (var generic in type.GetGenericInterfacesFor(interfaceType))
            {
                foreach (var arg in generic.GetGenericArguments())
                {
                    yield return arg;
                }
            }
        }

        private static IEnumerable<Type> GetGenericInterfacesFor(this Type type, Type interfaceType)
        {
            var candidates = type.GetInterfaces();
            foreach (var candidate in candidates)
            {
                if (!candidate.IsGenericType)
                {
                    continue;
                }

                var definition = candidate.GetGenericTypeDefinition();
                if (definition == interfaceType)
                {
                    yield return candidate;
                }
            }
        }
    }
}