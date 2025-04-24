using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Quark.Asset;

namespace Quark.Editor
{
    /// <summary>
    /// QuarkAsset构建配置编辑器窗口
    /// </summary>
    public class QuarkBuildConfigWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private QuarkBuildConfig selectedConfig;
        private string newConfigName = "New Build Config";
        
        private List<QuarkBuildConfig> buildConfigs = new List<QuarkBuildConfig>();
        private QuarkDataset selectedDataset;
        private string[] datasetNames;
        private List<QuarkDataset> datasets = new List<QuarkDataset>();
        private int selectedDatasetIndex = -1;
        
        private GUIStyle headerStyle;
        private GUIStyle groupBoxStyle;
        private GUIStyle boldLabelStyle;
        private GUIStyle buttonStyle;
        
        private bool showBuildSettings = true;
        private bool showEncryptionSettings = true;
        private bool showOptimizationSettings = true;
        private bool showExportSettings = true;
        
        private SerializedObject serializedConfig;
        private Dictionary<string, SerializedProperty> configProperties = new Dictionary<string, SerializedProperty>();
        
        // 添加到菜单
        [MenuItem("Quark/Build Configuration", false, 110)]
        public static void ShowWindow()
        {
            var window = GetWindow<QuarkBuildConfigWindow>("Quark Build Config");
            window.minSize = new Vector2(400, 500);
            window.Show();
        }
        
        private void OnEnable()
        {
            // 初始化样式
            InitStyles();
            
            // 加载配置
            LoadBuildConfigs();
            
            // 加载数据集
            LoadDatasets();
        }
        
        private void OnFocus()
        {
            // 重新加载配置
            LoadBuildConfigs();
            // 刷新数据集
            LoadDatasets();
        }
        
        private void InitStyles()
        {
            headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                margin = new RectOffset(5, 5, 10, 5),
                alignment = TextAnchor.MiddleLeft
            };
            
            groupBoxStyle = new GUIStyle(EditorStyles.helpBox)
            {
                margin = new RectOffset(5, 5, 5, 5),
                padding = new RectOffset(10, 10, 10, 10)
            };
            
            boldLabelStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                margin = new RectOffset(0, 0, 5, 5)
            };
            
            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                margin = new RectOffset(5, 5, 5, 5)
            };
        }
        
        private void LoadBuildConfigs()
        {
            buildConfigs = QuarkBuildConfigController.GetAllBuildConfigs();
            
            // 如果当前选择的配置不在列表中，重置选择
            if (selectedConfig != null && !buildConfigs.Contains(selectedConfig))
            {
                selectedConfig = null;
                serializedConfig = null;
                configProperties.Clear();
            }
        }
        
        private void LoadDatasets()
        {
            datasets.Clear();
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
            
            datasetNames = new string[datasets.Count];
            for (int i = 0; i < datasets.Count; i++)
            {
                datasetNames[i] = datasets[i].name;
            }
            
            // 重置选择
            if (selectedDatasetIndex >= datasets.Count || selectedDatasetIndex < 0)
            {
                selectedDatasetIndex = datasets.Count > 0 ? 0 : -1;
                selectedDataset = selectedDatasetIndex >= 0 ? datasets[selectedDatasetIndex] : null;
            }
        }
        
        private void SelectConfig(QuarkBuildConfig config)
        {
            selectedConfig = config;
            
            if (config != null)
            {
                serializedConfig = new SerializedObject(config);
                
                // 缓存属性
                configProperties.Clear();
                configProperties["configDescription"] = serializedConfig.FindProperty("configDescription");
                configProperties["buildTarget"] = serializedConfig.FindProperty("buildTarget");
                configProperties["buildVersion"] = serializedConfig.FindProperty("buildVersion");
                configProperties["internalBuildVersion"] = serializedConfig.FindProperty("internalBuildVersion");
                configProperties["relativeBuildPath"] = serializedConfig.FindProperty("relativeBuildPath");
                configProperties["bundleNameType"] = serializedConfig.FindProperty("bundleNameType");
                configProperties["compressType"] = serializedConfig.FindProperty("compressType");
                configProperties["copyToStreamingAssets"] = serializedConfig.FindProperty("copyToStreamingAssets");
                configProperties["streamingAssetsRelativePath"] = serializedConfig.FindProperty("streamingAssetsRelativePath");
                configProperties["clearStreamingAssetsPath"] = serializedConfig.FindProperty("clearStreamingAssetsPath");
                configProperties["useOffsetEncryption"] = serializedConfig.FindProperty("useOffsetEncryption");
                configProperties["encryptionOffset"] = serializedConfig.FindProperty("encryptionOffset");
                configProperties["useAesEncryption"] = serializedConfig.FindProperty("useAesEncryption");
                configProperties["aesEncryptionKey"] = serializedConfig.FindProperty("aesEncryptionKey");
                configProperties["deterministic"] = serializedConfig.FindProperty("deterministic");
                configProperties["disableWriteTypeTree"] = serializedConfig.FindProperty("disableWriteTypeTree");
                configProperties["forceRebuild"] = serializedConfig.FindProperty("forceRebuild");
                configProperties["ignoreTypeTreeChanges"] = serializedConfig.FindProperty("ignoreTypeTreeChanges");
                configProperties["buildType"] = serializedConfig.FindProperty("buildType");
                configProperties["forceRemoveAllAssetBundleNames"] = serializedConfig.FindProperty("forceRemoveAllAssetBundleNames");
                configProperties["buildHandlerName"] = serializedConfig.FindProperty("buildHandlerName");
                configProperties["useFixedBuildTime"] = serializedConfig.FindProperty("useFixedBuildTime");
                configProperties["fixedBuildTime"] = serializedConfig.FindProperty("fixedBuildTime");
                configProperties["useFixedBuildHash"] = serializedConfig.FindProperty("useFixedBuildHash");
                configProperties["buildHashSeed"] = serializedConfig.FindProperty("buildHashSeed");
            }
            else
            {
                serializedConfig = null;
                configProperties.Clear();
            }
        }
        
        private void OnGUI()
        {
            EditorGUILayout.LabelField("QuarkAsset构建配置", headerStyle);
            
            EditorGUILayout.BeginHorizontal();
            
            // 左侧配置列表
            DrawConfigList();
            
            // 右侧配置编辑
            DrawConfigSettings();
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawConfigList()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(200));
            
            EditorGUILayout.LabelField("配置列表", boldLabelStyle);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
            
            foreach (var config in buildConfigs)
            {
                GUI.backgroundColor = config == selectedConfig ? Color.green : Color.white;
                
                if (GUILayout.Button(config.ConfigDescription, GUILayout.Height(30)))
                {
                    SelectConfig(config);
                }
                
                GUI.backgroundColor = Color.white;
            }
            
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            // 新建配置
            EditorGUILayout.BeginHorizontal();
            newConfigName = EditorGUILayout.TextField(newConfigName);
            
            if (GUILayout.Button("创建", GUILayout.Width(60)))
            {
                var newConfig = QuarkBuildConfigController.CreateBuildConfig(newConfigName);
                LoadBuildConfigs();
                SelectConfig(newConfig);
                newConfigName = "New Build Config";
            }
            EditorGUILayout.EndHorizontal();
            
            // 应用全局设置
            if (GUILayout.Button("应用全局设置到所有配置"))
            {
                if (EditorUtility.DisplayDialog("应用全局设置", 
                    "确定要将全局设置应用到所有构建配置吗？", "确定", "取消"))
                {
                    QuarkBuildConfigController.ApplyGlobalConfigToAllBuildConfigs();
                    LoadBuildConfigs();
                    EditorUtility.DisplayDialog("操作完成", "已将全局设置应用到所有构建配置。", "确定");
                }
            }
            
            // 导入/导出配置
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("导入配置"))
            {
                ImportBuildConfig();
            }
            
            if (GUILayout.Button("导出配置"))
            {
                ExportBuildConfig();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawConfigSettings()
        {
            EditorGUILayout.BeginVertical();
            
            if (selectedConfig == null)
            {
                EditorGUILayout.HelpBox("请选择或创建一个构建配置", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }
            
            if (serializedConfig == null)
            {
                SelectConfig(selectedConfig);
            }
            
            serializedConfig.Update();
            
            var settingsScroll = EditorGUILayout.BeginScrollView(Vector2.zero);
            
            // 基本设置
            showBuildSettings = EditorGUILayout.Foldout(showBuildSettings, "基本设置", true);
            if (showBuildSettings)
            {
                EditorGUILayout.BeginVertical(groupBoxStyle);
                
                EditorGUILayout.PropertyField(configProperties["configDescription"], new GUIContent("配置描述"));
                EditorGUILayout.PropertyField(configProperties["buildTarget"], new GUIContent("构建平台"));
                EditorGUILayout.PropertyField(configProperties["buildVersion"], new GUIContent("构建版本"));
                EditorGUILayout.PropertyField(configProperties["internalBuildVersion"], new GUIContent("内部版本号"));
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("版本号+1"))
                {
                    QuarkBuildConfigController.UpdateConfigVersion(selectedConfig, false);
                    serializedConfig.Update();
                }
                
                if (GUILayout.Button("主版本号+1"))
                {
                    QuarkBuildConfigController.UpdateConfigVersion(selectedConfig, true);
                    serializedConfig.Update();
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.PropertyField(configProperties["relativeBuildPath"], new GUIContent("相对构建路径"));
                EditorGUILayout.PropertyField(configProperties["buildType"], new GUIContent("构建类型"));
                EditorGUILayout.PropertyField(configProperties["bundleNameType"], new GUIContent("名称类型"));
                EditorGUILayout.PropertyField(configProperties["compressType"], new GUIContent("压缩类型"));
                
                EditorGUILayout.EndVertical();
            }
            
            // 加密设置
            showEncryptionSettings = EditorGUILayout.Foldout(showEncryptionSettings, "加密设置", true);
            if (showEncryptionSettings)
            {
                EditorGUILayout.BeginVertical(groupBoxStyle);
                
                EditorGUILayout.PropertyField(configProperties["useOffsetEncryption"], new GUIContent("使用偏移加密"));
                if (configProperties["useOffsetEncryption"].boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(configProperties["encryptionOffset"], new GUIContent("偏移值"));
                    EditorGUI.indentLevel--;
                }
                
                EditorGUILayout.PropertyField(configProperties["useAesEncryption"], new GUIContent("使用AES加密"));
                if (configProperties["useAesEncryption"].boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(configProperties["aesEncryptionKey"], new GUIContent("AES密钥"));
                    EditorGUI.indentLevel--;
                }
                
                EditorGUILayout.Space();
                
                if (GUILayout.Button("应用全局配置的加密设置"))
                {
                    selectedConfig.ApplyFromGlobalConfig();
                    serializedConfig.Update();
                }
                
                EditorGUILayout.EndVertical();
            }
            
            // 优化设置
            showOptimizationSettings = EditorGUILayout.Foldout(showOptimizationSettings, "优化设置", true);
            if (showOptimizationSettings)
            {
                EditorGUILayout.BeginVertical(groupBoxStyle);
                
                EditorGUILayout.PropertyField(configProperties["deterministic"], new GUIContent("确定性构建"));
                EditorGUILayout.PropertyField(configProperties["disableWriteTypeTree"], new GUIContent("禁用类型树写入"));
                EditorGUILayout.PropertyField(configProperties["forceRebuild"], new GUIContent("强制重新构建"));
                EditorGUILayout.PropertyField(configProperties["ignoreTypeTreeChanges"], new GUIContent("忽略类型树变化"));
                EditorGUILayout.PropertyField(configProperties["forceRemoveAllAssetBundleNames"], new GUIContent("强制移除所有AB名称"));
                EditorGUILayout.PropertyField(configProperties["buildHandlerName"], new GUIContent("构建处理器名称"));
                
                EditorGUILayout.Space();
                
                EditorGUILayout.LabelField("跨机器构建一致性设置", boldLabelStyle);
                
                EditorGUILayout.PropertyField(configProperties["useFixedBuildTime"], new GUIContent("使用固定构建时间"));
                if (configProperties["useFixedBuildTime"].boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(configProperties["fixedBuildTime"], new GUIContent("固定时间"));
                    EditorGUI.indentLevel--;
                }
                
                EditorGUILayout.PropertyField(configProperties["useFixedBuildHash"], new GUIContent("使用固定构建哈希"));
                if (configProperties["useFixedBuildHash"].boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(configProperties["buildHashSeed"], new GUIContent("哈希种子"));
                    EditorGUI.indentLevel--;
                }
                
                EditorGUILayout.EndVertical();
            }
            
            // 导出设置
            showExportSettings = EditorGUILayout.Foldout(showExportSettings, "导出设置", true);
            if (showExportSettings)
            {
                EditorGUILayout.BeginVertical(groupBoxStyle);
                
                EditorGUILayout.PropertyField(configProperties["copyToStreamingAssets"], new GUIContent("复制到StreamingAssets"));
                if (configProperties["copyToStreamingAssets"].boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(configProperties["streamingAssetsRelativePath"], new GUIContent("相对路径"));
                    EditorGUILayout.PropertyField(configProperties["clearStreamingAssetsPath"], new GUIContent("清空目标路径"));
                    EditorGUI.indentLevel--;
                }
                
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.EndScrollView();
            
            // 应用修改
            if (serializedConfig.hasModifiedProperties)
            {
                serializedConfig.ApplyModifiedProperties();
            }
            
            EditorGUILayout.Space();
            
            // 构建操作
            EditorGUILayout.BeginVertical(groupBoxStyle);
            
            EditorGUILayout.LabelField("构建操作", boldLabelStyle);
            
            // 数据集选择
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("选择数据集:", GUILayout.Width(100));
            
            if (datasetNames != null && datasetNames.Length > 0)
            {
                int newIndex = EditorGUILayout.Popup(selectedDatasetIndex, datasetNames);
                if (newIndex != selectedDatasetIndex)
                {
                    selectedDatasetIndex = newIndex;
                    selectedDataset = datasets[selectedDatasetIndex];
                }
            }
            else
            {
                EditorGUILayout.LabelField("未找到数据集");
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // 构建按钮
            GUI.enabled = selectedDataset != null;
            if (GUILayout.Button("开始构建", GUILayout.Height(30)))
            {
                StartBuild();
            }
            GUI.enabled = true;
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            // 删除按钮
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("删除配置", GUILayout.Width(100)))
            {
                DeleteConfig();
            }
            GUI.backgroundColor = Color.white;
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        private void StartBuild()
        {
            if (selectedConfig == null || selectedDataset == null)
            {
                EditorUtility.DisplayDialog("构建错误", "请选择构建配置和数据集", "确定");
                return;
            }
            
            // 显示确认对话框
            if (EditorUtility.DisplayDialog("构建确认", 
                $"是否使用 '{selectedConfig.ConfigDescription}' 配置构建 '{selectedDataset.name}' 数据集？", 
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
        
        private void DeleteConfig()
        {
            if (selectedConfig == null)
                return;
                
            if (EditorUtility.DisplayDialog("删除确认", 
                $"确定要删除 '{selectedConfig.ConfigDescription}' 配置吗？此操作不可撤销。", 
                "删除", "取消"))
            {
                string assetPath = AssetDatabase.GetAssetPath(selectedConfig);
                AssetDatabase.DeleteAsset(assetPath);
                
                LoadBuildConfigs();
                selectedConfig = null;
                serializedConfig = null;
                configProperties.Clear();
            }
        }
        
        private void ImportBuildConfig()
        {
            string path = EditorUtility.OpenFilePanel("导入构建配置", "", "json");
            if (string.IsNullOrEmpty(path))
                return;
                
            try
            {
                string json = File.ReadAllText(path);
                var importedConfig = JsonUtility.FromJson<BuildConfigData>(json);
                
                if (importedConfig == null)
                {
                    EditorUtility.DisplayDialog("导入失败", "文件格式无效", "确定");
                    return;
                }
                
                // 创建新配置
                var newConfig = QuarkBuildConfigController.CreateBuildConfig(importedConfig.ConfigDescription);
                
                // 应用导入的数据
                newConfig.ConfigDescription = importedConfig.ConfigDescription;
                newConfig.BuildTarget = importedConfig.BuildTarget;
                newConfig.BuildVersion = importedConfig.BuildVersion;
                newConfig.InternalBuildVersion = importedConfig.InternalBuildVersion;
                newConfig.RelativeBuildPath = importedConfig.RelativeBuildPath;
                newConfig.BundleNameType = importedConfig.BundleNameType;
                newConfig.CompressType = importedConfig.CompressType;
                newConfig.CopyToStreamingAssets = importedConfig.CopyToStreamingAssets;
                newConfig.StreamingAssetsRelativePath = importedConfig.StreamingAssetsRelativePath;
                newConfig.ClearStreamingAssetsPath = importedConfig.ClearStreamingAssetsPath;
                newConfig.UseOffsetEncryption = importedConfig.UseOffsetEncryption;
                newConfig.EncryptionOffset = importedConfig.EncryptionOffset;
                newConfig.UseAesEncryption = importedConfig.UseAesEncryption;
                newConfig.AesEncryptionKey = importedConfig.AesEncryptionKey;
                newConfig.Deterministic = importedConfig.Deterministic;
                newConfig.DisableWriteTypeTree = importedConfig.DisableWriteTypeTree;
                newConfig.ForceRebuild = importedConfig.ForceRebuild;
                newConfig.IgnoreTypeTreeChanges = importedConfig.IgnoreTypeTreeChanges;
                newConfig.BuildType = importedConfig.BuildType;
                newConfig.ForceRemoveAllAssetBundleNames = importedConfig.ForceRemoveAllAssetBundleNames;
                newConfig.BuildHandlerName = importedConfig.BuildHandlerName;
                newConfig.UseFixedBuildTime = importedConfig.UseFixedBuildTime;
                newConfig.FixedBuildTime = importedConfig.FixedBuildTime;
                newConfig.UseFixedBuildHash = importedConfig.UseFixedBuildHash;
                newConfig.BuildHashSeed = importedConfig.BuildHashSeed;
                
                EditorUtility.SetDirty(newConfig);
                AssetDatabase.SaveAssets();
                
                LoadBuildConfigs();
                SelectConfig(newConfig);
                
                EditorUtility.DisplayDialog("导入成功", $"已导入配置 '{importedConfig.ConfigDescription}'", "确定");
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("导入失败", $"导入过程中出现错误:\n{e.Message}", "确定");
            }
        }
        
        private void ExportBuildConfig()
        {
            if (selectedConfig == null)
            {
                EditorUtility.DisplayDialog("导出失败", "请先选择一个配置", "确定");
                return;
            }
            
            string path = EditorUtility.SaveFilePanel("导出构建配置", "", selectedConfig.ConfigDescription, "json");
            if (string.IsNullOrEmpty(path))
                return;
                
            try
            {
                // 创建可序列化的数据对象
                var exportData = new BuildConfigData
                {
                    ConfigDescription = selectedConfig.ConfigDescription,
                    BuildTarget = selectedConfig.BuildTarget,
                    BuildVersion = selectedConfig.BuildVersion,
                    InternalBuildVersion = selectedConfig.InternalBuildVersion,
                    RelativeBuildPath = selectedConfig.RelativeBuildPath,
                    BundleNameType = selectedConfig.BundleNameType,
                    CompressType = selectedConfig.CompressType,
                    CopyToStreamingAssets = selectedConfig.CopyToStreamingAssets,
                    StreamingAssetsRelativePath = selectedConfig.StreamingAssetsRelativePath,
                    ClearStreamingAssetsPath = selectedConfig.ClearStreamingAssetsPath,
                    UseOffsetEncryption = selectedConfig.UseOffsetEncryption,
                    EncryptionOffset = selectedConfig.EncryptionOffset,
                    UseAesEncryption = selectedConfig.UseAesEncryption,
                    AesEncryptionKey = selectedConfig.AesEncryptionKey,
                    Deterministic = selectedConfig.Deterministic,
                    DisableWriteTypeTree = selectedConfig.DisableWriteTypeTree,
                    ForceRebuild = selectedConfig.ForceRebuild,
                    IgnoreTypeTreeChanges = selectedConfig.IgnoreTypeTreeChanges,
                    BuildType = selectedConfig.BuildType,
                    ForceRemoveAllAssetBundleNames = selectedConfig.ForceRemoveAllAssetBundleNames,
                    BuildHandlerName = selectedConfig.BuildHandlerName,
                    UseFixedBuildTime = selectedConfig.UseFixedBuildTime,
                    FixedBuildTime = selectedConfig.FixedBuildTime,
                    UseFixedBuildHash = selectedConfig.UseFixedBuildHash,
                    BuildHashSeed = selectedConfig.BuildHashSeed
                };
                
                string json = JsonUtility.ToJson(exportData, true);
                File.WriteAllText(path, json);
                
                EditorUtility.DisplayDialog("导出成功", $"已导出配置到:\n{path}", "确定");
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("导出失败", $"导出过程中出现错误:\n{e.Message}", "确定");
            }
        }
        
        // 可序列化的配置数据
        [System.Serializable]
        private class BuildConfigData
        {
            public string ConfigDescription;
            public BuildTarget BuildTarget;
            public string BuildVersion;
            public int InternalBuildVersion;
            public string RelativeBuildPath;
            public AssetBundleNameType BundleNameType;
            public AssetBundleCompressType CompressType;
            public bool CopyToStreamingAssets;
            public string StreamingAssetsRelativePath;
            public bool ClearStreamingAssetsPath;
            public bool UseOffsetEncryption;
            public int EncryptionOffset;
            public bool UseAesEncryption;
            public string AesEncryptionKey;
            public bool Deterministic;
            public bool DisableWriteTypeTree;
            public bool ForceRebuild;
            public bool IgnoreTypeTreeChanges;
            public QuarkBuildType BuildType;
            public bool ForceRemoveAllAssetBundleNames;
            public string BuildHandlerName;
            public bool UseFixedBuildTime;
            public string FixedBuildTime;
            public bool UseFixedBuildHash;
            public string BuildHashSeed;
        }
    }
}
