using UnityEngine;
using UnityEditor;

namespace Quark.Editor
{
    public class QuarkVersionWindow : EditorWindow
    {
        QuarkVersionWindowData wndData;

        internal const string QuarkVersionWindowDataName = "QuarkVersion_WindowData.json";
        QuarkManifestCompareTab manifestCompareTab;
        QuarkManifestMergeTab manifestMergeTab;
        QuarkManifestParseTab manifestParseTab;

        Vector2 scrollPosition;

        string[] tabArray = new string[] { "Compare", "Merge", "Parse" };
        public QuarkVersionWindow()
        {
            this.titleContent = new GUIContent("QuarkVersion");
        }
        [MenuItem("Window/QuarkAsset/QuarkVersion", false, 110)]
        public static void OpenWindow()
        {
            var window = GetWindow<QuarkVersionWindow>();
            window.minSize = new Vector2(640f, 480f);
        }
        private void OnEnable()
        {
            GetWindowData();
            if (manifestCompareTab == null)
                manifestCompareTab = new QuarkManifestCompareTab();
            if (manifestMergeTab == null)
                manifestMergeTab = new QuarkManifestMergeTab();
            if (manifestParseTab == null)
                manifestParseTab = new QuarkManifestParseTab();
            manifestCompareTab.OnEnable();
            manifestMergeTab.OnEnable();
            manifestParseTab.OnEnable();
        }
        private void OnGUI()
        {
            wndData.SelectedTabIndex = GUILayout.Toolbar(wndData.SelectedTabIndex, tabArray);
            GUILayout.Space(16);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            {
                switch (wndData.SelectedTabIndex)
                {
                    case 0:
                        manifestCompareTab.OnGUI(position);
                        break;
                    case 1:
                        manifestMergeTab.OnGUI(position);
                        break;
                    case 2:
                        manifestParseTab.OnGUI(position);
                        break;
                }
            }
            EditorGUILayout.EndScrollView();
        }
        private void OnDisable()
        {
            SaveWindowData();
            manifestCompareTab.OnDisable();
            manifestMergeTab.OnDisable();
            manifestParseTab.OnDisable();
        }
        void GetWindowData()
        {
            try
            {
                wndData = QuarkEditorUtility.GetData<QuarkVersionWindowData>(QuarkVersionWindowDataName);
            }
            catch
            {
                wndData = new QuarkVersionWindowData();
                QuarkEditorUtility.SaveData(QuarkVersionWindowDataName, wndData);
            }
        }
        void SaveWindowData()
        {
            QuarkEditorUtility.SaveData(QuarkVersionWindowDataName, wndData);
        }
    }
}
