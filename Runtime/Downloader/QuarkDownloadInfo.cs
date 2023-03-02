namespace Quark.Networking
{
    public struct QuarkDownloadInfo
    {
        public string DownloadUri;
        public string DownloadPath;
        public long RequiredDownloadSize;
        public QuarkDownloadInfo(string downloadUri, string downloadPath, long requiredDownloadSize)
        {
            DownloadUri = downloadUri;
            DownloadPath = downloadPath;
            RequiredDownloadSize = requiredDownloadSize;
        }
    }
}
