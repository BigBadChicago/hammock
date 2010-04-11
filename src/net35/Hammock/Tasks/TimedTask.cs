using System;
using System.Threading;

namespace Hammock.Tasks
{
    public class TimedTask : ITimedTask
    {
        protected readonly object Lock = new object();
        protected bool Stopped;
        protected int Iterations;
        protected Timer Timer;
        protected bool ContinueOnError;

        public Action<bool> Action { get; protected set; }
        public Exception Exception { get; protected set; }
        public TimeSpan DueTime { get; protected set; }
        public TimeSpan Interval { get; protected set; }

        public TimedTask(TimeSpan due,
                         TimeSpan interval,
                         int iterations,
                         bool continueOnError,
                         Action<bool> action) :
            this(due, interval, iterations, action)
        {
            ContinueOnError = continueOnError;
        }

        private void Start(bool continueOnError)
        {
            var count = 0;
            Timer = new Timer(state =>
                                  {
                                      try
                                      {
                                          Action(false);
                                          count++;
                                          if (Iterations > 0 && count >= Iterations)
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

        public TimedTask(TimeSpan due,
                         TimeSpan interval,
                         int iterations,
                         Action<bool> action)
        {
            DueTime = due;
            Interval = interval;
            Iterations = iterations;
            Action = action;
        }

        public virtual void Stop()
        {
            if (!Stopped)
            {
                lock (Lock)
                {
                    if (!Stopped)
                    {
                        Stopped = true;
                        Timer.Change(-1, -1);
                    }
                }
            }
        }

        public virtual void Start()
        {
            if (Stopped)
            {
                lock (Lock)
                {
                    if (Stopped)
                    {
                        Stopped = false;
                        if (Timer != null)
                        {
                            Timer.Change(DueTime, Interval);
                        }
                        else
                        {
                            Start(ContinueOnError);
                        }
                    }
                }
            }
        }

        public virtual void Start(TimeSpan dueTime, TimeSpan interval)
        {
            if (Stopped)
            {
                lock (Lock)
                {
                    if (Stopped)
                    {
                        DueTime = dueTime;
                        Interval = interval;
                        Timer.Change(DueTime, Interval);
                    }
                }
            }
        }

        public virtual void Dispose()
        {
            Timer.Dispose();
        }
    }

#if !SILVERLIGHT
    [Serializable]
#endif
    public class TimedTask<T> : TimedTask, ITimedTask<T>
    {
        public TimedTask(TimeSpan due,
                         TimeSpan interval,
                         int iterations,
                         bool continueOnError,
                         Action<bool> action,
                         IRateLimitingRule<T> rateLimitingRule) :
            base(due, interval, iterations, action)
        {
            RateLimitingRule = rateLimitingRule;

            var count = 0;
            Timer = new Timer(state =>
                                   {
                                       try
                                       {
                                           var skip = ShouldSkipForRateLimiting();
                                           Action(skip);
                                           lock (Lock)
                                           {
                                               count++;

                                               if (Iterations > 0 && count >= Iterations)
                                               {
                                                   Stop();
                                               }
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

        public virtual bool RateLimited
        {
            get { return RateLimitingRule != null; }
        }

        public IRateLimitingRule<T> RateLimitingRule { get; set; }

        #endregion

        private bool ShouldSkipForRateLimiting()
        {
            // [JD]: Only pre-skip via predicate; percentage based adjusts rate after the call
            if (RateLimitingRule == null || RateLimitingRule.RateLimitType != RateLimitType.ByPredicate)
            {
                return false;
            }

            if (RateLimitingRule.RateLimitIf == null)
            {
                throw new InvalidOperationException("Rule is set to use predicate, but no predicate is defined.");
            }

            var status = default(T);
            if (RateLimitingRule.GetRateLimitStatus != null)
            {
                status = RateLimitingRule.GetRateLimitStatus();
            }
            return !RateLimitingRule.RateLimitIf(status);
        }
    }
}