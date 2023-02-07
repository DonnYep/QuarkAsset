using Quark.Recyclable;
using System;

namespace Quark.Networking
{
    public class QuarkDownloadOverallProgressEventArgs : EventArgs, IRecyclable
    {
        /// <summary>
        /// 当前正在下载的uri资源；
        /// </summary>
        public string URI { get; private set; }
        /// <summary>
        ///整体下载百分比进度 0~100%；
        /// </summary>
        public float OverallProgress { get; private set; }
        /// <summary>
        /// 当前下载文件的百分比；
        /// </summary>
        public float IndividualProgress { get; private set; }
        /// <summary>
        /// 当前资源的下载缓存路径；
        /// </summary>
        public string DownloadPath { get; private set; }
        /// <summary>
        /// 资源下载的长度；
        /// </summary>
        public ulong DownloadedBytes { get; private set; }
        public void Clear()
        {
            URI = null;
            DownloadPath = null;
            OverallProgress = 0;
            DownloadedBytes = 0;
        }
        public static QuarkDownloadOverallProgressEventArgs Create(string uri, string downloadPath, float overallProgress, float individualProgress, ulong downloadedBytes)
        {
            var eventArgs = QuarkPool.Acquire<QuarkDownloadOverallProgressEventArgs>();
            eventArgs.URI = uri;
            eventArgs.OverallProgress = overallProgress;
            eventArgs.DownloadPath = downloadPath;
            eventArgs.IndividualProgress = individualProgress;
            eventArgs.DownloadedBytes = downloadedBytes;
            return eventArgs;

        }
        public static void Release(QuarkDownloadOverallProgressEventArgs eventArgs)
        {
            QuarkPool.Release(eventArgs);
        }
    }
}
