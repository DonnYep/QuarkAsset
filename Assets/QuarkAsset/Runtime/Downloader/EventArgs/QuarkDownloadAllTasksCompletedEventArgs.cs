using Quark.Recyclable;
using System;

namespace Quark.Networking
{
    public class QuarkDownloadAllTasksCompletedEventArgs : QuarkEventArgsBase
    {
        public QuarkDownloadTask[] DownloadSuccessedTasks { get; private set; }
        public QuarkDownloadTask[] DownloadFailedTasks { get; private set; }
        public QuarkDownloadNode[] DownloadSuccessedNodes { get; private set; }
        public QuarkDownloadNode[] DownloadFailedNodes { get; private set; }
        public TimeSpan DownloadAllTasksCompletedTimeSpan { get; private set; }
        public long DownloadedSize { get;private set; }
        public override void Clear()
        {
            DownloadSuccessedTasks = null;
            DownloadFailedTasks = null;
            DownloadSuccessedNodes = null;
            DownloadFailedNodes = null;
            DownloadAllTasksCompletedTimeSpan = TimeSpan.Zero;
            DownloadedSize =0;
        }
        //internal QuarkDownloadAllTasksCompletedEventArgs() { }
        public static QuarkDownloadAllTasksCompletedEventArgs Create(QuarkDownloadTask[] successedTasks, QuarkDownloadTask[] failedTasks, QuarkDownloadNode[] successedNodes, QuarkDownloadNode[] failedNodes, long downloadedSize, TimeSpan timeSpan)
        {
            var eventArgs = QuarkEventArgsPool.Acquire<QuarkDownloadAllTasksCompletedEventArgs>();
            eventArgs.DownloadSuccessedTasks = successedTasks;
            eventArgs.DownloadFailedTasks = failedTasks;
            eventArgs.DownloadSuccessedNodes = successedNodes;
            eventArgs.DownloadFailedNodes = failedNodes;
            eventArgs.DownloadedSize= downloadedSize;
            eventArgs.DownloadAllTasksCompletedTimeSpan = timeSpan;
            return eventArgs;
        }
        public static void Release(QuarkDownloadAllTasksCompletedEventArgs eventArgs)
        {
            QuarkEventArgsPool.Release(eventArgs);
        }
    }
}
