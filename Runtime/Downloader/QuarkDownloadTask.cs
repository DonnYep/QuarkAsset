using System;

namespace Quark.Networking
{
    internal class QuarkDownloadTask : IEquatable<QuarkDownloadTask>
    {
        /// <summary>
        /// URI绝对路径；
        /// </summary>
        public string DownloadUri { get; private set; }
        /// <summary>
        /// 本地资源的绝对路径；
        /// </summary>
        public string DownloadPath { get; private set; }
        /// <summary>
        /// 需要下载的大小
        /// </summary>
        public long RequiredDownloadSize { get; private set; }
        public QuarkDownloadTask(string downloadUri, string downloadPath, long requiredDownloadSize)
        {
            DownloadUri = downloadUri;
            DownloadPath = downloadPath;
            RequiredDownloadSize = requiredDownloadSize;
        }
        public bool Equals(QuarkDownloadTask other)
        {
            bool result = false;
            if (this.GetType() == other.GetType())
            {
                result = this.DownloadUri == other.DownloadUri &&
                    this.DownloadPath == other.DownloadPath &&
                    RequiredDownloadSize == other.RequiredDownloadSize;

            }
            return result;
        }
    }
}
