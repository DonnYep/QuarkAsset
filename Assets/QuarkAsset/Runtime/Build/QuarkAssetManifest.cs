using System;
using System.Collections.Generic;

namespace Quark.Asset
{
    [Serializable]
    public class QuarkAssetManifest : IQuarkLoaderData
    {
        [Serializable]
        public class QuarkBundleInfo
        {
            public string Hash;
            public string BundleName;
            public long BundleSize;
            public QuarkAssetBundle QuarkAssetBundle;
        }
        Dictionary<string, QuarkBundleInfo> bundleDict;
        public string BuildTime { get; set; }
        public string BuildVersion { get; set; }
        public Dictionary<string, QuarkBundleInfo> BundleInfoDict
        {
            get
            {
                if (bundleDict == null)
                    bundleDict = new Dictionary<string, QuarkBundleInfo>();
                return bundleDict;
            }
            set { bundleDict = value; }
        }
    }
}
