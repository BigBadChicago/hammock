using System;

namespace Hammock.Tasks
{
    public class RateLimitingRule<T> : IRateLimitingRule<T>
    {
        private readonly RateLimitingType _rateLimitingType;

        public RateLimitingRule(Predicate<T> predicate)
        {
            _rateLimitingType = RateLimitingType.ByPredicate;
            RateLimitPredicate = predicate;
        }

        public RateLimitingRule(double percentOfTotal)
        {
            _rateLimitingType = RateLimitingType.ByPercent;
            LimitToPercentOfTotal = percentOfTotal;
        }

        #region IRateLimitingRule Members

        public double? LimitToPercentOfTotal { get; private set; }
        public RateLimitingType RateLimitingType
        {
            get { return _rateLimitingType; }
        }

        public Func<T> GetRateLimitStatus { get; set; }
        public Predicate<T> RateLimitPredicate { get; private set; }

        #endregion
    }
}