using System;
using System.Net;

namespace Hammock
{
#if !Silverlight
    [Serializable]
#endif
    public class RestResponseBase
    {
        public virtual HttpStatusCode StatusCode { get; set; }
        public virtual string StatusDescription { get; set; }
        public virtual string Content { get; set; }
        public virtual string ContentType { get; set; }
        public virtual long ContentLength { get; set; }
        public virtual Uri ResponseUri { get; set; }

        public virtual bool IsFromCache
        {
            get
            {
                return StatusCode == 0 && StatusDescription == null && Content != null;
            }
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


