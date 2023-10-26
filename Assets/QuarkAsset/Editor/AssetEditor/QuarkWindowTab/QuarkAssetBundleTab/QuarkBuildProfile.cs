using System.IO;
using UnityEngine;
namespace Quark.Editor
{
    public class QuarkBuildProfile : ScriptableObject
    {
        [SerializeField] string profileDescription;
        public string ProfileDescription
        {
            get { return profileDescription; }
            set { profileDescription = value; }
        }
        [SerializeField] AssetBundleBuildProfileData assetBundleBuildProfileData;
        public AssetBundleBuildProfileData AssetBundleBuildProfileData
        {
            get
            {
                if (assetBundleBuildProfileData == null)
                    assetBundleBuildProfileData = new AssetBundleBuildProfileData();
                return assetBundleBuildProfileData;
            }
            set { assetBundleBuildProfileData = value; }
        }
        public QuarkBuildParams GetBuildParams()
        {
            var buildOption = QuarkBuildController.GetBuildAssetBundleOptions(AssetBundleBuildProfileData.AssetBundleCompressType,
                AssetBundleBuildProfileData.DisableWriteTypeTree,
                AssetBundleBuildProfileData.DeterministicAssetBundle,
                AssetBundleBuildProfileData.ForceRebuildAssetBundle,
                AssetBundleBuildProfileData.IgnoreTypeTreeChanges);

            QuarkBuildParams buildParams = new QuarkBuildParams()
            {
                //AssetBundleOutputPath = GetBuildPath(),
                BuildPath = AssetBundleBuildProfileData.BuildPath,
                AssetBundleCompressType = AssetBundleBuildProfileData.AssetBundleCompressType,
                BuildAssetBundleOptions = buildOption,
                BuildTarget = AssetBundleBuildProfileData.BuildTarget,
                CopyToStreamingAssets = AssetBundleBuildProfileData.CopyToStreamingAssets,
                ClearStreamingAssetsDestinationPath = AssetBundleBuildProfileData.ClearStreamingAssetsDestinationPath,
                StreamingRelativePath = AssetBundleBuildProfileData.StreamingRelativePath,
                BuildType = AssetBundleBuildProfileData.BuildType,
                BuildVersion = AssetBundleBuildProfileData.BuildVersion,
                InternalBuildVersion = AssetBundleBuildProfileData.InternalBuildVersion,
                AssetBundleNameType = AssetBundleBuildProfileData.AssetBundleNameType,
                UseOffsetEncryptionForAssetBundle = AssetBundleBuildProfileData.UseOffsetEncryptionForAssetBundle,
                EncryptionOffsetForAssetBundle = AssetBundleBuildProfileData.EncryptionOffsetForAssetBundle,
                UseAesEncryptionForManifest = AssetBundleBuildProfileData.UseAesEncryptionForManifest,
                AesEncryptionKeyForManifest = AssetBundleBuildProfileData.AesEncryptionKeyForManifest,
                ForceRemoveAllAssetBundleNames = AssetBundleBuildProfileData.ForceRemoveAllAssetBundleNames,
                BuildHandlerName = AssetBundleBuildProfileData.BuildHandlerName
            };
            if (AssetBundleBuildProfileData.BuildType == QuarkBuildType.Incremental && AssetBundleBuildProfileData.UseProjectRelativeBuildPath)
            {
                buildParams.BuildPath = Path.Combine(QuarkEditorUtility.ApplicationPath, AssetBundleBuildProfileData.ProjectRelativeBuildPath).Replace("\\", "/");
            }
            return buildParams;
        }
        public void Reset()
        {
            assetBundleBuildProfileData = new AssetBundleBuildProfileData();
            profileDescription = string.Empty;
        }
    }
}
