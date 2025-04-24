using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using UnityEditor;

namespace Quark.Editor
{
    /// <summary>
    /// QuarkAsset全局配置类
    /// 用于管理环境配置、版本号等全局设置
    /// </summary>
    public class QuarkGlobalConfig : ScriptableObject
    {
        private const string ConfigFileName = "QuarkGlobalConfig.asset";
        private static readonly string ConfigFolderPath = "Assets/QuarkAsset/Editor/GlobalConfig";
        private static readonly string ConfigFilePath = Path.Combine(ConfigFolderPath, ConfigFileName);

        private static QuarkGlobalConfig instance;
        
        /// <summary>
        /// 获取全局配置单例
        /// </summary>
        public static QuarkGlobalConfig Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = LoadOrCreateConfig();
                }
                return instance;
            }
        }

        [SerializeField]
        private List<QuarkProfile> profiles = new List<QuarkProfile>();
        
        [SerializeField]
        private int activeProfileIndex = 0;
        
        [SerializeField]
        private string defaultBuildVersion = "1.0.0";
        
        [SerializeField]
        private int defaultInternalBuildVersion = 1;
        
        /// <summary>
        /// 所有环境配置列表
        /// </summary>
        public List<QuarkProfile> Profiles
        {
            get { return profiles; }
        }
        
        /// <summary>
        /// 当前激活的环境配置索引
        /// </summary>
        public int ActiveProfileIndex
        {
            get { return activeProfileIndex; }
            set 
            { 
                activeProfileIndex = Mathf.Clamp(value, 0, profiles.Count - 1);
                EditorUtility.SetDirty(this);
            }
        }
        
        /// <summary>
        /// 当前激活的环境配置
        /// </summary>
        public QuarkProfile ActiveProfile
        {
            get 
            { 
                if (profiles.Count > 0 && activeProfileIndex >= 0 && activeProfileIndex < profiles.Count)
                    return profiles[activeProfileIndex];
                
                return null;
            }
        }
        
        /// <summary>
        /// 默认构建版本号
        /// </summary>
        public string DefaultBuildVersion
        {
            get { return defaultBuildVersion; }
            set 
            { 
                defaultBuildVersion = value;
                EditorUtility.SetDirty(this);
            }
        }
        
        /// <summary>
        /// 默认内部构建版本号
        /// </summary>
        public int DefaultInternalBuildVersion
        {
            get { return defaultInternalBuildVersion; }
            set 
            { 
                defaultInternalBuildVersion = value; 
                EditorUtility.SetDirty(this);
            }
        }
        
        /// <summary>
        /// 添加环境配置
        /// </summary>
        /// <param name="profile">要添加的环境配置</param>
        public void AddProfile(QuarkProfile profile)
        {
            if (profile == null)
                return;
                
            if (!profiles.Contains(profile))
            {
                profiles.Add(profile);
                EditorUtility.SetDirty(this);
            }
        }
        
        /// <summary>
        /// 移除环境配置
        /// </summary>
        /// <param name="profile">要移除的环境配置</param>
        public void RemoveProfile(QuarkProfile profile)
        {
            if (profile == null)
                return;
                
            if (profiles.Contains(profile))
            {
                profiles.Remove(profile);
                EditorUtility.SetDirty(this);
            }
        }
        
        /// <summary>
        /// 获取环境配置名称列表
        /// </summary>
        /// <returns>环境配置名称数组</returns>
        public string[] GetProfileNames()
        {
            return profiles.Select(p => p.ProfileName).ToArray();
        }
        
        /// <summary>
        /// 设置默认环境配置
        /// </summary>
        public void SetupDefaultProfiles()
        {
            // 确保至少有一个配置
            if (profiles.Count == 0)
            {
                // 创建开发环境配置
                var devProfile = CreateInstance<QuarkProfile>();
                devProfile.name = "Dev Profile";
                devProfile.ProfileName = "Development";
                devProfile.DownloadURL = "http://localhost:8080/quarkassets";
                devProfile.UseAesEncryption = false;
                
                // 创建测试环境配置
                var qaProfile = CreateInstance<QuarkProfile>();
                qaProfile.name = "QA Profile";
                qaProfile.ProfileName = "QA";
                qaProfile.DownloadURL = "http://test-server.com/quarkassets";
                qaProfile.UseAesEncryption = true;
                qaProfile.AesEncryptionKey = "QATestKey123456";
                
                // 创建生产环境配置
                var prodProfile = CreateInstance<QuarkProfile>();
                prodProfile.name = "Prod Profile";
                prodProfile.ProfileName = "Production";
                prodProfile.DownloadURL = "https://cdn.yourserver.com/quarkassets";
                prodProfile.UseAesEncryption = true;
                prodProfile.AesEncryptionKey = "ProdSecretKey789012";
                
                // 将配置保存到assets目录
                if (!Directory.Exists(ConfigFolderPath))
                {
                    Directory.CreateDirectory(ConfigFolderPath);
                }
                
                AssetDatabase.CreateAsset(devProfile, Path.Combine(ConfigFolderPath, "DevProfile.asset"));
                AssetDatabase.CreateAsset(qaProfile, Path.Combine(ConfigFolderPath, "QAProfile.asset"));
                AssetDatabase.CreateAsset(prodProfile, Path.Combine(ConfigFolderPath, "ProdProfile.asset"));
                
                // 添加到配置列表
                profiles.Add(devProfile);
                profiles.Add(qaProfile);
                profiles.Add(prodProfile);
                
                // 默认使用开发环境
                activeProfileIndex = 0;
                
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
            }
        }
        
        /// <summary>
        /// 应用当前配置到指定的构建配置
        /// </summary>
        /// <param name="buildProfile">构建配置</param>
        public void ApplyToQuarkBuildProfile(QuarkBuildProfile buildProfile)
        {
            if (buildProfile == null || ActiveProfile == null)
                return;
                
            var profileData = buildProfile.AssetBundleBuildProfileData;
            
            // 设置版本号
            profileData.BuildVersion = defaultBuildVersion;
            profileData.InternalBuildVersion = defaultInternalBuildVersion;
            
            // 设置下载地址
            // 注意：QuarkAsset可能没有直接在构建配置中设置下载地址的字段
            // 我们可以通过其他方式将这个配置传递给运行时
            
            // 设置加密
            profileData.UseAesEncryptionForManifest = ActiveProfile.UseAesEncryption;
            if (ActiveProfile.UseAesEncryption)
            {
                profileData.AesEncryptionKeyForManifest = ActiveProfile.AesEncryptionKey;
            }
            
            EditorUtility.SetDirty(buildProfile);
        }
        
        /// <summary>
        /// 应用当前配置到所有构建配置
        /// </summary>
        public void ApplyToAllBuildProfiles()
        {
            var guids = AssetDatabase.FindAssets("t:QuarkBuildProfile");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var profile = AssetDatabase.LoadAssetAtPath<QuarkBuildProfile>(path);
                if (profile != null)
                {
                    ApplyToQuarkBuildProfile(profile);
                }
            }
            
            AssetDatabase.SaveAssets();
        }
        
        /// <summary>
        /// 加载或创建配置文件
        /// </summary>
        /// <returns>配置对象</returns>
        private static QuarkGlobalConfig LoadOrCreateConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<QuarkGlobalConfig>(ConfigFilePath);
            
            if (config == null)
            {
                // 创建目录
                if (!Directory.Exists(ConfigFolderPath))
                {
                    Directory.CreateDirectory(ConfigFolderPath);
                }
                
                // 创建新配置
                config = CreateInstance<QuarkGlobalConfig>();
                
                // 设置默认配置
                config.SetupDefaultProfiles();
                
                // 保存配置
                AssetDatabase.CreateAsset(config, ConfigFilePath);
                AssetDatabase.SaveAssets();
            }
            
            return config;
        }
        
        /// <summary>
        /// 获取当前配置的运行时初始化代码
        /// </summary>
        /// <returns>初始化代码</returns>
        public string GetRuntimeInitializationCode()
        {
            if (ActiveProfile == null)
                return string.Empty;
                
            var sb = new System.Text.StringBuilder();
            
            sb.AppendLine("// QuarkAsset Global Configuration");
            sb.AppendLine("// Generated Code - Do Not Modify");
            sb.AppendLine();
            
            sb.AppendLine("using Quark;");
            sb.AppendLine();
            sb.AppendLine("namespace YourNamespace");
            sb.AppendLine("{");
            sb.AppendLine("    public static class QuarkAssetConfig");
            sb.AppendLine("    {");
            sb.AppendLine("        public static void Initialize()");
            sb.AppendLine("        {");
            sb.AppendLine($"            // Active profile: {ActiveProfile.ProfileName}");
            sb.AppendLine($"            QuarkDataProxy.URL = \"{ActiveProfile.DownloadURL}\";");
            
            if (ActiveProfile.UseAesEncryption)
            {
                sb.AppendLine($"            QuarkDataProxy.QuarkAesEncryptionKey = \"{ActiveProfile.AesEncryptionKey}\";");
            }
            
            sb.AppendLine($"            QuarkDataProxy.BuildVersion = \"{defaultBuildVersion}\";");
            sb.AppendLine($"            QuarkDataProxy.InternalBuildVersion = {defaultInternalBuildVersion};");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            
            return sb.ToString();
        }
    }
}
