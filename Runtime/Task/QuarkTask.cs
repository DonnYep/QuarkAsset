using System;
using System.Runtime.CompilerServices;

namespace Quark
{
    internal class QuarkTask<T> : IQuarkTask, INotifyCompletion, IDisposable
        where T : UnityEngine.Object
    {
        UnityEngine.Object asset;
        bool isCompleted;
        Action continuation;
        public long TaskId { get; private set; }

        public bool IsCompleted
        {
            get
            {
                return isCompleted;
            }
            set
            {
                isCompleted = value;
                if (isCompleted)
                {
                    continuation?.Invoke();
                }
            }
        }
        public void OnLoadDone(UnityEngine.Object asset)
        {
            this.asset = asset;
            IsCompleted = true;
        }
        public QuarkTask<T> GetAwaiter() { return this; }
        public UnityEngine.Object GetResult()
        {
            return asset;
        }
        public void OnCompleted(Action continuation)
        {
            this.continuation = continuation;
        }
        public void Dispose()
        {
            asset = null;
            TaskId = 0;
        }
    }
}
