using Quark.Asset;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Quark
{
    internal class QuarkManifestAwaiter : INotifyCompletion
    {
        QuarkManifest manifest;
        Action continuation;
        public QuarkManifestAwaiter(string manifestUrl, string manifestAesKey)
        {
            var aesKeyBytes = QuarkUtility.GenerateBytesAESKey(manifestAesKey);
            QuarkDataProxy.PersistentPath = manifestUrl;
            string uri = QuarkUtility.PlatformPerfix+ manifestUrl;
            if (!manifestUrl.EndsWith(QuarkConstant.MANIFEST_NAME))
            {
                uri = Path.Combine(manifestUrl, QuarkConstant.MANIFEST_NAME);
            }
            QuarkResources.QuarkManifestRequester.OnManifestAcquireSuccess(OnManifestAcquireSuccess);
            QuarkResources.QuarkManifestRequester.OnManifestAcquireFailure(OnManifestAcquireFailure);
            QuarkResources.QuarkManifestRequester.RequestManifestAsync(uri, aesKeyBytes);
        }
        public void OnCompleted(Action continuation)
        {
            this.continuation = continuation;
        }
        public bool IsCompleted { get; private set; }
        public QuarkManifest GetResult()
        {
            return manifest;
        }
        public QuarkManifestAwaiter GetAwaiter()
        {
            return this;
        }
        void OnManifestAcquireSuccess(QuarkManifest manifest)
        {
            this.manifest = manifest;
            continuation?.Invoke();
            IsCompleted = true;
        }
        void OnManifestAcquireFailure(string errorMessage)
        {
            continuation?.Invoke();
            IsCompleted = true;
        }
    }
}
