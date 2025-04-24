using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Quark.Editor
{
    /// <summary>
    /// QuarkAsset环境配置预览窗口
    /// 用于显示当前激活的环境配置，并预览在运行时的效果
    /// </summary>
    public class QuarkProfilePreviewWindow : EditorWindow
    {
        private QuarkGlobalConfig config;
        private QuarkProfile activeProfile;
        private Vector2 scrollPosition;
        private bool showSettings = true;
        private bool showPreview = true;
        
        private GUIStyle headerStyle;
        private GUIStyle boxStyle;
        private GUIStyle labelStyle;
        private GUIStyle valueStyle;
        private GUIStyle titleStyle;
        
        private Dictionary<string, bool> foldouts = new Dictionary<string, bool>();

        [MenuItem("Quark/Profile Previewer", false, 200)]
        public static void ShowWindow()
        {
            var window = GetWindow<QuarkProfilePreviewWindow>("Quark Profile Previewer");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        private void OnEnable()
        {
            LoadConfig();
            InitializeStyles();
        }
        
        private void OnFocus()
        {
            // 窗口获得焦点时刷新配置，确保数据最新
            LoadConfig();
        }

        private void LoadConfig()
        {
            config = QuarkGlobalConfig.Instance;
            activeProfile = config?.ActiveProfile;
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
            
            labelStyle = new GUIStyle(EditorStyles.label)
            {
                fontStyle = FontStyle.Bold,
                fixedWidth= 150
            };
            
            valueStyle = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = new Color(0.2f, 0.5f, 0.9f) }
            };
            
            titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                margin = new RectOffset(0, 0, 10, 5)
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
                    if (GUILayout.Button("创建配置"))
                    {
                        QuarkGlobalConfigMenu.ResetGlobalConfiguration();
                        LoadConfig();
                    }
                    return;
                }
            }

            if (activeProfile == null)
            {
                EditorGUILayout.HelpBox("没有激活的环境配置。请先在全局配置窗口中设置。", MessageType.Warning);
                if (GUILayout.Button("打开全局配置"))
                {
                    QuarkGlobalConfigWindow.ShowWindow();
                }
                return;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            // 标题
            EditorGUILayout.LabelField("QuarkAsset 环境配置预览", headerStyle);
            
            // 当前环境
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            var profileNames = config.GetProfileNames();
            if (profileNames.Length > 0)
            {
                EditorGUI.BeginChangeCheck();
                var newIndex = EditorGUILayout.Popup("当前环境", config.ActiveProfileIndex, profileNames);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(config, "切换环境");
                    config.ActiveProfileIndex = newIndex;
                    activeProfile = config.ActiveProfile;
                    EditorUtility.SetDirty(config);
                }
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            // 环境设置
            showSettings = EditorGUILayout.Foldout(showSettings, "环境设置", true);
            if (showSettings)
            {
                EditorGUILayout.BeginVertical(boxStyle);
                
                DrawProfileInfo(activeProfile);
                
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.Space();
            
            // 运行时预览
            showPreview = EditorGUILayout.Foldout(showPreview, "运行时预览", true);
            if (showPreview)
            {
                EditorGUILayout.BeginVertical(boxStyle);
                
                EditorGUILayout.LabelField("在运行时，将使用以下值初始化QuarkAsset:", titleStyle);
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("下载地址:", labelStyle);
                EditorGUILayout.LabelField(activeProfile.DownloadURL, valueStyle);
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("版本号:", labelStyle);
                EditorGUILayout.LabelField($"{config.DefaultBuildVersion} (内部版本: {config.DefaultInternalBuildVersion})", valueStyle);
                EditorGUILayout.EndHorizontal();
                
                if (activeProfile.UseAesEncryption)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("AES加密:", labelStyle);
                    EditorGUILayout.LabelField($"启用 (密钥: {activeProfile.AesEncryptionKey})", valueStyle);
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("AES加密:", labelStyle);
                    EditorGUILayout.LabelField("禁用", valueStyle);
                    EditorGUILayout.EndHorizontal();
                }
                
                if (activeProfile.UseOffsetEncryption)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("偏移加密:", labelStyle);
                    EditorGUILayout.LabelField($"启用 (偏移: {activeProfile.OffsetEncryptionValue})", valueStyle);
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("偏移加密:", labelStyle);
                    EditorGUILayout.LabelField("禁用", valueStyle);
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.Space();
                
                EditorGUILayout.HelpBox("这些设置将影响资源的下载地址和解密方式。确保运行时使用正确的配置。", MessageType.Info);
                
                EditorGUILayout.BeginHorizontal();
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
                
                if (GUILayout.Button("生成初始化代码"))
                {
                    QuarkGlobalConfigMenu.GenerateRuntimeCode();
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.Space();
            
            // 操作按钮
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("打开全局配置"))
            {
                QuarkGlobalConfigWindow.ShowWindow();
            }
            
            if (GUILayout.Button("生成配置类"))
            {
                QuarkGlobalConfigMenu.GenerateProfileClass();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndScrollView();
        }

        private void DrawProfileInfo(QuarkProfile profile)
        {
            if (profile == null)
                return;
                
            EditorGUILayout.LabelField("环境信息", titleStyle);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("名称:", labelStyle);
            EditorGUILayout.LabelField(profile.ProfileName, valueStyle);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("下载地址:", labelStyle);
            EditorGUILayout.LabelField(profile.DownloadURL, valueStyle);
            EditorGUILayout.EndHorizontal();
            
            if (!string.IsNullOrEmpty(profile.CustomDescription))
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("描述:", labelStyle);
                EditorGUILayout.LabelField(profile.CustomDescription, valueStyle);
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("加密设置", titleStyle);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("AES加密:", labelStyle);
            EditorGUILayout.LabelField(profile.UseAesEncryption ? "启用" : "禁用", valueStyle);
            EditorGUILayout.EndHorizontal();
            
            if (profile.UseAesEncryption)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("AES密钥:", labelStyle);
                EditorGUILayout.LabelField(profile.AesEncryptionKey, valueStyle);
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("偏移加密:", labelStyle);
            EditorGUILayout.LabelField(profile.UseOffsetEncryption ? "启用" : "禁用", valueStyle);
            EditorGUILayout.EndHorizontal();
            
            if (profile.UseOffsetEncryption)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("偏移值:", labelStyle);
                EditorGUILayout.LabelField(profile.OffsetEncryptionValue.ToString(), valueStyle);
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("版本设置", titleStyle);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("构建版本号:", labelStyle);
            EditorGUILayout.LabelField(config.DefaultBuildVersion, valueStyle);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("内部版本号:", labelStyle);
            EditorGUILayout.LabelField(config.DefaultInternalBuildVersion.ToString(), valueStyle);
            EditorGUILayout.EndHorizontal();
        }
    }
}
