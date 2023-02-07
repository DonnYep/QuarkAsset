using System;

namespace Quark.Networking
{
    internal class QuarkDownloadTask : IEquatable<QuarkDownloadTask>
    {
        /// <summary>
        /// URI绝对路径；
        /// </summary>
        public string URI { get; private set; }
        /// <summary>
        /// 本地资源的绝对路径；
        /// </summary>
        public string DownloadPath { get; private set; }
        public QuarkDownloadTask(string uri, string downloadPath)
        {
            URI = uri;
            DownloadPath = downloadPath;
        }
        public bool Equals(QuarkDownloadTask other)
        {
            bool result = false;
            if (this.GetType() == other.GetType())
            {
                result = this.URI == other.URI && this.DownloadPath == other.DownloadPath;
            }
            return result;
        }
    }
}
