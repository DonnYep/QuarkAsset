using Quark.Recyclable;
using System;

namespace Quark.Networking
{
    public class QuarkAllDownloadCompletedEventArgs : QuarkEventArgsBase
    {
        public QuarkDownloadNode[] DownloadSuccessedNodes { get; private set; }
        public QuarkDownloadNode[] DownloadFailedNodes { get; private set; }
        public TimeSpan AllDownloadCompletedTimeSpan { get; private set; }
        public override void Clear()
        {
            DownloadSuccessedNodes = null;
            DownloadFailedNodes = null;
            AllDownloadCompletedTimeSpan = TimeSpan.Zero;
        }
        //internal QuarkAllDownloadCompletedEventArgs() { }
        public static QuarkAllDownloadCompletedEventArgs Create(QuarkDownloadNode[] successedNodes, QuarkDownloadNode[] failedNodes, TimeSpan timeSpan)
        {
            var eventArgs = QuarkEventArgsPool.Acquire<QuarkAllDownloadCompletedEventArgs>();
            eventArgs.DownloadSuccessedNodes = successedNodes;
            eventArgs.DownloadFailedNodes = failedNodes;
            eventArgs.AllDownloadCompletedTimeSpan = timeSpan;
            return eventArgs;
        }
        public static void Release(QuarkAllDownloadCompletedEventArgs eventArgs)
        {
            QuarkEventArgsPool.Release(eventArgs);
        }
    }
}
