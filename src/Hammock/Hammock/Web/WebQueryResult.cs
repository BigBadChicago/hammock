using System;

namespace Hammock.Web
{
    public class WebQueryResult
    {
        public virtual Uri RequestUri { get; set; }
        public virtual string RequestHttpMethod { get; set; }
        public virtual DateTime? RequestDate { get; set; }

        public virtual DateTime? ResponseDate { get; set; }
        public virtual string Response { get; set; }
        public virtual string ResponseType { get; set; }
        public virtual int ResponseHttpStatusCode { get; set; }
        public virtual string ResponseHttpStatusDescription { get; set; }
        public virtual long ResponseLength { get; set; }
        public virtual Uri ResponseUri { get; set; }

        public WebQueryResult PreviousResult { get; set; }
    }
}