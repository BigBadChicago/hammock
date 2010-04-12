using System;

namespace Hammock.Tasks
{
    public interface ITaskOptions<T>
    {
        RateLimitType RateLimitType { get; set; }
        double RateLimitPercent { get; set; }
        Predicate<T> RateLimitingPredicate { get; }
        Func<T> GetRateLimitStatus { get; }
    }

    public interface ITaskOptions
    {
        TimeSpan DueTime { get; set; }
        int RepeatTimes { get; set; }
        TimeSpan RepeatInterval { get; set; }
        bool ContinueOnError { get; set; }
    }
}