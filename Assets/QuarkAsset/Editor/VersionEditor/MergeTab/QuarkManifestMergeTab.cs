using Quark.Asset;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Quark.Editor
{
    public class QuarkManifestMergeTab
    {
        internal const string MergeManifestTabDataFileName = "QuarkVersion_MergeManifestTabData.json";
        QuarkManifestMergeTabData tabData;
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
                    tabData.SrcManifestPath = EditorGUILayout.TextField("SrcManifestPath", tabData.SrcManifestPath);
                    if (GUILayout.Button("Browse", GUILayout.MaxWidth(128f)))
                    {
                        var newPath = EditorUtility.OpenFilePanel("SrcManifestPath", tabData.SrcManifestPath, string.Empty);
                        if (!string.IsNullOrEmpty(newPath))
                        {
                            tabData.SrcManifestPath = newPath.Replace("\\", "/");
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

                tabData.SrcManifestAesKey = EditorGUILayout.TextField("SrcManifestAesKey", tabData.SrcManifestAesKey.Trim());

                GUILayout.Space(16);

                EditorGUILayout.BeginHorizontal();
                {
                    tabData.DiffManifestPath = EditorGUILayout.TextField("DiffManifestPath", tabData.DiffManifestPath);
                    if (GUILayout.Button("Browse", GUILayout.MaxWidth(128f)))
                    {
                        var newPath = EditorUtility.OpenFilePanel("DiffManifestPath", tabData.DiffManifestPath, string.Empty);
                        if (!string.IsNullOrEmpty(newPath))
                        {
                            tabData.DiffManifestPath = newPath.Replace("\\", "/");
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
                tabData.DiffManifestAesKey = EditorGUILayout.TextField("DiffManifestAesKey", tabData.DiffManifestAesKey.Trim());

                GUILayout.Space(16);
                EditorGUILayout.BeginHorizontal();
                {
                    tabData.MergedManifestOutputPath = EditorGUILayout.TextField("CompareResultPath", tabData.MergedManifestOutputPath);
                    if (GUILayout.Button("Browse", GUILayout.MaxWidth(128f)))
                    {
                        var newPath = EditorUtility.OpenFolderPanel("CompareResultPath", tabData.MergedManifestOutputPath, string.Empty);
                        if (!string.IsNullOrEmpty(newPath))
                        {
                            tabData.MergedManifestOutputPath = newPath.Replace("\\", "/");
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

                tabData.OpenMergedManifestPathWhenMerge = EditorGUILayout.ToggleLeft("Open merged manifest path when output", tabData.OpenMergedManifestPathWhenMerge);

                GUILayout.Space(16);
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
                        QuarkUtility.Manifest.MergeManifest(srcManifest, diffManifest, out var  mergedManifest);
                        if (Directory.Exists(tabData.MergedManifestOutputPath))
                        {
                            var resultFileName = Path.Combine(tabData.MergedManifestOutputPath, QuarkConstant.MERGED_MANIFEST_NAME);
                            QuarkUtility.OverwriteTextFile(resultFileName, QuarkUtility.ToJson(mergedManifest));
                            QuarkUtility.LogInfo("Merged manifest overwrite done ! ");
                            if (tabData.OpenMergedManifestPathWhenMerge)
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
                tabData = QuarkEditorUtility.GetData<QuarkManifestMergeTabData>(MergeManifestTabDataFileName);
            }
            catch
            {
                tabData = new QuarkManifestMergeTabData();
                QuarkEditorUtility.SaveData(MergeManifestTabDataFileName, tabData);
            }
        }
        void SaveWindowData()
        {
            QuarkEditorUtility.SaveData(MergeManifestTabDataFileName, tabData);
        }
    }
}
