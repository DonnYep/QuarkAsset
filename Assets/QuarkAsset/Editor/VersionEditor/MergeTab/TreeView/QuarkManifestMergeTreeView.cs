using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Quark.Asset;
using UnityEditor;

namespace Quark.Editor
{
    public class QuarkManifestMergeTreeView : TreeView
    {
        List<QuarkMergedBundleAsset> mergedBundleAssets = new List<QuarkMergedBundleAsset>();
        public QuarkManifestMergeTreeView(TreeViewState treeViewState, MultiColumnHeader multiColumnHeader)
: base(treeViewState, multiColumnHeader)
        {
            Reload();
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            multiColumnHeader.sortingChanged += OnMultiColumnHeaderSortingChanged;
        }
        public void SetManifest(QuarkMergedManifest mergedManifest)
        {
            mergedBundleAssets.Clear();
            if (mergedManifest == null)
                return;
            else
            {
                foreach (var mb in mergedManifest.MergedBundles)
                {
                    if (mb.IsIncremental)
                    {
                        if (QuarkManifestMergeLabelDataProxy.ShowIncremental)
                        {
                            mergedBundleAssets.Add(mb);
                        }
                    }
                    else
                    {
                        if (QuarkManifestMergeLabelDataProxy.ShowBuiltIn)
                        {
                            mergedBundleAssets.Add(mb);
                        }
                    }
                }
            }
            Reload();
        }
        public void Clear()
        {
            mergedBundleAssets.Clear();
            Reload();
        }
        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { id = -1, depth = -1, displayName = "Root" };
            var allItems = new List<TreeViewItem>();
            var addIcon = EditorGUIUtility.FindTexture("CollabCreate Icon");
            var defaultIcon = QuarkEditorUtility.GetFolderIcon();
            var length = mergedBundleAssets.Count;
            for (int i = 0; i < length; i++)
            {
                var mba = mergedBundleAssets[i];
                var item = new QuarkManifestMergeTreeViewItem(i, 1, mba.QuarkBundleAsset.BundleName)
                {
                    BundleFormatSize = QuarkUtility.FormatBytes(mba.QuarkBundleAsset.BundleSize),
                    BundleHash = mba.QuarkBundleAsset.Hash,
                    BundleKey = mba.QuarkBundleAsset.QuarkAssetBundle.BundleKey,
                    BundleSize = mba.QuarkBundleAsset.BundleSize,
                    ObjectCount = mba.QuarkBundleAsset.QuarkAssetBundle.ObjectList.Count,
                    IsIncremental = mba.IsIncremental,
                };
                if (mba.IsIncremental)
                {
                    item.icon = addIcon;
                }
                else
                {
                    item.icon = defaultIcon;
                }
                allItems.Add(item);
            }
            SetupParentsAndChildrenFromDepths(root, allItems);
            return root;
        }
        protected override void RowGUI(RowGUIArgs args)
        {
            var length = args.GetNumVisibleColumns();
            for (int i = 0; i < length; i++)
            {
                DrawCellGUI(args.GetCellRect(i), args.item as QuarkManifestMergeTreeViewItem, args.GetColumn(i), ref args);
            }
        }
        protected override void DoubleClickedItem(int id)
        {
            if (id < mergedBundleAssets.Count)
            {
                var rstInfo = mergedBundleAssets[id];
                var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(rstInfo.QuarkBundleAsset.QuarkAssetBundle.BundlePath);
                EditorGUIUtility.PingObject(obj);
                Selection.activeObject = obj;
            }
            base.DoubleClickedItem(id);
        }
        void DrawCellGUI(Rect cellRect, QuarkManifestMergeTreeViewItem treeView, int column, ref RowGUIArgs args)
        {
            switch (column)
            {
                case 0:
                    {
                        DefaultGUI.Label(cellRect, args.row.ToString(), args.selected, args.focused);
                    }
                    break;
                case 1:
                    {
                        var iconRect = new Rect(cellRect.x + 4, cellRect.y, cellRect.height, cellRect.height);
                        if (treeView.icon != null)
                            GUI.DrawTexture(iconRect, treeView.icon, ScaleMode.ScaleToFit);
                    }
                    break;
                case 2:
                    {
                        DefaultGUI.Label(cellRect, treeView.ObjectCount.ToString(), args.selected, args.focused);
                    }
                    break;
                case 3:
                    {
                        DefaultGUI.Label(cellRect, treeView.displayName, args.selected, args.focused);
                    }
                    break;
                case 4:
                    {
                        DefaultGUI.Label(cellRect, treeView.BundleFormatSize, args.selected, args.focused);
                    }
                    break;
                case 5:
                    {
                        DefaultGUI.Label(cellRect, treeView.BundleKey, args.selected, args.focused);
                    }
                    break;
                case 6:
                    {
                        DefaultGUI.Label(cellRect, treeView.BundleHash, args.selected, args.focused);
                    }
                    break;
            }
        }
        void OnMultiColumnHeaderSortingChanged(MultiColumnHeader multiColumnHeader)
        {
            var sortedColumns = multiColumnHeader.state.sortedColumns;
            if (sortedColumns.Length == 0)
                return;
            var sortedType = sortedColumns[0];
            var ascending = multiColumnHeader.IsSortedAscending(sortedType);
            switch (sortedType)
            {
                case 0://index
                    break;
                case 1://imcremental type
                    {
                        if (ascending)
                            mergedBundleAssets.Sort((lhs, rhs) => lhs.IsIncremental.CompareTo(rhs.IsIncremental));
                        else
                            mergedBundleAssets.Sort((lhs, rhs) => rhs.IsIncremental.CompareTo(lhs.IsIncremental));
                    }
                    break;
                case 2://ObjectCount
                    {
                        if (ascending)
                            mergedBundleAssets.Sort((lhs, rhs) => lhs.QuarkBundleAsset.QuarkAssetBundle.ObjectList.Count.CompareTo(rhs.QuarkBundleAsset.QuarkAssetBundle.ObjectList.Count));
                        else
                            mergedBundleAssets.Sort((lhs, rhs) => rhs.QuarkBundleAsset.QuarkAssetBundle.ObjectList.Count.CompareTo(lhs.QuarkBundleAsset.QuarkAssetBundle.ObjectList.Count));
                    }
                    break;
                case 3://BundleName
                    {
                        if (ascending)
                            mergedBundleAssets.Sort((lhs, rhs) => lhs.QuarkBundleAsset.BundleName.CompareTo(rhs.QuarkBundleAsset.BundleName));
                        else
                            mergedBundleAssets.Sort((lhs, rhs) => rhs.QuarkBundleAsset.BundleName.CompareTo(lhs.QuarkBundleAsset.BundleName));
                    }
                    break;
                case 4://BundleFormatSize
                    {
                        if (ascending)
                            mergedBundleAssets.Sort((lhs, rhs) => lhs.QuarkBundleAsset.BundleSize.CompareTo(rhs.QuarkBundleAsset.BundleSize));
                        else
                            mergedBundleAssets.Sort((lhs, rhs) => rhs.QuarkBundleAsset.BundleSize.CompareTo(lhs.QuarkBundleAsset.BundleSize));
                    }
                    break;
                case 5://BundleKey
                    {
                        if (ascending)
                            mergedBundleAssets.Sort((lhs, rhs) => lhs.QuarkBundleAsset.QuarkAssetBundle.BundleKey.CompareTo(rhs.QuarkBundleAsset.QuarkAssetBundle.BundleKey));
                        else
                            mergedBundleAssets.Sort((lhs, rhs) => rhs.QuarkBundleAsset.QuarkAssetBundle.BundleKey.CompareTo(lhs.QuarkBundleAsset.QuarkAssetBundle.BundleKey));
                    }
                    break;
                case 6://BundleHash
                    {
                        if (ascending)
                            mergedBundleAssets.Sort((lhs, rhs) => lhs.QuarkBundleAsset.Hash.CompareTo(rhs.QuarkBundleAsset.Hash));
                        else
                            mergedBundleAssets.Sort((lhs, rhs) => rhs.QuarkBundleAsset.Hash.CompareTo(lhs.QuarkBundleAsset.Hash));
                    }
                    break;
            }
            Reload();
        }
    }
}
