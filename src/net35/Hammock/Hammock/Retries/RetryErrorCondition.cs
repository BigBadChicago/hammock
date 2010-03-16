using System;
using System.Net;

namespace Hammock.Retries
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public abstract class RetryErrorCondition : IRetryCondition<WebException>
    {
        public virtual Predicate<WebException> RetryIf
        {
            get { return e => false; }
        }
    }
}