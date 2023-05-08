using System;
namespace Quark.Networking
{
    public struct QuarkDownloadInfo : IEquatable<QuarkDownloadInfo>
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
        public QuarkDownloadInfo(string downloadUri, string downloadPath, long requiredDownloadSize)
        {
            DownloadUri = downloadUri;
            DownloadPath = downloadPath;
            RequiredDownloadSize = requiredDownloadSize;
        }
        public bool Equals(QuarkDownloadInfo other)
        {
            return this.DownloadUri == other.DownloadUri &&
                       this.DownloadPath == other.DownloadPath &&
                       this.RequiredDownloadSize == other.RequiredDownloadSize;
        }
    }
}
