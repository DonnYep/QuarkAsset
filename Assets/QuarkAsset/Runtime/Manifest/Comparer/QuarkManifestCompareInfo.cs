using System;
namespace Quark.Manifest
{
    [Serializable]
    public class QuarkManifestCompareInfo : IEquatable<QuarkManifestCompareInfo>
    {
        public string BundleName;
        /// <summary>
        /// 用于寻址的文件名；
        /// </summary>
        public string BundleKey;
        public long BundleSize;
        public string BundleHash;
        public QuarkManifestCompareInfo(string bundleName, string bundleKey, long bundleSize, string bundleHash)
        {
            BundleName = bundleName;
            BundleKey = bundleKey;
            BundleSize = bundleSize;
            BundleHash = bundleHash;
        }
        public bool Equals(QuarkManifestCompareInfo other)
        {
            return other.BundleName == this.BundleName &&
                        other.BundleKey == this.BundleKey &&
                        other.BundleSize == this.BundleSize &&
                        other.BundleHash == this.BundleHash;
        }
    }
}
