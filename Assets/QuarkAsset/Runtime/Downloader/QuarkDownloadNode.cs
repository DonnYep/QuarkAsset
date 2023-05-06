using System;

namespace Quark.Networking
{
    public struct QuarkDownloadNode:IEquatable<QuarkDownloadNode>
    {
        public string DownloadUri { get; private set; }
        public string DownloadPath { get; private set; }
        /// <summary>
        /// length of downloaded file
        /// </summary>
        public long DownloadedBytes { get; private set; }
        /// <summary>
        /// 0~1
        /// </summary>
        public float DownloadProgress{ get; private set; }
        /// <summary>
        /// Length of time spent downloading
        /// </summary>
        public TimeSpan DownloadTimeSpan { get; private set; }
        public QuarkDownloadNode(string downloadUri, string downloadPath, long downloadedLength, float downloadProgress, TimeSpan downloadTimeSpan)
        {
            DownloadUri = downloadUri;
            DownloadPath = downloadPath;
            DownloadedBytes = downloadedLength;
            DownloadProgress = downloadProgress;
            DownloadTimeSpan = downloadTimeSpan;
        }
        public bool Equals(QuarkDownloadNode other)
        {
            return this.DownloadUri == other.DownloadUri &&
                this.DownloadPath == other.DownloadPath;
        }
        public override string ToString()
        {
            return $"URI: {DownloadUri}; DownloadPath: {DownloadPath}; DownloadedBytes: {DownloadedBytes}; DownloadTimeSpan: {DownloadTimeSpan}";
        }
    }
}
