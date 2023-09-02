using UnityEngine;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using System;
using Quark.Asset;

namespace Quark.Editor
{
    public class QuarkObjectSearchLabel
    {
        QuarkObjectTreeView treeView;
        TreeViewState treeViewState;
        SearchField searchField;
        public QuarkObjectTreeView TreeView { get { return treeView; } }
        public event Action<List<QuarkObjectInfo>> OnObjectSelectionChanged
        {
            add { treeView.onObjectSelectionChanged += value; }
            remove { treeView.onObjectSelectionChanged -= value; }
        }
        public void OnEnable()
        {
            searchField = new SearchField();
            treeViewState = new TreeViewState();
            var multiColumnHeaderState = new MultiColumnHeader(QuarkEditorUtility.CreateObjectMultiColumnHeader());
            treeView = new QuarkObjectTreeView(treeViewState, multiColumnHeaderState);
            searchField.downOrUpArrowKeyPressed += treeView.SetFocusAndEnsureSelectedItem;
        }
        public void OnGUI(Rect rect)
        {
            GUILayout.BeginVertical();
            DrawTreeView(rect);
            GUILayout.EndVertical();
        }
        void DrawTreeView(Rect rect)
        {
            //GUILayout.BeginVertical(GUILayout.MaxWidth(rect.width * 0.62f));
            GUILayout.BeginVertical(GUILayout.Width(rect.width));
            {
                treeView.searchString = searchField.OnToolbarGUI(treeView.searchString);
                Rect viewRect = GUILayoutUtility.GetRect(32, 8192, 32, 8192);
                treeView.OnGUI(viewRect);
            }
            GUILayout.EndVertical();
        }
    }
}
