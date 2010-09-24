using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using Hammock.Extensions;

#if SILVERLIGHT
using Hammock.Silverlight.Compat;
#endif

namespace Hammock
{
#if !Silverlight
    [Serializable]
#endif
    public class RestResponseBase
    {
        public virtual WebResponse InnerResponse { get; set; }
        public virtual WebException InnerException { get; set; }
        public virtual DateTime? RequestDate { get; set; }
        public virtual DateTime? ResponseDate { get; set; }
        public virtual byte[] ResponseBytes { get; set; }
        public virtual string RequestMethod { get; set; }
        public virtual bool RequestKeptAlive { get; set; }
        public virtual HttpStatusCode StatusCode { get; set; }
        public virtual string StatusDescription { get; set; }
        public virtual string Content { get; set; }
        public virtual string ContentType { get; set; }
        public virtual long ContentLength { get; set; }
        public virtual Uri RequestUri { get; set; }
        public virtual Uri ResponseUri { get; set; }
        public virtual bool IsMock { get; set; }
        public virtual bool TimedOut { get; set; }
        public virtual int TimesTried { get; set; }
        public virtual object Tag { get; set; }
        public virtual NameValueCollection Headers { get; set; }
        public virtual bool SkippedDueToRateLimitingRule { get; set; }
        public virtual bool IsFromCache
        {
            get
            {
                return StatusCode == 0 && 
                       StatusDescription.IsNullOrBlank() && 
                       Content != null;
            }
        }

        public RestResponseBase()
        {
            Initialize();
        }

        private void Initialize()
        {
            Headers = new NameValueCollection(0);
        }
    }

#if !Silverlight
    [Serializable]
#endif
    public class RestResponse : RestResponseBase
    {
        public virtual object ContentEntity { get; set; }
    }

#if !Silverlight
    [Serializable]
#endif
    public class RestResponse<T> : RestResponseBase
    {
        public virtual T ContentEntity { get; set; }
    }
}


