using Quark.Asset;
using System.IO;
using UnityEditor;
using System.Linq;

namespace Quark.Editor
{
    public class QuarkBuildPipeline
    {
        static AssetBundleBuildProfileData profileData;
        static QuarkBuildProfile buildProfile;
        static QuarkDataset dataset;
        public static QuarkDataset DefaultDataset
        {
            get
            {
                if (dataset == null)
                {
                    dataset = AssetDatabase.LoadAssetAtPath<QuarkDataset>(QuarkEditorConstant.DEFAULT_DATASET_PATH);
                }
                return dataset;
            }
        }
        public static QuarkBuildProfile DefaultBuildProfile
        {
            get
            {
                if (buildProfile == null)
                {
                    buildProfile = AssetDatabase.LoadAssetAtPath<QuarkBuildProfile>(QuarkEditorConstant.DEFAULT_BUILD_PROFILE_PATH);
                }
                return buildProfile;
            }
        }
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
            if (DefaultDataset == null)
            {
                QuarkUtility.LogError($"Path: {QuarkEditorConstant.DEFAULT_DATASET_PATH} invalid !");
                return string.Empty;
            }
            profileData = new AssetBundleBuildProfileData();
            profileData.BuildTarget = buildTarget;
            if (nameByHash)
                profileData.AssetBundleNameType = AssetBundleNameType.HashInstead;
            else
                profileData.AssetBundleNameType = AssetBundleNameType.DefaultName;
            profileData.CopyToStreamingAssets = copyToStreamingAssets;
            var buildPath = Path.Combine(QuarkEditorUtility.ApplicationPath, profileData.ProjectRelativeBuildPath, profileData.BuildPath, profileData.BuildTarget.ToString(), $"{profileData.BuildVersion}_{profileData.InternalBuildVersion}").Replace("\\", "/");
            profileData.StreamingRelativePath = buildTarget.ToString().ToLower();
            BuildAssetBundle(dataset, profileData, buildPath);
            return buildPath;
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
            dataset = AssetDatabase.LoadAssetAtPath<QuarkDataset>(QuarkEditorConstant.DEFAULT_DATASET_PATH);
            if (dataset == null)
            {
                QuarkUtility.LogError($"Path: {QuarkEditorConstant.DEFAULT_DATASET_PATH} invalid !");
                return string.Empty;
            }
            profileData = new AssetBundleBuildProfileData();
            profileData.BuildTarget = buildTarget;
            if (!string.IsNullOrEmpty(aseKey))
            {
                var aesKeyLength = System.Text.Encoding.UTF8.GetBytes(aseKey).Length;
                if (aesKeyLength != 16 && aesKeyLength != 24 && aesKeyLength != 32)
                {
                    QuarkUtility.LogError("QuarkAsset build aes key is invalid , key should be 16,24 or 32 bytes long !");
                    return string.Empty;
                }
                profileData.UseAesEncryptionForManifest = true;
                profileData.AesEncryptionKeyForManifest = aseKey;
            }
            else
            {
                profileData.UseAesEncryptionForManifest = false;
            }
            profileData.UseOffsetEncryptionForAssetBundle = true;
            profileData.EncryptionOffsetForAssetBundle = offset;
            if (profileData.EncryptionOffsetForAssetBundle < 0)
                profileData.EncryptionOffsetForAssetBundle = 0;
            if (nameByHash)
                profileData.AssetBundleNameType = AssetBundleNameType.HashInstead;
            else
                profileData.AssetBundleNameType = AssetBundleNameType.DefaultName;
            profileData.CopyToStreamingAssets = copyToStreamingAssets;
            var buildPath = Path.Combine(QuarkEditorUtility.ApplicationPath, profileData.ProjectRelativeBuildPath, profileData.BuildPath, profileData.BuildTarget.ToString(), $"{profileData.BuildVersion}_{profileData.InternalBuildVersion}").Replace("\\", "/");

            profileData.StreamingRelativePath = buildTarget.ToString().ToLower();
            BuildAssetBundle(dataset, profileData, buildPath);
            return buildPath;
        }
        [MenuItem("Window/QuarkAsset/Build/BuildAssetBundleByProfile")]
        public static void BuildAssetBundleByProfile()
        {
            if (DefaultDataset == null)
            {
                QuarkUtility.LogError($"QuarkDataset : {QuarkEditorConstant.DEFAULT_DATASET_PATH} not exist !");
                return;
            }
            if (DefaultBuildProfile == null)
            {
                QuarkUtility.LogError($"QuarkBuildProfile : {QuarkEditorConstant.DEFAULT_BUILD_PROFILE_PATH} not exist !");
                return;
            }
            var profileData = DefaultBuildProfile.AssetBundleBuildProfileData;
            var buildPath = Path.Combine(QuarkEditorUtility.ApplicationPath, profileData.ProjectRelativeBuildPath, profileData.BuildTarget.ToString(), $"{profileData.BuildVersion}_{profileData.InternalBuildVersion}").Replace("\\", "/");
            var buildParams = DefaultBuildProfile.GetBuildParams();
            buildParams.AssetBundleOutputPath = buildPath;
            BuildAssetBundle(DefaultDataset, buildParams);
        }
        public static string[] GetBuildScenePath()
        {
            dataset = AssetDatabase.LoadAssetAtPath<QuarkDataset>(QuarkEditorConstant.DEFAULT_DATASET_PATH);
            if (dataset == null)
            {
                QuarkUtility.LogError($"Path: {QuarkEditorConstant.DEFAULT_DATASET_PATH} invalid !");
                return new string[0];
            }
            return dataset.QuarkSceneList.Select(s => s.ObjectPath).ToArray();
        }
        static void BuildAssetBundle(QuarkDataset dataset, AssetBundleBuildProfileData profileData, string buildPath)
        {
            QuarkUtility.LogInfo("Quark build pipeline start");
            var assetBundleBuildPath = buildPath;
            QuarkUtility.EmptyFolder(assetBundleBuildPath);
            var quarkManifest = new QuarkManifest();
            var buildParams = new QuarkBuildParams()
            {
                AesEncryptionKeyForManifest = profileData.AesEncryptionKeyForManifest,
                AssetBundleOutputPath = assetBundleBuildPath,
                AssetBundleCompressType = profileData.AssetBundleCompressType,
                AssetBundleNameType = profileData.AssetBundleNameType,
                BuildAssetBundleOptions = profileData.BuildAssetBundleOptions,
                BuildTarget = profileData.BuildTarget,
                BuildVersion = profileData.BuildVersion,
                CopyToStreamingAssets = profileData.CopyToStreamingAssets,
                EncryptionOffsetForAssetBundle = profileData.EncryptionOffsetForAssetBundle,
                InternalBuildVersion = profileData.InternalBuildVersion,
                StreamingRelativePath = profileData.StreamingRelativePath,
                UseAesEncryptionForManifest = profileData.UseAesEncryptionForManifest,
                UseOffsetEncryptionForAssetBundle = profileData.UseOffsetEncryptionForAssetBundle,
                ClearStreamingAssetsDestinationPath = true
            };
            QuarkBuildController.BuildDataset(dataset);
            dataset.CacheAllBundleInfos();
            QuarkBuildController.ProcessBundleInfos(dataset, quarkManifest, buildParams);
            QuarkBuildController.SetBundleDependent(dataset, quarkManifest);
            var assetBundleManifest = BuildPipeline.BuildAssetBundles(assetBundleBuildPath, profileData.BuildAssetBundleOptions, profileData.BuildTarget);
            QuarkBuildController.FinishBuild(assetBundleManifest, dataset, quarkManifest, buildParams);
            QuarkBuildController.OverwriteManifest(quarkManifest, buildParams);
            QuarkUtility.LogInfo("Quark build pipeline done");
        }

        static void BuildAssetBundle(QuarkDataset dataset, QuarkBuildParams buildParams)
        {
            var quarkManifest = new QuarkManifest();
            var assetBundleBuildPath = buildParams.AssetBundleOutputPath;
            QuarkUtility.EmptyFolder(assetBundleBuildPath);
            QuarkBuildController.BuildDataset(dataset);
            dataset.CacheAllBundleInfos();
            QuarkBuildController.ProcessBundleInfos(dataset, quarkManifest, buildParams);
            QuarkBuildController.SetBundleDependent(dataset, quarkManifest);
            var assetBundleManifest = BuildPipeline.BuildAssetBundles(buildParams.AssetBundleOutputPath, buildParams.BuildAssetBundleOptions, buildParams.BuildTarget);
            QuarkBuildController.FinishBuild(assetBundleManifest, dataset, quarkManifest, buildParams);
            QuarkBuildController.OverwriteManifest(quarkManifest, buildParams);
            QuarkBuildController.CopyToStreamingAssets(buildParams);
            QuarkUtility.LogInfo("Quark build pipeline done");
        }
    }
}
