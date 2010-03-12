using System;
using System.Threading;

namespace Hammock.Tasks
{
    public class TimedTask<T> : ITimedTask<T>
    {
        private readonly int _iterations;
        private readonly Timer _timer;

        public TimedTask(TimeSpan due, 
                         TimeSpan interval, 
                         bool continueOnError, 
                         int iterations,
                         IRateLimitingRule<T> rateLimitingRule, 
                         Action<bool> action)
        {
            Action = action;

            DueTime = due;
            Interval = interval;
            _iterations = iterations;

            RateLimitingRule = rateLimitingRule;

            var count = 0;

            _timer = new Timer(state =>
                                   {
                                       try
                                       {
                                           var skip = ShouldSkipForRateLimiting();
                                           Action(skip);

                                           count++;

                                           if (_iterations > 0 && count >= _iterations)
                                           {
                                               Stop();
                                           }
                                       }
                                       catch (Exception ex)
                                       {
                                           Exception = ex;
                                           if (!continueOnError)
                                           {
                                               Stop();
                                           }
                                       }
                                   }, null, DueTime, Interval);
        }

        #region ITimedTask Members

        public Action<bool> Action { get; private set; }
        public Exception Exception { get; private set; }

        public bool RateLimited
        {
            get { return RateLimitingRule != null; }
        }

        public TimeSpan DueTime { get; private set; }
        public TimeSpan Interval { get; private set; }
        public IRateLimitingRule<T> RateLimitingRule { get; set; }

        public void Stop()
        {
            _timer.Change(-1, -1);
        }

        public void Start()
        {
            _timer.Change(DueTime, Interval);
        }

        public void Start(TimeSpan dueTime, TimeSpan interval)
        {
            DueTime = dueTime;
            Interval = interval;
            _timer.Change(DueTime, Interval);
        }

        public void Dispose()
        {
            _timer.Dispose();
        }

        #endregion

        private bool ShouldSkipForRateLimiting()
        {
            // [JD]: Only pre-skip via predicate; percentage based adjusts rate after the call
            if (RateLimitingRule == null || RateLimitingRule.RateLimitingType != RateLimitingType.ByPredicate)
            {
                return false;
            }

            if (RateLimitingRule.RateLimitPredicate == null)
            {
                throw new InvalidOperationException("Rule is set to use predicate, but no predicate is defined.");
            }

            var status = default(T);
            if (RateLimitingRule.GetRateLimitStatus != null)
            {
                status = RateLimitingRule.GetRateLimitStatus();
            }
            return !RateLimitingRule.RateLimitPredicate(status);
        }
    }
}