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
        QuarkAssetDataset latestDataset;
        /// <summary>
        /// dataset是否为空处理标记；
        /// </summary>
        bool datasetAssigned = false;
        Texture2D refreshIcon;
        public QuarkAssetWindow()
        {
            this.titleContent = new GUIContent("QuarkAsset");
        }
        [MenuItem("Window/QuarkAsset/QuarkEditor", false, 100)]
        public static void OpenWindow()
        {
            var window = GetWindow<QuarkAssetWindow>();
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
            datasetAssigned = false;
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
            datasetAssigned = false;
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
                latestDataset = (QuarkAssetDataset)EditorGUILayout.ObjectField("QuarkAssetDataset", latestDataset, typeof(QuarkAssetDataset), false);
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
                if (QuarkEditorDataProxy.QuarkAssetDataset != null && !datasetAssigned)
                    AssignDataset();
                else
                    UnassignDataset();
            }
            else
            {
                if (QuarkEditorDataProxy.QuarkAssetDataset == null)
                {
                    if (datasetAssigned)
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
                        assetDatabaseTab.OnGUI();
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
            datasetAssigned = false;
            QuarkEditorDataProxy.QuarkAssetDataset = null;
        }

        QuarkAssetDataset CreateQuarkAssetDataset()
        {
            var dataset = ScriptableObject.CreateInstance<QuarkAssetDataset>();
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
                latestDataset = AssetDatabase.LoadAssetAtPath<QuarkAssetDataset>(windowData.QuarkDatasetPath);
            }
        }
        void SaveWindowData()
        {
            if (QuarkEditorDataProxy.QuarkAssetDataset != null)
            {
                windowData.QuarkDatasetPath = AssetDatabase.GetAssetPath(QuarkEditorDataProxy.QuarkAssetDataset);
                EditorUtility.SetDirty(QuarkEditorDataProxy.QuarkAssetDataset);
                AssetDatabase.Refresh();
            }
            QuarkEditorUtility.SaveData(QuarkAssetWindowDataName, windowData);
        }
        void AssignDataset()
        {
            assetDatabaseTab.OnDatasetAssign();
            assetBundleTab.OnDatasetAssign();
            assetDatasetTab.OnDatasetAssign();
            datasetAssigned = true;
            windowData.QuarkDatasetPath = AssetDatabase.GetAssetPath(latestDataset);
            QuarkEditorUtility.SaveData(QuarkAssetWindowDataName, windowData);
        }
        void UnassignDataset()
        {
            assetDatabaseTab.OnDatasetUnassign();
            assetBundleTab.OnDatasetUnassign();
            assetDatasetTab.OnDatasetUnassign();
            datasetAssigned = false;
            windowData.QuarkDatasetPath = string.Empty;
            QuarkEditorUtility.SaveData(QuarkAssetWindowDataName, windowData);
        }
    }
}