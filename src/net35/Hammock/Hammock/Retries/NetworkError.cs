using System;
using System.Net;

namespace Hammock.Retries
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public class NetworkError : RetryErrorCondition
    {
        public override Predicate<WebException> RetryIf
        {
            get
            {
                return e => e.Status != WebExceptionStatus.Success &&
#if !SILVERLIGHT
                            e.Status != WebExceptionStatus.ProtocolError &&
#endif
                            e.Status != WebExceptionStatus.Pending;
            }
        }
    }
}