using Quark.Recyclable;
using System;

namespace Quark.Networking
{
    public class QuarkDownloadFailureEventArgs : QuarkEventArgsBase
    {
        public QuarkDownloadNode CurrentDownloadNode { get; private set; }
        public string ErrorMessage { get; private set; }

        public override void Clear()
        {
            CurrentDownloadNode = default;
            ErrorMessage = string.Empty;
        }
        //internal QuarkDownloadFailureEventArgs() { }
        internal static QuarkDownloadFailureEventArgs Create(QuarkDownloadNode node, string errorMessage)
        {
            var eventArgs = QuarkEventArgsPool.Acquire<QuarkDownloadFailureEventArgs>();
            eventArgs.CurrentDownloadNode = node;
            eventArgs.ErrorMessage = errorMessage;
            return eventArgs;
        }
        internal static void Release(QuarkDownloadFailureEventArgs eventArgs)
        {
            QuarkEventArgsPool.Release(eventArgs);
        }
    }
}
