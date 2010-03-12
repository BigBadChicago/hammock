using System;

namespace Hammock.Caching
{
    public static class CacheFactory
    {
#if !Smartphone && !Silverlight
        public static IDependencyCache AspNetCache
        {
            get { return new AspNetCache(); }
        }
#endif

        public static ICache InMemoryCache
        {
            get { return new SimpleCache(); }
        }

#if Silverlight
        public static ICache IsolatedStorageCache
        {
            get { throw new NotImplementedException(); }
        }
#endif
    }
}