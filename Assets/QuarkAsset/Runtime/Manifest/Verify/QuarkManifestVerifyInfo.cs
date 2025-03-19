﻿using System;

namespace Quark.Manifest
{
    public struct QuarkManifestVerifyInfo : IEquatable<QuarkManifestVerifyInfo>
    {
        /// <summary>
        /// 文件的地址；
        /// </summary>
        public string Url;
        
        /// <summary>
        /// 资源包的名称；
        /// </summary>
        public string ResourceBundleName;
        
        /// <summary>
        /// 包应该存在的长度
        /// </summary>
        public long ResourceBundleSize;
        
        /// <summary>
        /// 文件长度是否匹配；
        /// </summary>
        public bool ResourceBundleSizeMatched;
        
        /// <summary>
        /// 请求到的文件长度
        /// </summary>
        public long RequestedBundleLength;
        
        /// <summary>
        /// 向后兼容：包的名称
        /// </summary>
        [Obsolete("Use ResourceBundleName instead")]
        public string BundleName { get { return ResourceBundleName; } }
        
        /// <summary>
        /// 向后兼容：包应该存在的长度
        /// </summary>
        [Obsolete("Use ResourceBundleSize instead")]
        public long BundleSize { get { return ResourceBundleSize; } }
        
        /// <summary>
        /// 向后兼容：文件长度是否匹配
        /// </summary>
        [Obsolete("Use ResourceBundleSizeMatched instead")]
        public bool BundleLengthMatched { get { return ResourceBundleSizeMatched; } }
        
        public QuarkManifestVerifyInfo(string url, string bundleName, long bundleSize, bool bundleLengthMatched, long requestedBundleLength)
        {
            Url = url;
            ResourceBundleName = bundleName;
            ResourceBundleSize = bundleSize;
            ResourceBundleSizeMatched = bundleLengthMatched;
            RequestedBundleLength = requestedBundleLength;
        }
        
        public bool Equals(QuarkManifestVerifyInfo other)
        {
            return Url == other.Url &&
                ResourceBundleSize == other.ResourceBundleSize &&
                ResourceBundleName == other.ResourceBundleName &&
                ResourceBundleSizeMatched == other.ResourceBundleSizeMatched;
        }
    }
}
