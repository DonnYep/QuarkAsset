using System.Runtime.InteropServices;
using System;

namespace Quark.Verify
{
    [StructLayout(LayoutKind.Auto)]
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
        /// 文件长度是否匹配；
        /// </summary>
        public bool BundleLengthMatched;
        /// <summary>
        /// 请求到的文件长度
        /// </summary>
        public long RequestedBundleLength;
        public QuarkManifestVerifyInfo(string url, string bundleName, bool bundleLengthMatched, long requestedBundleLength)
        {
            Url = url;
            BundleName = bundleName;
            BundleLengthMatched = bundleLengthMatched;
            RequestedBundleLength = requestedBundleLength;
        }
        public bool Equals(QuarkManifestVerifyInfo other)
        {
            return Url == other.Url &&
                BundleName == other.BundleName &&
                BundleLengthMatched == other.BundleLengthMatched;
        }
    }
}
