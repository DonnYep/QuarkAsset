using System.Collections.Generic;
using System;
namespace Quark.Editor
{
    [Serializable]
    public class QuarkBuildCache
    {
        public string BuildVerison;
        public int InternalBuildVerison;
        public AssetBundleNameType NameType;
        public List<AssetCache> BundleCacheList;
    }
}
