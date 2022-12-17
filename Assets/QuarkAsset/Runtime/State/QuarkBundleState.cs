using System;
namespace Quark
{
    /// <summary>
    /// 状态信息；
    /// </summary>
    public struct QuarkBundleState : IEquatable<QuarkBundleState>
    {
        /// <summary>
        /// AB包的名称；
        /// </summary>
        public string AssetBundleName { get; private set; }
        /// <summary>
        /// 包体对应的引用计数；
        /// </summary>
        public int ReferenceCount { get; private set; }
        public bool Equals(QuarkBundleState other)
        {
            return other.AssetBundleName==this.AssetBundleName&&
                other.ReferenceCount==this.ReferenceCount;
        }
        public override string ToString()
        {
            return $"AssetBundleName:{AssetBundleName},ReferenceCount:{ReferenceCount}";
        }
        internal static QuarkBundleState Create(string assetBundleName,int referenceCount)
        {
            QuarkBundleState info = new QuarkBundleState();
            info.AssetBundleName = assetBundleName;
            info.ReferenceCount = referenceCount;
            return info;
        }
    }
}
