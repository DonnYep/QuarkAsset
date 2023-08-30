using Quark.Asset;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Quark.Editor
{
    public class QuarkBundleDetailLabel
    {
        SearchField searchField;
        TreeViewState treeViewState;
        QuarkBundleDetailTreeView treeView;
        public int BundleDetailCount { get { return treeView.BundleDetailCount; } }
        public void OnEnable()
        {
            searchField = new SearchField();
            treeViewState = new TreeViewState();
            var multiColumnHeaderState = new MultiColumnHeader(QuarkEditorUtility.CreateBundleDetailMultiColumnHeader());
            treeView = new QuarkBundleDetailTreeView(treeViewState, multiColumnHeaderState);
            searchField.downOrUpArrowKeyPressed += treeView.SetFocusAndEnsureSelectedItem;
        }
        public void Clear()
        {
            treeView.Clear();
        }
        public void Reload()
        {
            treeView.Reload();
        }
        public void AddBundle(QuarkBundleInfo bundleInfo)
        {
            treeView.AddBundle(bundleInfo);
        }
        public void OnGUI(Rect rect)
        {
            GUILayout.BeginVertical();
            DrawTreeView(rect);
            GUILayout.EndVertical();
        }
        public void SetSelection(IList<int> selectedIds)
        {
            var bundleInfos = QuarkEditorDataProxy.QuarkAssetDataset.QuarkBundleInfoList;
            var idlen = selectedIds.Count;
            treeView.Clear();
            for (int i = 0; i < idlen; i++)
            {
                var id = selectedIds[i];
                if (id >= bundleInfos.Count)
                    continue;
                var bundleInfo = bundleInfos[id];
                treeView.AddBundle(bundleInfo);
            }
            treeView.Reload();
        }
        void DrawTreeView(Rect rect)
        {
            //var width = rect.width * 0.62f;
            //GUILayout.BeginVertical(GUILayout.MaxWidth(width));
            GUILayout.BeginVertical(GUILayout.MaxWidth(rect.width));
            {
                GUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Search", GUILayout.MaxWidth(48));
                    treeView.searchString = searchField.OnToolbarGUI(treeView.searchString);
                    if (GUILayout.Button("ExpandAll", EditorStyles.miniButton, GUILayout.MaxWidth(92)))
                    {
                        treeView.ExpandAll();
                    }
                    if (GUILayout.Button("CollapseAll", EditorStyles.miniButton, GUILayout.MaxWidth(92)))
                    {
                        treeView.CollapseAll();
                    }
                }
                GUILayout.EndHorizontal();
                Rect viewRect = GUILayoutUtility.GetRect(32, 8192, 32, 8192);
                treeView.OnGUI(viewRect);
            }
            GUILayout.EndVertical();
        }
    }
}