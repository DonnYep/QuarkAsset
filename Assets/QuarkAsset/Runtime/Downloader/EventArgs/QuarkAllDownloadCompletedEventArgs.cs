using Quark.Recyclable;
using System;

namespace Quark.Networking
{
    public class QuarkAllDownloadCompletedEventArgs : EventArgs, IRecyclable
    {
        public QuarkDownloadCompletedInfo[] DownloadSuccessedInfos { get; private set; }
        public QuarkDownloadCompletedInfo[] DownloadFailedInfos { get; private set; }
        public TimeSpan AllDownloadCompletedTimeSpan { get; private set; }
        public void Clear()
        {
            DownloadSuccessedInfos = null;
            DownloadFailedInfos = null;
            AllDownloadCompletedTimeSpan = TimeSpan.Zero;
        }
        public static QuarkAllDownloadCompletedEventArgs Create(QuarkDownloadCompletedInfo[] successedInfos, QuarkDownloadCompletedInfo[] failedInfos, TimeSpan timeSpan)
        {
            var eventArgs = QuarkPool.Acquire<QuarkAllDownloadCompletedEventArgs>();
            eventArgs.DownloadSuccessedInfos = successedInfos;
            eventArgs.DownloadFailedInfos = failedInfos;
            eventArgs.AllDownloadCompletedTimeSpan = timeSpan;
            return eventArgs;
        }
        public static void Release(QuarkAllDownloadCompletedEventArgs eventArgs)
        {
            QuarkPool.Release(eventArgs);
        }
    }
}
