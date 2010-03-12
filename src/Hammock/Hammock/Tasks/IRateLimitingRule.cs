using System;

namespace Hammock.Tasks
{
    public interface IRateLimitingRule<T>
    {
        double? LimitToPercentOfTotal { get; }
        RateLimitingType RateLimitingType { get; }
        Func<T> GetRateLimitStatus { get; set; }
        Predicate<T> RateLimitPredicate { get; }
    }
}