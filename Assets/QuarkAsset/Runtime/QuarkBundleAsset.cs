using System;
using System.Collections.Generic;
using Quark.Asset;

namespace Quark
{
    /// <summary>
    /// Quark资源包信息
    /// </summary>
    [Serializable]
    public class QuarkBundleAsset : IEquatable<QuarkBundleAsset>
    {
        /// <summary>
        /// 资源包名称
        /// </summary>
        public string BundleName;
        
        /// <summary>
        /// 资源包哈希值
        /// </summary>
        public string Hash;
        
        /// <summary>
        /// 资源包大小（字节数）
        /// </summary>
        public long BundleSize;
        
        /// <summary>
        /// 压缩类型
        /// </summary>
        public AssetBundleCompressType CompressType;
        
        /// <summary>
        /// 资源包对象
        /// </summary>
        public QuarkBundle QuarkAssetBundle;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public QuarkBundleAsset()
        {
            QuarkAssetBundle = new QuarkBundle();
        }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">资源包名称</param>
        /// <param name="hash">资源包哈希值</param>
        /// <param name="size">资源包大小</param>
        /// <param name="compressType">压缩类型</param>
        /// <param name="bundle">资源包对象</param>
        public QuarkBundleAsset(string name, string hash, long size, AssetBundleCompressType compressType, QuarkBundle bundle)
        {
            BundleName = name;
            Hash = hash;
            BundleSize = size;
            CompressType = compressType;
            QuarkAssetBundle = bundle;
        }
        
        /// <summary>
        /// 相等比较
        /// </summary>
        /// <param name="other">比较对象</param>
        /// <returns>是否相等</returns>
        public bool Equals(QuarkBundleAsset other)
        {
            if (other == null)
                return false;
                
            return BundleName == other.BundleName && 
                   Hash == other.Hash && 
                   BundleSize == other.BundleSize && 
                   CompressType == other.CompressType;
        }
        
        /// <summary>
        /// 相等比较
        /// </summary>
        /// <param name="obj">比较对象</param>
        /// <returns>是否相等</returns>
        public override bool Equals(object obj)
        {
            if (obj is QuarkBundleAsset other)
                return Equals(other);
            return false;
        }
        
        /// <summary>
        /// 获取哈希码
        /// </summary>
        /// <returns>哈希码</returns>
        public override int GetHashCode()
        {
            var hashCode = 17;
            hashCode = hashCode * 31 + (BundleName?.GetHashCode() ?? 0);
            hashCode = hashCode * 31 + (Hash?.GetHashCode() ?? 0);
            hashCode = hashCode * 31 + BundleSize.GetHashCode();
            hashCode = hashCode * 31 + CompressType.GetHashCode();
            return hashCode;
        }
        
        /// <summary>
        /// 转为字符串
        /// </summary>
        /// <returns>字符串表示</returns>
        public override string ToString()
        {
            return $"BundleAsset[{BundleName}, Size: {QuarkUtility.FormatBytes(BundleSize)}, Hash: {Hash}]";
        }
    }
    
    /// <summary>
    /// 资源包压缩类型
    /// </summary>
    public enum AssetBundleCompressType
    {
        /// <summary>
        /// 不压缩
        /// </summary>
        None = 0,
        
        /// <summary>
        /// LZMA压缩
        /// </summary>
        LZMA = 1,
        
        /// <summary>
        /// LZ4压缩
        /// </summary>
        LZ4 = 2,
        
        /// <summary>
        /// LZ4HC压缩
        /// </summary>
        LZ4HC = 3,
    }
    
    /// <summary>
    /// 资源包名称类型
    /// </summary>
    public enum AssetBundleNameType
    {
        /// <summary>
        /// 使用文件夹名称
        /// </summary>
        FolderName = 0,
        
        /// <summary>
        /// 使用文件名称
        /// </summary>
        FileName = 1,
        
        /// <summary>
        /// 使用文件路径
        /// </summary>
        FilePath = 2,
    }
    
    /// <summary>
    /// Quark构建类型
    /// </summary>
    public enum QuarkBuildType
    {
        /// <summary>
        /// 全量构建
        /// </summary>
        Full = 0,
        
        /// <summary>
        /// 增量构建
        /// </summary>
        Incremental = 1,
    }
}
