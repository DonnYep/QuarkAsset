using UnityEngine;
using UnityEditor;

namespace Quark.Editor
{
    public class QuarkVersionWindow : EditorWindow
    {
        QuarkVersionWindowData wndData;

        internal const string QuarkVersionWindowDataName = "QuarkVersion_WindowData.json";
        QuarkManifestCompareTab manifestCompareTab;
        QuarkManifestDecryptTab manifestDecryptTab;
        QuarkManifestMergeTab manifestMergeTab;

        Vector2 scrollPosition;

        string[] tabArray = new string[] { "Compare", "Decrypt", "Merge" };
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
            if (manifestDecryptTab == null)
                manifestDecryptTab = new QuarkManifestDecryptTab();
            if (manifestMergeTab == null)
                manifestMergeTab = new QuarkManifestMergeTab();
            manifestCompareTab.OnEnable();
            manifestDecryptTab.OnEnable();
            manifestMergeTab.OnEnable();
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
                        manifestDecryptTab.OnGUI();
                        break;
                    case 2:
                        manifestMergeTab.OnGUI();
                        break;
                }
            }
            EditorGUILayout.EndScrollView();
        }
        private void OnDisable()
        {
            SaveWindowData();
            manifestCompareTab.OnDisable();
            manifestDecryptTab.OnDisable();
            manifestMergeTab.OnDisable();
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
