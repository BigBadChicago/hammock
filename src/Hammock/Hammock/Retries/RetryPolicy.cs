using System;
using System.Collections.Generic;
using System.Linq;

namespace Hammock.Retries
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public class RetryPolicy
    {
        protected internal ICollection<IRetryCondition> RetryConditions { get; set; }
        
        public virtual int RetryCount { get; set; }
        
        public RetryPolicy()
        {
            RetryConditions = new List<IRetryCondition>(0);
        }

        public virtual void RetryOn(IEnumerable<IRetryCondition> conditions)
        {
            foreach(var condition in conditions)
            {
                RetryConditions.Add(condition);    
            }
        }

        public virtual void RetryOn(params IRetryCondition[] conditions)
        {
            RetryOn(conditions.AsEnumerable());
        }
    }
}
