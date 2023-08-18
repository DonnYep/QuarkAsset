using UnityEditor;

namespace Quark.Editor
{
    public class QuarkBuildParams
    {
        /// <summary>
        /// AB构建的平台
        /// </summary>
        public BuildTarget BuildTarget;
        /// <summary>
        /// 输出的路径
        /// </summary>
        public string BuildPath;
        /// <summary>
        /// 构建的版本
        /// </summary>
        public string BuildVersion;
        /// <summary>
        /// 构建的内部小版本号
        /// </summary>
        public int InternalBuildVersion;
        /// <summary>
        /// 资源输出的路径
        /// </summary>
        public string AssetBundleOutputPath;
        /// <summary>
        /// 拷贝到streamingAsset文件；
        /// </summary>
        public bool CopyToStreamingAssets;
        /// <summary>
        /// StreamingAsset相对路径；
        /// </summary>
        public string StreamingRelativePath;
        /// <summary>
        /// AB包名称类型
        /// </summary>
        public AssetBundleNameType AssetBundleNameType;
        /// <summary>
        /// AB资源压缩类型；
        /// </summary>
        public AssetBundleCompressType AssetBundleCompressType;
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
        /// <summary>
        /// ab打包选项
        /// </summary>
        public BuildAssetBundleOptions BuildAssetBundleOptions;
        /// <summary>
        /// 构建类型，全量或增量
        /// </summary>
        public BuildType BuildType;
        /// <summary>
        /// 清空拷贝到的StreamingAssets路径
        /// </summary>
        public bool ClearStreamingAssetsDestinationPath;
        public static readonly QuarkBuildParams None=new QuarkBuildParams();
    }
}
