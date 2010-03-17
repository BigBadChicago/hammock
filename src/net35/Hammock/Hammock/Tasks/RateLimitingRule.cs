using System;

namespace Hammock.Tasks
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public class RateLimitingRule<T> : IRateLimitingRule<T>
    {
        private readonly RateLimitType _rateLimitType;

        public RateLimitingRule(Predicate<T> predicate)
        {
            _rateLimitType = RateLimitType.ByPredicate;
            RateLimitPredicate = predicate;
        }

        public RateLimitingRule(double percentOfTotal)
        {
            _rateLimitType = RateLimitType.ByPercent;
            LimitToPercentOfTotal = percentOfTotal;
        }

        public RateLimitingRule(Func<T> getRateLimitStatus, Predicate<T> rateLimitPredicate)
        {
            GetRateLimitStatus = getRateLimitStatus;
            RateLimitPredicate = rateLimitPredicate;
        }

        #region IRateLimitingRule Members

        public virtual double? LimitToPercentOfTotal { get; private set; }
        public virtual RateLimitType RateLimitType
        {
            get { return _rateLimitType; }
        }

        public Func<T> GetRateLimitStatus { get; set; }
        public Predicate<T> RateLimitPredicate { get; private set; }

        #endregion
    }
}