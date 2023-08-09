using Quark.Asset;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Quark.Editor
{
    public class QuarkManifestCompareTab
    {
        internal const string ManifestCompareTabDataFileName = "QuarkVersion_ManifesCompareTabData.json";
        QuarkManifestCompareTabData wndData;
        public void OnEnable()
        {
            GetWindowData();
        }
        public void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.BeginHorizontal();
                {
                    wndData.SrcManifestPath = EditorGUILayout.TextField("SrcManifestPath", wndData.SrcManifestPath);
                    if (GUILayout.Button("Browse", GUILayout.MaxWidth(128f)))
                    {
                        var newPath = EditorUtility.OpenFilePanel("SrcManifestPath", wndData.SrcManifestPath, string.Empty);
                        if (!string.IsNullOrEmpty(newPath))
                        {
                            wndData.SrcManifestPath = newPath.Replace("\\", "/");
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

                wndData.SrcManifestAesKey = EditorGUILayout.TextField("SrcManifestAesKey", wndData.SrcManifestAesKey);

                GUILayout.Space(16);

                EditorGUILayout.BeginHorizontal();
                {
                    wndData.DiffManifestPath = EditorGUILayout.TextField("DiffManifestPath", wndData.DiffManifestPath);
                    if (GUILayout.Button("Browse", GUILayout.MaxWidth(128f)))
                    {
                        var newPath = EditorUtility.OpenFilePanel("DiffManifestPath", wndData.DiffManifestPath, string.Empty);
                        if (!string.IsNullOrEmpty(newPath))
                        {
                            wndData.DiffManifestPath = newPath.Replace("\\", "/");
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
                wndData.DiffManifestAesKey = EditorGUILayout.TextField("DiffManifestAesKey", wndData.DiffManifestAesKey);

                GUILayout.Space(16);
                EditorGUILayout.BeginHorizontal();
                {
                    wndData.CompareResultOutputPath = EditorGUILayout.TextField("CompareResultPath", wndData.CompareResultOutputPath);
                    if (GUILayout.Button("Browse", GUILayout.MaxWidth(128f)))
                    {
                        var newPath = EditorUtility.OpenFolderPanel("CompareResultPath", wndData.CompareResultOutputPath, string.Empty);
                        if (!string.IsNullOrEmpty(newPath))
                        {
                            wndData.CompareResultOutputPath = newPath.Replace("\\", "/");
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

                wndData.OpenCompareResultPathWhenCompareDone = EditorGUILayout.ToggleLeft("Open compare result path output", wndData.OpenCompareResultPathWhenCompareDone);

                GUILayout.Space(16);
                if (GUILayout.Button("Compare"))
                {
                    var srcManifest = LoadManifest(wndData.SrcManifestPath, wndData.SrcManifestAesKey);
                    var diffManifest = LoadManifest(wndData.DiffManifestPath, wndData.DiffManifestAesKey);
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
                        QuarkResources.QuarlManifestComparer.CompareManifest(srcManifest, diffManifest, out var result);
                        if (Directory.Exists(wndData.CompareResultOutputPath))
                        {
                            var resultFileName = Path.Combine(wndData.CompareResultOutputPath, QuarkConstant.MANIFEST_COMPARE_RESULT_NAME);
                            QuarkUtility.OverwriteTextFile(resultFileName, QuarkUtility.ToJson(result));
                            QuarkUtility.LogInfo("Compare result overwrite done ! ");
                            if (wndData.OpenCompareResultPathWhenCompareDone)
                            {
                                EditorUtility.RevealInFinder(resultFileName);
                            }
                        }
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }
        public void OnDisable()
        {
            SaveWindowData();
        }
        QuarkManifest LoadManifest(string path, string key)
        {
            if (File.Exists(path))
            {
                var context = QuarkUtility.ReadTextFileContent(path);
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
                wndData = QuarkEditorUtility.GetData<QuarkManifestCompareTabData>(ManifestCompareTabDataFileName);
            }
            catch
            {
                wndData = new QuarkManifestCompareTabData();
                QuarkEditorUtility.SaveData(ManifestCompareTabDataFileName, wndData);
            }
        }
        void SaveWindowData()
        {
            QuarkEditorUtility.SaveData(ManifestCompareTabDataFileName, wndData);
        }
    }
}
