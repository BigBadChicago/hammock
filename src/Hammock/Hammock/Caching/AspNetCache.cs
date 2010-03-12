using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Caching;

namespace Hammock.Caching
{
    /// <summary>
    /// The ASP.NET Cache system implemented behind <see cref="IWebCache" />
    /// </summary>
    internal class AspNetCache : IWebCache
    {
        #region IWebCache Members

        public int Count
        {
            get { return HttpRuntime.Cache.Count; }
        }

        public void Add(string key, object value, CacheDependency dependency, DateTime absoluteExpiration,
                        TimeSpan slidingExpiration, CacheItemPriority priority,
                        CacheItemRemovedCallback onRemoveCallback)
        {
            HttpRuntime.Cache.Add(key, value, dependency, absoluteExpiration, Cache.NoSlidingExpiration, priority,
                                  onRemoveCallback);
        }

        public void Insert(string key, object value)
        {
            HttpRuntime.Cache.Insert(key, value);
        }

        public void Insert(string key, object value, DateTime absoluteExpiration)
        {
            HttpRuntime.Cache.Insert(key, value, null, absoluteExpiration, Cache.NoSlidingExpiration);
        }

        public void Insert(string key, object value, TimeSpan slidingExpiration)
        {
            HttpRuntime.Cache.Insert(key, value, null, Cache.NoAbsoluteExpiration, slidingExpiration);
        }

        public void Insert(string key, object value, CacheDependency dependencies)
        {
            HttpRuntime.Cache.Insert(key, value, dependencies, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration);
        }

        public void Insert(string key, object value, CacheDependency dependencies, DateTime absoluteExpiration)
        {
            HttpRuntime.Cache.Insert(key, value, dependencies, absoluteExpiration, Cache.NoSlidingExpiration);
        }

        public void Insert(string key, object value, CacheDependency dependencies, TimeSpan slidingExpiration)
        {
            HttpRuntime.Cache.Insert(key, value, dependencies, Cache.NoAbsoluteExpiration, slidingExpiration);
        }

        public void Insert(string key, object value, CacheDependency dependencies, TimeSpan slidingExpiration,
                           CacheItemPriority priority, CacheItemRemovedCallback onRemoveCallback)
        {
            HttpRuntime.Cache.Insert(key, value, dependencies, Cache.NoAbsoluteExpiration, slidingExpiration, priority,
                                     onRemoveCallback);
        }

        public void Insert(string key, object value, CacheDependency dependencies, DateTime absoluteExpiration,
                           CacheItemPriority priority, CacheItemRemovedCallback onRemoveCallback)
        {
            HttpRuntime.Cache.Insert(key, value, dependencies, absoluteExpiration, Cache.NoSlidingExpiration, priority,
                                     onRemoveCallback);
        }

        public void Insert(string key, object value, CacheDependency dependencies, TimeSpan slidingExpiration,
                           CacheItemUpdateCallback onUpdateCallback)
        {
            HttpRuntime.Cache.Insert(key, value, dependencies, Cache.NoAbsoluteExpiration, slidingExpiration,
                                     onUpdateCallback);
        }

        public void Insert(string key, object value, CacheDependency dependencies, DateTime absoluteExpiration,
                           CacheItemUpdateCallback onUpdateCallback)
        {
            HttpRuntime.Cache.Insert(key, value, dependencies, absoluteExpiration, Cache.NoSlidingExpiration,
                                     onUpdateCallback);
        }

        public void Clear()
        {
            var keys = new List<string>();
            var enumerator = HttpRuntime.Cache.GetEnumerator();

            while (enumerator.MoveNext())
            {
                var key = enumerator.Key.ToString();
                keys.Add(key);
            }

            foreach (var key in keys)
            {
                HttpRuntime.Cache.Remove(key);
            }
        }

        public T Get<T>(string key)
        {
            return (T) HttpRuntime.Cache.Get(key);
        }

        public void Remove(string key)
        {
            HttpRuntime.Cache.Remove(key);
        }

        public IEnumerable<string> Keys
        {
            get
            {
                var enumerator = HttpRuntime.Cache.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    yield return enumerator.Key.ToString();
                }
            }
        }

        #endregion
    }
}