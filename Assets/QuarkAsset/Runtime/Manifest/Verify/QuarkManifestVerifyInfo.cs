﻿using System;

namespace Quark.Manifest
{
    /// <summary>
    /// 清单文件验证信息
    /// </summary>
    [Serializable]
    public struct QuarkManifestVerifyInfo : IEquatable<QuarkManifestVerifyInfo>
    {
        private string resourceBundleName;
        private string resourcePath;
        private string hash;
        private long resourceBundleSize;
        private bool resourceBundleSizeMatched;

        /// <summary>
        /// 资源包名称
        /// </summary>
        public string ResourceBundleName
        {
            get { return resourceBundleName; }
            set { resourceBundleName = value; }
        }

        /// <summary>
        /// 资源路径
        /// </summary>
        public string ResourcePath
        {
            get { return resourcePath; }
            set { resourcePath = value; }
        }

        /// <summary>
        /// 资源哈希值
        /// </summary>
        public string Hash
        {
            get { return hash; }
            set { hash = value; }
        }

        /// <summary>
        /// 资源包大小
        /// </summary>
        public long ResourceBundleSize
        {
            get { return resourceBundleSize; }
            set { resourceBundleSize = value; }
        }

        /// <summary>
        /// 资源包大小是否匹配
        /// </summary>
        public bool ResourceBundleSizeMatched
        {
            get { return resourceBundleSizeMatched; }
            set { resourceBundleSizeMatched = value; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="bundleName">资源包名称</param>
        /// <param name="path">资源路径</param>
        /// <param name="hash">资源哈希值</param>
        /// <param name="size">资源包大小</param>
        /// <param name="sizeMatched">资源包大小是否匹配</param>
        public QuarkManifestVerifyInfo(string bundleName, string path, string hash, long size, bool sizeMatched)
        {
            resourceBundleName = bundleName;
            resourcePath = path;
            this.hash = hash;
            resourceBundleSize = size;
            resourceBundleSizeMatched = sizeMatched;
        }

        /// <summary>
        /// 相等性比较
        /// </summary>
        /// <param name="other">比较对象</param>
        /// <returns>是否相等</returns>
        public bool Equals(QuarkManifestVerifyInfo other)
        {
            return resourceBundleName == other.resourceBundleName &&
                   resourcePath == other.resourcePath &&
                   hash == other.hash &&
                   resourceBundleSize == other.resourceBundleSize &&
                   resourceBundleSizeMatched == other.resourceBundleSizeMatched;
        }

        // 兼容性属性 - 用于支持可能存在的旧代码
        public string BundleName { get { return resourceBundleName; } set { resourceBundleName = value; } }
        public string Path { get { return resourcePath; } set { resourcePath = value; } }
        public long BundleSize { get { return resourceBundleSize; } set { resourceBundleSize = value; } }
        public bool SizeMatched { get { return resourceBundleSizeMatched; } set { resourceBundleSizeMatched = value; } }
    }
}
