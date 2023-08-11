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
        public string BundlePath { get; set; }
        public string BundleHash { get; private set; }
        public long BundleSize { get; private set; }
        public string BundleFormatSize { get; private set; }
        public QuarkBundleChangeType BundleChangeType { get; set; }
        public QuarkManifestCompareInfo(string bundleName, string bundleKey, string bundleHash, long bundleSize, string bundleFormatSize, QuarkBundleChangeType bundleChangeType)
        {
            BundleName = bundleName;
            BundleKey = bundleKey;
            BundleHash = bundleHash;
            BundleSize = bundleSize;
            BundleFormatSize = bundleFormatSize;
            BundleChangeType = bundleChangeType;
            BundlePath = string.Empty;
        }
        public bool Equals(QuarkManifestCompareInfo other)
        {
            return other.BundleName == this.BundleName &&
                        other.BundleKey == this.BundleKey &&
                        other.BundleHash == this.BundleHash &&
                        other.BundleSize == this.BundleSize &&
                        other.BundleChangeType == this.BundleChangeType;
        }
    }
}
