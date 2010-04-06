using System;
using System.Net;

namespace Hammock.Web
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public class WebQueryResult
    {
        private WebQueryResult _previousResult; 

        // Set by WebQuery
        public virtual DateTime? RequestDate { get; set; }
        public virtual Uri RequestUri { get; set; }
        public virtual string RequestHttpMethod { get; set; }
        public virtual bool RequestKeptAlive { get; set; }
        
        public virtual DateTime? ResponseDate { get; set; }
        public virtual WebResponse WebResponse { get; set; }
        public virtual string Response { get; set; }
        public virtual string ResponseType { get; set; }
        public virtual int ResponseHttpStatusCode { get; set; }
        public virtual string ResponseHttpStatusDescription { get; set; }
        public virtual long ResponseLength { get; set; }
        public virtual Uri ResponseUri { get; set; }

        public virtual bool IsMock { get; set; }

        // Set by RestClient
        public virtual WebQueryResult PreviousResult
        {
            get { return _previousResult; }
            set
            {
                if ( ReferenceEquals(this, value))
                {
                    throw new InvalidOperationException("Result can't be its own previous result");
                }
                _previousResult = value; 
            }
        }
        public virtual WebException Exception { get; set; }
        public virtual bool WasRateLimited { get; set; }
    }
}