using Quark.Asset;
using Quark.Manifest;
using System.Collections.Generic;
using System.IO;

namespace Quark
{
    public partial class QuarkUtility
    {
        public class Intergrity
        {
            public static void MonitoringIntegrity(QuarkManifest manifest, string path, out QuarkFileIntergrityResult result)
            {
                result = new QuarkFileIntergrityResult();
                result.IntergrityInfos = new QuarkFileIntergrityInfo[manifest.BundleInfoDict.Count];
                int index = 0;
                foreach (var bundleInfo in manifest.BundleInfoDict.Values)
                {
                    var bundleKey = bundleInfo.QuarkAssetBundle.BundleKey;
                    var bundleName = bundleInfo.QuarkAssetBundle.BundleName;
                    var filePath = Path.Combine(path, bundleKey);
                    long fileLength = 0;
                    if (File.Exists(filePath))
                    {
                        var fileInfo = new FileInfo(filePath);
                        fileLength = fileInfo.Length;
                    }
                    var intergrityInfo = new QuarkFileIntergrityInfo(fileLength, bundleInfo.BundleSize, bundleKey, bundleName);
                    result.IntergrityInfos[index] = intergrityInfo;
                    index++;
                }
            }
            public static void MonitoringIntegrity(IList<QuarkBundleAsset> bundles, string path, out QuarkFileIntergrityResult result)
            {
                result = new QuarkFileIntergrityResult();
                result.IntergrityInfos = new QuarkFileIntergrityInfo[bundles.Count];
                int index = 0;
                foreach (var bundleInfo in bundles)
                {
                    var bundleKey = bundleInfo.QuarkAssetBundle.BundleKey;
                    var bundleName = bundleInfo.QuarkAssetBundle.BundleName;
                    var filePath = Path.Combine(path, bundleKey);
                    long fileLength = 0;
                    if (File.Exists(filePath))
                    {
                        var fileInfo = new FileInfo(filePath);
                        fileLength = fileInfo.Length;
                    }
                    var intergrityInfo = new QuarkFileIntergrityInfo(fileLength, bundleInfo.BundleSize, bundleKey, bundleName);
                    result.IntergrityInfos[index] = intergrityInfo;
                    index++;
                }
            }
            public static void MonitoringIntegrity(QuarkMergedManifest mergedManifest, string path, out QuarkFileIntergrityResult result)
            {
                result = new QuarkFileIntergrityResult();
                List<QuarkFileIntergrityInfo> intergrityInfoList = new List<QuarkFileIntergrityInfo>();
                foreach (var mergedBundle in mergedManifest.MergedBundles)
                {
                    if (!mergedBundle.IsIncremental)
                    {
                        continue;
                    }
                    var bundleKey = mergedBundle.QuarkBundleAsset.QuarkAssetBundle.BundleKey;
                    var bundleName = mergedBundle.QuarkBundleAsset.QuarkAssetBundle.BundleName;
                    var filePath = Path.Combine(path, bundleKey);
                    long fileLength = 0;
                    if (File.Exists(filePath))
                    {
                        var fileInfo = new FileInfo(filePath);
                        fileLength = fileInfo.Length;
                    }
                    var intergrityInfo = new QuarkFileIntergrityInfo(fileLength, mergedBundle.QuarkBundleAsset.BundleSize, bundleKey, bundleName);
                    intergrityInfoList.Add(intergrityInfo);
                }
                result.IntergrityInfos = intergrityInfoList.ToArray();
            }
        }
    }
}