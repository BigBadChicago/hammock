using System;
using System.Collections.Generic;

namespace Hammock.Retries
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public class RetryPolicy
    {
        public virtual int RetryCount { get; set; }
        public virtual ICollection<IRetryCondition> RetryConditions { get; set; }
        
        public RetryPolicy()
        {
            RetryConditions = new List<IRetryCondition>(0);
        }
    }
}
