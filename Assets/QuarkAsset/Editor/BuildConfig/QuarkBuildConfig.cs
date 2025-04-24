using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Quark.Editor
{
    /// <summary>
    /// QuarkAsset打包配置
    /// 用于保存打包参数，确保跨机器构建一致性
    /// </summary>
    public class QuarkBuildConfig : ScriptableObject
    {
        [SerializeField] private string configDescription = "Default Build Config";
        
        [SerializeField] private BuildTarget buildTarget = BuildTarget.StandaloneWindows64;
        
        [SerializeField] private string buildVersion = "1.0.0";
        
        [SerializeField] private int internalBuildVersion = 1;
        
        [SerializeField] private string relativeBuildPath = "QuarkBuild";
        
        [SerializeField] private AssetBundleNameType bundleNameType = AssetBundleNameType.DefaultName;
        
        [SerializeField] private AssetBundleCompressType compressType = AssetBundleCompressType.ChunkBasedCompression_LZ4;
        
        [SerializeField] private bool copyToStreamingAssets = false;
        
        [SerializeField] private string streamingAssetsRelativePath = "QuarkAssets";
        
        [SerializeField] private bool clearStreamingAssetsPath = true;
        
        [SerializeField] private bool useOffsetEncryption = false;
        
        [SerializeField] private int encryptionOffset = 32;
        
        [SerializeField] private bool useAesEncryption = false;
        
        [SerializeField] private string aesEncryptionKey = "QuarkAssetKey";
        
        [SerializeField] private bool deterministic = true;
        
        [SerializeField] private bool disableWriteTypeTree = false;
        
        [SerializeField] private bool forceRebuild = true;
        
        [SerializeField] private bool ignoreTypeTreeChanges = false;
        
        [SerializeField] private QuarkBuildType buildType = QuarkBuildType.Full;
        
        [SerializeField] private bool forceRemoveAllAssetBundleNames = false;
        
        [SerializeField] private string buildHandlerName = "";
        
        // 添加新选项，用于确保跨机器构建一致性
        [SerializeField] private bool useFixedBuildTime = true;
        
        [SerializeField] private string fixedBuildTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        [SerializeField] private bool useFixedBuildHash = true;
        
        [SerializeField] private string buildHashSeed = "QuarkAsset";
        
        /// <summary>
        /// 配置描述
        /// </summary>
        public string ConfigDescription
        {
            get { return configDescription; }
            set { configDescription = value; }
        }
        
        /// <summary>
        /// 构建目标平台
        /// </summary>
        public BuildTarget BuildTarget
        {
            get { return buildTarget; }
            set { buildTarget = value; }
        }
        
        /// <summary>
        /// 构建版本号
        /// </summary>
        public string BuildVersion
        {
            get { return buildVersion; }
            set { buildVersion = value; }
        }
        
        /// <summary>
        /// 内部构建版本号
        /// </summary>
        public int InternalBuildVersion
        {
            get { return internalBuildVersion; }
            set { internalBuildVersion = value; }
        }
        
        /// <summary>
        /// 相对构建路径（相对于项目根目录）
        /// </summary>
        public string RelativeBuildPath
        {
            get { return relativeBuildPath; }
            set { relativeBuildPath = value; }
        }
        
        /// <summary>
        /// AssetBundle名称类型
        /// </summary>
        public AssetBundleNameType BundleNameType
        {
            get { return bundleNameType; }
            set { bundleNameType = value; }
        }
        
        /// <summary>
        /// 压缩类型
        /// </summary>
        public AssetBundleCompressType CompressType
        {
            get { return compressType; }
            set { compressType = value; }
        }
        
        /// <summary>
        /// 是否复制到StreamingAssets
        /// </summary>
        public bool CopyToStreamingAssets
        {
            get { return copyToStreamingAssets; }
            set { copyToStreamingAssets = value; }
        }
        
        /// <summary>
        /// StreamingAssets中的相对路径
        /// </summary>
        public string StreamingAssetsRelativePath
        {
            get { return streamingAssetsRelativePath; }
            set { streamingAssetsRelativePath = value; }
        }
        
        /// <summary>
        /// 是否清空StreamingAssets目标路径
        /// </summary>
        public bool ClearStreamingAssetsPath
        {
            get { return clearStreamingAssetsPath; }
            set { clearStreamingAssetsPath = value; }
        }
        
        /// <summary>
        /// 是否使用偏移加密
        /// </summary>
        public bool UseOffsetEncryption
        {
            get { return useOffsetEncryption; }
            set { useOffsetEncryption = value; }
        }
        
        /// <summary>
        /// 加密偏移量
        /// </summary>
        public int EncryptionOffset
        {
            get { return encryptionOffset; }
            set { encryptionOffset = value; }
        }
        
        /// <summary>
        /// 是否使用AES加密
        /// </summary>
        public bool UseAesEncryption
        {
            get { return useAesEncryption; }
            set { useAesEncryption = value; }
        }
        
        /// <summary>
        /// AES加密密钥
        /// </summary>
        public string AesEncryptionKey
        {
            get { return aesEncryptionKey; }
            set { aesEncryptionKey = value; }
        }
        
        /// <summary>
        /// 是否使用确定性构建
        /// </summary>
        public bool Deterministic
        {
            get { return deterministic; }
            set { deterministic = value; }
        }
        
        /// <summary>
        /// 是否禁用类型树写入
        /// </summary>
        public bool DisableWriteTypeTree
        {
            get { return disableWriteTypeTree; }
            set { disableWriteTypeTree = value; }
        }
        
        /// <summary>
        /// 是否强制重新构建
        /// </summary>
        public bool ForceRebuild
        {
            get { return forceRebuild; }
            set { forceRebuild = value; }
        }
        
        /// <summary>
        /// 是否忽略类型树变化
        /// </summary>
        public bool IgnoreTypeTreeChanges
        {
            get { return ignoreTypeTreeChanges; }
            set { ignoreTypeTreeChanges = value; }
        }
        
        /// <summary>
        /// 构建类型
        /// </summary>
        public QuarkBuildType BuildType
        {
            get { return buildType; }
            set { buildType = value; }
        }
        
        /// <summary>
        /// 是否强制移除所有AssetBundle名称
        /// </summary>
        public bool ForceRemoveAllAssetBundleNames
        {
            get { return forceRemoveAllAssetBundleNames; }
            set { forceRemoveAllAssetBundleNames = value; }
        }
        
        /// <summary>
        /// 构建处理器名称
        /// </summary>
        public string BuildHandlerName
        {
            get { return buildHandlerName; }
            set { buildHandlerName = value; }
        }
        
        /// <summary>
        /// 是否使用固定构建时间
        /// </summary>
        public bool UseFixedBuildTime
        {
            get { return useFixedBuildTime; }
            set { useFixedBuildTime = value; }
        }
        
        /// <summary>
        /// 固定构建时间
        /// </summary>
        public string FixedBuildTime
        {
            get { return fixedBuildTime; }
            set { fixedBuildTime = value; }
        }
        
        /// <summary>
        /// 是否使用固定构建哈希
        /// </summary>
        public bool UseFixedBuildHash
        {
            get { return useFixedBuildHash; }
            set { useFixedBuildHash = value; }
        }
        
        /// <summary>
        /// 构建哈希种子
        /// </summary>
        public string BuildHashSeed
        {
            get { return buildHashSeed; }
            set { buildHashSeed = value; }
        }
        
        /// <summary>
        /// 从全局配置应用设置
        /// </summary>
        public void ApplyFromGlobalConfig()
        {
            var globalConfig = QuarkGlobalConfig.Instance;
            if (globalConfig == null || globalConfig.ActiveProfile == null)
                return;
                
            BuildVersion = globalConfig.DefaultBuildVersion;
            InternalBuildVersion = globalConfig.DefaultInternalBuildVersion;
            
            var profile = globalConfig.ActiveProfile;
            UseAesEncryption = profile.UseAesEncryption;
            AesEncryptionKey = profile.UseAesEncryption ? profile.AesEncryptionKey : AesEncryptionKey;
            
            UseOffsetEncryption = profile.UseOffsetEncryption;
            EncryptionOffset = profile.UseOffsetEncryption ? profile.OffsetEncryptionValue : EncryptionOffset;
            
            EditorUtility.SetDirty(this);
        }
        
        /// <summary>
        /// 获取构建参数
        /// </summary>
        /// <returns>构建参数对象</returns>
        public QuarkBuildParams GetBuildParams()
        {
            var buildOption = QuarkBuildController.GetBuildAssetBundleOptions(
                CompressType,
                DisableWriteTypeTree,
                Deterministic,
                ForceRebuild,
                IgnoreTypeTreeChanges);

            string absoluteBuildPath = Path.Combine(QuarkEditorUtility.ApplicationPath, RelativeBuildPath);
            string assetBundleOutputPath = Path.Combine(absoluteBuildPath, BuildTarget.ToString(), BuildVersion + "_" + InternalBuildVersion);
            
            var buildParams = new QuarkBuildParams
            {
                BuildPath = absoluteBuildPath,
                AssetBundleOutputPath = assetBundleOutputPath,
                BuildTarget = BuildTarget,
                BuildVersion = BuildVersion,
                InternalBuildVersion = InternalBuildVersion,
                AssetBundleNameType = BundleNameType,
                AssetBundleCompressType = CompressType,
                BuildAssetBundleOptions = buildOption,
                CopyToStreamingAssets = CopyToStreamingAssets,
                StreamingRelativePath = StreamingAssetsRelativePath,
                ClearStreamingAssetsDestinationPath = ClearStreamingAssetsPath,
                UseOffsetEncryptionForAssetBundle = UseOffsetEncryption,
                EncryptionOffsetForAssetBundle = EncryptionOffset,
                UseAesEncryptionForManifest = UseAesEncryption,
                AesEncryptionKeyForManifest = AesEncryptionKey,
                BuildType = BuildType,
                ForceRemoveAllAssetBundleNames = ForceRemoveAllAssetBundleNames,
                BuildHandlerName = BuildHandlerName
            };
            
            return buildParams;
        }
        
        /// <summary>
        /// 创建默认配置
        /// </summary>
        /// <returns>默认配置对象</returns>
        public static QuarkBuildConfig CreateDefault()
        {
            var config = CreateInstance<QuarkBuildConfig>();
            config.ConfigDescription = "Default Build Config";
            
            // 从全局配置应用设置
            config.ApplyFromGlobalConfig();
            
            return config;
        }
        
        /// <summary>
        /// 生成确定性构建哈希
        /// </summary>
        /// <returns>构建哈希</returns>
        public string GenerateDeterministicBuildHash()
        {
            if (UseFixedBuildHash)
            {
                // 使用固定种子生成哈希，确保不同机器生成相同的哈希
                string hashInput = $"{BuildHashSeed}_{BuildVersion}_{InternalBuildVersion}";
                return QuarkUtility.ComputeMD5(hashInput);
            }
            else
            {
                // 使用随机GUID
                return System.Guid.NewGuid().ToString();
            }
        }
        
        /// <summary>
        /// 获取构建时间
        /// </summary>
        /// <returns>构建时间字符串</returns>
        public string GetBuildTime()
        {
            if (UseFixedBuildTime)
            {
                return FixedBuildTime;
            }
            else
            {
                return System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }
        
        /// <summary>
        /// 更新构建版本
        /// </summary>
        /// <param name="incrementMajor">是否增加主版本号</param>
        public void UpdateBuildVersion(bool incrementMajor = false)
        {
            if (incrementMajor)
            {
                // 主版本号+1
                var parts = BuildVersion.Split('.');
                if (parts.Length > 0 && int.TryParse(parts[0], out int majorVersion))
                {
                    parts[0] = (majorVersion + 1).ToString();
                    BuildVersion = string.Join(".", parts);
                    InternalBuildVersion = 1;  // 重置内部版本号
                }
            }
            else
            {
                // 内部版本号+1
                InternalBuildVersion++;
            }
            
            // 更新固定构建时间
            FixedBuildTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            
            EditorUtility.SetDirty(this);
        }
    }
}
