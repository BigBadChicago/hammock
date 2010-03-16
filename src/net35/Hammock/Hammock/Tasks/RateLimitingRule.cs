using System;

namespace Hammock.Tasks
{
#if !SILVERLIGHT
    [Serializable]
#endif
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

        public RateLimitingRule(Func<T> getRateLimitStatus, Predicate<T> rateLimitPredicate)
        {
            GetRateLimitStatus = getRateLimitStatus;
            RateLimitPredicate = rateLimitPredicate;
        }

        #region IRateLimitingRule Members

        public virtual double? LimitToPercentOfTotal { get; private set; }
        public virtual RateLimitingType RateLimitingType
        {
            get { return _rateLimitingType; }
        }

        public virtual Func<T> GetRateLimitStatus { get; set; }
        public virtual Predicate<T> RateLimitPredicate { get; private set; }

        #endregion
    }
}