using System;
using System.Runtime.InteropServices;
namespace Quark.Editor
{
    [StructLayout(LayoutKind.Auto)]
    public class QuarkAssetBundleItem : IEquatable<QuarkAssetBundleItem>
    {
        public long AssetBundleSize { get; private set; }
        public int ObjectCount { get; private set; }
        public string AssetBundleName { get; private set; }
        public string AssetBundlePath{ get; private set; }
        public QuarkAssetBundleItem(long assetBundleSize, int objectCount, string assetBundleName,string assetBundlePath)
        {
            AssetBundleSize = assetBundleSize;
            ObjectCount = objectCount;
            AssetBundleName = assetBundleName;
            AssetBundlePath = assetBundlePath;
        }
        public bool Equals(QuarkAssetBundleItem other)
        {
            return other.AssetBundleSize == this.AssetBundleSize &&
                other.ObjectCount == this.ObjectCount &&
                other.AssetBundleName == this.AssetBundleName&&
                other.AssetBundlePath==this.AssetBundlePath;
        }
    }
}
