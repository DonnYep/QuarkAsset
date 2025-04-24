using System.IO;
using UnityEngine;
using UnityEditor;
using Quark.Asset;

namespace Quark.Editor
{
    /// <summary>
    /// QuarkAsset编辑器菜单系统
    /// 统一管理所有Quark相关的菜单项和菜单结构
    /// </summary>
    public static class QuarkEditorMenu
    {
        // 主菜单路径常量
        private const string MENU_ROOT = "Quark";
        
        // 子菜单分类常量
        private const string MENU_WINDOW = MENU_ROOT + "/Windows";
        private const string MENU_CONFIG = MENU_ROOT + "/Configuration";
        private const string MENU_BUILD = MENU_ROOT + "/Build";
        private const string MENU_TOOLS = MENU_ROOT + "/Tools";
        private const string MENU_HELP = MENU_ROOT + "/Help";
        
        // 菜单优先级常量
        private const int PRIORITY_WINDOWS = 0;
        private const int PRIORITY_CONFIG = 100;
        private const int PRIORITY_BUILD = 200;
        private const int PRIORITY_TOOLS = 300;
        private const int PRIORITY_HELP = 1000;
        
        #region 主窗口菜单项
        
        [MenuItem(MENU_WINDOW + "/Asset Manager", false, PRIORITY_WINDOWS)]
        public static void OpenAssetWindow()
        {
            EditorWindow.GetWindow<QuarkAssetWindow>("Quark Asset Manager");
        }
        
        [MenuItem(MENU_WINDOW + "/Version Control", false, PRIORITY_WINDOWS + 1)]
        public static void OpenVersionWindow()
        {
            EditorWindow.GetWindow<QuarkVersionWindow>("Quark Version Control");
        }
        
        [MenuItem(MENU_WINDOW + "/Profile Previewer", false, PRIORITY_WINDOWS + 2)]
        public static void OpenProfilePreviewWindow()
        {
            EditorWindow.GetWindow<QuarkProfilePreviewWindow>("Quark Profile Previewer");
        }
        #endregion
        
        #region 配置菜单项
        
        [MenuItem(MENU_CONFIG + "/Global Settings", false, PRIORITY_CONFIG)]
        public static void OpenGlobalConfigWindow()
        {
            EditorWindow.GetWindow<QuarkGlobalConfigWindow>("Quark Global Settings");
        }
        
        [MenuItem(MENU_CONFIG + "/Build Profiles", false, PRIORITY_CONFIG + 1)]
        public static void OpenBuildConfigWindow()
        {
            EditorWindow.GetWindow<QuarkBuildConfigWindow>("Quark Build Profiles");
        }
        
        [MenuItem(MENU_CONFIG + "/Apply Current Environment", false, PRIORITY_CONFIG + 50)]
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
        
        [MenuItem(MENU_CONFIG + "/Reset Global Configuration", false, PRIORITY_CONFIG + 51)]
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
                
                // 创建新配置
                var newConfig = QuarkGlobalConfig.Instance;
                
                // 确保目录存在
                if (!Directory.Exists("Assets/QuarkAsset/Editor/GlobalConfig"))
                {
                    Directory.CreateDirectory("Assets/QuarkAsset/Editor/GlobalConfig");
                }
                
                // 保存
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                // 显示提示
                EditorUtility.DisplayDialog("重置成功", "QuarkAsset全局配置已重置。", "确定");
                
                // 打开配置窗口
                OpenGlobalConfigWindow();
            }
        }
        #endregion
        
        #region 构建菜单项
        
        [MenuItem(MENU_BUILD + "/Quick Build/Full Build", false, PRIORITY_BUILD)]
        public static void QuickFullBuild()
        {
            BuildWithType(QuarkBuildType.Full);
        }
        
        [MenuItem(MENU_BUILD + "/Quick Build/Incremental Build", false, PRIORITY_BUILD + 1)]
        public static void QuickIncrementalBuild()
        {
            BuildWithType(QuarkBuildType.Incremental);
        }
        
        [MenuItem(MENU_BUILD + "/Open Build Configuration", false, PRIORITY_BUILD + 50)]
        public static void OpenBuildConfiguration()
        {
            OpenBuildConfigWindow();
        }
        
        [MenuItem(MENU_BUILD + "/Create New Build Profile", false, PRIORITY_BUILD + 51)]
        public static void CreateNewBuildProfile()
        {
            var newConfig = QuarkBuildConfigController.CreateBuildConfig("NewBuildProfile");
            EditorUtility.DisplayDialog("创建成功", "已创建新的构建配置，请在构建配置窗口中编辑它。", "确定");
            
            Selection.activeObject = newConfig;
            EditorGUIUtility.PingObject(newConfig);
            OpenBuildConfigWindow();
        }
        
        [MenuItem(MENU_BUILD + "/Clean Build Output", false, PRIORITY_BUILD + 100)]
        public static void CleanBuildOutput()
        {
            if (EditorUtility.DisplayDialog("清理构建输出", 
                "这将删除所有QuarkAsset构建输出。确定要继续吗？", "确定", "取消"))
            {
                string buildPath = Path.Combine(QuarkEditorUtility.ApplicationPath, "QuarkBuild");
                
                if (Directory.Exists(buildPath))
                {
                    try
                    {
                        Directory.Delete(buildPath, true);
                        AssetDatabase.Refresh();
                        EditorUtility.DisplayDialog("清理成功", "已清理所有构建输出。", "确定");
                    }
                    catch (System.Exception e)
                    {
                        EditorUtility.DisplayDialog("清理失败", $"清理过程中出现错误:\n{e.Message}", "确定");
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("清理完成", "没有找到构建输出目录，无需清理。", "确定");
                }
            }
        }
        #endregion
        
        #region 工具菜单项
        
        [MenuItem(MENU_TOOLS + "/Generate Runtime Config", false, PRIORITY_TOOLS)]
        public static void GenerateRuntimeConfigCode()
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
                File.WriteAllText(path, code);
                EditorUtility.DisplayDialog("保存成功", $"代码已保存到: {path}", "确定");
                
                // 如果保存到项目内，则刷新资源
                if (path.StartsWith(Application.dataPath))
                {
                    AssetDatabase.Refresh();
                }
            }
        }
        
        [MenuItem(MENU_TOOLS + "/Generate Profile Class", false, PRIORITY_TOOLS + 1)]
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
                File.WriteAllText(path, code);
                EditorUtility.DisplayDialog("保存成功", $"配置类已保存到: {path}", "确定");
                
                // 如果保存到项目内，则刷新资源
                if (path.StartsWith(Application.dataPath))
                {
                    AssetDatabase.Refresh();
                }
            }
        }
        
        [MenuItem(MENU_TOOLS + "/Create Assets", false, PRIORITY_TOOLS + 50)]
        public static void ShowCreateMenu()
        {
            GenericMenu menu = new GenericMenu();
            
            menu.AddItem(new GUIContent("Environment Profile"), false, CreateEnvironmentProfile);
            menu.AddItem(new GUIContent("Build Profile"), false, CreateNewBuildProfile);
            
            menu.ShowAsContext();
        }
        
        private static void CreateEnvironmentProfile()
        {
            var asset = ScriptableObject.CreateInstance<QuarkProfile>();
            asset.name = "New Environment Profile";
            asset.ProfileName = "New Environment";

            string path = "Assets/QuarkAsset/Editor/GlobalConfig";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string assetPath = AssetDatabase.GenerateUniqueAssetPath(path + "/New Environment Profile.asset");
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }
        #endregion
        
        #region 帮助菜单项
        
        [MenuItem(MENU_HELP + "/Documentation", false, PRIORITY_HELP)]
        public static void OpenDocumentation()
        {
            Application.OpenURL("https://github.com/DonnYep/QuarkAsset");
        }
        
        [MenuItem(MENU_HELP + "/About QuarkAsset", false, PRIORITY_HELP + 1)]
        public static void ShowAboutDialog()
        {
            string version = "1.0.0"; // 您可以根据实际情况设置版本号
            string message = $"QuarkAsset 版本 {version}\n\n" + 
                             "一个强大的Unity资源管理系统\n" +
                             "提供完整的热更新、多环境配置和灵活的构建功能\n\n" +
                             "© 2023 QuarkAsset Team";
                             
            EditorUtility.DisplayDialog("关于 QuarkAsset", message, "确定");
        }
        #endregion
        
        #region 辅助方法
        
        private static void BuildWithType(QuarkBuildType buildType)
        {
            // 查找默认构建配置
            var configs = QuarkBuildConfigController.GetAllBuildConfigs();
            if (configs.Count == 0)
            {
                if (EditorUtility.DisplayDialog("未找到构建配置", 
                    "没有找到任何构建配置，是否创建一个新的构建配置？", "创建", "取消"))
                {
                    var newConfig = QuarkBuildConfigController.CreateBuildConfig("DefaultBuildConfig");
                    configs.Add(newConfig);
                }
                else
                {
                    return;
                }
            }
            
            var selectedConfig = configs[0];
            selectedConfig.BuildType = buildType;
            EditorUtility.SetDirty(selectedConfig);
            
            // 查找默认数据集
            var datasets = new List<QuarkDataset>();
            var guids = AssetDatabase.FindAssets("t:QuarkDataset");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var dataset = AssetDatabase.LoadAssetAtPath<QuarkDataset>(path);
                if (dataset != null)
                {
                    datasets.Add(dataset);
                }
            }
            
            if (datasets.Count == 0)
            {
                EditorUtility.DisplayDialog("构建错误", "未找到任何QuarkDataset资源，请先创建数据集。", "确定");
                return;
            }
            
            // 找到第一个数据集
            var selectedDataset = datasets[0];
            
            // 显示确认对话框
            if (EditorUtility.DisplayDialog("快速构建确认", 
                $"是否使用 '{selectedConfig.ConfigDescription}' 配置进行{(buildType == QuarkBuildType.Full ? "全量" : "增量")}构建？\n\n" +
                $"数据集: {selectedDataset.name}\n" +
                $"版本: {selectedConfig.BuildVersion}_{selectedConfig.InternalBuildVersion}", 
                "开始构建", "取消"))
            {
                // 显示构建进度
                EditorUtility.DisplayProgressBar("QuarkAsset构建", "正在准备构建...", 0.1f);
                
                try
                {
                    // 开始构建
                    bool success = QuarkBuildConfigController.BuildWithConfig(selectedConfig, selectedDataset);
                    
                    EditorUtility.ClearProgressBar();
                    
                    if (success)
                    {
                        EditorUtility.DisplayDialog("构建成功", 
                            $"已成功构建 '{selectedDataset.name}' 资源\n版本: {selectedConfig.BuildVersion}_{selectedConfig.InternalBuildVersion}", 
                            "确定");
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("构建失败", 
                            "构建过程中出现错误，请检查控制台日志获取详细信息", 
                            "确定");
                    }
                }
                catch (System.Exception e)
                {
                    EditorUtility.ClearProgressBar();
                    EditorUtility.DisplayDialog("构建失败", 
                        $"构建过程中出现异常:\n{e.Message}", 
                        "确定");
                }
            }
        }
        #endregion
    }
}
