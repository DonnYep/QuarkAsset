using UnityEngine;
using UnityEditor;
using Quark.Asset;
namespace Quark.Editor
{
    public class QuarkAssetWindow : EditorWindow
    {
        string[] tabArray = new string[] { "AssetDatabaseTab", "AssetBundleTab", "AssetDatasetTab" };
        QuarkAssetDatabaseTab assetDatabaseTab;
        QuarkAssetBundleTab assetBundleTab;
        QuarkAssetDatasetTab assetDatasetTab;
        Vector2 scrollPosition;
        internal const string QuarkAssetWindowDataName = "QuarkAsset_WindowData.json";

        QuarkAssetWindowData windowData;
        QuarkDataset latestDataset;
        Texture2D refreshIcon;
        public QuarkAssetWindow()
        {
            this.titleContent = new GUIContent("QuarkAsset");
        }
        [MenuItem("Window/QuarkAsset/QuarkEditor", false, 100)]
        public static void OpenWindow()
        {
            var window = GetWindow<QuarkAssetWindow>();
            window.minSize = new Vector2(960f, 540f);
        }
        void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            if (assetDatabaseTab == null)
                assetDatabaseTab = new QuarkAssetDatabaseTab();
            if (assetBundleTab == null)
                assetBundleTab = new QuarkAssetBundleTab();
            if (assetDatasetTab == null)
                assetDatasetTab = new QuarkAssetDatasetTab();
            QuarkEditorDataProxy.QuarkAssetDataset = null;
            GetWindowData();
            assetDatabaseTab.OnEnable();
            assetBundleTab.OnEnable();
            assetBundleTab.SetAssetDatabaseTab(assetDatabaseTab);
            assetDatasetTab.OnEnable();
            refreshIcon = QuarkEditorUtility.GetRefreshIcon();
        }

        void OnDisable()
        {
            SaveWindowData();
            assetDatabaseTab.OnDisable();
            assetBundleTab.OnDisable();
            assetDatasetTab.OnDisable();
            QuarkEditorDataProxy.QuarkAssetDataset = null;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }
        void OnGUI()
        {
            windowData.SelectedTabIndex = GUILayout.Toolbar(windowData.SelectedTabIndex, tabArray);
            GUILayout.Space(16);
            EditorGUILayout.BeginHorizontal();
            {
                latestDataset = (QuarkDataset)EditorGUILayout.ObjectField("QuarkAssetDataset", latestDataset, typeof(QuarkDataset), false);
                if (GUILayout.Button(refreshIcon, GUILayout.MaxWidth(32)))
                {
                    if (latestDataset == null)
                        return;
                    switch (windowData.SelectedTabIndex)
                    {
                        case 0:
                            assetDatabaseTab.OnDatasetRefresh();
                            break;
                        case 1:
                            assetBundleTab.OnDatasetRefresh();
                            break;
                        case 2:
                            assetDatasetTab.OnDatasetRefresh();
                            break;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            if (QuarkEditorDataProxy.QuarkAssetDataset != latestDataset)
            {
                QuarkEditorDataProxy.QuarkAssetDataset = latestDataset;
                //dataset不相等
                if (latestDataset != null)
                {
                    //若不为空，则刷新
                    AssignDataset();
                }
                else
                {
                    //若为空，则释放
                    UnassignDataset();
                }
            }
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("CreateDataset", GUILayout.MaxWidth(128f)))
                {
                    latestDataset = CreateQuarkAssetDataset();
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(16);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            {
                switch (windowData.SelectedTabIndex)
                {
                    case 0:
                        assetDatabaseTab.OnGUI(position);
                        break;
                    case 1:
                        assetBundleTab.OnGUI();
                        break;
                    case 2:
                        assetDatasetTab.OnGUI();
                        break;
                }
            }
            EditorGUILayout.EndScrollView();
        }
        void OnPlayModeStateChanged(PlayModeStateChange stateChange)
        {
            QuarkEditorDataProxy.QuarkAssetDataset = null;
        }

        QuarkDataset CreateQuarkAssetDataset()
        {
            var dataset = ScriptableObject.CreateInstance<QuarkDataset>();
            dataset.hideFlags = HideFlags.NotEditable;
            AssetDatabase.CreateAsset(dataset, "Assets/New QuarkAssetDataset.asset");
            dataset.QuarkAssetExts.AddRange(QuarkEditorConstant.Extensions);
            EditorUtility.SetDirty(dataset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            QuarkUtility.LogInfo("QuarkAssetDataset is created");
            return dataset;
        }
        void GetWindowData()
        {
            try
            {
                windowData = QuarkEditorUtility.GetData<QuarkAssetWindowData>(QuarkAssetWindowDataName);
            }
            catch
            {
                windowData = new QuarkAssetWindowData();
                QuarkEditorUtility.SaveData(QuarkAssetWindowDataName, windowData);
            }
            if (!string.IsNullOrEmpty(windowData.QuarkDatasetPath))
            {
                latestDataset = AssetDatabase.LoadAssetAtPath<QuarkDataset>(windowData.QuarkDatasetPath);
            }
        }
        void SaveWindowData()
        {
            if (QuarkEditorDataProxy.QuarkAssetDataset != null)
            {
                windowData.QuarkDatasetPath = AssetDatabase.GetAssetPath(QuarkEditorDataProxy.QuarkAssetDataset);
                EditorUtility.SetDirty(QuarkEditorDataProxy.QuarkAssetDataset);
#if UNITY_2021_1_OR_NEWER
                AssetDatabase.SaveAssetIfDirty(QuarkEditorDataProxy.QuarkAssetDataset);
#elif UNITY_2019_1_OR_NEWER
                AssetDatabase.SaveAssets();
#endif
                AssetDatabase.Refresh();
            }
            QuarkEditorUtility.SaveData(QuarkAssetWindowDataName, windowData);
        }
        void AssignDataset()
        {
            assetDatabaseTab.OnDatasetAssign();
            assetBundleTab.OnDatasetAssign();
            assetDatasetTab.OnDatasetAssign();
            windowData.QuarkDatasetPath = AssetDatabase.GetAssetPath(latestDataset);
            QuarkEditorUtility.SaveData(QuarkAssetWindowDataName, windowData);
        }
        void UnassignDataset()
        {
            assetDatabaseTab.OnDatasetUnassign();
            assetBundleTab.OnDatasetUnassign();
            assetDatasetTab.OnDatasetUnassign();
            windowData.QuarkDatasetPath = string.Empty;
            QuarkEditorUtility.SaveData(QuarkAssetWindowDataName, windowData);
        }
    }
}