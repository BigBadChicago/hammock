using System;
using System.Net;

namespace Hammock
{
#if !Silverlight
    [Serializable]
#endif
    public class RestResponse
    {
        public virtual HttpStatusCode StatusCode { get; set; }
        public virtual string StatusDescription { get; set; }
        public virtual string Content { get; set; }
        public virtual object ContentEntity { get; set; }
        public virtual string ContentType { get; set; }
        public virtual Uri ResponseUri { get; set; }
    }

#if !Silverlight
    [Serializable]
#endif
    public class RestResponse<T>
    {
        public virtual HttpStatusCode StatusCode { get; set; }
        public virtual string StatusDescription { get; set; }
        public virtual string Content { get; set; }
        public virtual T ContentEntity { get; set; }
        public virtual string ContentType { get; set; }
        public virtual Uri ResponseUri { get; set; }
    }
}


