using UnityEngine;
using UnityEditor;

namespace Quark.Editor
{
    /// <summary>
    /// 为QuarkAsset全局配置系统提供编辑器菜单项
    /// </summary>
    public static class QuarkGlobalConfigMenu
    {
        [MenuItem("Assets/Create/Quark/Environment Profile", priority = 200)]
        public static void CreateEnvironmentProfile()
        {
            var asset = ScriptableObject.CreateInstance<QuarkProfile>();
            asset.name = "New Environment Profile";
            asset.ProfileName = "New Environment";

            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(path))
            {
                path = "Assets";
            }
            else if (!AssetDatabase.IsValidFolder(path))
            {
                path = System.IO.Path.GetDirectoryName(path);
            }

            string assetPath = AssetDatabase.GenerateUniqueAssetPath(path + "/New Environment Profile.asset");
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
        
        [MenuItem("Quark/Reset Global Configuration", priority = 101)]
        public static void ResetGlobalConfiguration()
        {
            if (EditorUtility.DisplayDialog("重置全局配置", 
                "这将重置所有QuarkAsset全局配置设置。确定要继续吗？", "确定", "取消"))
            {
                // 获取现有配置的路径
                string configPath = "Assets/QuarkAsset/Editor/GlobalConfig/QuarkGlobalConfig.asset";
                
                // 删除现有配置
                if (AssetDatabase.LoadAssetAtPath<QuarkGlobalConfig>(configPath) != null)
                {
                    AssetDatabase.DeleteAsset(configPath);
                }
                
                // 创建新配置 - LoadOrCreateConfig方法会自动创建
                var newConfig = QuarkGlobalConfig.Instance;
                
                // 确保目录存在
                if (!System.IO.Directory.Exists("Assets/QuarkAsset/Editor/GlobalConfig"))
                {
                    System.IO.Directory.CreateDirectory("Assets/QuarkAsset/Editor/GlobalConfig");
                }
                
                // 保存
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                // 显示提示
                EditorUtility.DisplayDialog("重置成功", "QuarkAsset全局配置已重置。", "确定");
                
                // 打开配置窗口
                QuarkGlobalConfigWindow.ShowWindow();
            }
        }
        
        [MenuItem("Quark/Apply Active Profile", priority = 102)]
        public static void ApplyActiveProfile()
        {
            var config = QuarkGlobalConfig.Instance;
            
            if (config.ActiveProfile == null)
            {
                EditorUtility.DisplayDialog("错误", "没有激活的环境配置。请先在全局配置窗口中设置。", "确定");
                return;
            }
            
            if (EditorUtility.DisplayDialog("应用当前环境", 
                $"这将应用当前环境 ({config.ActiveProfile.ProfileName}) 的设置到所有构建配置。确定要继续吗？", 
                "确定", "取消"))
            {
                config.ApplyToAllBuildProfiles();
                EditorUtility.DisplayDialog("应用成功", $"已将环境 {config.ActiveProfile.ProfileName} 应用到所有构建配置。", "确定");
            }
        }
        
        [MenuItem("Quark/Generate Runtime Initialization Code", priority = 103)]
        public static void GenerateRuntimeCode()
        {
            var config = QuarkGlobalConfig.Instance;
            
            if (config.ActiveProfile == null)
            {
                EditorUtility.DisplayDialog("错误", "没有激活的环境配置。请先在全局配置窗口中设置。", "确定");
                return;
            }
            
            string code = config.GetRuntimeInitializationCode();
            
            string path = EditorUtility.SaveFilePanel(
                "保存初始化代码",
                Application.dataPath,
                "QuarkAssetConfig.cs",
                "cs");
                
            if (!string.IsNullOrEmpty(path))
            {
                System.IO.File.WriteAllText(path, code);
                EditorUtility.DisplayDialog("保存成功", $"代码已保存到: {path}", "确定");
                
                // 如果保存到项目内，则刷新资源
                if (path.StartsWith(Application.dataPath))
                {
                    AssetDatabase.Refresh();
                }
            }
        }
        
        [MenuItem("Quark/Generate Profile Class", priority = 104)]
        public static void GenerateProfileClass()
        {
            var config = QuarkGlobalConfig.Instance;
            
            if (config.ActiveProfile == null)
            {
                EditorUtility.DisplayDialog("错误", "没有激活的环境配置。请先在全局配置窗口中设置。", "确定");
                return;
            }
            
            // 创建配置类代码
            var profile = config.ActiveProfile;
            var sb = new System.Text.StringBuilder();
            
            sb.AppendLine("// QuarkAsset Profile Configuration");
            sb.AppendLine("// Generated Code - Do Not Modify");
            sb.AppendLine();
            
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine();
            sb.AppendLine("namespace Quark");
            sb.AppendLine("{");
            sb.AppendLine($"    /// <summary>");
            sb.AppendLine($"    /// {profile.ProfileName} 环境配置");
            sb.AppendLine($"    /// </summary>");
            sb.AppendLine($"    [CreateAssetMenu(fileName = \"{profile.ProfileName}Config\", menuName = \"Quark/Runtime/{profile.ProfileName} Configuration\")]");
            sb.AppendLine($"    public class Quark{profile.ProfileName.Replace(" ", "")}Config : ScriptableObject");
            sb.AppendLine("    {");
            sb.AppendLine($"        [SerializeField]");
            sb.AppendLine($"        private string downloadURL = \"{profile.DownloadURL}\";");
            sb.AppendLine();
            sb.AppendLine($"        [SerializeField]");
            sb.AppendLine($"        private string buildVersion = \"{config.DefaultBuildVersion}\";");
            sb.AppendLine();
            sb.AppendLine($"        [SerializeField]");
            sb.AppendLine($"        private int internalBuildVersion = {config.DefaultInternalBuildVersion};");
            sb.AppendLine();
            
            if (profile.UseAesEncryption)
            {
                sb.AppendLine($"        [SerializeField]");
                sb.AppendLine($"        private string aesEncryptionKey = \"{profile.AesEncryptionKey}\";");
                sb.AppendLine();
            }
            
            if (profile.UseOffsetEncryption)
            {
                sb.AppendLine($"        [SerializeField]");
                sb.AppendLine($"        private int offsetEncryptionValue = {profile.OffsetEncryptionValue};");
                sb.AppendLine();
            }
            
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// 资源下载地址");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public string DownloadURL => downloadURL;");
            sb.AppendLine();
            
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// 构建版本号");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public string BuildVersion => buildVersion;");
            sb.AppendLine();
            
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// 内部构建版本号");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public int InternalBuildVersion => internalBuildVersion;");
            sb.AppendLine();
            
            if (profile.UseAesEncryption)
            {
                sb.AppendLine("        /// <summary>");
                sb.AppendLine("        /// AES加密密钥");
                sb.AppendLine("        /// </summary>");
                sb.AppendLine("        public string AesEncryptionKey => aesEncryptionKey;");
                sb.AppendLine();
            }
            
            if (profile.UseOffsetEncryption)
            {
                sb.AppendLine("        /// <summary>");
                sb.AppendLine("        /// 偏移加密值");
                sb.AppendLine("        /// </summary>");
                sb.AppendLine("        public int OffsetEncryptionValue => offsetEncryptionValue;");
                sb.AppendLine();
            }
            
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// 应用配置到QuarkAsset");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public void Apply()");
            sb.AppendLine("        {");
            sb.AppendLine("            QuarkDataProxy.URL = downloadURL;");
            sb.AppendLine("            QuarkDataProxy.BuildVersion = buildVersion;");
            sb.AppendLine("            QuarkDataProxy.InternalBuildVersion = internalBuildVersion;");
            
            if (profile.UseAesEncryption)
            {
                sb.AppendLine("            QuarkDataProxy.QuarkAesEncryptionKey = aesEncryptionKey;");
            }
            
            if (profile.UseOffsetEncryption)
            {
                sb.AppendLine("            QuarkDataProxy.QuarkEncryptionOffset = (ulong)offsetEncryptionValue;");
            }
            
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            
            string code = sb.ToString();
            
            string path = EditorUtility.SaveFilePanel(
                "保存配置类",
                Application.dataPath,
                $"Quark{profile.ProfileName.Replace(" ", "")}Config.cs",
                "cs");
                
            if (!string.IsNullOrEmpty(path))
            {
                System.IO.File.WriteAllText(path, code);
                EditorUtility.DisplayDialog("保存成功", $"配置类已保存到: {path}", "确定");
                
                // 如果保存到项目内，则刷新资源
                if (path.StartsWith(Application.dataPath))
                {
                    AssetDatabase.Refresh();
                }
            }
        }
        
        [MenuItem("Quark/Profile Previewer", priority = 200)]
        public static void OpenProfilePreviewer()
        {
            QuarkProfilePreviewWindow.ShowWindow();
        }
    }
}
