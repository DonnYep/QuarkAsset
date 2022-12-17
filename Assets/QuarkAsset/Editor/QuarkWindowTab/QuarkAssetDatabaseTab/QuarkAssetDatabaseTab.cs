using Quark.Asset;
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

        QuarkBundleSearchLabel assetBundleSearchLabel = new QuarkBundleSearchLabel();
        QuarkObjectSearchLabel assetObjectSearchLabel = new QuarkObjectSearchLabel();

        QuarkDataset dataset;
        EditorCoroutine dataAssignCoroutine;

        int loadingObjectProgress;
        /// <summary>
        /// 加载的ab包数量；
        /// </summary>
        int loadingBundleLength;
        /// <summary>
        /// 当前加载的ab包序号；
        /// </summary>
        int currentLoadingBundleIndex;
        bool loadingQuarkObject = false;
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
            if (dataAssignCoroutine != null)
                QuarkEditorUtility.StopCoroutine(dataAssignCoroutine);
        }
        public void OnDatasetAssign()
        {
            if (dataAssignCoroutine != null)
                QuarkEditorUtility.StopCoroutine(dataAssignCoroutine);
            var dataset = QuarkEditorDataProxy.QuarkAssetDataset;
            dataAssignCoroutine = QuarkEditorUtility.StartCoroutine(EnumOnAssignDataset(dataset));
        }
        public void OnDatasetRefresh()
        {
            OnDatasetAssign();
        }
        public void OnDatasetUnassign()
        {
            if (dataAssignCoroutine != null)
                QuarkEditorUtility.StopCoroutine(dataAssignCoroutine);
            assetObjectSearchLabel.TreeView.Clear();
            assetBundleSearchLabel.TreeView.Clear();
        }
        public void OnGUI(Rect rect)
        {
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
                assetBundleSearchLabel.OnGUI(rect);
                assetObjectSearchLabel.OnGUI(rect);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(16);
            GUILayout.BeginHorizontal();
            {

                GUILayout.BeginHorizontal(GUILayout.MinWidth(128));
                {
                    if (loadingQuarkObject)
                    {
                        EditorGUILayout.Space(8);
                        EditorGUILayout.LabelField($"Loading . . .  {currentLoadingBundleIndex}/{loadingBundleLength}", GUILayout.MaxWidth(128));
                        EditorGUILayout.Space(16);
                        EditorGUILayout.LabelField($"Progress . . .  {loadingObjectProgress}%", GUILayout.MaxWidth(128));
                    }
                    else
                    {
                        EditorGUILayout.Space(8);
                        EditorGUILayout.LabelField("Quark Asset Editor", GUILayout.MaxWidth(128));
                    }
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("ClearAllAssetBundle"))
                    {
                        QuarkEditorDataProxy.QuarkAssetDataset.QuarkBundleInfoList?.Clear();
                        assetBundleSearchLabel.TreeView.Clear();
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(16);
        }
        public EditorCoroutine BuildDataset()
        {
            return QuarkEditorUtility.StartCoroutine(EnumBuildDataset());
        }
        void ClearDataset()
        {
            if (dataset == null)
                return;
            dataset.Dispose();
            assetBundleSearchLabel.TreeView.Clear();
            assetObjectSearchLabel.TreeView.Clear();
            assetBundleSearchLabel.TreeView.Reload();
            assetObjectSearchLabel.TreeView.Reload();
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
            var bundleInfos = dataset.QuarkBundleInfoList;
            var extensions = dataset.QuarkAssetExts;
            List<QuarkObjectInfo> quarkSceneList = new List<QuarkObjectInfo>();
            List<QuarkBundleInfo> invalidBundleInfos = new List<QuarkBundleInfo>();
            var sceneAssetFullName = typeof(SceneAsset).FullName;
            int currentBundleIndex = 0;
            int bundleCount = bundleInfos.Count;
            foreach (var bundleInfo in bundleInfos)
            {
                currentBundleIndex++;
                var bundlePath = bundleInfo.BundlePath;
                if (!AssetDatabase.IsValidFolder(bundlePath))
                {
                    invalidBundleInfos.Add(bundleInfo);
                    continue;
                }
                bundleInfo.ObjectInfoList.Clear();
                bundleInfo.BundleSize = QuarkEditorUtility.GetUnityDirectorySize(bundlePath, QuarkEditorDataProxy.QuarkAssetDataset.QuarkAssetExts);
                bundleInfo.BundleFormatBytes = EditorUtility.FormatBytes(bundleInfo.BundleSize);
                bundleInfo.BundleKey = bundleInfo.BundleName;
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
                        var objectInfo = new QuarkObjectInfo()
                        {
                            ObjectName = Path.GetFileNameWithoutExtension(filePath),
                            ObjectExtension = lowerExt,
                            BundleName = bundleInfo.BundleName,
                            ObjectPath = lowerExtFilePath,
                            ObjectType = AssetDatabase.LoadAssetAtPath(filePath, typeof(Object)).GetType().FullName,
                            ObjectIcon = AssetDatabase.GetCachedIcon(filePath) as Texture2D,
                            ObjectSize = QuarkUtility.GetFileSize(filePath)
                        };
                        objectInfo.ObjectFormatBytes = EditorUtility.FormatBytes(objectInfo.ObjectSize);
                        if (objectInfo.ObjectType == sceneAssetFullName)
                        {
                            quarkSceneList.Add(objectInfo);
                        }
                        bundleInfo.ObjectInfoList.Add(objectInfo);
                    }
                }
                EditorUtility.DisplayCancelableProgressBar("QuarkAsset", "QuarkDataset Building", currentBundleIndex / (float)bundleCount);
                yield return null;
            }
            EditorUtility.ClearProgressBar();
            for (int i = 0; i < invalidBundleInfos.Count; i++)
            {
                bundleInfos.Remove(invalidBundleInfos[i]);
            }
            dataset.QuarkSceneList.Clear();
            dataset.QuarkSceneList.AddRange(quarkSceneList);

            EditorUtility.SetDirty(dataset);
            QuarkEditorUtility.SaveData(QuarkAssetDatabaseTabDataFileName, tabData);
            yield return null;
            yield return EnumOnAssignDataset(dataset);
#if UNITY_2021_1_OR_NEWER
            AssetDatabase.SaveAssetIfDirty(dataset);
#elif UNITY_2019_1_OR_NEWER
            AssetDatabase.SaveAssets();
#endif
            QuarkUtility.LogInfo("Quark asset  build done ");
        }
        IEnumerator EnumOnAssignDataset(QuarkDataset dataset)
        {
            loadingQuarkObject = true;
            loadingObjectProgress = 0;
            currentLoadingBundleIndex = 0;
            this.dataset = dataset;
            var bundles = dataset.QuarkBundleInfoList;
            var bundleLen = bundles.Count;
            loadingBundleLength = bundleLen;
            assetBundleSearchLabel.TreeView.Clear();
            assetObjectSearchLabel.TreeView.Clear();
            for (int i = 0; i < bundleLen; i++)
            {
                currentLoadingBundleIndex++;
                var bundle = bundles[i];
                assetBundleSearchLabel.TreeView.AddBundle(bundle);
                var objectInfos = bundle.ObjectInfoList;
                var objectInfoLength = objectInfos.Count;
                for (int j = 0; j < objectInfoLength; j++)
                {
                    var objectInfo = objectInfos[j];
                    assetObjectSearchLabel.TreeView.AddPath(objectInfo);
                    var progress = Mathf.RoundToInt((float)j / (objectInfoLength - 1) * 100);
                    loadingObjectProgress = progress > 0 ? progress : 0;
                }
                yield return null;
                assetObjectSearchLabel.TreeView.Reload();
            }
            yield return null;
            loadingQuarkObject = false;
            assetObjectSearchLabel.TreeView.Reload();
            assetBundleSearchLabel.TreeView.Reload();
        }
    }
}
