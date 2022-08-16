using System.Runtime.InteropServices;
using System;
namespace Quark.Editor
{
    [StructLayout(LayoutKind.Auto)]
    public struct QuarkObjectItem : IEquatable<QuarkObjectItem>
    {
        public string AssetName { get; private set; }
        public string AssetExtension { get; private set; }
        public string AssetPath { get; private set; }
        public string AssetBundleName { get; private set; }
        public QuarkObjectItem(string assetName, string assetExtension, string assetPath, string assetBundleName)
        {
            AssetName = assetName;
            AssetExtension = assetExtension;
            AssetPath = assetPath;
            AssetBundleName = assetBundleName;
        }
        public bool Equals(QuarkObjectItem other)
        {
            return other.AssetName == this.AssetName &&
                other.AssetExtension == this.AssetExtension &&
                other.AssetPath == this.AssetPath &&
                other.AssetBundleName == this.AssetBundleName;
        }
    }
}
