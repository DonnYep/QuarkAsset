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
        EditorWindow parentWindow = null;

        QuarkDataset dataset;
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

        Rect horizontalSplitterRect;
        Rect rightRect;
        Rect leftRect;
        Rect position;
        Rect treeViewRect;
        bool resizingHorizontalSplitter = false;
        float horizontalSplitterPercent;

        bool rectSplitterInited;

        long totalBundleSize;

        int selectedBundleCount;
        long selectedBundleSize;

        int selectedObjectCount;
        long selectedObjectSize;


        public void OnEnable(Rect pos, EditorWindow parentWindow)
        {
            this.parentWindow = parentWindow;
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
            bundleSearchLabel.OnBundleDelete += OnBundleDelete;
            bundleSearchLabel.OnAllDelete += OnAllBundleDelete;
            bundleSearchLabel.OnBundleSort += OnBundleSort;
            objectSearchLabel.OnObjectSelectionChanged += OnObjectSelectionChanged;
            InitRects(pos);
        }



        public void OnDisable()
        {
            QuarkEditorUtility.SaveData(QuarkAssetDatabaseTabDataFileName, tabData);
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
            totalBundleSize = 0;
            loadingBundleLength = bundleLen;
            bundleSearchLabel.TreeView.Clear();
            objectSearchLabel.TreeView.Clear();
            for (int i = 0; i < bundleLen; i++)
            {
                var bundle = bundles[i];
                bundleSearchLabel.TreeView.AddBundle(bundle);
                totalBundleSize += bundle.BundleSize;
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
            bundleDetailLabel.Clear();
            bundleDetailLabel.Reload();
            objectSearchLabel.TreeView.Clear();
            objectSearchLabel.TreeView.Reload();
            bundleSearchLabel.TreeView.Clear();
            bundleSearchLabel.TreeView.Reload();
            totalBundleSize = 0;
            selectedBundleCount = 0;
            selectedBundleSize = 0;
            selectedObjectCount = 0;
            selectedObjectSize = 0;
        }
        public void OnGUI(Rect rect)
        {
            this.position = rect;
            HandleHorizontalResize();
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


            treeViewRect = EditorGUILayout.BeginHorizontal();
            {
                GUILayout.BeginVertical();
                {
                    var width = GUILayout.Width((parentWindow.position.width * horizontalSplitterPercent - 6) / 2);

                    using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
                    {
                        GUILayout.Label($"Bundle:{bundleSearchLabel.TreeView.Count}/{QuarkUtility.FormatBytes(totalBundleSize)}", EditorStyles.label, width);
                        GUILayout.Label($"Selected:{selectedBundleCount}/{QuarkUtility.FormatBytes(selectedBundleSize)}", EditorStyles.label, width);
                    }
                    bundleSearchLabel.OnGUI(leftRect);
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                {
                    GUILayout.BeginHorizontal();
                    {
                        tabData.LabelTabIndex = EditorGUILayout.Popup(tabData.LabelTabIndex, tabArray, EditorStyles.toolbarPopup, GUILayout.MaxWidth(128));

                        GUILayout.Label($"Object:{objectSearchLabel.TreeView.Count}", EditorStyles.label);

                        GUILayout.Label($"Selected:{selectedObjectCount}/{QuarkUtility.FormatBytes(selectedObjectSize)}", EditorStyles.label);

                        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
                        {
                            if (tabData.LabelTabIndex == 0)
                            {
                                GUILayout.Label($"PreviewSize", EditorStyles.label, GUILayout.MaxWidth(92));
                                if (GUILayout.Button("-", GUILayout.MaxWidth(24)))
                                {
                                    tabData.LabelRowHeight -= 2;
                                }
                                tabData.LabelRowHeight = EditorGUILayout.IntSlider(tabData.LabelRowHeight, 18, 160);
                                if (GUILayout.Button("+", GUILayout.MaxWidth(24)))
                                {
                                    tabData.LabelRowHeight += 2;
                                }
                            }
                            else if (tabData.LabelTabIndex == 1)
                            {
                                GUILayout.Label($"Detail", EditorStyles.label);
                            }
                        }
                    }
                    GUILayout.EndHorizontal();

                    if (tabData.LabelTabIndex == 0)
                    {
                        objectSearchLabel.TreeView.TreeViewRowHeight = tabData.LabelRowHeight;
                        if (objectSearchLabel.TreeView.TreeViewRowHeight >= QuarkEditorConstant.DetailIconPreviewSize)
                        {
                            objectSearchLabel.TreeView.OnDetailPreview();
                        }
                        else
                        {
                            objectSearchLabel.TreeView.OnCachedIconPreview();
                        }
                        objectSearchLabel.OnGUI(rightRect);
                    }
                    else if (tabData.LabelTabIndex == 1)
                    {
                        bundleDetailLabel.OnGUI(rightRect);
                    }
                }
                GUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();

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
            OnDatasetUnassign();
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
                        subBundleInfo.BundleSize = QuarkEditorUtility.GetUnityDirectorySize(subBundlePath, QuarkEditorDataProxy.QuarkAssetDataset.QuarkAssetExts);
                        subBundleInfo.BundleFormatBytes = EditorUtility.FormatBytes(subBundleInfo.BundleSize);

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
            var bundleDict = bundleInfoList.ToDictionary(d => d.BundleKey);

            for (int i = 0; i < bundleInfoList.Count; i++)
            {
                var bundleInfo = bundleInfoList[i];
                var importer = AssetImporter.GetAtPath(bundleInfo.BundlePath);
                bundleInfo.DependentBundleKeyList.Clear();
                var dependencies = AssetDatabase.GetAssetBundleDependencies(importer.assetBundleName, true);
                for (int j = 0; j < dependencies.Length; j++)
                {
                    var dep = dependencies[j];
                    if (bundleDict.TryGetValue(dep, out var bInfo))
                    {
                        var depInfo = new QuarkBundleDependentInfo()
                        {
                            BundleKey = dep,
                            BundleName = bInfo.BundleName
                        };
                        bundleInfo.DependentBundleKeyList.Add(depInfo);
                    }
                }
            }
            for (int i = 0; i < bundleInfoList.Count; i++)
            {
                var bundleInfo = bundleInfoList[i];
                var importer = AssetImporter.GetAtPath(bundleInfo.BundlePath);
                importer.assetBundleName = string.Empty;
            }
            dataset.QuarkSceneList.Clear();
            dataset.QuarkSceneList.AddRange(quarkSceneList);

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

            OnDatasetRefresh();

            QuarkUtility.LogInfo("Quark dataset build done ");
            OnSelectionChanged(tabData.SelectedBundleIds);
            QuarkEditorDataProxy.QuarkAssetDataset.CacheAllBundleInfos();
        }
        void OnSelectionChanged(IList<int> selectedIds)
        {
            if (selectionCoroutine != null)
                QuarkEditorUtility.StopCoroutine(selectionCoroutine);
            selectionCoroutine = QuarkEditorUtility.StartCoroutine(EnumSelectionChanged(selectedIds));
        }
        void OnObjectSelectionChanged(List<QuarkObjectInfo> selectedObjects)
        {
            selectedObjectCount = selectedObjects.Count;
            var length = selectedObjectCount;
            selectedObjectSize = 0;
            for (int i = 0; i < length; i++)
            {
                var selectedObject = selectedObjects[i];
                selectedObjectSize += selectedObject.ObjectSize;
            }
        }
        IEnumerator EnumSelectionChanged(IList<int> selectedIds)
        {
            if (dataset == null)
                yield break;
            loadingQuarkObject = true;
            selectedBundleCount = selectedIds.Count;
            selectedBundleSize = 0;
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
                var bundleInfo = bundleInfos[id];
                selectedBundleSize += bundleInfo.BundleSize;
                bundleDetailLabel.AddBundle(bundleInfo);
                var objectInfos = bundleInfo.ObjectInfoList;
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
        void InitRects(Rect pos)
        {
            horizontalSplitterPercent = 0.312f;

            position = pos;
            var leftWidth = position.width * horizontalSplitterPercent;
            var rightWidth = position.width * (1 - horizontalSplitterPercent);
            horizontalSplitterRect = new Rect(leftWidth, 92, 5, position.height);
            rightRect = new Rect(0, 0, rightWidth, position.height);
            leftRect = new Rect(0, 0, leftWidth, position.height);
        }
        void HandleHorizontalResize()
        {
            EditorGUIUtility.AddCursorRect(horizontalSplitterRect, MouseCursor.ResizeHorizontal);
            if (Event.current.type == EventType.MouseDown && horizontalSplitterRect.Contains(Event.current.mousePosition))
                resizingHorizontalSplitter = true;
            horizontalSplitterRect.height = treeViewRect.height;
            if (!rectSplitterInited)
            {
                horizontalSplitterRect.x = position.width * horizontalSplitterPercent;
                rightRect.width = position.width * (1 - horizontalSplitterPercent);
                leftRect.width = position.width * horizontalSplitterPercent;
                parentWindow.Repaint();
                rectSplitterInited = true;
            }
            if (resizingHorizontalSplitter)
            {
                horizontalSplitterPercent = Mathf.Clamp(Event.current.mousePosition.x / position.width, 0.1f, 0.9f);
            }
            horizontalSplitterRect.x = position.width * horizontalSplitterPercent;
            rightRect.width = position.width * (1 - horizontalSplitterPercent);
            leftRect.width = position.width * horizontalSplitterPercent;
            parentWindow.Repaint();
            if (Event.current.type == EventType.MouseUp)
                resizingHorizontalSplitter = false;
        }

    }
}
