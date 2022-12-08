using Quark.Asset;
using System.Collections.Generic;
using System.IO;

namespace Quark.Verifiy
{
    public class QuarkManifestVerifier
    {
        /// <summary>
        /// 比较manifest；
        /// </summary>
        /// <param name="source">原始文件</param>
        /// <param name="comparer">用于比较的文件</param>
        /// <param name="latestInfo">最新的信息</param>
        /// <param name="expiredInfo">过期的信息</param>
        public void VerificateManifest(QuarkAssetManifest source, QuarkAssetManifest comparer, out QuarkBundleVerifiyInfo[] latestInfo, out QuarkBundleVerifiyInfo[] expiredInfo)
        {

            latestInfo = new QuarkBundleVerifiyInfo[0];
            expiredInfo = new QuarkBundleVerifiyInfo[0];
            //名字相同，但是HASH不同，则认为资源有作修改，需要加入到最新队列中；
            List<QuarkBundleVerifiyInfo> latest = new List<QuarkBundleVerifiyInfo>();
            //本地有但是远程没有，则标记为可过期文件；
            List<QuarkBundleVerifiyInfo> expired = new List<QuarkBundleVerifiyInfo>();

            long totalSize = 0;

            {
                //远端有本地没有，则缓存至latest；
                //远端没有本地有，则缓存至expired；
                foreach (var cmpBuildInfo in comparer.BundleInfoDict.Values)
                {
                    var cmpBundleName = cmpBuildInfo.BundleName;
                    var cmpBundleKey = cmpBuildInfo.QuarkAssetBundle.AssetBundleKey;
                    var cmpBundleSize = cmpBuildInfo.BundleSize;
                    var cmpBundleHash = cmpBuildInfo.Hash;
                    if (source.BundleInfoDict.TryGetValue(cmpBundleName, out var srcBuildInfo))
                    {
                        if (srcBuildInfo.Hash != cmpBuildInfo.Hash)
                        {
                            var verifiyInfo = new QuarkBundleVerifiyInfo()
                            {
                                BundleHash = cmpBundleHash,
                                BundleKey = cmpBundleKey,
                                BundleName = cmpBundleName,
                                BundleSize = cmpBundleSize
                            };
                            totalSize += cmpBundleSize;
                            latest.Add(verifiyInfo);
                            expired.Add(verifiyInfo);
                        }
                        else
                        {
                            //检测远端包体与本地包体的大小是否相同。
                            //在Hash相同的情况下，若包体不同，则可能是本地的包不完整，因此需要重新加入下载队列。
                            var srcBundleKey = srcBuildInfo.QuarkAssetBundle.AssetBundleKey;
                            var srcBundlePath = Path.Combine(QuarkDataProxy.PersistentPath, srcBundleKey);
                            var srcLocalBundleSize = QuarkUtility.GetFileSize(srcBundlePath);
                            if (cmpBundleSize != srcLocalBundleSize)
                            {
                                var remainBundleSize = cmpBundleSize - srcLocalBundleSize;
                                totalSize += remainBundleSize;

                                var latestVerifiyInfo = new QuarkBundleVerifiyInfo()
                                {
                                    BundleHash = cmpBundleHash,
                                    BundleKey = cmpBundleKey,
                                    BundleName = cmpBundleName,
                                    BundleSize = cmpBundleSize
                                };
                                latest.Add(latestVerifiyInfo);
                                if (cmpBundleSize < srcLocalBundleSize)//若本地包体大于远端包体，则表示为本地包为过期包
                                {
                                    var expiredVerifiyInfo = new QuarkBundleVerifiyInfo()
                                    {
                                        BundleHash = srcBuildInfo.Hash,
                                        BundleKey = srcBundleKey,
                                        BundleName = srcBuildInfo.BundleName,
                                        BundleSize = srcBuildInfo.BundleSize
                                    };
                                    expired.Add(expiredVerifiyInfo);
                                }
                            }
                        }
                    }
                    else
                    {
                        totalSize += cmpBundleSize;
                        var verifiyInfo = new QuarkBundleVerifiyInfo()
                        {
                            BundleHash = cmpBundleHash,
                            BundleKey = cmpBundleKey,
                            BundleName = cmpBundleName,
                            BundleSize = cmpBundleSize
                        };
                        latest.Add(verifiyInfo);
                    }
                    foreach (var srcInfo in source.BundleInfoDict.Values)
                    {
                        var bundleKey = srcInfo.QuarkAssetBundle.AssetBundleKey;
                        if (!comparer.BundleInfoDict.ContainsKey(bundleKey))
                        {
                            var expiredVerifiyInfo = new QuarkBundleVerifiyInfo()
                            {
                                BundleHash = srcInfo.Hash,
                                BundleKey = srcInfo.QuarkAssetBundle.AssetBundleKey,
                                BundleName = srcInfo.BundleName,
                                BundleSize = srcInfo.BundleSize
                            };
                            expired.Add(expiredVerifiyInfo);
                        }
                    }
                }
            }
            latestInfo = latest.ToArray();
            expiredInfo = expired.ToArray();
        }
        /// <summary>
        /// 校验本地文件完整性；
        /// </summary>
        /// <param name="manifest">文件清单</param>
        /// <param name="invalidInfos">无效的包信息</param>
        public void VerificateManifest(QuarkAssetManifest manifest, out QuarkBundleVerifiyInfo[] invalidInfos)
        {
            invalidInfos = new QuarkBundleVerifiyInfo[0];
            var invalids = new List<QuarkBundleVerifiyInfo>();
            long totalSize = 0;
            foreach (var buildInfo in manifest.BundleInfoDict.Values)
            {
                //检测远端包体与本地包体的大小是否相同。
                //在Hash相同的情况下，若包体不同，则可能是本地的包不完整，因此需要重新加入下载队列。
                var bundleKey = buildInfo.QuarkAssetBundle.AssetBundleKey;
                var bundlePath = Path.Combine(QuarkDataProxy.PersistentPath, bundleKey);
                var bundleSize = buildInfo.BundleSize;
                var localBundleSize = QuarkUtility.GetFileSize(bundlePath);
                if (localBundleSize != bundleSize)
                {
                    var remainBundleSize = bundleSize - localBundleSize;
                    totalSize += remainBundleSize;
                    var verifiyInfo = new QuarkBundleVerifiyInfo()
                    {
                        BundleHash = buildInfo.Hash,
                        BundleKey = bundleKey,
                        BundleName = buildInfo.BundleName,
                        BundleSize = bundleSize
                    };
                    invalids.Add(verifiyInfo);
                }
            }
            invalidInfos = invalids.ToArray();
        }
    }
}
