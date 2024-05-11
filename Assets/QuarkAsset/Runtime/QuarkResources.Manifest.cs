namespace Quark
{
    public static partial class QuarkResources
    {
        public static DownloadRemoteManifestOperation DownloadRemoteManifest(string url)
        {
            var op = new DownloadRemoteManifestOperation(url);
            QuarkUtility.Unity.CheckCoroutineProvider();
            OperationSystem.StartOperation(op);
            return op;
        }
        public static DownloadRemoteManifestOperation DownloadRemoteManifest(string url, string aesKey)
        {
            var op = new DownloadRemoteManifestOperation(url, aesKey);
            QuarkUtility.Unity.CheckCoroutineProvider();
            OperationSystem.StartOperation(op);
            return op;
        }
        public static DownloadRemoteManifestOperation DownloadRemoteManifest(string url, byte[] aesKeyBytes)
        {
            var op = new DownloadRemoteManifestOperation(url, aesKeyBytes);
            QuarkUtility.Unity.CheckCoroutineProvider();
            OperationSystem.StartOperation(op);
            return op;
        }
    }
}
