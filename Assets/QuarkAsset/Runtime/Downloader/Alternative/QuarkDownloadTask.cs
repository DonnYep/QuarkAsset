using System;

namespace Quark.Networking
{
    public struct QuarkDownloadTask : IEquatable<QuarkDownloadTask>
    {
        /// <summary>
        /// DownloadUri = url/bundle key
        /// </summary>
        public string DownloadUri { get; private set; }
        /// <summary>
        /// DownloadPath =abs path/bundle key
        /// </summary>
        public string DownloadPath { get; private set; }
        /// <summary>
        /// bundle size on local path
        /// </summary>
        public long LocalBundleSize { get; private set; }
        /// <summary>
        /// bundle size on manifest
        /// </summary>
        public long RecordedBundleSize { get; private set; }
        public QuarkDownloadTask(string downloadUri, string downloadPath, long recordedBundleSize)
            : this(downloadUri, downloadPath, 0, recordedBundleSize) { }
        public QuarkDownloadTask(string downloadUri, string downloadPath, long localBundleSize, long recordedBundleSize)
        {
            DownloadUri = downloadUri;
            DownloadPath = downloadPath;
            LocalBundleSize = localBundleSize;
            RecordedBundleSize = recordedBundleSize;
        }
        public bool Equals(QuarkDownloadTask other)
        {
            return this.DownloadUri == other.DownloadUri &&
                this.DownloadPath == other.DownloadPath &&
                this.LocalBundleSize == other.LocalBundleSize &&
               this.RecordedBundleSize == other.RecordedBundleSize;
        }
    }
}
