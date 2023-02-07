using Quark.Recyclable;
using System;

namespace Quark.Networking
{
    public class QuarkDownloadSuccessEventArgs : EventArgs, IRecyclable
    {
        public QuarkDownloadCompletedInfo CompletedInfo { get; private set; }
        public void Clear()
        {
            CompletedInfo = default;
        }
        internal static QuarkDownloadSuccessEventArgs Create(QuarkDownloadCompletedInfo info)
        {
            var eventArgs = QuarkPool.Acquire<QuarkDownloadSuccessEventArgs>();
            eventArgs.CompletedInfo = info;
            return eventArgs;
        }
        internal static void Release(QuarkDownloadSuccessEventArgs eventArgs)
        {
            QuarkPool.Release(eventArgs);
        }
    }
}
