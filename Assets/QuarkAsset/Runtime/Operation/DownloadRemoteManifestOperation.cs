namespace Quark
{
    public class DownloadRemoteManifestOperation : AsyncOperationBase
    {
        string remoteUrl;
        byte[] aesKeyBytes;
        public Asset.QuarkManifest Manifest { get; private set; }
        long taskId;
        public DownloadRemoteManifestOperation(string url)
        {
            remoteUrl = url;
            aesKeyBytes = new byte[0];
        }
        public DownloadRemoteManifestOperation(string url, string aesKey)
        {
            remoteUrl = url;
            aesKeyBytes = QuarkUtility.GenerateBytesAESKey(aesKey);
        }
        public DownloadRemoteManifestOperation(string url, byte[] aesKeyBytes)
        {
            remoteUrl = url;
            this.aesKeyBytes = aesKeyBytes;
        }
        internal override void OnAbort()
        {
            QuarkResources.QuarkManifestRequester.RemoveTask(taskId);
        }
        internal override void OnStart()
        {
            var remoteManifestUrl = QuarkUtility.WebPathCombine(remoteUrl, QuarkConstant.MANIFEST_NAME);
            taskId = QuarkResources.QuarkManifestRequester.AddTask(remoteManifestUrl, aesKeyBytes, OnSuccess, OnFailure);
            QuarkResources.QuarkManifestRequester.StartRequestManifest();
        }
        internal override void OnUpdate()
        {

        }
        void OnSuccess(Asset.QuarkManifest quarkManifest)
        {
            Manifest = quarkManifest;
            Status = AsyncOperationStatus.Succeeded;
        }
        void OnFailure(string errorMessage)
        {
            Error = errorMessage;
            Status = AsyncOperationStatus.Failed;
        }
    }
}
