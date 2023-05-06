using System;
namespace Quark.Manifest
{
    [Serializable]
    public struct QuarkManifestCompareInfo : IEquatable<QuarkManifestCompareInfo>
    {
        public string BundleName { get; private set; }
        /// <summary>
        /// 用于寻址的文件名；
        /// </summary>
        public string BundleKey { get; private set; }
        public string BundleHash { get; private set; }
        public long BundleSize { get; private set; }
        public QuarkManifestCompareInfo(string bundleName, string bundleKey, string bundleHash, long bundleSize)
        {
            BundleName = bundleName;
            BundleKey = bundleKey;
            BundleHash = bundleHash;
            BundleSize = bundleSize;
        }
        public bool Equals(QuarkManifestCompareInfo other)
        {
            return other.BundleName == this.BundleName &&
                        other.BundleKey == this.BundleKey &&
                        other.BundleHash == this.BundleHash &&
                        other.BundleSize == this.BundleSize;
        }
    }
}
