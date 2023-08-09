using System;

namespace Quark.Editor
{
    [Serializable]
    public class AssetCache : IEquatable<AssetCache>
    {
        public string BundleName;
        public string BundlePath;
        public string BundleHash;
        //public long BundleSize;
        public string[] AssetNames;

        public bool Equals(AssetCache other)
        {
            return BundleName == other.BundleName &&
                BundlePath == other.BundlePath &&
                BundleHash == other.BundleHash;
        }
    }
}
