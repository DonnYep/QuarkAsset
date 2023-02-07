using System;
using System.Runtime.CompilerServices;
namespace Quark
{
    internal class QuarkLoadAwaiter<T> : INotifyCompletion where T : UnityEngine.Object
    {
        Action continuation;
        T asset;
        public QuarkLoadAwaiter(string assetName)
        {
            QuarkResources.LoadAssetAsync<T>(assetName, OnLoad);
        }
        public bool IsCompleted { get; private set; }
        public T GetResult()
        {
            return asset;
        }
        public void OnCompleted(Action continuation)
        {
            this.continuation = continuation;
        }
        public QuarkLoadAwaiter<T> GetAwaiter()
        {
            return this;
        }
        void OnLoad(T asset)
        {
            this.asset = asset;
            continuation?.Invoke();
            IsCompleted = true;
        }
    }
}
