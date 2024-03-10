using System;
using System.Runtime.InteropServices;
namespace Quark
{
    /// <summary>
    /// 状态信息
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
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
        /// <summary>
        /// 资源数量
        /// </summary>
        public int ObjectCount { get; private set; }
        public readonly static QuarkBundleState None = new QuarkBundleState();
        public bool Equals(QuarkBundleState other)
        {
            return other.AssetBundleName == this.AssetBundleName &&
                other.ReferenceCount == this.ReferenceCount &&
                other.ObjectCount == this.ObjectCount;
        }
        public override string ToString()
        {
            return $"AssetBundleName:{AssetBundleName},ReferenceCount:{ReferenceCount},ObjectCount:{ObjectCount}";
        }
        internal static QuarkBundleState Create(string assetBundleName, int referenceCount, int objectCount)
        {
            QuarkBundleState info = new QuarkBundleState();
            info.AssetBundleName = assetBundleName;
            info.ReferenceCount = referenceCount;
            info.ObjectCount = objectCount;
            return info;
        }
    }
}
