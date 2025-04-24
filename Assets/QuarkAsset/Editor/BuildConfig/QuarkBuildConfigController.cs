using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Quark.Asset;

namespace Quark.Editor
{
    /// <summary>
    /// QuarkAsset优化构建控制器
    /// 确保跨机器打包哈希一致性
    /// </summary>
    public static class QuarkBuildConfigController
    {
        private const string CONFIG_DIRECTORY = "Assets/QuarkAsset/Editor/BuildConfig/Configs";
        
        /// <summary>
        /// 使用构建配置进行构建
        /// </summary>
        /// <param name="config">构建配置</param>
        /// <param name="dataset">资源数据集</param>
        /// <returns>成功与否</returns>
        public static bool BuildWithConfig(QuarkBuildConfig config, QuarkDataset dataset)
        {
            if (config == null)
            {
                Debug.LogError("构建配置不能为空");
                return false;
            }
            
            if (dataset == null)
            {
                Debug.LogError("资源数据集不能为空");
                return false;
            }
            
            try
            {
                // 获取构建参数
                var buildParams = config.GetBuildParams();
                
                // 创建清单
                var quarkManifest = new QuarkManifest
                {
                    BuildVersion = config.BuildVersion,
                    InternalBuildVersion = config.InternalBuildVersion,
                    BuildTime = config.GetBuildTime(),
                    BuildHash = config.GenerateDeterministicBuildHash()
                };
                
                // 确保输出目录存在
                if (!Directory.Exists(buildParams.AssetBundleOutputPath))
                {
                    Directory.CreateDirectory(buildParams.AssetBundleOutputPath);
                }
                
                // 设置构建缓存
                QuarkBuildCache buildCache = new QuarkBuildCache();
                
                // 准备数据
                QuarkBuildController.BuildDataset(dataset);
                
                // 处理包信息
                QuarkBuildController.ProcessBundleInfos(dataset, quarkManifest, buildParams);
                
                // 标记依赖
                QuarkBuildController.SetBundleDependent(dataset, quarkManifest);
                
                // 启用确定性构建
                EnableDeterministicBuild();
                
                // 构建AssetBundle
                AssetBundleManifest assetBundleManifest = BuildPipeline.BuildAssetBundles(
                    buildParams.AssetBundleOutputPath,
                    buildParams.BuildAssetBundleOptions,
                    buildParams.BuildTarget
                );
                
                // 恢复设置
                RestoreBuildSettings();
                
                if (assetBundleManifest == null)
                {
                    Debug.LogError("构建AssetBundle失败");
                    return false;
                }
                
                // 完成构建
                QuarkBuildController.FinishBuild(assetBundleManifest, dataset, quarkManifest, buildParams);
                
                // 写入清单，使用确定性方法
                WriteManifestWithConfig(quarkManifest, buildParams, config);
                
                // 更新构建缓存
                if (buildParams.BuildType == QuarkBuildType.Incremental)
                {
                    foreach (var bundle in quarkManifest.BundleInfoDict.Values)
                    {
                        string bundlePath = bundle.QuarkAssetBundle.BundlePath;
                        string bundleName = bundle.QuarkAssetBundle.BundleName;
                        
                        QuarkBundleAsset quarkBundleAsset = bundle;
                        var cache = new AssetCache
                        {
                            BundleName = bundleName,
                            BundlePath = bundlePath,
                            BundleHash = quarkBundleAsset.Hash,
                            AssetNames = GetAssetNamesFromBundle(bundle.QuarkAssetBundle)
                        };
                        
                        buildCache.BundleCacheList.Add(cache);
                    }
                    
                    // 写入构建缓存
                    QuarkBuildController.OverwriteBuildCache(buildCache, buildParams);
                }
                
                // 复制到StreamingAssets
                if (buildParams.CopyToStreamingAssets)
                {
                    QuarkBuildController.CopyToStreamingAssets(buildParams);
                }
                
                // 记录构建日志
                LogBuildSuccess(config, buildParams);
                
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"构建过程出错: {e.Message}\n{e.StackTrace}");
                RestoreBuildSettings();
                return false;
            }
        }
        
        /// <summary>
        /// 使用配置写入清单
        /// </summary>
        /// <param name="quarkManifest">清单对象</param>
        /// <param name="buildParams">构建参数</param>
        /// <param name="config">构建配置</param>
        private static void WriteManifestWithConfig(QuarkManifest quarkManifest, QuarkBuildParams buildParams, QuarkBuildConfig config)
        {
            // 设置确定性属性
            quarkManifest.BuildTime = config.GetBuildTime();
            quarkManifest.BuildHash = config.GenerateDeterministicBuildHash();
            quarkManifest.BuildVersion = config.BuildVersion;
            quarkManifest.InternalBuildVersion = config.InternalBuildVersion;
            
            var manifestJson = QuarkUtility.ToJson(quarkManifest);
            var manifestContext = manifestJson;
            var manifestWritePath = Path.Combine(buildParams.AssetBundleOutputPath, QuarkConstant.MANIFEST_NAME);
            
            if (buildParams.UseAesEncryptionForManifest)
            {
                var key = QuarkUtility.GenerateBytesAESKey(buildParams.AesEncryptionKeyForManifest);
                manifestContext = QuarkUtility.AESEncryptStringToString(manifestJson, key);
            }
            
            QuarkUtility.OverwriteTextFile(manifestWritePath, manifestContext);
        }
        
        /// <summary>
        /// 从Bundle获取资源名称数组
        /// </summary>
        /// <param name="bundle">Bundle对象</param>
        /// <returns>资源名称数组</returns>
        private static string[] GetAssetNamesFromBundle(QuarkBundle bundle)
        {
            List<string> assetNames = new List<string>();
            foreach (var obj in bundle.ObjectList)
            {
                assetNames.Add(obj.ObjectPath);
            }
            return assetNames.ToArray();
        }
        
        /// <summary>
        /// 记录构建成功日志
        /// </summary>
        /// <param name="config">构建配置</param>
        /// <param name="buildParams">构建参数</param>
        private static void LogBuildSuccess(QuarkBuildConfig config, QuarkBuildParams buildParams)
        {
            Debug.Log($"<color=green>构建成功!</color> 版本: {config.BuildVersion}_{config.InternalBuildVersion}");
            Debug.Log($"输出目录: {buildParams.AssetBundleOutputPath}");
            
            if (buildParams.CopyToStreamingAssets)
            {
                string streamingPath = Path.Combine(Application.streamingAssetsPath, buildParams.StreamingRelativePath);
                Debug.Log($"已复制到StreamingAssets: {streamingPath}");
            }
        }
        
        /// <summary>
        /// 启用确定性构建设置
        /// </summary>
        private static void EnableDeterministicBuild()
        {
            // 保存当前设置
            // 设置确定性编译环境变量
            Environment.SetEnvironmentVariable("UNITY_DETERMINISTIC_COMPILATION", "1");
        }
        
        /// <summary>
        /// 恢复构建设置
        /// </summary>
        private static void RestoreBuildSettings()
        {
            // 恢复环境变量
            Environment.SetEnvironmentVariable("UNITY_DETERMINISTIC_COMPILATION", null);
        }
        
        /// <summary>
        /// 创建新的构建配置
        /// </summary>
        /// <param name="configName">配置名称</param>
        /// <returns>新建的配置</returns>
        public static QuarkBuildConfig CreateBuildConfig(string configName)
        {
            if (string.IsNullOrEmpty(configName))
            {
                configName = "NewBuildConfig";
            }
            
            // 确保目录存在
            if (!Directory.Exists(CONFIG_DIRECTORY))
            {
                Directory.CreateDirectory(CONFIG_DIRECTORY);
            }
            
            var config = QuarkBuildConfig.CreateDefault();
            config.ConfigDescription = configName;
            
            // 从全局配置应用设置
            config.ApplyFromGlobalConfig();
            
            // 保存配置
            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{CONFIG_DIRECTORY}/{configName}.asset");
            AssetDatabase.CreateAsset(config, assetPath);
            AssetDatabase.SaveAssets();
            
            Debug.Log($"创建构建配置: {assetPath}");
            
            return config;
        }
        
        /// <summary>
        /// 获取所有构建配置
        /// </summary>
        /// <returns>构建配置列表</returns>
        public static List<QuarkBuildConfig> GetAllBuildConfigs()
        {
            var configs = new List<QuarkBuildConfig>();
            
            var guids = AssetDatabase.FindAssets("t:QuarkBuildConfig");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var config = AssetDatabase.LoadAssetAtPath<QuarkBuildConfig>(path);
                if (config != null)
                {
                    configs.Add(config);
                }
            }
            
            return configs;
        }
        
        /// <summary>
        /// 更新配置版本号
        /// </summary>
        /// <param name="config">构建配置</param>
        /// <param name="incrementMajor">是否增加主版本号</param>
        public static void UpdateConfigVersion(QuarkBuildConfig config, bool incrementMajor = false)
        {
            if (config == null)
                return;
                
            config.UpdateBuildVersion(incrementMajor);
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
        }
        
        /// <summary>
        /// 应用全局配置到所有构建配置
        /// </summary>
        public static void ApplyGlobalConfigToAllBuildConfigs()
        {
            var configs = GetAllBuildConfigs();
            foreach (var config in configs)
            {
                config.ApplyFromGlobalConfig();
            }
            
            AssetDatabase.SaveAssets();
        }
    }
}
