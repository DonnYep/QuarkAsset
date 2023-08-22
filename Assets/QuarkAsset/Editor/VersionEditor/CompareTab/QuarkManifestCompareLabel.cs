using Quark.Manifest;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Quark.Editor
{
    public class QuarkManifestCompareLabel
    {
        QuarkManifestCompareTreeView treeView;
        TreeViewState treeViewState;
        SearchField searchField;
        Rect buttonRect;
        QuarkManifestCompareTab owner;
        public QuarkManifestCompareLabel(QuarkManifestCompareTab owner)
        {
            this.owner = owner;
        }
        public void OnEnable()
        {
            searchField = new SearchField();
            treeViewState = new TreeViewState();
            var multiColumnHeaderState = new MultiColumnHeader(QuarkEditorUtility.CreateCompareInfoMultiColumnHeader());
            treeView = new QuarkManifestCompareTreeView(treeViewState, multiColumnHeaderState);
            searchField.downOrUpArrowKeyPressed += treeView.SetFocusAndEnsureSelectedItem;
        }
        public void OnGUI(Rect rect)
        {
            GUILayout.BeginVertical();
            DrawTreeView(rect);
            GUILayout.EndVertical();
        }
        public void SetResult(QuarkManifestCompareResult compareResult)
        {
            treeView.SetResult(compareResult);
        }
        public void Clear()
        {
            treeView.Clear();
        }
        void DrawTreeView(Rect rect)
        {
            GUILayout.BeginVertical(GUILayout.MaxWidth(rect.width));

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Bundle change type filter", EditorStyles.toolbarPopup, GUILayout.MaxWidth(192)))
                {
                    var popup = new ChangeTypePopup();
                    popup.onClose = () => owner.RefreshCompareResult();
                    PopupWindow.Show(buttonRect, popup);
                }
                if (Event.current.type == EventType.Repaint)
                    buttonRect = GUILayoutUtility.GetLastRect();

                treeView.searchString = searchField.OnToolbarGUI(treeView.searchString);
            }
            GUILayout.EndHorizontal();


            Rect viewRect = GUILayoutUtility.GetRect(32, 8192, 32, 8192);
            treeView.OnGUI(viewRect);
            GUILayout.EndVertical();
        }
    }
}
