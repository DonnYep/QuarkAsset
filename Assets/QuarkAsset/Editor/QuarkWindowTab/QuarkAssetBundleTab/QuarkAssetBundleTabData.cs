using System;
using System.IO;
using UnityEditor;
namespace Quark.Editor
{
    internal enum AssetBundleNameType : byte
    {
        DefaultName = 0,
        HashInstead = 2
    }
    [Serializable]
    internal class QuarkAssetBundleTabData
    {
        public BuildTarget BuildTarget;
        public string BuildVersion;
        public int InternalBuildVersion;
        public string BuildPath;
        public string AssetBundleBuildPath;
        public bool ClearOutputFolders;
        public bool CopyToStreamingAssets;
        public string StreamingRelativePath;
        public AssetBundleNameType AssetBundleNameType;
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

        public QuarkAssetBundleTabData()
        {
            BuildTarget = BuildTarget.StandaloneWindows;
            BuildPath = Path.Combine(Path.GetFullPath("."), "AssetBundles", "QuarkAsset").Replace("\\", "/");
            ClearOutputFolders = true;
            CopyToStreamingAssets = false;
            AssetBundleNameType = AssetBundleNameType.DefaultName;
            UseOffsetEncryptionForAssetBundle = false;
            EncryptionOffsetForAssetBundle = 32;
            UseAesEncryptionForManifest = false;
            AesEncryptionKeyForManifest = "QuarkAssetAesKey";
            BuildAssetBundleOptions = BuildAssetBundleOptions.ChunkBasedCompression;
            BuildVersion = "0_0_1";
            StreamingRelativePath = BuildVersion;
        }
    }
}