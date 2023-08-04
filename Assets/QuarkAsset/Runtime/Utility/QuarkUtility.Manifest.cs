using Quark.Asset;
using System.Collections.Generic;
using System.Linq;

namespace Quark
{
    public partial class QuarkUtility
    {
        public class Manifest
        {
            public static string SerializeManifest(QuarkManifest manifest, string aesKey)
            {
                var aesKeyBytes = QuarkUtility.GenerateBytesAESKey(aesKey);
                return SerializeManifest(manifest, aesKeyBytes);
            }
            public static string SerializeManifest(QuarkManifest manifest, byte[] aesKeyBytes)
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
            public static string SerializeMergedManifest(QuarkMergedManifest mergedManifest, string aesKey)
            {
                var aesKeyBytes = QuarkUtility.GenerateBytesAESKey(aesKey);
                return SerializeMergedManifest(mergedManifest, aesKeyBytes);
            }
            public static string SerializeMergedManifest(QuarkMergedManifest mergedManifest, byte[] aesKeyBytes)
            {
                var manifestJson = QuarkUtility.ToJson(mergedManifest);
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
            public static QuarkManifest DeserializeManifest(string manifestContext)
            {
                return DeserializeManifest(manifestContext, QuarkDataProxy.QuarkAesEncryptionKey);
            }
            /// <summary>
            /// 使用自定义的aesKey反序列化manifest；
            /// </summary>
            /// <param name="manifestContext">读取到的文本内容</param>
            /// <param name="aesKey">对称加密密钥</param>
            /// <returns>反序列化后的内容</returns>
            public static QuarkManifest DeserializeManifest(string manifestContext, string aesKey)
            {
                var aesKeyBytes = QuarkUtility.GenerateBytesAESKey(aesKey);
                return DeserializeManifest(manifestContext, aesKeyBytes);
            }
            public static QuarkManifest DeserializeManifest(string manifestContext, byte[] aesKeyBytes)
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
            public static QuarkMergedManifest DeserializeMergedManifest(string mergedManifestContext)
            {
                return DeserializeMergedManifest(mergedManifestContext, QuarkDataProxy.QuarkAesEncryptionKey);
            }
            public static QuarkMergedManifest DeserializeMergedManifest(string mergedManifestContext, string aesKey)
            {
                var aesKeyBytes = QuarkUtility.GenerateBytesAESKey(aesKey);
                return DeserializeMergedManifest(mergedManifestContext, aesKeyBytes);
            }
            public static QuarkMergedManifest DeserializeMergedManifest(string mergedManifestContext, byte[] aesKeyBytes)
            {
                QuarkMergedManifest mergedManifest = null;
                try
                {
                    var encrypted = aesKeyBytes.Length > 0 ? true : false;
                    var unencryptedManifest = mergedManifestContext;
                    if (encrypted)
                    {
                        unencryptedManifest = QuarkUtility.AESDecryptStringToString(mergedManifestContext, aesKeyBytes);
                    }
                    mergedManifest = QuarkUtility.ToObject<QuarkMergedManifest>(unencryptedManifest);
                }
                catch { }
                return mergedManifest;
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
                        var equal = diffBundle.Hash == srcBundle.Hash;
                        var mergeInfo = new QuarkMergedBundleAsset
                        {
                            IsIncremental = !equal,
                            QuarkBundleAsset = diffBundle
                        };
                        mergedBundleList.Add(mergeInfo);
                    }
                }
                foreach (var diffBundle in diffBundleInfoDict.Values)
                {
                    var diffBundlePath = diffBundle.QuarkAssetBundle.BundlePath;
                    if (!srcBundleInfoDict.TryGetValue(diffBundlePath, out var srcBundle))
                    {
                        //src中不存在，diff中存在，则表示为新增的
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
                mergeResult.BuildHash = diffManifest.BuildHash;
                mergeResult.MergedBundles = mergedBundleList;
            }
            public static void MergeManifest(QuarkMergedManifest srcMergedManifest, QuarkMergedManifest diffMergedManifest, out QuarkMergedManifest mergeResult)
            {
                //取并集
                mergeResult = new QuarkMergedManifest();
                var srcBundleInfoDict = srcMergedManifest.MergedBundles.ToDictionary(b => b.QuarkBundleAsset.QuarkAssetBundle.BundlePath);
                var diffBundleInfoDict = diffMergedManifest.MergedBundles.ToDictionary(b => b.QuarkBundleAsset.QuarkAssetBundle.BundlePath);
                List<QuarkMergedBundleAsset> mergedBundleList = new List<QuarkMergedBundleAsset>();
                foreach (var srcBundle in srcBundleInfoDict.Values)
                {
                    var srcBundlePath = srcBundle.QuarkBundleAsset.QuarkAssetBundle.BundlePath;
                    if (diffBundleInfoDict.TryGetValue(srcBundlePath, out var diffBundle))
                    {
                        //在diffmanifest中存在，比较bundlekey
                        var equal = diffBundle.QuarkBundleAsset.Hash == srcBundle.QuarkBundleAsset.Hash;
                        var mergeInfo = new QuarkMergedBundleAsset
                        {
                            IsIncremental = !equal,
                            QuarkBundleAsset = diffBundle.QuarkBundleAsset
                        };
                        mergedBundleList.Add(mergeInfo);
                    }
                }
                foreach (var diffBundle in diffBundleInfoDict.Values)
                {
                    var diffBundlePath = diffBundle.QuarkBundleAsset.QuarkAssetBundle.BundlePath;
                    if (!srcBundleInfoDict.TryGetValue(diffBundlePath, out var srcBundle))
                    {
                        //src中不存在，diff中存在，则表示为新增的
                        var mergeInfo = new QuarkMergedBundleAsset
                        {
                            IsIncremental = true,
                            QuarkBundleAsset = diffBundle.QuarkBundleAsset
                        };
                        mergedBundleList.Add(mergeInfo);
                    }
                }
                mergeResult.BuildTime = diffMergedManifest.BuildTime;
                mergeResult.BuildVersion = diffMergedManifest.BuildVersion;
                mergeResult.InternalBuildVersion = diffMergedManifest.InternalBuildVersion;
                mergeResult.BuildHash = diffMergedManifest.BuildHash;
                mergeResult.MergedBundles = mergedBundleList;
            }
            public static void MergeManifest(QuarkMergedManifest srcMergedManifest, QuarkManifest diffManifest, out QuarkMergedManifest mergeResult)
            {
                //取并集
                mergeResult = new QuarkMergedManifest();
                var srcBundleInfoDict = srcMergedManifest.MergedBundles.ToDictionary(b => b.QuarkBundleAsset.QuarkAssetBundle.BundlePath);
                var diffBundleInfoDict = diffManifest.BundleInfoDict.Values.ToDictionary(b => b.QuarkAssetBundle.BundlePath);
                List<QuarkMergedBundleAsset> mergedBundleList = new List<QuarkMergedBundleAsset>();
                foreach (var srcBundle in srcBundleInfoDict.Values)
                {
                    var srcBundlePath = srcBundle.QuarkBundleAsset.QuarkAssetBundle.BundlePath;
                    if (diffBundleInfoDict.TryGetValue(srcBundlePath, out var diffBundle))
                    {
                        //在diffmanifest中存在，比较bundlekey
                        var equal = diffBundle.Hash == srcBundle.QuarkBundleAsset.Hash;
                        var mergeInfo = new QuarkMergedBundleAsset
                        {
                            IsIncremental = !equal,
                            QuarkBundleAsset = diffBundle
                        };
                        mergedBundleList.Add(mergeInfo);
                    }
                }
                foreach (var diffBundle in diffBundleInfoDict.Values)
                {
                    var diffBundlePath = diffBundle.QuarkAssetBundle.BundlePath;
                    if (!srcBundleInfoDict.TryGetValue(diffBundlePath, out var srcBundle))
                    {
                        //src中不存在，diff中存在，则表示为新增的
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
                mergeResult.BuildHash = diffManifest.BuildHash;
                mergeResult.MergedBundles = mergedBundleList;
            }
        }
    }
}