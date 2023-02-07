using Quark.Recyclable;
using System;

namespace Quark.Networking
{
    public class QuarkDownloadFailureEventArgs : EventArgs, IRecyclable
    {
        public QuarkDownloadCompletedInfo CompletedInfo { get; private set; }
        public string ErrorMessage { get; private set; }
        public void Clear()
        {
            CompletedInfo = default;
            ErrorMessage = string.Empty;
        }
        internal static QuarkDownloadFailureEventArgs Create(QuarkDownloadCompletedInfo info, string errorMessage)
        {
            var eventArgs = QuarkPool.Acquire<QuarkDownloadFailureEventArgs>();
            eventArgs.CompletedInfo = info;
            eventArgs.ErrorMessage = errorMessage;
            return eventArgs;
        }
        internal static void Release(QuarkDownloadFailureEventArgs eventArgs)
        {
            QuarkPool.Release(eventArgs);
        }
    }
}
