using System;

namespace Hammock.Web.Query
{
    public class WebQueryResult
    {
        public Uri RequestUri { get; set; }
        public virtual string RequestHttpMethod { get; set; }
        public DateTime? RequestDate { get; set; }
        
        public DateTime? ResponseDate { get; set; }
        public string Response { get; set; }
        public virtual string ResponseType { get; set; }
        public int ResponseHttpStatusCode { get; set; }
        public string ResponseHttpStatusDescription { get; set; }
        public virtual long ResponseLength { get; set; }
        public virtual Uri ResponseUri { get; set; }
    }
}