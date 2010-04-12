using System;

namespace Hammock.Tasks
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public class TaskOptions<T> : TaskOptions, ITaskOptions<T>
    {
        public virtual RateLimitType RateLimitType { get; set; }
        public virtual double RateLimitPercent { get; set; }
        public virtual Predicate<T> RateLimitingPredicate { get; set; }
        public virtual Func<T> GetRateLimitStatus { get; set; }
    }

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
