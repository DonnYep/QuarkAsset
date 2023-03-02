using System.Collections.Concurrent;

namespace Quark
{
    internal class QuarkTaskMonitor
    {
        static readonly object locker = new object();
        static QuarkTaskMonitor instance;
        public static QuarkTaskMonitor Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (locker)
                    {
                        if (instance == null)
                        {
                            instance = new QuarkTaskMonitor();
                        }
                    }
                }
                return instance;
            }
        }
        ConcurrentDictionary<long, IQuarkTask> taskDict;
        public QuarkTaskMonitor()
        {
            taskDict = new ConcurrentDictionary<long, IQuarkTask>();
        }
        public bool AddTask(IQuarkTask quarkTask)
        {
            return taskDict.TryAdd(quarkTask.TaskId, quarkTask);
        }
        public bool RemoveTask(long taskId)
        {
            return taskDict.TryRemove(taskId, out _);
        }
        public bool PeekTask(long taskId, out IQuarkTask quarkTask)
        {
            return taskDict.TryGetValue(taskId, out quarkTask);
        }
        public void Dispose()
        {
            taskDict.Clear();
        }
        public void TickRefresh()
        {
            if (taskDict.Count == 0)
                return;
            foreach (var task in taskDict)
            {
                if (task.Value.IsCompleted)
                {
                    taskDict.TryRemove(task.Key, out _);
                }
            }
        }
    }
}
