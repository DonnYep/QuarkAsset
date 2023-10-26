using System;
using System.IO;
using UnityEditor;

namespace Quark.Editor
{
    [Serializable]
    public class AssetBundleBuildProfileData
    {
        public BuildTarget BuildTarget;
        public string BuildVersion;
        public int InternalBuildVersion;
        /// <summary>
        /// AB构建的文件夹{WORKSPACE}/{BuildTarget}/
        /// </summary>
        public string BuildPath;

        public bool UseProjectRelativeBuildPath;
        public string ProjectRelativeBuildPath;

        public bool CopyToStreamingAssets;
        public string StreamingRelativePath;
        public AssetBundleNameType AssetBundleNameType;
        /// <summary>
        /// AB资源压缩类型；
        /// </summary>
        public AssetBundleCompressType AssetBundleCompressType;
        /// <summary>
        /// 不会在AssetBundle中包含类型信息;
        /// </summary>
        public bool DisableWriteTypeTree;
        /// <summary>
        /// 使用存储在Asset Bundle中的对象的id的哈希构建Asset Bundle;
        /// </summary>
        public bool DeterministicAssetBundle;
        /// <summary>
        /// 强制重建Asset Bundles;
        /// </summary>
        public bool ForceRebuildAssetBundle;
        /// <summary>
        /// 执行增量构建检查时忽略类型树更改;
        /// </summary>
        public bool IgnoreTypeTreeChanges;
        /// <summary>
        /// 使用偏移加密；
        /// </summary>
        public bool UseOffsetEncryptionForAssetBundle;
        /// <summary>
        /// 加密偏移量；
        /// </summary>
        public int EncryptionOffsetForAssetBundle;

        /// <summary>
        /// 使用对称加密对build信息进行加密;
        /// </summary>
        public bool UseAesEncryptionForManifest;
        /// <summary>
        /// 对称加密的密钥；
        /// </summary>
        public string AesEncryptionKeyForManifest;

        public BuildAssetBundleOptions BuildAssetBundleOptions;

        public string BuildHandlerName;

        public int QuarBuildHandlerIndex;
        /// <summary>
        /// 构建类型
        /// </summary>
        public QuarkBuildType BuildType;
        /// <summary>
        /// 强制清除所有ab名称
        /// </summary>
        public bool ForceRemoveAllAssetBundleNames;
        /// <summary>
        /// 清空拷贝到的StreamingAssets路径
        /// </summary>
        public bool ClearStreamingAssetsDestinationPath;
        /// <summary>
        /// 构建预设地址
        /// </summary>
        public string BuildPresetPath;
        public AssetBundleBuildProfileData()
        {
            BuildTarget = BuildTarget.StandaloneWindows;
            UseProjectRelativeBuildPath = true;
            AssetBundleCompressType = AssetBundleCompressType.ChunkBasedCompression_LZ4;
            ProjectRelativeBuildPath = QuarkEditorConstant.DEFAULT_ASSETBUNDLE_RELATIVE_PATH;
            BuildPath = Path.Combine(Path.GetFullPath("."), ProjectRelativeBuildPath).Replace("\\", "/");
            CopyToStreamingAssets = false;
            AssetBundleNameType = AssetBundleNameType.DefaultName;
            UseOffsetEncryptionForAssetBundle = false;
            EncryptionOffsetForAssetBundle = 32;
            UseAesEncryptionForManifest = false;
            AesEncryptionKeyForManifest = "QuarkAssetAesKey";
            BuildAssetBundleOptions = BuildAssetBundleOptions.ChunkBasedCompression;
            BuildVersion = "0.0.1";
            StreamingRelativePath = BuildVersion;
            BuildHandlerName = QuarkConstant.NONE;
            QuarBuildHandlerIndex = 0;
            ForceRebuildAssetBundle = false;
            DisableWriteTypeTree = false;
            DeterministicAssetBundle = false;
            IgnoreTypeTreeChanges = false;
            BuildType = QuarkBuildType.Full;
        }
    }
}
