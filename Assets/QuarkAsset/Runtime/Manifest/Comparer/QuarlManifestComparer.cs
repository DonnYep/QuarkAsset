using Quark.Asset;
using System.Collections.Generic;

namespace Quark.Compare
{
    public class QuarlManifestComparer
    {
        internal QuarlManifestComparer() { }

        public void CompareManifest(QuarkManifest sourceManifest, QuarkManifest comparisonManifest, out QuarkManifestCompareResult result)
        {
            result = new QuarkManifestCompareResult();
            List<QuarkManifestCompareInfo> expired = new List<QuarkManifestCompareInfo>();
            List<QuarkManifestCompareInfo> changed = new List<QuarkManifestCompareInfo>();
            List<QuarkManifestCompareInfo> newlyAdded = new List<QuarkManifestCompareInfo>();
            List<QuarkManifestCompareInfo> unchanged = new List<QuarkManifestCompareInfo>();
            //这里使用src的文件清单遍历comparison的文件清单;
            foreach (var srcBundleInfoKeyValue in sourceManifest.BundleInfoDict)
            {
                var srcBundleInfo = srcBundleInfoKeyValue.Value;
                var info = new QuarkManifestCompareInfo(srcBundleInfo.QuarkAssetBundle.BundleName, srcBundleInfo.QuarkAssetBundle.BundleKey, srcBundleInfo.BundleSize, srcBundleInfo.Hash);

                if (!comparisonManifest.BundleInfoDict.TryGetValue(srcBundleInfoKeyValue.Key, out var cmpBundleInfo))
                {
                    //如果comparison中不存在，表示资源已经过期，加入到移除的列表中；
                    expired.Add(info);
                }
                else
                {
                    //如果comparison中存在，则比较Hash
                    if (srcBundleInfo.Hash != cmpBundleInfo.Hash)
                    {
                        //Hash不一致，表示需要更新；
                        changed.Add(info);
                    }
                    else
                    {
                        //Hash一致，无需更新；
                        unchanged.Add(info);
                    }
                }
            }
            foreach (var cmpBundleInfoKeyValue in comparisonManifest.BundleInfoDict)
            {
                var cmpBundleInfo = cmpBundleInfoKeyValue.Value;
                if (!sourceManifest.BundleInfoDict.ContainsKey(cmpBundleInfoKeyValue.Key))
                {
                    //source中不存在，表示为新增资源；
                    newlyAdded.Add(new QuarkManifestCompareInfo(cmpBundleInfo.QuarkAssetBundle.BundleName, cmpBundleInfo.QuarkAssetBundle.BundleKey, cmpBundleInfo.BundleSize, cmpBundleInfo.Hash));
                }
            }
            result.ChangedInfos = changed.ToArray();
            result.NewlyAddedInfos = newlyAdded.ToArray();
            result.ExpiredInfos = expired.ToArray();
            result.UnchangedInfos = unchanged.ToArray();
        }
    }
}
