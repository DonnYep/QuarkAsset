using System.Collections.Generic;
using System.Linq;

namespace Quark.Asset
{
    public class QuarkDiffManifestMerger
    {
        public static void MergeDiffManifest(QuarkManifest manifest, QuarkDiffManifest diffManifest, out QuarkMergedManifest mergeResult)
        {
            mergeResult = new QuarkMergedManifest();
            var srcBundleInfoDict = manifest.BundleInfoDict.Values.ToDictionary(b => b.QuarkAssetBundle.BundlePath);
            var diffBundleInfoDict = diffManifest.BundleInfoDict.Values.ToDictionary(b => b.QuarkAssetBundle.BundlePath);
            List<QuarkMergedBundleAsset> mergedBundleList = new List<QuarkMergedBundleAsset>();
            foreach (var srcBundle in srcBundleInfoDict.Values)
            {
                var srcBundlePath = srcBundle.QuarkAssetBundle.BundlePath;
                if (diffBundleInfoDict.TryGetValue(srcBundlePath, out var diffBundle))
                {
                    //在diffmanifest中存在，表示此资源需要更新，使用diff的信息
                    var mergeInfo = new QuarkMergedBundleAsset
                    {
                        IsIncremental = true,
                        QuarkBundleAsset = diffBundle
                    };
                    mergedBundleList.Add(mergeInfo);
                }
                else
                {
                    //在diffmanifest中不存在，无需更新资源，使用src的信息
                    var mergeInfo = new QuarkMergedBundleAsset
                    {
                        IsIncremental = false,
                        QuarkBundleAsset = srcBundle
                    };
                    mergedBundleList.Add(mergeInfo);
                }
            }
            mergeResult.BuildTime = diffManifest.BuildTime;
            mergeResult.BuildVersion = diffManifest.BuildVersion;
            mergeResult.InternalBuildVersion = diffManifest.InternalBuildVersion;
            mergeResult.MergedBundles= mergedBundleList;

        }
    }
}
