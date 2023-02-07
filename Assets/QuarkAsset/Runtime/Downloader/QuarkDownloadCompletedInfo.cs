using System;
using System.Runtime.InteropServices;

namespace Quark.Networking
{
    [StructLayout(LayoutKind.Auto)]
    public struct QuarkDownloadCompletedInfo:IEquatable<QuarkDownloadCompletedInfo>
    {
        public QuarkDownloadCompletedInfo(string uRI, string downloadPath, long downloadedLength, TimeSpan downloadTimeSpan)
        {
            URI = uRI;
            DownloadPath = downloadPath;
            DownloadedLength = downloadedLength;
            DownloadTimeSpan = downloadTimeSpan;
        }
        public string URI { get; private set; }
        public string DownloadPath { get; private set; }
        /// <summary>
        /// length of downloaded file
        /// </summary>
        public long DownloadedLength { get; private set; }
        /// <summary>
        /// Length of time spent downloading
        /// </summary>
        public TimeSpan DownloadTimeSpan { get; private set; }

        public bool Equals(QuarkDownloadCompletedInfo other)
        {
            return this.URI == other.URI &&
                this.DownloadPath == other.DownloadPath;
        }
        public override string ToString()
        {
            return $"URI: {URI}; DownloadPath: {DownloadPath}; DownloadedLength: {DownloadedLength}; DownloadTimeSpan: {DownloadTimeSpan}";
        }
    }
}
