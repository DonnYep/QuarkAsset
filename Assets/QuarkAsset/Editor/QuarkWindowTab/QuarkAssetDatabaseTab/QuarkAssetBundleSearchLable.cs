using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
namespace Quark.Editor
{
    public class QuarkAssetBundleSearchLable
    {
        QuarkAssetBundleTreeView treeView;
        TreeViewState treeViewState;
        SearchField searchField;
        public QuarkAssetBundleTreeView TreeView { get { return treeView; } }
        public void OnEnable()
        {
            searchField = new SearchField();
            treeViewState = new TreeViewState();
            treeView = new QuarkAssetBundleTreeView(treeViewState);
            searchField.downOrUpArrowKeyPressed += treeView.SetFocusAndEnsureSelectedItem;
        }
        public void OnGUI()
        {
            GUILayout.BeginVertical();
            DrawDragRect();
            DrawToolbar();
            DrawTreeView();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("ClearAllAssetBundle"))
            {
                QuarkEditorDataProxy.QuarkAssetDataset.QuarkAssetBundleList?.Clear();
                treeView.Clear();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
        void DrawDragRect()
        {
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
                        if (!(obj is MonoScript))
                        {
                            var isInSameBundle = QuarkUtility.CheckAssetsAndScenesInOneAssetBundle(path);
                            if (isInSameBundle)
                            {
                                QuarkUtility.LogError($"Cannot mark assets and scenes in one AssetBundle. AssetBundle name is {path}");
                                continue;
                            }
                            treeView.AddPath(path);
                        }
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
        void DrawTreeView()
        {
            GUILayout.BeginVertical("box");
            Rect rect = GUILayoutUtility.GetRect(32, 8192, 32, 8192);
            treeView.OnGUI(rect);
            GUILayout.EndVertical();
        }
    }
}
