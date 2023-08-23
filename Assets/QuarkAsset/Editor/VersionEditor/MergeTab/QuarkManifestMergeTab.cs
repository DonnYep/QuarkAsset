using Quark.Asset;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Quark.Editor
{
    public class QuarkManifestMergeTab
    {
        internal const string MergeManifestTabDataFileName = "QuarkVersion_MergeManifestTabData.json";
        QuarkManifestMergeTabData tabData;
        QuarkManifestMergeLabel manifestMergeLabel;
        public void OnEnable()
        {
            manifestMergeLabel = new QuarkManifestMergeLabel(this);
            GetWindowData();
            manifestMergeLabel.OnEnable();
            if (tabData.ShowMergedManifest)
                GetCachedMergedManifest();
        }
        public void OnGUI(Rect rect)
        {
            EditorGUILayout.BeginVertical();
            {
                DrawConfig();
                GUILayout.Space(16);
                DrawButton();
                manifestMergeLabel.OnGUI(rect);
            }
            EditorGUILayout.EndVertical();
        }
        public void OnDisable()
        {
            SaveWindowData();
        }
        internal void RefreshMergedManifest()
        {
            GetCachedMergedManifest();
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
                tabData = QuarkEditorUtility.GetData<QuarkManifestMergeTabData>(MergeManifestTabDataFileName);
            }
            catch
            {
                tabData = new QuarkManifestMergeTabData();
                QuarkEditorUtility.SaveData(MergeManifestTabDataFileName, tabData);
            }
            QuarkManifestMergeLabelDataProxy.ShowBuiltIn = tabData.ShowBuiltIn;
            QuarkManifestMergeLabelDataProxy.ShowIncremental = tabData.ShowIncremental;
        }
        void SaveWindowData()
        {
            tabData.ShowBuiltIn = QuarkManifestMergeLabelDataProxy.ShowBuiltIn;
            tabData.ShowIncremental = QuarkManifestMergeLabelDataProxy.ShowIncremental;
            QuarkEditorUtility.SaveData(MergeManifestTabDataFileName, tabData);
        }
        void DrawConfig()
        {
            EditorGUILayout.BeginHorizontal();
            {
                tabData.SrcManifestPath = EditorGUILayout.TextField("SrcManifestPath", tabData.SrcManifestPath);
                if (GUILayout.Button("Browse", GUILayout.MaxWidth(128f)))
                {
                    var newPath = EditorUtility.OpenFolderPanel("SrcManifestPath", tabData.SrcManifestPath, string.Empty);
                    if (!string.IsNullOrEmpty(newPath))
                    {
                        tabData.SrcManifestPath = newPath.Replace("\\", "/");
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            tabData.SrcManifestAesKey = EditorGUILayout.TextField("SrcManifestAesKey", tabData.SrcManifestAesKey?.Trim());

            GUILayout.Space(16);

            EditorGUILayout.BeginHorizontal();
            {
                tabData.DiffManifestPath = EditorGUILayout.TextField("DiffManifestPath", tabData.DiffManifestPath);
                if (GUILayout.Button("Browse", GUILayout.MaxWidth(128f)))
                {
                    var newPath = EditorUtility.OpenFolderPanel("DiffManifestPath", tabData.DiffManifestPath, string.Empty);
                    if (!string.IsNullOrEmpty(newPath))
                    {
                        tabData.DiffManifestPath = newPath.Replace("\\", "/");
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            tabData.DiffManifestAesKey = EditorGUILayout.TextField("DiffManifestAesKey", tabData.DiffManifestAesKey?.Trim());
        }
        void DrawButton()
        {
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Compare"))
                {
                    var srcManifest = LoadManifest(tabData.SrcManifestPath, tabData.SrcManifestAesKey);
                    var diffManifest = LoadManifest(tabData.DiffManifestPath, tabData.DiffManifestAesKey);
                    if (srcManifest == null)
                    {
                        QuarkUtility.LogError("srcManifest invalid ,check you config !");
                    }
                    if (diffManifest == null)
                    {
                        QuarkUtility.LogError("diffManifestinvalid ,check you config !");
                    }
                    if (srcManifest != null && diffManifest != null)
                    {
                        QuarkUtility.Manifest.MergeManifest(srcManifest, diffManifest, out var mergedManifest);
                        manifestMergeLabel.SetManifest(mergedManifest);
                        QuarkEditorUtility.SaveData(QuarkConstant.MERGED_MANIFEST_NAME, mergedManifest);
                        QuarkUtility.LogInfo("Merged manifest overwrite done ! ");
                        tabData.ShowMergedManifest=true;
                    }
                }
                if (GUILayout.Button("Clear"))
                {
                    tabData.ShowMergedManifest = false;
                    manifestMergeLabel.Clear();
                }
            }
            GUILayout.EndHorizontal();
        }
        void GetCachedMergedManifest()
        {
            try
            {
                var manifest = QuarkEditorUtility.GetData<QuarkMergedManifest>(QuarkConstant.MERGED_MANIFEST_NAME);
                manifestMergeLabel.SetManifest(manifest);
            }
            catch { }
        }
    }
}
