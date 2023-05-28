using System.Collections.Generic;

namespace Quark.Asset
{
    internal class QuarkMergedManifest
    {
        public string BuildTime { get; set; }
        public string BuildVersion { get; set; }
        public int InternalBuildVersion { get; set; }
        public List<QuarkMergedBundleAsset> MergedBundles{ get; set; }
    }
}
