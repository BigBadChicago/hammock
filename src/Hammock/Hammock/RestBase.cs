using Hammock.Authentication;
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
        protected internal NameValueCollection Headers { get; set; }
        protected internal WebParameterCollection Parameters { get; set; }
        public string VersionPath { get; set; }
        public string UserAgent { get; set; }
        public WebMethod? Method { get; set; }
        public IWebCredentials Credentials { get; set; }
        
        public ISerializer Serializer { get; set; }
        public IDeserializer Deserializer { get; set; }

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