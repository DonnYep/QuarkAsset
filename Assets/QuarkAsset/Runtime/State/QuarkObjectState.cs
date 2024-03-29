﻿using System;
using System.Runtime.InteropServices;
namespace Quark
{
    /// <summary>
    /// 资源体的状态信息
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public struct QuarkObjectState : IEquatable<QuarkObjectState>
    {
        /// <summary>
        /// 资源名称；
        /// </summary>
        public string AssetName { get; private set; }
        /// <summary>
        /// 资源的相对路径；
        /// </summary>
        public string AssetPath { get; private set; }
        /// <summary>
        /// 资源所属AB包名；
        /// </summary>
        public string AssetBundleName { get; private set; }
        /// <summary>
        /// 资源在unity中的类型；
        /// </summary>
        public string AssetType { get; private set; }
        /// <summary>
        /// 引用计数；
        /// </summary>
        public int ReferenceCount { get; private set; }
        /// <summary>
        /// 源文件的后缀名；
        /// </summary>
        public string AssetExtension { get; private set; }
        public QuarkObjectState Clone()
        {
            return Create(this.AssetName, this.AssetPath, this.AssetBundleName, this.AssetExtension, this.AssetType, this.ReferenceCount);
        }
        public bool Equals(QuarkObjectState other)
        {
            return other.AssetName == this.AssetName &&
                other.AssetPath == this.AssetPath &&
                other.AssetBundleName == this.AssetBundleName &&
                other.AssetExtension == this.AssetExtension &&
                other.ReferenceCount == this.ReferenceCount &&
                other.AssetType == this.AssetType;
        }
        public override string ToString()
        {
            return $"AssetName:{AssetName},AssetPath:{AssetPath},AssetType{AssetType}" +
                $",AssetBundleName:{AssetBundleName},ReferenceCount:{ReferenceCount}";
        }
        public static QuarkObjectState None { get { return new QuarkObjectState(); } }
        internal static QuarkObjectState Create(string assetName, string assetPath, string assetBundleName, string assetExtension, string assetType, int referenceCount)
        {
            QuarkObjectState info = new QuarkObjectState();
            info.AssetName = assetName;
            info.AssetPath = assetPath;
            info.AssetBundleName = assetBundleName;
            info.ReferenceCount = referenceCount;
            info.AssetExtension = assetExtension;
            info.AssetType = assetType;
            return info;
        }
    }
}
