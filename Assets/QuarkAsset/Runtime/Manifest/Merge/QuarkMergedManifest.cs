using System.Collections.Generic;

namespace Quark.Asset
{
    public class QuarkMergedManifest
    {
        public string BuildTime { get; set; }
        public string BuildVersion { get; set; }
        public int InternalBuildVersion { get; set; }
        public string BuildHash { get; set; }
        public List<QuarkMergedBundleAsset> MergedBundles{ get; set; }
    }
}
