using System;

namespace Quark.Asset
{
    [Serializable]
    public class QuarkBundleAsset
    {
        public string Hash;
        public string BundleName;
        public long BundleSize;
        public QuarkBundle QuarkAssetBundle;
    }
}
