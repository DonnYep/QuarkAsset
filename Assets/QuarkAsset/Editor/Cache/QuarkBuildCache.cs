using System.Collections.Generic;
using System;
namespace Quark.Editor
{
    [Serializable]
    public class QuarkBuildCache
    {
        public string BuildVerison;
        public int InternalBuildVerison;
        public List<AssetCache> BundleCacheList;
    }
}
