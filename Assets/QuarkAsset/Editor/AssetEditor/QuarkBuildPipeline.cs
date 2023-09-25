using Quark.Asset;
using System.IO;
using UnityEditor;
using System.Linq;

namespace Quark.Editor
{
    public class QuarkBuildPipeline
    {
        static string QuarkDatasetPath = "Assets/QuarkAssetDataset.asset";
        static AssetBundleBuildProfileData tabData;
        static QuarkDataset dataset;
        /// <summary>
        /// 默认不拷贝到StreamingAssets文件夹；
        /// </summary>
        [MenuItem("Window/QuarkAsset/Build/ActivePlatform")]
        public static void BuildActivePlatformAssetBundle()
        {
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            BuildAssetBundle(buildTarget);
        }
        /// <summary>
        /// 默认不拷贝到StreamingAssets文件夹；
        /// </summary>
        [MenuItem("Window/QuarkAsset/Build/ActivePlatformNameByHash")]
        public static void BuildActivePlatformAssetBundleNameByHash()
        {
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            BuildAssetBundle(buildTarget, true);
        }
        /// <summary>
        /// 默认不拷贝到StreamingAssets文件夹；
        /// </summary>
        [MenuItem("Window/QuarkAsset/Build/Android")]
        public static void BuildAndroidAssetBundle()
        {
            BuildAssetBundle(BuildTarget.Android);
        }
        /// <summary>
        /// 默认不拷贝到StreamingAssets文件夹；
        /// </summary>
        [MenuItem("Window/QuarkAsset/Build/AndroidNameByHash")]
        public static void BuildAndroidAssetBundleNameByHash()
        {
            BuildAssetBundle(BuildTarget.Android, true);
        }
        /// <summary>
        /// 默认不拷贝到StreamingAssets文件夹；
        /// </summary>
        [MenuItem("Window/QuarkAsset/Build/iOS")]
        public static void BuildiOSAssetBundle()
        {
            BuildAssetBundle(BuildTarget.iOS);
        }
        /// <summary>
        /// 默认不拷贝到StreamingAssets文件夹；
        /// </summary>
        [MenuItem("Window/QuarkAsset/Build/iOSNameByHash")]
        public static void BuildiOSAssetBundleNameByHash()
        {
            BuildAssetBundle(BuildTarget.iOS, true);
        }
        /// <summary>
        /// 默认不拷贝到StreamingAssets文件夹；
        /// </summary>
        [MenuItem("Window/QuarkAsset/Build/StandaloneWindows")]
        public static void BuildStandaloneWindowsAssetBundle()
        {
            BuildAssetBundle(BuildTarget.StandaloneWindows);
        }
        /// <summary>
        /// 默认不拷贝到StreamingAssets文件夹；
        /// </summary>
        [MenuItem("Window/QuarkAsset/Build/StandaloneWindowsNameByHash")]
        public static void BuildStandaloneWindowsAssetBundleNameByHash()
        {
            BuildAssetBundle(BuildTarget.StandaloneWindows, true);
        }
        /// <summary>
        /// 构建assetbundle
        /// </summary>
        /// <param name="buildTarget">目标平台</param>
        /// <param name="nameByHash">是否以hash命名bundle</param>
        /// <param name="copyToStreamingAssets">拷贝到StreamingAssets目录</param>
        /// <returns>生成后的地址</returns>
        public static string BuildAssetBundle(BuildTarget buildTarget, bool nameByHash = false, bool copyToStreamingAssets = false)
        {
            dataset = AssetDatabase.LoadAssetAtPath<QuarkDataset>(QuarkDatasetPath);
            if (dataset == null)
            {
                QuarkUtility.LogError($"Path: {QuarkDatasetPath} invalid !");
                return string.Empty;
            }
            tabData = new AssetBundleBuildProfileData();
            tabData.BuildTarget = buildTarget;
            if (nameByHash)
                tabData.AssetBundleNameType = AssetBundleNameType.HashInstead;
            else
                tabData.AssetBundleNameType = AssetBundleNameType.DefaultName;
            tabData.CopyToStreamingAssets = copyToStreamingAssets;
            tabData.AssetBundleOutputPath = Path.Combine(tabData.BuildPath, tabData.BuildTarget.ToString(), $"{tabData.BuildVersion}_{tabData.InternalBuildVersion}").Replace("\\", "/");
            tabData.StreamingRelativePath = buildTarget.ToString().ToLower();
            BuildAssetBundle(dataset, tabData);
            return tabData.AssetBundleOutputPath;
        }
        /// <summary>
        /// 加密构建assetBundle；
        /// </summary>
        /// <param name="buildTarget">目标平台</param>
        /// <param name="aseKey">manifest加密的aes密钥</param>
        /// <param name="offset">bundle byte向右偏移量</param>
        /// <param name="nameByHash">是否以hash命名bundle</param>
        /// <param name="copyToStreamingAssets">拷贝到StreamingAssets目录</param>
        /// <returns>生成后的地址</returns>
        public static string EncryptBuildAssetBundle(BuildTarget buildTarget, string aseKey, int offset, bool nameByHash = false, bool copyToStreamingAssets = false)
        {
            dataset = AssetDatabase.LoadAssetAtPath<QuarkDataset>(QuarkDatasetPath);
            if (dataset == null)
            {
                QuarkUtility.LogError($"Path: {QuarkDatasetPath} invalid !");
                return string.Empty;
            }
            tabData = new AssetBundleBuildProfileData();
            tabData.BuildTarget = buildTarget;
            if (!string.IsNullOrEmpty(aseKey))
            {
                var aesKeyLength = System.Text.Encoding.UTF8.GetBytes(aseKey).Length;
                if (aesKeyLength != 16 && aesKeyLength != 24 && aesKeyLength != 32)
                {
                    QuarkUtility.LogError("QuarkAsset build aes key is invalid , key should be 16,24 or 32 bytes long !");
                    return string.Empty;
                }
                tabData.UseAesEncryptionForManifest = true;
                tabData.AesEncryptionKeyForManifest = aseKey;
            }
            else
            {
                tabData.UseAesEncryptionForManifest = false;
            }
            tabData.UseOffsetEncryptionForAssetBundle = true;
            tabData.EncryptionOffsetForAssetBundle = offset;
            if (tabData.EncryptionOffsetForAssetBundle < 0)
                tabData.EncryptionOffsetForAssetBundle = 0;
            if (nameByHash)
                tabData.AssetBundleNameType = AssetBundleNameType.HashInstead;
            else
                tabData.AssetBundleNameType = AssetBundleNameType.DefaultName;
            tabData.CopyToStreamingAssets = copyToStreamingAssets;
            tabData.AssetBundleOutputPath = Path.Combine(tabData.BuildPath, tabData.BuildTarget.ToString(), $"{tabData.BuildVersion}_{tabData.InternalBuildVersion}").Replace("\\", "/");
            tabData.StreamingRelativePath = buildTarget.ToString().ToLower();
            BuildAssetBundle(dataset, tabData);
            return tabData.AssetBundleOutputPath;
        }
        public static string[] GetBuildScenePath()
        {
            dataset = AssetDatabase.LoadAssetAtPath<QuarkDataset>(QuarkDatasetPath);
            if (dataset == null)
            {
                QuarkUtility.LogError($"Path: {QuarkDatasetPath} invalid !");
                return new string[0];
            }
            return dataset.QuarkSceneList.Select(s => s.ObjectPath).ToArray();
        }
        static void BuildAssetBundle(QuarkDataset dataset, AssetBundleBuildProfileData tabData)
        {
            QuarkUtility.LogInfo("Quark build pipeline start");
            var assetBundleBuildPath = tabData.AssetBundleOutputPath;
            if (Directory.Exists(assetBundleBuildPath))
            {
                QuarkUtility.DeleteFolder(assetBundleBuildPath);
            }
            if (!Directory.Exists(assetBundleBuildPath))
            {
                Directory.CreateDirectory(assetBundleBuildPath);
            }
            var quarkManifest = new QuarkManifest();
            dataset.CacheAllBundleInfos();
            var buildParams = new QuarkBuildParams()
            {
                AesEncryptionKeyForManifest = tabData.AesEncryptionKeyForManifest,
                AssetBundleOutputPath = tabData.AssetBundleOutputPath,
                AssetBundleCompressType = tabData.AssetBundleCompressType,
                AssetBundleNameType = tabData.AssetBundleNameType,
                BuildAssetBundleOptions = tabData.BuildAssetBundleOptions,
                BuildTarget = tabData.BuildTarget,
                BuildVersion = tabData.BuildVersion,
                CopyToStreamingAssets = tabData.CopyToStreamingAssets,
                EncryptionOffsetForAssetBundle = tabData.EncryptionOffsetForAssetBundle,
                InternalBuildVersion = tabData.InternalBuildVersion,
                StreamingRelativePath = tabData.StreamingRelativePath,
                UseAesEncryptionForManifest = tabData.UseAesEncryptionForManifest,
                UseOffsetEncryptionForAssetBundle = tabData.UseOffsetEncryptionForAssetBundle,
                ClearStreamingAssetsDestinationPath = true
            };
            QuarkBuildController.BuildDataset(dataset);
            QuarkBuildController.ProcessBundleInfos(dataset, quarkManifest, buildParams);
            QuarkBuildController.SetBundleDependent(dataset, quarkManifest);
            var assetBundleManifest = BuildPipeline.BuildAssetBundles(assetBundleBuildPath, tabData.BuildAssetBundleOptions, tabData.BuildTarget);
            QuarkBuildController.FinishBuild(assetBundleManifest, dataset, quarkManifest, buildParams);
            QuarkBuildController.OverwriteManifest(quarkManifest, buildParams);
            QuarkUtility.LogInfo("Quark build pipeline done");
        }
    }
}
