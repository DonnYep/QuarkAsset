using Quark.Asset;
using UnityEngine;

namespace Quark
{
    /// <summary>
    /// QuarkBundle的包裹对象，记录引用计数
    /// </summary>
    public class QuarkBundleWarpper
    {
        int referenceCount;
        QuarkBundle quarkAssetBundle;
        string bundlePersistentPath;
        public string BundlePersistentPath { get { return bundlePersistentPath; } }
        public QuarkBundle QuarkAssetBundle { get { return quarkAssetBundle; } }
        public int ReferenceCount
        {
            get { return referenceCount; }
            set
            {
                referenceCount = value;
                if (referenceCount < 0)
                    referenceCount = 0;
            }
        }
        /// <summary>
        /// AssetBundle 包体对象；
        /// </summary>
        public AssetBundle AssetBundle { get; set; }
        public QuarkBundleWarpper(QuarkBundle quarkAssetBundle, string bundlePersistentPath)
        {
            this.quarkAssetBundle = quarkAssetBundle;
            this.bundlePersistentPath = bundlePersistentPath;
        }
        public T LoadAsset<T>(string assetName)
            where T : UnityEngine.Object
        {
            if (AssetBundle == null)
                return null;
            return AssetBundle?.LoadAsset<T>(assetName);
        }
        public void UnloadAsset(string assetName)
        {

        }
    }
}
