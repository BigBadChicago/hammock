using System;

namespace Hammock.Tasks
{
    public interface ITimedTask<T> : IDisposable
    {
        Action<bool> Action { get; }
        Exception Exception { get; }

        TimeSpan DueTime { get; }
        TimeSpan Interval { get; }

        bool RateLimited { get; }
        IRateLimitingRule<T> RateLimitingRule { get; }
        
        void Start();
        void Start(TimeSpan dueTime, TimeSpan interval);

        void Stop();
    }
}