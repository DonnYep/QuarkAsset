using Quark.Asset;
using Quark.Manifest;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Quark.Editor
{
    public class QuarkManifestCompareTab
    {
        internal const string ManifestCompareTabDataFileName = "QuarkVersion_ManifesCompareTabData.json";
        QuarkManifestCompareTabData tabData;
        QuarkManifestCompareLabel compareResultLabel;
        public void OnEnable()
        {
            compareResultLabel = new QuarkManifestCompareLabel(this);
            compareResultLabel.OnEnable();
            GetWindowData();
            GetCachedCompareResult();
        }
        public void OnGUI(Rect rect)
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.BeginVertical();
                {
                    DrawCompareConfig();
                    DrawCompareButton();
                    GUILayout.Space(16);
                    compareResultLabel.OnGUI(rect);
                }
                EditorGUILayout.EndVertical();

            }
            EditorGUILayout.EndHorizontal();
        }
        public void OnDisable()
        {
            SaveWindowData();
        }
        internal void RefreshCompareResult()
        {
            GetCachedCompareResult();
        }
        QuarkManifest LoadManifest(string path, string key)
        {
            var manifestPath = Path.Combine(path, QuarkConstant.MANIFEST_NAME);
            if (File.Exists(manifestPath))
            {
                var context = QuarkUtility.ReadTextFileContent(manifestPath);
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
                tabData = QuarkEditorUtility.GetData<QuarkManifestCompareTabData>(ManifestCompareTabDataFileName);
            }
            catch
            {
                tabData = new QuarkManifestCompareTabData();
                QuarkEditorUtility.SaveData(ManifestCompareTabDataFileName, tabData);
            }
            QuarkManifestCompareTabDataProxy.ShowChanged = tabData.Changed;
            QuarkManifestCompareTabDataProxy.ShowNewlyAdded = tabData.NewlyAdded;
            QuarkManifestCompareTabDataProxy.ShowDeleted = tabData.Deleted;
            QuarkManifestCompareTabDataProxy.ShowUnchanged = tabData.Unchanged;
        }
        void SaveWindowData()
        {
            tabData.Changed = QuarkManifestCompareTabDataProxy.ShowChanged;
            tabData.NewlyAdded = QuarkManifestCompareTabDataProxy.ShowNewlyAdded;
            tabData.Deleted = QuarkManifestCompareTabDataProxy.ShowDeleted;
            tabData.Unchanged = QuarkManifestCompareTabDataProxy.ShowUnchanged;
            QuarkEditorUtility.SaveData(ManifestCompareTabDataFileName, tabData);
        }
        void GetCachedCompareResult()
        {
            try
            {
                var cmpRst = QuarkEditorUtility.GetData<QuarkManifestCompareResult>(QuarkConstant.MANIFEST_COMPARE_RESULT_NAME);
                compareResultLabel.SetResult(cmpRst);
            }
            catch { }
        }
        void DrawCompareConfig()
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

            tabData.SrcManifestAesKey = EditorGUILayout.TextField("SrcManifestAesKey", tabData.SrcManifestAesKey);

            //GUILayout.Space(16);

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
            tabData.DiffManifestAesKey = EditorGUILayout.TextField("DiffManifestAesKey", tabData.DiffManifestAesKey);

            GUILayout.Space(16);

            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Open result path", GUILayout.MaxWidth(128f)))
                {
                    var filePath = Path.Combine(QuarkEditorUtility.LibraryPath, QuarkConstant.MANIFEST_COMPARE_RESULT_NAME);
                    EditorUtility.RevealInFinder(filePath);
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(16);
        }
        void DrawCompareButton()
        {
            EditorGUILayout.BeginHorizontal();
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
                        QuarkUtility.Manifest.CompareManifestByBundleName(srcManifest, diffManifest, out var result);
                        compareResultLabel.SetResult(result);
                        QuarkEditorUtility.SaveData(QuarkConstant.MANIFEST_COMPARE_RESULT_NAME, result);
                        QuarkUtility.LogInfo("Compare result overwrite done ! ");
                    }
                }
                if (GUILayout.Button("Clear"))
                {
                    compareResultLabel.Clear();
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
