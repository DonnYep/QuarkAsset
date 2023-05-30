using Quark.Asset;
using System.Collections.Generic;
using System.Linq;

namespace Quark
{
    public partial class QuarkUtility
    {
        public class Manifest
        {
            public static string Serialize(QuarkManifest manifest, string aesKey)
            {
                var aesKeyBytes = QuarkUtility.GenerateBytesAESKey(aesKey);
                return Serialize(manifest, aesKeyBytes);
            }
            public static string Serialize(QuarkManifest manifest, byte[] aesKeyBytes)
            {
                var manifestJson = QuarkUtility.ToJson(manifest);
                var encrypt = aesKeyBytes != null && aesKeyBytes.Length > 0;
                string serializedContext = string.Empty;
                if (encrypt)
                    serializedContext = QuarkUtility.AESEncryptStringToString(manifestJson, aesKeyBytes);
                else
                    serializedContext = manifestJson;
                return serializedContext;
            }
            /// <summary>
            /// 使用配置的aesKey反序列化manifest；
            /// </summary>
            /// <param name="manifestContext">读取到的文本内容</param>
            /// <returns>反序列化后的内容</returns>
            public static QuarkManifest Deserialize(string manifestContext)
            {
                return Deserialize(manifestContext, QuarkDataProxy.QuarkAesEncryptionKey);
            }
            /// <summary>
            /// 使用自定义的aesKey反序列化manifest；
            /// </summary>
            /// <param name="manifestContext">读取到的文本内容</param>
            /// <param name="aesKey">对称加密密钥</param>
            /// <returns>反序列化后的内容</returns>
            public static QuarkManifest Deserialize(string manifestContext, string aesKey)
            {
                var aesKeyBytes = QuarkUtility.GenerateBytesAESKey(aesKey);
                return Deserialize(manifestContext, aesKeyBytes);
            }
            public static QuarkManifest Deserialize(string manifestContext, byte[] aesKeyBytes)
            {
                QuarkManifest quarkAssetManifest = null;
                try
                {
                    var encrypted = aesKeyBytes.Length > 0 ? true : false;
                    var unencryptedManifest = manifestContext;
                    if (encrypted)
                    {
                        unencryptedManifest = QuarkUtility.AESDecryptStringToString(manifestContext, aesKeyBytes);
                    }
                    quarkAssetManifest = QuarkUtility.ToObject<QuarkManifest>(unencryptedManifest);
                }
                catch { }
                return quarkAssetManifest;
            }
            public static void MergeManifest(QuarkManifest srcManifest, QuarkManifest diffManifest, out QuarkMergedManifest mergeResult)
            {
                //取并集
                mergeResult = new QuarkMergedManifest();
                var srcBundleInfoDict = srcManifest.BundleInfoDict.Values.ToDictionary(b => b.QuarkAssetBundle.BundlePath);
                var diffBundleInfoDict = diffManifest.BundleInfoDict.Values.ToDictionary(b => b.QuarkAssetBundle.BundlePath);
                List<QuarkMergedBundleAsset> mergedBundleList = new List<QuarkMergedBundleAsset>();
                foreach (var srcBundle in srcBundleInfoDict.Values)
                {
                    var srcBundlePath = srcBundle.QuarkAssetBundle.BundlePath;
                    if (diffBundleInfoDict.TryGetValue(srcBundlePath, out var diffBundle))
                    {
                        //在diffmanifest中存在，比较bundlekey
                        var isIncremental = diffBundle.Hash != srcBundle.Hash;
                        var mergeInfo = new QuarkMergedBundleAsset
                        {
                            IsIncremental = isIncremental,
                            QuarkBundleAsset = diffBundle
                        };
                        mergedBundleList.Add(mergeInfo);
                    }
                    else
                    {
                        //在diffmanifest中不存在，表示为母包的资源
                        var mergeInfo = new QuarkMergedBundleAsset
                        {
                            IsIncremental = false,
                            QuarkBundleAsset = srcBundle
                        };
                        mergedBundleList.Add(mergeInfo);
                    }
                }
                foreach (var diffBundle in diffBundleInfoDict.Values)
                {
                    var diffBundlePath = diffBundle.QuarkAssetBundle.BundlePath;
                    if (!srcBundleInfoDict.TryGetValue(diffBundlePath, out var srcBundle))
                    {
                        //src中不存在，则加入增量
                        var mergeInfo = new QuarkMergedBundleAsset
                        {
                            IsIncremental = true,
                            QuarkBundleAsset = diffBundle
                        };
                        mergedBundleList.Add(mergeInfo);
                    }

                }
                mergeResult.BuildTime = diffManifest.BuildTime;
                mergeResult.BuildVersion = diffManifest.BuildVersion;
                mergeResult.InternalBuildVersion = diffManifest.InternalBuildVersion;
                mergeResult.MergedBundles = mergedBundleList;
            }

        }
    }
}