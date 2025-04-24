using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;

namespace Quark.Editor
{
    /// <summary>
    /// QuarkAsset全局配置编辑器窗口
    /// </summary>
    public class QuarkGlobalConfigWindow : EditorWindow
    {
        private QuarkGlobalConfig config;
        private Vector2 scrollPosition;
        private bool showVersionSettings = true;
        private bool showProfileSettings = true;
        private bool showBuildProfileSettings = true;
        private bool showCodeGenerationSettings = false;
        
        private string newProfileName = "New Profile";
        private string codePreview = "";
        private GUIStyle headerStyle;
        private GUIStyle boxStyle;
        private GUIStyle previewStyle;
        private GUIStyle boldLabelStyle;
        
        // 用于编辑已有Profile的临时数据
        private Dictionary<string, bool> profileFoldouts = new Dictionary<string, bool>();

        [MenuItem("Quark/Global Configuration", false, 100)]
        public static void ShowWindow()
        {
            var window = GetWindow<QuarkGlobalConfigWindow>("Quark Global Config");
            window.minSize = new Vector2(400, 500);
            window.Show();
        }

        private void OnEnable()
        {
            LoadConfig();
            InitializeStyles();
        }

        private void LoadConfig()
        {
            config = QuarkGlobalConfig.Instance;
        }

        private void InitializeStyles()
        {
            headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                margin = new RectOffset(5, 5, 10, 5)
            };
            
            boxStyle = new GUIStyle(EditorStyles.helpBox)
            {
                margin = new RectOffset(5, 5, 5, 5),
                padding = new RectOffset(10, 10, 10, 10)
            };
            
            previewStyle = new GUIStyle(EditorStyles.textArea)
            {
                font = EditorStyles.standardFont,
                wordWrap = true
            };
            
            boldLabelStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                margin = new RectOffset(0, 0, 5, 5)
            };
        }

        private void OnGUI()
        {
            if (config == null)
            {
                LoadConfig();
                if (config == null)
                {
                    EditorGUILayout.HelpBox("无法加载配置。", MessageType.Error);
                    return;
                }
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            // 标题
            EditorGUILayout.LabelField("QuarkAsset 全局配置", headerStyle);
            EditorGUILayout.Space();
            
            // 版本设置
            DrawVersionSettings();
            
            // 环境配置设置
            DrawProfileSettings();
            
            // 构建配置设置
            DrawBuildProfileSettings();
            
            // 代码生成设置
            DrawCodeGenerationSettings();
            
            EditorGUILayout.EndScrollView();
        }

        private void DrawVersionSettings()
        {
            showVersionSettings = EditorGUILayout.Foldout(showVersionSettings, "全局版本设置", true);
            if (showVersionSettings)
            {
                EditorGUILayout.BeginVertical(boxStyle);
                
                EditorGUI.BeginChangeCheck();
                
                var newBuildVersion = EditorGUILayout.TextField("构建版本号", config.DefaultBuildVersion);
                var newInternalVersion = EditorGUILayout.IntField("内部版本号", config.DefaultInternalBuildVersion);
                
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(config, "修改版本号");
                    config.DefaultBuildVersion = newBuildVersion;
                    config.DefaultInternalBuildVersion = newInternalVersion;
                    EditorUtility.SetDirty(config);
                }
                
                EditorGUILayout.Space();
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("版本号+1"))
                {
                    Undo.RecordObject(config, "版本号+1");
                    config.DefaultInternalBuildVersion++;
                    EditorUtility.SetDirty(config);
                }
                
                if (GUILayout.Button("主版本号+1"))
                {
                    Undo.RecordObject(config, "主版本号+1");
                    // 简单地把第一个数字+1，不是标准的语义版本控制，但足够此处使用
                    var parts = config.DefaultBuildVersion.Split('.');
                    if (parts.Length > 0 && int.TryParse(parts[0], out int majorVersion))
                    {
                        parts[0] = (majorVersion + 1).ToString();
                        config.DefaultBuildVersion = string.Join(".", parts);
                        EditorUtility.SetDirty(config);
                    }
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawProfileSettings()
        {
            showProfileSettings = EditorGUILayout.Foldout(showProfileSettings, "环境配置", true);
            if (showProfileSettings)
            {
                EditorGUILayout.BeginVertical(boxStyle);
                
                // 当前激活的配置
                var profileNames = config.GetProfileNames();
                if (profileNames.Length > 0)
                {
                    EditorGUI.BeginChangeCheck();
                    var newIndex = EditorGUILayout.Popup("当前环境", config.ActiveProfileIndex, profileNames);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(config, "切换环境");
                        config.ActiveProfileIndex = newIndex;
                        EditorUtility.SetDirty(config);
                    }
                    
                    if (config.ActiveProfile != null)
                    {
                        EditorGUILayout.HelpBox($"当前环境: {config.ActiveProfile.GetDescription()}", MessageType.Info);
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("尚未创建任何环境配置。", MessageType.Warning);
                }
                
                EditorGUILayout.Space();
                
                // 所有环境配置列表
                foreach (var profile in config.Profiles)
                {
                    if (profile == null)
                        continue;
                        
                    string key = profile.GetInstanceID().ToString();
                    if (!profileFoldouts.ContainsKey(key))
                    {
                        profileFoldouts[key] = false;
                    }
                    
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    
                    EditorGUILayout.BeginHorizontal();
                    profileFoldouts[key] = EditorGUILayout.Foldout(profileFoldouts[key], profile.ProfileName, true);
                    
                    GUI.backgroundColor = profile == config.ActiveProfile ? Color.green : Color.white;
                    if (GUILayout.Button("设为当前", GUILayout.Width(80)))
                    {
                        Undo.RecordObject(config, "设置当前环境");
                        config.ActiveProfileIndex = config.Profiles.IndexOf(profile);
                        EditorUtility.SetDirty(config);
                    }
                    GUI.backgroundColor = Color.white;
                    
                    if (GUILayout.Button("复制", GUILayout.Width(60)))
                    {
                        DuplicateProfile(profile);
                    }
                    
                    if (GUILayout.Button("删除", GUILayout.Width(60)))
                    {
                        if (EditorUtility.DisplayDialog("删除环境配置", 
                            $"确定要删除环境配置 \"{profile.ProfileName}\" 吗?", "删除", "取消"))
                        {
                            DeleteProfile(profile);
                            GUIUtility.ExitGUI();
                            return;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    
                    if (profileFoldouts[key])
                    {
                        EditorGUI.indentLevel++;
                        EditorGUI.BeginChangeCheck();
                        
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        
                        // 基本信息
                        var newProfileName = EditorGUILayout.TextField("环境名称", profile.ProfileName);
                        var newDownloadURL = EditorGUILayout.TextField("下载地址", profile.DownloadURL);
                        var newDescription = EditorGUILayout.TextField("描述", profile.CustomDescription);
                        
                        EditorGUILayout.Space();
                        
                        // AES加密设置
                        var newUseAesEncryption = EditorGUILayout.Toggle("使用AES加密", profile.UseAesEncryption);
                        EditorGUI.BeginDisabledGroup(!newUseAesEncryption);
                        var newAesKey = EditorGUILayout.TextField("AES密钥", profile.AesEncryptionKey);
                        EditorGUI.EndDisabledGroup();
                        
                        EditorGUILayout.Space();
                        
                        // 偏移加密设置
                        var newUseOffsetEncryption = EditorGUILayout.Toggle("使用偏移加密", profile.UseOffsetEncryption);
                        EditorGUI.BeginDisabledGroup(!newUseOffsetEncryption);
                        var newOffsetValue = EditorGUILayout.IntField("偏移值", profile.OffsetEncryptionValue);
                        EditorGUI.EndDisabledGroup();
                        
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(profile, "修改环境配置");
                            profile.ProfileName = newProfileName;
                            profile.DownloadURL = newDownloadURL;
                            profile.CustomDescription = newDescription;
                            profile.UseAesEncryption = newUseAesEncryption;
                            profile.AesEncryptionKey = newAesKey;
                            profile.UseOffsetEncryption = newUseOffsetEncryption;
                            profile.OffsetEncryptionValue = newOffsetValue;
                            EditorUtility.SetDirty(profile);
                        }
                        
                        EditorGUILayout.EndVertical();
                        EditorGUI.indentLevel--;
                    }
                    
                    EditorGUILayout.EndVertical();
                }
                
                EditorGUILayout.Space();
                
                // 新建环境
                EditorGUILayout.BeginHorizontal();
                newProfileName = EditorGUILayout.TextField(newProfileName);
                if (GUILayout.Button("新建环境", GUILayout.Width(100)))
                {
                    CreateNewProfile();
                }
                EditorGUILayout.EndHorizontal();
                
                if (GUILayout.Button("重新设置默认环境"))
                {
                    if (EditorUtility.DisplayDialog("重设环境", 
                        "这将删除所有现有环境，并创建默认的开发、测试和生产环境配置。确定要继续吗?", 
                        "确定", "取消"))
                    {
                        ResetDefaultProfiles();
                    }
                }
                
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawBuildProfileSettings()
        {
            showBuildProfileSettings = EditorGUILayout.Foldout(showBuildProfileSettings, "构建配置设置", true);
            if (showBuildProfileSettings)
            {
                EditorGUILayout.BeginVertical(boxStyle);
                
                EditorGUILayout.LabelField("应用当前环境配置到构建配置", boldLabelStyle);
                EditorGUILayout.HelpBox("这将把当前环境的下载地址、加密等设置应用到构建配置中。", MessageType.Info);
                
                if (GUILayout.Button("应用到所有构建配置"))
                {
                    if (EditorUtility.DisplayDialog("应用配置", 
                        "这将把当前环境配置应用到所有QuarkBuildProfile。确定要继续吗?", 
                        "确定", "取消"))
                    {
                        config.ApplyToAllBuildProfiles();
                        EditorUtility.DisplayDialog("应用成功", "已将当前环境配置应用到所有构建配置。", "确定");
                    }
                }
                
                EditorGUILayout.Space();
                
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("关联当前打开的构建配置", GUILayout.Width(200)))
                {
                    var activeObject = Selection.activeObject as QuarkBuildProfile;
                    if (activeObject != null)
                    {
                        Undo.RecordObject(activeObject, "应用环境配置");
                        config.ApplyToQuarkBuildProfile(activeObject);
                        EditorUtility.DisplayDialog("应用成功", $"已将当前环境配置应用到 {activeObject.name}。", "确定");
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("错误", "请先在Project窗口中选择一个QuarkBuildProfile资源。", "确定");
                    }
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawCodeGenerationSettings()
        {
            showCodeGenerationSettings = EditorGUILayout.Foldout(showCodeGenerationSettings, "代码生成", true);
            if (showCodeGenerationSettings)
            {
                EditorGUILayout.BeginVertical(boxStyle);
                
                EditorGUILayout.LabelField("生成运行时初始化代码", boldLabelStyle);
                EditorGUILayout.HelpBox("生成用于初始化QuarkAsset的代码，包含当前环境的下载地址和加密设置。", MessageType.Info);
                
                if (GUILayout.Button("预览代码"))
                {
                    codePreview = config.GetRuntimeInitializationCode();
                }
                
                if (!string.IsNullOrEmpty(codePreview))
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("代码预览:", boldLabelStyle);
                    
                    float height = Mathf.Max(100, EditorGUIUtility.singleLineHeight * 20);
                    codePreview = EditorGUILayout.TextArea(codePreview, previewStyle, GUILayout.Height(height));
                    
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("复制到剪贴板"))
                    {
                        EditorGUIUtility.systemCopyBuffer = codePreview;
                        EditorUtility.DisplayDialog("复制成功", "代码已复制到剪贴板。", "确定");
                    }
                    
                    if (GUILayout.Button("保存到文件"))
                    {
                        SaveCodeToFile();
                    }
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.EndVertical();
            }
        }

        private void CreateNewProfile()
        {
            var profile = CreateInstance<QuarkProfile>();
            profile.name = newProfileName;
            profile.ProfileName = newProfileName;
            
            // 创建资源文件
            if (!Directory.Exists("Assets/QuarkAsset/Editor/GlobalConfig"))
            {
                Directory.CreateDirectory("Assets/QuarkAsset/Editor/GlobalConfig");
            }
            
            string assetPath = AssetDatabase.GenerateUniqueAssetPath(
                $"Assets/QuarkAsset/Editor/GlobalConfig/{newProfileName}.asset");
            
            AssetDatabase.CreateAsset(profile, assetPath);
            
            // 添加到配置
            Undo.RecordObject(config, "添加环境配置");
            config.AddProfile(profile);
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            
            // 重置新配置名称
            newProfileName = "New Profile";
        }

        private void DuplicateProfile(QuarkProfile source)
        {
            if (source == null)
                return;
                
            var clone = source.Clone();
            clone.name = source.name + " (Copy)";
            
            // 创建资源文件
            string assetPath = AssetDatabase.GenerateUniqueAssetPath(
                $"Assets/QuarkAsset/Editor/GlobalConfig/{clone.ProfileName}.asset");
            
            AssetDatabase.CreateAsset(clone, assetPath);
            
            // 添加到配置
            Undo.RecordObject(config, "复制环境配置");
            config.AddProfile(clone);
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
        }

        private void DeleteProfile(QuarkProfile profile)
        {
            if (profile == null)
                return;
                
            // 从配置中移除
            Undo.RecordObject(config, "删除环境配置");
            int index = config.Profiles.IndexOf(profile);
            config.RemoveProfile(profile);
            
            // 调整当前索引
            if (config.ActiveProfileIndex >= config.Profiles.Count)
            {
                config.ActiveProfileIndex = Mathf.Max(0, config.Profiles.Count - 1);
            }
            
            EditorUtility.SetDirty(config);
            
            // 从资源中删除
            string assetPath = AssetDatabase.GetAssetPath(profile);
            if (!string.IsNullOrEmpty(assetPath))
            {
                AssetDatabase.DeleteAsset(assetPath);
            }
            
            AssetDatabase.SaveAssets();
        }

        private void ResetDefaultProfiles()
        {
            // 删除所有现有配置
            var profiles = new List<QuarkProfile>(config.Profiles);
            foreach (var profile in profiles)
            {
                string assetPath = AssetDatabase.GetAssetPath(profile);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    AssetDatabase.DeleteAsset(assetPath);
                }
            }
            
            // 清空配置列表
            Undo.RecordObject(config, "重设默认环境");
            config.Profiles.Clear();
            
            // 创建默认配置
            config.SetupDefaultProfiles();
            
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
        }

        private void SaveCodeToFile()
        {
            string path = EditorUtility.SaveFilePanel(
                "保存初始化代码",
                Application.dataPath,
                "QuarkAssetConfig.cs",
                "cs");
                
            if (!string.IsNullOrEmpty(path))
            {
                File.WriteAllText(path, codePreview);
                EditorUtility.DisplayDialog("保存成功", $"代码已保存到: {path}", "确定");
                
                // 如果保存到项目内，则刷新资源
                if (path.StartsWith(Application.dataPath))
                {
                    AssetDatabase.Refresh();
                }
            }
        }
    }
}
