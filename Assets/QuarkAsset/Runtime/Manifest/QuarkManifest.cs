using System;
using System.Collections.Generic;

namespace Quark.Asset
{
    [Serializable]
    public class QuarkManifest : IQuarkLoaderData
    {
        Dictionary<string, QuarkBundleAsset> bundleDict;
        public string BuildTime { get; set; }
        public string BuildVersion { get; set; }
        public int InternalBuildVersion { get; set; }
        public string BuildHash { get; set; }
        public Dictionary<string, QuarkBundleAsset> BundleInfoDict
        {
            get
            {
                if (bundleDict == null)
                    bundleDict = new Dictionary<string, QuarkBundleAsset>();
                return bundleDict;
            }
            set { bundleDict = value; }
        }
    }
}
