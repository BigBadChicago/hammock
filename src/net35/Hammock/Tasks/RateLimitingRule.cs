using System;

namespace Hammock.Tasks
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public class RateLimitingRule<T> : IRateLimitingRule<T>
    {
        private readonly RateLimitType _rateLimitType;

        public RateLimitingRule(Predicate<T> rateLimitIf)
        {
            _rateLimitType = RateLimitType.ByPredicate;
            RateLimitIf = rateLimitIf;
        }

        public RateLimitingRule(double percentOfTotal)
        {
            _rateLimitType = RateLimitType.ByPercent;
            LimitToPercentOfTotal = percentOfTotal;
        }

        public RateLimitingRule(Func<T> getRateLimitStatus, Predicate<T> rateLimitIf)
        {
            _rateLimitType = RateLimitType.ByPredicate;
            GetRateLimitStatus = getRateLimitStatus;
            RateLimitIf = rateLimitIf;
        }

        public RateLimitingRule(Func<T> getRateLimitStatus, double percentOfTotal)
        {
            _rateLimitType = RateLimitType.ByPercent;
            GetRateLimitStatus = getRateLimitStatus;
            LimitToPercentOfTotal = percentOfTotal;
        }

        #region IRateLimitingRule Members

        public virtual double? LimitToPercentOfTotal { get; private set; }
        public virtual RateLimitType RateLimitType
        {
            get { return _rateLimitType; }
        }

        public Func<T> GetRateLimitStatus { get; set; }
        public Predicate<T> RateLimitIf { get; private set; }

        #endregion
    }
}