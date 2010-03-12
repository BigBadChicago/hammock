using System;
using System.Collections.Generic;

namespace Hammock.Caching
{
    /// <summary>
    /// A basic in-memory cache.
    /// </summary>
    internal class SimpleCache : ICache
    {
        private const string NOT_SUPPORTED_MESSAGE = "This simple cache does not support expiration.";

        private static readonly IDictionary<string, object> _cache = new Dictionary<string, object>(0);

        public int Count
        {
            get { return _cache.Count; }
        }

        public IEnumerable<string> Keys
        {
            get { return _cache.Keys; }
        }

        #region ICache Members

        public void Insert(string key, object value)
        {
            if (!_cache.ContainsKey(key))
            {
                _cache.Add(key, value);
            }
            else
            {
                _cache[key] = value;
            }
        }

        public void Insert(string key, object value, DateTime absoluteExpiration)
        {
            throw new NotSupportedException(NOT_SUPPORTED_MESSAGE);
        }

        public void Insert(string key, object value, TimeSpan slidingExpiration)
        {
            throw new NotSupportedException(NOT_SUPPORTED_MESSAGE);
        }

        public T Get<T>(string key)
        {
            if (_cache.ContainsKey(key))
            {
                return (T)_cache[key];
            }
            return default(T);
        }

        public void Remove(string key)
        {
            if (_cache.ContainsKey(key))
            {
                _cache.Remove(key);
            }
        }

        #endregion
    }

}
