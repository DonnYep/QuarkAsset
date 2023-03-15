using Quark.Asset;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Quark.Editor
{
    public class QuarkAssetDatabaseTab
    {
        QuarkAssetDatabaseTabData tabData;
        internal const string QuarkAssetDatabaseTabDataFileName = "QuarkAsset_DatabaseTabData.json";

        QuarkBundleSearchLabel bundleSearchLabel = new QuarkBundleSearchLabel();
        QuarkObjectSearchLabel objectSearchLabel = new QuarkObjectSearchLabel();
        QuarkBundleDetailLabel bundleDetailLabel = new QuarkBundleDetailLabel();

        QuarkDataset dataset;
        EditorCoroutine dataAssignCoroutine;
        /// <summary>
        /// 选中的协程；
        /// </summary>
        EditorCoroutine selectionCoroutine;
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
        string[] tabArray = new string[] { "Objects", "Dependencies" };

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
            bundleSearchLabel.OnEnable();
            objectSearchLabel.OnEnable();
            bundleDetailLabel.OnEnable();
            bundleSearchLabel.OnSelectionChanged += OnSelectionChanged;
            bundleSearchLabel.OnBundleDelete += OnBundleDelete; ;
            bundleSearchLabel.OnAllDelete += OnAllBundleDelete;
            bundleSearchLabel.OnBundleSort += OnBundleSort; ;
        }
        public void OnDisable()
        {
            QuarkEditorUtility.SaveData(QuarkAssetDatabaseTabDataFileName, tabData);
            if (dataAssignCoroutine != null)
                QuarkEditorUtility.StopCoroutine(dataAssignCoroutine);
            if (selectionCoroutine != null)
                QuarkEditorUtility.StopCoroutine(selectionCoroutine);
        }
        public void OnDatasetAssign()
        {
            if (QuarkEditorDataProxy.QuarkAssetDataset == null)
                return;
            dataset = QuarkEditorDataProxy.QuarkAssetDataset;
            var bundles = dataset.QuarkBundleInfoList;
            var bundleLen = bundles.Count;
            loadingBundleLength = bundleLen;
            bundleSearchLabel.TreeView.Clear();
            objectSearchLabel.TreeView.Clear();
            for (int i = 0; i < bundleLen; i++)
            {
                var bundle = bundles[i];
                bundleSearchLabel.TreeView.AddBundle(bundle);
            }
            objectSearchLabel.TreeView.Reload();
            bundleSearchLabel.TreeView.Reload();
        }
        public void OnDatasetRefresh()
        {
            OnDatasetAssign();
        }
        public void OnDatasetUnassign()
        {
            if (dataAssignCoroutine != null)
                QuarkEditorUtility.StopCoroutine(dataAssignCoroutine);
            objectSearchLabel.TreeView.Clear();
            objectSearchLabel.TreeView.Reload();
            bundleSearchLabel.TreeView.Clear();
            bundleSearchLabel.TreeView.Reload();
            bundleDetailLabel.Clear();
            bundleDetailLabel.Reload();
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
                GUILayout.BeginVertical();
                {
                    using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
                    {
                        GUILayout.Label($"Bundle", EditorStyles.label);
                    }
                    bundleSearchLabel.OnGUI(rect);
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                {
                    GUILayout.BeginHorizontal();
                    {
                        tabData.LabelTabIndex = EditorGUILayout.Popup(tabData.LabelTabIndex, tabArray, EditorStyles.toolbarPopup, GUILayout.MaxWidth(128));
                        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
                        {
                            GUILayout.Label($"Detail", EditorStyles.label);
                        }
                    }
                    GUILayout.EndHorizontal();

                    if (tabData.LabelTabIndex == 0)
                    {
                        objectSearchLabel.OnGUI(rect);
                    }
                    else if (tabData.LabelTabIndex == 1)
                    {
                        bundleDetailLabel.OnGUI(rect);
                    }
                }
                GUILayout.EndVertical();
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
                        bundleSearchLabel.TreeView.Clear();
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
        void OnAllBundleDelete()
        {
            dataset.QuarkBundleInfoList.Clear();
            tabData.SelectedBundleIds.Clear();
            objectSearchLabel.TreeView.Clear();
            objectSearchLabel.TreeView.Reload();
            bundleSearchLabel.TreeView.Clear();
            bundleSearchLabel.TreeView.Reload();
            EditorUtility.SetDirty(dataset);
            QuarkEditorUtility.SaveData(QuarkAssetDatabaseTabDataFileName, tabData);
        }
        void OnBundleDelete(IList<int> bundleIds, IList<int> selectedIds)
        {
            if (dataset == null)
                return;
            if (selectionCoroutine != null)
                QuarkEditorUtility.StopCoroutine(selectionCoroutine);
            var bundleInfos = dataset.QuarkBundleInfoList;
            var rmlen = bundleIds.Count;
            var rmbundleInfos = new QuarkBundleInfo[rmlen];
            for (int i = 0; i < rmlen; i++)
            {
                var rmid = bundleIds[i];
                rmbundleInfos[i] = bundleInfos[rmid];
                tabData.SelectedBundleIds.Remove(rmid);
            }
            for (int i = 0; i < rmlen; i++)
            {
                bundleInfos.Remove(rmbundleInfos[i]);
            }
            objectSearchLabel.TreeView.Clear();
            objectSearchLabel.TreeView.Reload();
            bundleSearchLabel.TreeView.Reload();
            OnSelectionChanged(selectedIds);
        }
        void OnBundleSort(IList<int> selectedIds)
        {
            OnSelectionChanged(selectedIds);
        }
        void ClearDataset()
        {
            if (dataset == null)
                return;
            dataset.Dispose();
            bundleDetailLabel.Clear();
            bundleDetailLabel.Reload();
            bundleSearchLabel.TreeView.Clear();
            objectSearchLabel.TreeView.Clear();
            bundleSearchLabel.TreeView.Reload();
            objectSearchLabel.TreeView.Reload();
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
            bundleDetailLabel.Clear();

            EditorUtility.ClearProgressBar();
            var bundleInfoList = dataset.QuarkBundleInfoList;
            var extensions = dataset.QuarkAssetExts;
            List<QuarkObjectInfo> quarkSceneList = new List<QuarkObjectInfo>();
            List<QuarkBundleInfo> invalidBundleInfos = new List<QuarkBundleInfo>();
            var sceneAssetFullName = typeof(SceneAsset).FullName;
            int currentBundleIndex = 0;
            int bundleCount = bundleInfoList.Count;
            foreach (var bundleInfo in bundleInfoList)
            {
                currentBundleIndex++;
                var bundlePath = bundleInfo.BundlePath;
                if (!AssetDatabase.IsValidFolder(bundlePath))
                {
                    invalidBundleInfos.Add(bundleInfo);
                    continue;
                }
                bundleInfo.SubBundleInfoList.Clear();
                #region SunBundlle
                if (bundleInfo.Splittable)
                {
                    var subBundlePaths = AssetDatabase.GetSubFolders(bundlePath);
                    var subBundlePathLength = subBundlePaths.Length;
                    for (int i = 0; i < subBundlePathLength; i++)
                    {
                        var subBundlePath = subBundlePaths[i];
                        var isSceneInSameBundle = QuarkUtility.CheckAssetsAndScenesInOneAssetBundle(subBundlePath);
                        if (isSceneInSameBundle)
                        {
                            QuarkUtility.LogError($"Cannot mark assets and scenes in one AssetBundle. AssetBundle name is {subBundlePath}");
                            continue;
                        }
                        var subBundleInfo = new QuarkSubBundleInfo()
                        {
                            BundleName = subBundlePath,
                            BundlePath = subBundlePath
                        };
                        if (!bundleInfo.SubBundleInfoList.Contains(subBundleInfo))
                        {
                            bundleInfo.SubBundleInfoList.Add(subBundleInfo);
                            subBundleInfo.BundleKey = subBundleInfo.BundleName;
                        }
                        var subImporter = AssetImporter.GetAtPath(subBundleInfo.BundlePath);
                        subImporter.assetBundleName = subBundleInfo.BundleName;
                        var subFilePaths = Directory.GetFiles(subBundlePath, ".", SearchOption.AllDirectories);
                        var subFileLength = subFilePaths.Length;
                        for (int j = 0; j < subFileLength; j++)
                        {
                            //强制将文件的后缀名统一成小写
                            var subFilePath = subFilePaths[j].Replace("\\", "/");
                            var srcFileExt = Path.GetExtension(subFilePath);
                            var lowerExt = srcFileExt.ToLower();
                            var lowerExtFilePath = subFilePath.Replace(srcFileExt, lowerExt);
                            if (extensions.Contains(lowerExt))
                            {
                                var objectInfo = new QuarkObjectInfo()
                                {
                                    ObjectName = Path.GetFileNameWithoutExtension(subFilePath),
                                    ObjectExtension = lowerExt,
                                    BundleName = subBundleInfo.BundleName,
                                    ObjectPath = lowerExtFilePath,
                                    ObjectType = AssetDatabase.LoadAssetAtPath(subFilePath, typeof(Object)).GetType().FullName,
                                    ObjectSize = QuarkUtility.GetFileSize(subFilePath)
                                };
                                objectInfo.ObjectValid = AssetDatabase.LoadMainAssetAtPath(objectInfo.ObjectPath) != null;
                                objectInfo.ObjectFormatBytes = EditorUtility.FormatBytes(objectInfo.ObjectSize);
                                if (objectInfo.ObjectType == sceneAssetFullName)
                                {
                                    quarkSceneList.Add(objectInfo);
                                }
                                subBundleInfo.ObjectInfoList.Add(objectInfo);
                            }
                        }
                    }
                }
                #endregion
                //else

                {
                    //清除分包带来的影响
                    var subBundlePaths = AssetDatabase.GetSubFolders(bundlePath);
                    var subBundlePathLength = subBundlePaths.Length;
                    for (int j = 0; j < subBundlePathLength; j++)
                    {
                        var subImporter = AssetImporter.GetAtPath(subBundlePaths[j]);
                        subImporter.assetBundleName = string.Empty; ;
                    }
                    AssetDatabase.Refresh();

                    var importer = AssetImporter.GetAtPath(bundleInfo.BundlePath);
                    importer.assetBundleName = bundleInfo.BundleName;

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
                                ObjectSize = QuarkUtility.GetFileSize(filePath)
                            };
                            objectInfo.ObjectValid = AssetDatabase.LoadMainAssetAtPath(objectInfo.ObjectPath) != null;
                            objectInfo.ObjectFormatBytes = EditorUtility.FormatBytes(objectInfo.ObjectSize);
                            if (objectInfo.ObjectType == sceneAssetFullName)
                            {
                                quarkSceneList.Add(objectInfo);
                            }
                            bundleInfo.ObjectInfoList.Add(objectInfo);
                        }
                    }
                }
                EditorUtility.DisplayCancelableProgressBar("QuarkAsset", "QuarkDataset Building", currentBundleIndex / (float)bundleCount);
                yield return null;
            }
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            for (int i = 0; i < invalidBundleInfos.Count; i++)
            {
                bundleInfoList.Remove(invalidBundleInfos[i]);
            }
            for (int i = 0; i < bundleInfoList.Count; i++)
            {
                var bundleInfo = bundleInfoList[i];
                var importer = AssetImporter.GetAtPath(bundleInfo.BundlePath);
                bundleInfo.DependentBundleKeyList.Clear();
                bundleInfo.DependentBundleKeyList.AddRange(AssetDatabase.GetAssetBundleDependencies(importer.assetBundleName, true));
            }
            for (int i = 0; i < bundleInfoList.Count; i++)
            {
                var bundleInfo = bundleInfoList[i];
                var importer = AssetImporter.GetAtPath(bundleInfo.BundlePath);
                importer.assetBundleName = string.Empty;
            }
            dataset.QuarkSceneList.Clear();
            dataset.QuarkSceneList.AddRange(quarkSceneList);

            dataset.RegenerateBundleInfoDict();

            EditorUtility.SetDirty(dataset);
            QuarkEditorUtility.SaveData(QuarkAssetDatabaseTabDataFileName, tabData);
            yield return null;
            OnSelectionChanged(tabData.SelectedBundleIds);
#if UNITY_2021_1_OR_NEWER
            AssetDatabase.SaveAssetIfDirty(dataset);
#elif UNITY_2019_1_OR_NEWER
            AssetDatabase.SaveAssets();
#endif
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            bundleSearchLabel.TreeView.Reload();

            QuarkUtility.LogInfo("Quark asset  build done ");
            OnSelectionChanged(tabData.SelectedBundleIds);
        }
        void OnSelectionChanged(IList<int> selectedIds)
        {
            if (selectionCoroutine != null)
                QuarkEditorUtility.StopCoroutine(selectionCoroutine);
            selectionCoroutine = QuarkEditorUtility.StartCoroutine(EnumSelectionChanged(selectedIds));
        }
        IEnumerator EnumSelectionChanged(IList<int> selectedIds)
        {
            if (dataset == null)
                yield break;
            loadingQuarkObject = true;

            var bundleInfos = dataset.QuarkBundleInfoList;

            var idLength = selectedIds.Count;

            bundleDetailLabel.Clear();
            bundleDetailLabel.Reload();

            objectSearchLabel.TreeView.Clear();
            objectSearchLabel.TreeView.Reload();
            for (int i = 0; i < idLength; i++)
            {
                currentLoadingBundleIndex++;

                var id = selectedIds[i];
                if (id >= bundleInfos.Count)
                    continue;
                bundleDetailLabel.AddBundle(bundleInfos[id]);
                var objectInfos = bundleInfos[id].ObjectInfoList;
                var objectInfoLength = objectInfos.Count;
                for (int j = 0; j < objectInfoLength; j++)
                {
                    var objectInfo = objectInfos[j];
                    objectSearchLabel.TreeView.AddPath(objectInfo);
                    var progress = Mathf.RoundToInt((float)j / (objectInfoLength - 1) * 100);
                    loadingObjectProgress = progress > 0 ? progress : 0;
                }
                yield return null;
                objectSearchLabel.TreeView.Reload();

                bundleDetailLabel.Reload();
            }
            yield return null;
            loadingQuarkObject = false;
            tabData.SelectedBundleIds.Clear();
            tabData.SelectedBundleIds.AddRange(selectedIds);
            QuarkEditorUtility.SaveData(QuarkAssetDatabaseTabDataFileName, tabData);
        }
    }
}
