using System;
using System.Net;

namespace Hammock.Retries
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public class Timeout : RetryErrorCondition
    {
        public override Predicate<WebException> RetryIf
        {
            get
            {
                return e => e.Status == WebExceptionStatus.RequestCanceled ||
                            e.Status == WebExceptionStatus.Timeout;
            }
        }
    }
}