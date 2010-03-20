using System;
using System.Net;

namespace Hammock.Retries
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public class ConnectionClosed : RetryErrorCondition
    {
        public override Predicate<WebException> RetryIf
        {
            get
            {
                return e => e.Status == WebExceptionStatus.ConnectionClosed ||
                            e.Status == WebExceptionStatus.KeepAliveFailure;
            }
        }
    }
}