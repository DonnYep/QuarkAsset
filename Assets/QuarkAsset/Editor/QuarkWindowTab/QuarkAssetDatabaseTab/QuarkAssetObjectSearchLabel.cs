using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
namespace Quark.Editor
{
    public class QuarkAssetObjectSearchLabel
    {
        QuarkAssetObjectTreeView treeView;
        TreeViewState treeViewState;
        SearchField searchField;
        public QuarkAssetObjectTreeView TreeView { get { return treeView; } }

        public void OnEnable()
        {
            searchField = new SearchField();
            treeViewState = new TreeViewState();
            var multiColumnHeaderState = new MultiColumnHeader(QuarkEditorUtility.CreateObjectMultiColumnHeader());
            treeView = new QuarkAssetObjectTreeView(treeViewState, multiColumnHeaderState);
            searchField.downOrUpArrowKeyPressed += treeView.SetFocusAndEnsureSelectedItem;
        }
        public void OnGUI(Rect rect)
        {
            GUILayout.BeginVertical();
            DrawToolbar();
            DrawTreeView(rect);
            GUILayout.EndVertical();
        }
        void DrawToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            treeView.searchString = searchField.OnToolbarGUI(treeView.searchString);
            GUILayout.EndHorizontal();
        }
        void DrawTreeView(Rect rect)
        {
            GUILayout.BeginVertical(GUILayout.MaxWidth(rect.width * 0.6f));
            Rect viewRect = GUILayoutUtility.GetRect(32, 8192, 32, 8192);
            treeView.OnGUI(viewRect);
            GUILayout.EndVertical();
        }
    }
}
