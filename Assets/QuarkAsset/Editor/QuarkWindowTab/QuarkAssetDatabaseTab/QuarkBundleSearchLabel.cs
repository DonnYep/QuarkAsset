using Quark.Asset;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Object=UnityEngine.Object;
namespace Quark.Editor
{
    public class QuarkBundleSearchLabel
    {
        QuarkBundleTreeView treeView;
        TreeViewState treeViewState;
        SearchField searchField;
        public QuarkBundleTreeView TreeView { get { return treeView; } }
        Rect lableRect;
        public event Action<IList<int>> OnSelectionChanged
        {
            add { treeView.onSelectionChanged += value; }
            remove { treeView.onSelectionChanged -= value; }
        }
        public event Action<IList<int>> OnDelete
        {
            add { treeView.onDelete += value; }
            remove { treeView.onDelete -= value; }
        }
        public event Action OnAllDelete
        {
            add { treeView.onAllDelete += value; }
            remove { treeView.onAllDelete -= value; }
        }
        public void OnEnable()
        {
            searchField = new SearchField();
            treeViewState = new TreeViewState();
            var multiColumnHeaderState = new MultiColumnHeader(QuarkEditorUtility.CreateBundleMultiColumnHeader());
            treeView = new QuarkBundleTreeView(treeViewState, multiColumnHeaderState);
            searchField.downOrUpArrowKeyPressed += treeView.SetFocusAndEnsureSelectedItem;
        }
        public void OnGUI(Rect rect)
        {
            lableRect = rect;
            GUILayout.BeginVertical();
            {
                DrawDragRect();
                DrawToolbar();
                DrawTreeView(rect);
            }
            GUILayout.EndVertical();
        }
        void DrawDragRect()
        {
            if (QuarkEditorDataProxy.QuarkAssetDataset== null)
                return;
            if (Event.current.type == EventType.DragUpdated)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                Event.current.Use();
            }
            else if (Event.current.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                if (DragAndDrop.paths.Length == 0 && DragAndDrop.objectReferences.Length > 0)
                {
                    foreach (Object obj in DragAndDrop.objectReferences)
                    {
                        QuarkUtility.LogInfo("- " + obj);
                    }
                }
                else if (DragAndDrop.paths.Length > 0 && DragAndDrop.objectReferences.Length == 0)
                {
                    foreach (string path in DragAndDrop.paths)
                    {
                        QuarkUtility.LogInfo("- " + path);
                    }
                }
                else if (DragAndDrop.paths.Length == DragAndDrop.objectReferences.Length)
                {
                    for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                    {
                        Object obj = DragAndDrop.objectReferences[i];
                        string path = DragAndDrop.paths[i];
                        if (!(obj is MonoScript) && (obj is DefaultAsset))
                        {
                            var bundleInfoList = QuarkEditorDataProxy.QuarkAssetDataset.QuarkBundleInfoList;
                            var isInSameBundle = QuarkUtility.CheckAssetsAndScenesInOneAssetBundle(path);
                            if (isInSameBundle)
                            {
                                QuarkUtility.LogError($"Cannot mark assets and scenes in one AssetBundle. AssetBundle name is {path}");
                                continue;
                            }
                            var bundleInfo = new QuarkBundleInfo()
                            {
                                BundleName = path,
                                BundlePath = path
                            };
                            if (!bundleInfoList.Contains(bundleInfo))
                            {
                                bundleInfoList.Add(bundleInfo);
                                bundleInfo.BundleKey = bundleInfo.BundleName;
                                treeView.AddBundle(bundleInfo);
                            }
                        }
                        treeView.Reload();
                    }
                }
                else
                {
                    QuarkUtility.LogInfo("Out of reach");
                    QuarkUtility.LogInfo("Paths:");
                    foreach (string path in DragAndDrop.paths)
                    {
                        QuarkUtility.LogInfo("- " + path);
                    }
                    QuarkUtility.LogInfo("ObjectReferences:");
                    foreach (Object obj in DragAndDrop.objectReferences)
                    {
                        QuarkUtility.LogInfo("- " + obj);
                    }
                }
            }
        }
        void DrawToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            treeView.searchString = searchField.OnToolbarGUI(treeView.searchString);
            GUILayout.EndHorizontal();
        }
        void DrawTreeView(Rect rect)
        {
            GUILayout.BeginVertical(GUILayout.MaxWidth(rect.width * 0.4f));
            Rect viewRect = GUILayoutUtility.GetRect(32, 8192, 32, 8192);
            treeView.OnGUI(viewRect);
            GUILayout.EndVertical();
        }
    }
}
