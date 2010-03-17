using System;

namespace Hammock.Tasks
{
#if !NETCF
#if !SILVERLIGHT
    [Serializable]
#endif
    public class TaskOptions<T> : TaskOptions, ITaskOptions<T>
    {
        public virtual IRateLimitingRule<T> RateLimitingRule { get; set; }
        public virtual RateLimitType RateLimitType { get; set; }
        public virtual double RateLimitPercent { get; set; }
        public virtual Predicate<T> RateLimitingPredicate { get; set; }
        public virtual Func<T> GetRateLimitStatus { get; set; }
    }
#endif

#if !SILVERLIGHT
    [Serializable]
#endif
    public class TaskOptions : ITaskOptions
    {
        public virtual TimeSpan DueTime { get; set; }
        public virtual int RepeatTimes { get; set; }
        public virtual TimeSpan RepeatInterval { get; set; }
        public virtual bool ContinueOnError { get; set; }
    }
}
