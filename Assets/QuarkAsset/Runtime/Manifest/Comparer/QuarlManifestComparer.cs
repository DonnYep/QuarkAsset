using Quark.Asset;
using System.Collections.Generic;

namespace Quark.Manifest
{
    public class QuarlManifestComparer
    {
        internal QuarlManifestComparer() { }
        public void CompareManifest(QuarkManifest sourceManifest, QuarkManifest comparisonManifest, out QuarkManifestCompareResult result)
        {
            //time complexity O=n
            result = new QuarkManifestCompareResult();
            List<QuarkManifestCompareInfo> expired = new List<QuarkManifestCompareInfo>();
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
                    var expiredInfo = new QuarkManifestCompareInfo(srcBundleInfo.QuarkAssetBundle.BundleName, srcBundleInfo.QuarkAssetBundle.BundleKey, srcBundleInfo.Hash, srcBundleInfo.BundleSize);
                    expired.Add(expiredInfo);
                }
                else
                {
                    //如果comparison中存在，则比较Hash
                    if (srcBundleInfo.Hash != cmpBundleInfo.Hash)
                    {
                        //Hash不一致，表示需要更新；
                        var changedInfo = new QuarkManifestCompareInfo(cmpBundleInfo.QuarkAssetBundle.BundleName, cmpBundleInfo.QuarkAssetBundle.BundleKey, cmpBundleInfo.Hash, cmpBundleInfo.BundleSize);
                        changed.Add(changedInfo);
                    }
                    else
                    {
                        //Hash一致，无需更新；
                        var unchangedInfo=new QuarkManifestCompareInfo(srcBundleInfo.QuarkAssetBundle.BundleName, srcBundleInfo.QuarkAssetBundle.BundleKey, srcBundleInfo.Hash, srcBundleInfo.BundleSize);
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
                    var newlyAddedInfo = new QuarkManifestCompareInfo(cmpBundleInfo.QuarkAssetBundle.BundleName, cmpBundleInfo.QuarkAssetBundle.BundleKey, cmpBundleInfo.Hash, cmpBundleInfo.BundleSize);
                    newlyAdded.Add(newlyAddedInfo);
                }
            }
            result.ChangedInfos = changed.ToArray();
            result.NewlyAddedInfos = newlyAdded.ToArray();
            result.ExpiredInfos = expired.ToArray();
            result.UnchangedInfos = unchanged.ToArray();
        }
    }
}
