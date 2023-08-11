using Quark.Asset;
using System.Collections.Generic;
using System.Linq;

namespace Quark.Manifest
{
    public class QuarlManifestComparer
    {
        internal QuarlManifestComparer() { }
        public void CompareManifest(QuarkManifest sourceManifest, QuarkManifest comparisonManifest, out QuarkManifestCompareResult result)
        {
            result = new QuarkManifestCompareResult();
            List<QuarkManifestCompareInfo> deleted = new List<QuarkManifestCompareInfo>();
            List<QuarkManifestCompareInfo> changed = new List<QuarkManifestCompareInfo>();
            List<QuarkManifestCompareInfo> newlyAdded = new List<QuarkManifestCompareInfo>();
            List<QuarkManifestCompareInfo> unchanged = new List<QuarkManifestCompareInfo>();
            //这里使用src的文件清单遍历comparison的文件清单;
            foreach (var srcBundleInfoKeyValue in sourceManifest.BundleInfoDict)
            {
                var srcBundleInfo = srcBundleInfoKeyValue.Value;
                if (!comparisonManifest.BundleInfoDict.TryGetValue(srcBundleInfoKeyValue.Key, out var cmpBundleInfo))
                {
                    //如果comparison中不存在，表示资源已经过期，加入到移除的列表中；
                    var formatSize = QuarkUtility.FormatBytes(srcBundleInfo.BundleSize);
                    var deletedInfo = new QuarkManifestCompareInfo(srcBundleInfo.QuarkAssetBundle.BundleName, srcBundleInfo.QuarkAssetBundle.BundleKey, srcBundleInfo.Hash, srcBundleInfo.BundleSize, formatSize, QuarkBundleChangeType.Deleted);
                    deletedInfo.BundlePath = srcBundleInfo.QuarkAssetBundle.BundlePath;
                    deleted.Add(deletedInfo);
                }
                else
                {
                    //如果comparison中存在，则比较Hash
                    if (srcBundleInfo.Hash != cmpBundleInfo.Hash)
                    {
                        //Hash不一致，表示需要更新；
                        var formatSize = QuarkUtility.FormatBytes(srcBundleInfo.BundleSize);
                        var changedInfo = new QuarkManifestCompareInfo(cmpBundleInfo.QuarkAssetBundle.BundleName, cmpBundleInfo.QuarkAssetBundle.BundleKey, cmpBundleInfo.Hash, cmpBundleInfo.BundleSize, formatSize, QuarkBundleChangeType.Changed);
                        changedInfo.BundlePath = srcBundleInfo.QuarkAssetBundle.BundlePath;
                        changed.Add(changedInfo);
                    }
                    else
                    {
                        //Hash一致，无需更新；
                        var formatSize = QuarkUtility.FormatBytes(srcBundleInfo.BundleSize);
                        var unchangedInfo = new QuarkManifestCompareInfo(srcBundleInfo.QuarkAssetBundle.BundleName, srcBundleInfo.QuarkAssetBundle.BundleKey, srcBundleInfo.Hash, srcBundleInfo.BundleSize, formatSize, QuarkBundleChangeType.Unchanged);
                        unchangedInfo.BundlePath = srcBundleInfo.QuarkAssetBundle.BundlePath;
                        unchanged.Add(unchangedInfo);
                    }
                }
            }
            foreach (var cmpBundleInfoKeyValue in comparisonManifest.BundleInfoDict)
            {
                var cmpBundleInfo = cmpBundleInfoKeyValue.Value;
                if (!sourceManifest.BundleInfoDict.ContainsKey(cmpBundleInfoKeyValue.Key))
                {
                    //source中不存在，表示为新增资源；
                    var formatSize = QuarkUtility.FormatBytes(cmpBundleInfo.BundleSize);
                    var newlyAddedInfo = new QuarkManifestCompareInfo(cmpBundleInfo.QuarkAssetBundle.BundleName, cmpBundleInfo.QuarkAssetBundle.BundleKey, cmpBundleInfo.Hash, cmpBundleInfo.BundleSize, formatSize, QuarkBundleChangeType.NewlyAdded);
                    newlyAddedInfo.BundlePath = cmpBundleInfo.QuarkAssetBundle.BundlePath;
                    newlyAdded.Add(newlyAddedInfo);
                }
            }
            result.ChangedInfos = changed.ToArray();
            result.NewlyAddedInfos = newlyAdded.ToArray();
            result.DeletedInfos = deleted.ToArray();
            result.UnchangedInfos = unchanged.ToArray();
        }
        public void CompareManifestByBundleName(QuarkManifest sourceManifest, QuarkManifest comparisonManifest, out QuarkManifestCompareResult result)
        {
            result = new QuarkManifestCompareResult();
            List<QuarkManifestCompareInfo> deleted = new List<QuarkManifestCompareInfo>();
            List<QuarkManifestCompareInfo> changed = new List<QuarkManifestCompareInfo>();
            List<QuarkManifestCompareInfo> newlyAdded = new List<QuarkManifestCompareInfo>();
            List<QuarkManifestCompareInfo> unchanged = new List<QuarkManifestCompareInfo>();
            var srcDict = sourceManifest.BundleInfoDict.Values.ToDictionary(b => b.BundleName);
            var cmpDict = comparisonManifest.BundleInfoDict.Values.ToDictionary(b => b.BundleName);
            //这里使用src的文件清单遍历comparison的文件清单;
            foreach (var srcBundleInfoKeyValue in srcDict)
            {
                var srcBundleInfo = srcBundleInfoKeyValue.Value;
                if (!cmpDict.TryGetValue(srcBundleInfoKeyValue.Key, out var cmpBundleInfo))
                {
                    //如果comparison中不存在，表示资源已经过期，加入到移除的列表中；
                    var formatSize = QuarkUtility.FormatBytes(srcBundleInfo.BundleSize);
                    var deletedInfo = new QuarkManifestCompareInfo(srcBundleInfo.QuarkAssetBundle.BundleName, srcBundleInfo.QuarkAssetBundle.BundleKey, srcBundleInfo.Hash, srcBundleInfo.BundleSize, formatSize, QuarkBundleChangeType.Deleted);
                    deletedInfo.BundlePath = srcBundleInfo.QuarkAssetBundle.BundlePath;
                    deleted.Add(deletedInfo);
                }
                else
                {
                    //如果comparison中存在，则比较Hash
                    if (srcBundleInfo.Hash != cmpBundleInfo.Hash)
                    {
                        //Hash不一致，表示需要更新；
                        var formatSize = QuarkUtility.FormatBytes(srcBundleInfo.BundleSize);
                        var changedInfo = new QuarkManifestCompareInfo(cmpBundleInfo.QuarkAssetBundle.BundleName, cmpBundleInfo.QuarkAssetBundle.BundleKey, cmpBundleInfo.Hash, cmpBundleInfo.BundleSize, formatSize, QuarkBundleChangeType.Changed);
                        changedInfo.BundlePath = srcBundleInfo.QuarkAssetBundle.BundlePath;
                        changed.Add(changedInfo);
                    }
                    else
                    {
                        //Hash一致，无需更新；
                        var formatSize = QuarkUtility.FormatBytes(srcBundleInfo.BundleSize);
                        var unchangedInfo = new QuarkManifestCompareInfo(srcBundleInfo.QuarkAssetBundle.BundleName, srcBundleInfo.QuarkAssetBundle.BundleKey, srcBundleInfo.Hash, srcBundleInfo.BundleSize, formatSize, QuarkBundleChangeType.Unchanged);
                        unchangedInfo.BundlePath = srcBundleInfo.QuarkAssetBundle.BundlePath;
                        unchanged.Add(unchangedInfo);
                    }
                }
            }
            foreach (var cmpBundleInfoKeyValue in cmpDict)
            {
                var cmpBundleInfo = cmpBundleInfoKeyValue.Value;
                if (!srcDict.ContainsKey(cmpBundleInfoKeyValue.Key))
                {
                    //source中不存在，表示为新增资源；
                    var formatSize = QuarkUtility.FormatBytes(cmpBundleInfo.BundleSize);
                    var newlyAddedInfo = new QuarkManifestCompareInfo(cmpBundleInfo.QuarkAssetBundle.BundleName, cmpBundleInfo.QuarkAssetBundle.BundleKey, cmpBundleInfo.Hash, cmpBundleInfo.BundleSize, formatSize, QuarkBundleChangeType.NewlyAdded);
                    newlyAddedInfo.BundlePath = cmpBundleInfo.QuarkAssetBundle.BundlePath;
                    newlyAdded.Add(newlyAddedInfo);
                }
            }
            result.ChangedInfos = changed.ToArray();
            result.NewlyAddedInfos = newlyAdded.ToArray();
            result.DeletedInfos = deleted.ToArray();
            result.UnchangedInfos = unchanged.ToArray();
        }
    }
}
