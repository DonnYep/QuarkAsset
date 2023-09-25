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
        Texture2D createAddNewIcon;
        Texture2D SaveActiveIcon;
        public QuarkAssetWindow()
        {
            this.titleContent = new GUIContent("QuarkAsset");
        }
        [MenuItem("Window/QuarkAsset/QuarkAsset", false, 100)]
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
            assetDatabaseTab.OnEnable(position, this);
            assetBundleTab.OnEnable();
            assetBundleTab.SetAssetDatabaseTab(assetDatabaseTab);
            assetDatasetTab.OnEnable();
            refreshIcon = QuarkEditorUtility.GetRefreshIcon();
            createAddNewIcon = QuarkEditorUtility.GetCreateAddNewIcon();
            SaveActiveIcon = QuarkEditorUtility.GetSaveActiveIcon();
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
                if (GUILayout.Button(refreshIcon, GUILayout.MaxWidth(QuarkEditorConstant.ICON_WIDTH)))
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
                if (GUILayout.Button(createAddNewIcon, GUILayout.MaxWidth(QuarkEditorConstant.ICON_WIDTH)))
                {
                    latestDataset = CreateQuarkAssetDataset();
                }
                if (GUILayout.Button(SaveActiveIcon, GUILayout.MaxWidth(QuarkEditorConstant.ICON_WIDTH)))
                {
                    QuarkEditorUtility.SaveScriptableObject(QuarkEditorDataProxy.QuarkAssetDataset);
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
            var dataset = QuarkEditorUtility.CreateScriptableObject<QuarkDataset>("Assets/NewQuarkAssetDataset.asset", HideFlags.NotEditable);
            dataset.QuarkAssetExts.AddRange(QuarkEditorConstant.Extensions);
            QuarkEditorUtility.SaveScriptableObject(dataset);
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
                QuarkEditorUtility.SaveScriptableObject(QuarkEditorDataProxy.QuarkAssetDataset);
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