using System;
using System.Collections.Generic;

namespace Hammock.Tasks
{
#if !SILVERLIGHT
    [Serializable]
#endif
    internal class TimedTaskCollection<T>
    {
        private readonly List<ITimedTask<T>> _tasks = new List<ITimedTask<T>>(0);

        public virtual ITimedTask<T> this[int index]
        {
            get { return _tasks[index]; }
        }

        public virtual void StopAll()
        {
            foreach (var task in _tasks)
            {
                task.Stop();
            }

            _tasks.Clear();
        }

        protected internal virtual void Add(ITimedTask<T> task)
        {
            _tasks.Add(task);
        }
    }
}