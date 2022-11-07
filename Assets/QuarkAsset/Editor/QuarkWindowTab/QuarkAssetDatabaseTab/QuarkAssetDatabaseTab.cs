﻿using Quark.Asset;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace Quark.Editor
{
    public class QuarkAssetDatabaseTab
    {
        QuarkAssetDatabaseTabData tabData;
        internal const string QuarkAssetDatabaseTabDataFileName = "QuarkAsset_DatabaseTabData.json";

        readonly string[] selectBarArr = new string[] { "Asset bundle lable", "Asset object lable" };

        QuarkAssetBundleSearchLabel assetBundleSearchLabel = new QuarkAssetBundleSearchLabel();
        QuarkAssetObjectSearchLabel assetObjectSearchLabel = new QuarkAssetObjectSearchLabel();

        QuarkAssetDataset dataset;
        EditorCoroutine coroutine;
        public void OnEnable()
        {
            try
            {
                tabData = QuarkEditorUtility.GetData<QuarkAssetDatabaseTabData>(QuarkAssetDatabaseTabDataFileName);
            }
            catch
            {
                tabData = new QuarkAssetDatabaseTabData();
                QuarkEditorUtility.SaveData(QuarkAssetDatabaseTabDataFileName, tabData);
            }
            assetBundleSearchLabel.OnEnable();
            assetObjectSearchLabel.OnEnable();
        }
        public void OnDisable()
        {
            QuarkEditorUtility.SaveData(QuarkAssetDatabaseTabDataFileName, tabData);
        }
        public void OnDatasetAssign()
        {
            if (coroutine != null)
                QuarkEditorUtility.StopCoroutine(coroutine);
            var dataset = QuarkEditorDataProxy.QuarkAssetDataset;
            coroutine = QuarkEditorUtility.StartCoroutine(EnumOnAssignDataset(dataset));
        }
        public void OnDatasetRefresh()
        {
            OnDatasetAssign();
        }
        public void OnDatasetUnassign()
        {
            if (coroutine != null)
                QuarkEditorUtility.StopCoroutine(coroutine);
            assetObjectSearchLabel.TreeView.Clear();
            assetBundleSearchLabel.TreeView.Clear();
        }
        public void OnGUI()
        {
            GUILayout.BeginVertical();
            {
                tabData.GenerateAssetPathCode = EditorGUILayout.ToggleLeft("GenerateAssetPath", tabData.GenerateAssetPathCode);
            }
            GUILayout.EndVertical();
            GUILayout.Space(16);
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Build"))
                {
                    QuarkEditorUtility.StartCoroutine(EnumBuildDataset());
                }
                if (GUILayout.Button("Clear"))
                {
                    ClearDataset();
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(16);
            GUILayout.BeginHorizontal();
            {
                GUILayout.BeginVertical(GUILayout.MinWidth(128));
                {
                    tabData.SelectedBarIndex = GUILayout.SelectionGrid(tabData.SelectedBarIndex, selectBarArr, 1);
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                {

                    switch (tabData.SelectedBarIndex)
                    {
                        case 0:
                            assetBundleSearchLabel.OnGUI();
                            break;
                        case 1:
                            assetObjectSearchLabel.OnGUI();
                            break;
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }
        public EditorCoroutine BuildDataset()
        {
            return QuarkEditorUtility.StartCoroutine(EnumBuildDataset());
        }
        void ClearDataset()
        {
            dataset.Dispose();
            assetBundleSearchLabel.TreeView.Clear();
            assetObjectSearchLabel.TreeView.Clear();
            EditorUtility.SetDirty(dataset);
            QuarkEditorUtility.ClearData(QuarkAssetDatabaseTabDataFileName);
            QuarkUtility.LogInfo("Quark asset clear done ");
        }
        IEnumerator EnumBuildDataset()
        {
            if (dataset == null)
            {
                QuarkUtility.LogError("QuarkAssetDataset is invalid !");
                yield break;
            }
            EditorUtility.ClearProgressBar();
            var bundles = dataset.QuarkAssetBundleList;
            var extensions = dataset.QuarkAssetExts;
            List<QuarkObject> quarkAssetList = new List<QuarkObject>();
            List<QuarkObject> quarkSceneList = new List<QuarkObject>();
            List<QuarkAssetBundle> validBundleList = new List<QuarkAssetBundle>();
            var sceneAssetFullName = typeof(SceneAsset).FullName;
            int currentBundleIndex = 0;
            int bundleCount = bundles.Count;
            foreach (var bundle in bundles)
            {
                currentBundleIndex++;
                var bundlePath = bundle.AssetBundlePath;
                if (!AssetDatabase.IsValidFolder(bundlePath))
                    continue;
                bundle.AssetBundleSize = QuarkEditorUtility.GetUnityDirectorySize(bundlePath, QuarkEditorDataProxy.QuarkAssetDataset.QuarkAssetExts);
                validBundleList.Add(bundle);
                bundle.QuarkObjects.Clear();
                var filePaths = Directory.GetFiles(bundlePath, ".", SearchOption.AllDirectories);
                var fileLength = filePaths.Length;
                for (int i = 0; i < fileLength; i++)
                {
                    //强制将文件的后缀名统一成小写
                    var filePath = filePaths[i].Replace("\\", "/");
                    var srcFileExt = Path.GetExtension(filePath);
                    var lowerExt = srcFileExt.ToLower();
                    var lowerExtFilePath = filePath.Replace(srcFileExt, lowerExt);
                    if (extensions.Contains(lowerExt))
                    {
                        var assetObject = new QuarkObject()
                        {
                            AssetName = Path.GetFileNameWithoutExtension(filePath),
                            AssetExtension = lowerExt,
                            AssetBundleName = bundle.AssetBundleName,
                            AssetPath = lowerExtFilePath,
                            AssetType = AssetDatabase.LoadAssetAtPath(filePath, typeof(Object)).GetType().FullName
                        };
                        quarkAssetList.Add(assetObject);
                        if (assetObject.AssetType == sceneAssetFullName)
                        {
                            quarkSceneList.Add(assetObject);
                        }
                        bundle.QuarkObjects.Add(assetObject);
                    }
                }
                EditorUtility.DisplayCancelableProgressBar("QuarkAsset", "QuarkDataset Building", currentBundleIndex / (float)bundleCount);
                yield return null;
            }
            EditorUtility.ClearProgressBar();
            dataset.QuarkObjectList.Clear();
            dataset.QuarkObjectList.AddRange(quarkAssetList);
            dataset.QuarkAssetBundleList.Clear();
            dataset.QuarkAssetBundleList.AddRange(validBundleList);
            dataset.QuarkSceneList.Clear();
            dataset.QuarkSceneList.AddRange(quarkSceneList);

            EditorUtility.SetDirty(dataset);
            QuarkEditorUtility.SaveData(QuarkAssetDatabaseTabDataFileName, tabData);
            if (tabData.GenerateAssetPathCode)
                CreateAssetPathScript();
            yield return null;
            yield return EnumOnAssignDataset(dataset);
#if UNITY_2021_1_OR_NEWER
            AssetDatabase.SaveAssetIfDirty(dataset);
#elif UNITY_2019_1_OR_NEWER
            AssetDatabase.SaveAssets();
#endif
            QuarkUtility.LogInfo("Quark asset  build done ");
        }
        void CreateAssetPathScript()
        {
            var str = "public static class QuarkAssetDefine\n{\n";
            var con = "    public static string ";
            for (int i = 0; i < dataset.QuarkObjectList.Count; i++)
            {
                var srcName = dataset.QuarkObjectList[i].AssetName;
                srcName = srcName.Trim();
                var fnlName = srcName.Contains(".") == true ? srcName.Replace(".", "_") : srcName;
                fnlName = srcName.Contains(" ") == true ? srcName.Replace(" ", "_") : srcName;
                str = QuarkUtility.Append(str, con, fnlName, "= \"", srcName, "\"", " ;\n");
            }
            str += "\n}";
            QuarkUtility.OverwriteTextFile(Application.dataPath, "QuarkAssetDefine.cs", str);
            AssetDatabase.Refresh();
        }
        IEnumerator EnumOnAssignDataset(QuarkAssetDataset dataset)
        {
            this.dataset = dataset;
            var bundles = dataset.QuarkAssetBundleList;
            var bundleLen = bundles.Count;
            assetBundleSearchLabel.TreeView.Clear();
            assetObjectSearchLabel.TreeView.Clear();
            for (int i = 0; i < bundleLen; i++)
            {
                var bundle = bundles[i];
                assetBundleSearchLabel.TreeView.AddPath(bundle.AssetBundlePath);
                var objects = bundle.QuarkObjects;
                var length = objects.Count;
                for (int j = 0; j < length; j++)
                {
                    var obj = objects[j];
                    var item = new QuarkObjectItem(obj.AssetName, obj.AssetExtension, obj.AssetPath, obj.AssetBundleName);
                    assetObjectSearchLabel.TreeView.AddPath(item);
                }
                yield return null;
            }
            assetObjectSearchLabel.TreeView.Reload();
        }
    }
}