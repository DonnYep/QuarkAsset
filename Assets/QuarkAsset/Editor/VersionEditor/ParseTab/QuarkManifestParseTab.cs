using Quark.Asset;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Quark.Editor
{
    public class QuarkManifestParseTab
    {
        internal const string ManifestParseTabDataFileName = "QuarkVersion_ManifestParseTabData.json";
        QuarkManifestParseTabData tabData;
        QuarkManifestParseLabel parseLabel;
        public void OnEnable()
        {
            if (parseLabel == null)
                parseLabel = new QuarkManifestParseLabel();
            parseLabel.OnEnable();
            GetWindowData();
            if (tabData.ShowManifest)
                GetCachedManifest();
        }
        public void OnGUI(Rect rect)
        {
            DrawConfig();

            GUILayout.Space(16);
            DrawButton();

            parseLabel.OnGUI(rect);
        }
        public void OnDisable()
        {
            SaveWindowData();
        }
        QuarkManifest LoadManifest(string path, string key)
        {
            var filePath = Path.Combine(path, QuarkConstant.MANIFEST_NAME);
            if (File.Exists(filePath))
            {
                var context = QuarkUtility.ReadTextFileContent(filePath);
                return Quark.QuarkUtility.Manifest.DeserializeManifest(context, key);
            }
            else
            {
                return null;
            }
        }
        void GetWindowData()
        {
            try
            {
                tabData = QuarkEditorUtility.GetData<QuarkManifestParseTabData>(ManifestParseTabDataFileName);
            }
            catch
            {
                tabData = new QuarkManifestParseTabData();
                QuarkEditorUtility.SaveData(ManifestParseTabDataFileName, tabData);
            }
        }
        void SaveWindowData()
        {
            QuarkEditorUtility.SaveData(ManifestParseTabDataFileName, tabData);
        }
        void DrawConfig()
        {
            EditorGUILayout.BeginHorizontal();
            {
                tabData.ManifestPath = EditorGUILayout.TextField("ManifestPath", tabData.ManifestPath);
                if (GUILayout.Button("Browse", GUILayout.MaxWidth(128f)))
                {
                    var newPath = EditorUtility.OpenFolderPanel("ManifestPath", tabData.ManifestPath, string.Empty);
                    if (!string.IsNullOrEmpty(newPath))
                    {
                        tabData.ManifestPath = newPath.Replace("\\", "/");
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            tabData.ManifestAesKey = EditorGUILayout.TextField("ManifestAesKey", tabData.ManifestAesKey);

            GUILayout.Space(16);
        }
        void DrawButton()
        {
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Parse"))
                {
                    var manifest = LoadManifest(tabData.ManifestPath, tabData.ManifestAesKey);
                    if (manifest != null)
                    {
                        parseLabel.SetManifest(manifest);
                        QuarkEditorUtility.SaveData(QuarkConstant.MANIFEST_NAME, manifest);

                        QuarkUtility.LogInfo("Manifest overwrite done ! ");
                        tabData.ShowManifest = true;

                    }
                }
                if (GUILayout.Button("Clear"))
                {
                    tabData.ShowManifest = false;
                    parseLabel.Clear();
                }
            }
            GUILayout.EndHorizontal();
        }
        void GetCachedManifest()
        {
            try
            {
                var manifest = QuarkEditorUtility.GetData<QuarkManifest>(QuarkConstant.MANIFEST_NAME);
                parseLabel.SetManifest(manifest);
            }
            catch { }
        }
    }
}
