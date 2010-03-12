using System.Collections.Generic;

namespace Hammock.Tasks
{
    public class TimedTaskCollection<T>
    {
        private readonly List<ITimedTask<T>> _tasks = new List<ITimedTask<T>>(0);

        public ITimedTask<T> this[int index]
        {
            get { return _tasks[index]; }
        }

        public void StopAll()
        {
            foreach (var task in _tasks)
            {
                task.Stop();
            }

            _tasks.Clear();
        }

        protected internal void Add(ITimedTask<T> task)
        {
            _tasks.Add(task);
        }
    }
}