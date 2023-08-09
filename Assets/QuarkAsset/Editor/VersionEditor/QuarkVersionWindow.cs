using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using System.IO;
using Quark.Asset;

namespace Quark.Editor
{
    public class QuarkVersionWindow : EditorWindow
    {
        QuarkVersionWindowData wndData;

        internal const string QuarkVersionWindowDataName = "QuarkVersion_WindowData.json";
        QuarkManifestCompareTab manifestCompareTab;
        QuarkManifestDecryptTab manifestDecryptTab;

        Vector2 scrollPosition;

        string[] tabArray = new string[] { "Compare", "Decrypt" };
        public QuarkVersionWindow()
        {
            this.titleContent = new GUIContent("QuarkVersionEditor");
        }
        [MenuItem("Window/QuarkAsset/VersionEditor", false, 110)]
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
            manifestCompareTab.OnEnable();
            manifestDecryptTab.OnEnable();
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
                        manifestCompareTab.OnGUI();
                        break;
                    case 1:
                        manifestDecryptTab.OnGUI();
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
