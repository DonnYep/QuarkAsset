using UnityEditor;
using UnityEngine;
namespace Quark.Editor
{
    public class QuarkBuildProfile : ScriptableObject
    {
        public string ProfileDescription;

        public BuildTarget BuildTarget;
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
        /// 强制清除所有ab名称
        /// </summary>
        public bool ForceRemoveAllAssetBundleNames;
        public string BuildVersion;
        public int InternalBuildVersion;
        public AssetBundleNameType AssetBundleNameType;
        public BuildType BuildType;
        public bool UseAesEncryptionForManifest;
        public string AesEncryptionKeyForManifest;
        public int EncryptionOffsetForAssetBundle;
        public bool UseOffsetEncryptionForAssetBundle;
    }
}
