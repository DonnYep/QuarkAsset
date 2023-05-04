using System;

namespace Quark.Manifest
{
    public struct QuarkManifestVerifyInfo : IEquatable<QuarkManifestVerifyInfo>
    {
        /// <summary>
        /// 文件的地址；
        /// </summary>
        public string Url;
        /// <summary>
        /// 包的名称；
        /// </summary>
        public string BundleName;
        /// <summary>
        /// 包应该存在的长度
        /// </summary>
        public long BundleSize;
        /// <summary>
        /// 文件长度是否匹配；
        /// </summary>
        public bool BundleLengthMatched;
        /// <summary>
        /// 请求到的文件长度
        /// </summary>
        public long RequestedBundleLength;
        public QuarkManifestVerifyInfo(string url, string bundleName, long bundleSize, bool bundleLengthMatched, long requestedBundleLength)
        {
            Url = url;
            BundleName = bundleName;
            BundleSize = bundleSize;
            BundleLengthMatched = bundleLengthMatched;
            RequestedBundleLength = requestedBundleLength;
        }
        public bool Equals(QuarkManifestVerifyInfo other)
        {
            return Url == other.Url &&
                BundleSize == other.BundleSize &&
                BundleName == other.BundleName &&
                BundleLengthMatched == other.BundleLengthMatched;
        }
    }
}
