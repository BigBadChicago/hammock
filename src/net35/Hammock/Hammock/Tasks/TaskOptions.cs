using System;

namespace Hammock.Tasks
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public class TaskOptions<T> : TaskOptions
    {
        public virtual IRateLimitingRule<T> RateLimitingRule { get; set; }
    }

#if !SILVERLIGHT
    [Serializable]
#endif
    public class TaskOptions
    {
        public virtual int RepeatTimes { get; set; }
        public virtual TimeSpan RepeatInterval { get; set; }
    }
}
