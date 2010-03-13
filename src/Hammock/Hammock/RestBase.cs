using System;
using System.Net;
using Hammock.Authentication;
using Hammock.Caching;
using Hammock.Model;
using Hammock.Serialization;
using Hammock.Web;

#if SILVERLIGHT
using Hammock.Silverlight.Compat;
#else
using System;
using System.Collections.Specialized;
#endif

namespace Hammock
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public abstract class RestBase : PropertyChangedBase 
    {
#if !Silverlight
        public virtual ServicePoint ServicePoint { get; set; }
#endif
        protected virtual internal NameValueCollection Headers { get; set; }
        protected virtual internal WebParameterCollection Parameters { get; set; }
        public virtual string Proxy { get; set; }
        public virtual TimeSpan? Timeout { get; set; }
        public virtual string VersionPath { get; set; }
        public virtual string UserAgent { get; set; }
        public virtual WebMethod? Method { get; set; }
        public virtual IWebCredentials Credentials { get; set; }

        public virtual ISerializer Serializer { get; set; }
        public virtual IDeserializer Deserializer { get; set; }
        
        public virtual ICache Cache { get; set; }
        public virtual CacheOptions CacheOptions { get; set; }
        public virtual Func<string> CacheKeyFunction { get; set; }
        public DecompressionMethods DecompressionMethods { get; set; }

        public void AddHeader(string name, string value)
        {
            Headers.Add(name, value);
        }

        public void AddParameter(string name, string value)
        {
            Parameters.Add(name, value);
        }
    }
}