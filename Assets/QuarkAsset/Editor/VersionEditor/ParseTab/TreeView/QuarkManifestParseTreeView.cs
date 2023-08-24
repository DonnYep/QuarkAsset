using Quark.Asset;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Quark.Editor
{
    public class QuarkManifestParseTreeView : TreeView
    {
        List<QuarkBundleAsset> bundleAssets = new List<QuarkBundleAsset>();
        public QuarkManifestParseTreeView(TreeViewState treeViewState, MultiColumnHeader multiColumnHeader)
: base(treeViewState, multiColumnHeader)
        {
            Reload();
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            multiColumnHeader.sortingChanged += OnMultiColumnHeaderSortingChanged;
        }
        public void SetManifest(QuarkManifest manifest)
        {
            bundleAssets.Clear();
            if (manifest != null)
            {
                bundleAssets.AddRange(manifest.BundleInfoDict.Values);
            }
            Reload();
        }
        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { id = -1, depth = -1, displayName = "Root" };
            var allItems = new List<TreeViewItem>();
            var defaultIcon = QuarkEditorUtility.GetFolderIcon();
            var length = bundleAssets.Count;
            for (int i = 0; i < length; i++)
            {
                var ba = bundleAssets[i];
                var item = new QuarkManifestMergeTreeViewItem(i, 1, ba.BundleName)
                {
                    BundleFormatSize = QuarkUtility.FormatBytes(ba.BundleSize),
                    BundleHash = ba.Hash,
                    BundleKey = ba.QuarkAssetBundle.BundleKey,
                    BundleSize = ba.BundleSize,
                    ObjectCount = ba.QuarkAssetBundle.ObjectList.Count,
                    icon = defaultIcon
                };
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
            if (id < bundleAssets.Count)
            {
                var rstInfo = bundleAssets[id];
                var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(rstInfo.QuarkAssetBundle.BundlePath);
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
                        DefaultGUI.Label(cellRect, treeView.ObjectCount.ToString(), args.selected, args.focused);
                    }
                    break;
                case 2:
                    {
                        var iconRect = new Rect(cellRect.x + 4, cellRect.y, cellRect.height, cellRect.height);
                        if (treeView.icon != null)
                            GUI.DrawTexture(iconRect, treeView.icon, ScaleMode.ScaleToFit);
                        var labelCellRect = new Rect(cellRect.x + iconRect.width + 4, cellRect.y, cellRect.width - iconRect.width, cellRect.height);
                        DefaultGUI.Label(labelCellRect, treeView.displayName, args.selected, args.focused);
                    }
                    break;
                case 3:
                    {
                        DefaultGUI.Label(cellRect, treeView.BundleFormatSize, args.selected, args.focused);
                    }
                    break;
                case 4:
                    {
                        DefaultGUI.Label(cellRect, treeView.BundleKey, args.selected, args.focused);
                    }
                    break;
                case 5:
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
                case 1://object count
                    {
                        if (ascending)
                            bundleAssets.Sort((lhs, rhs) => lhs.QuarkAssetBundle.ObjectList.Count.CompareTo(rhs.QuarkAssetBundle.ObjectList.Count));
                        else
                            bundleAssets.Sort((lhs, rhs) => rhs.QuarkAssetBundle.ObjectList.Count.CompareTo(lhs.QuarkAssetBundle.ObjectList.Count));
                    }
                    break;
                case 2://BundleName
                    {
                        if (ascending)
                            bundleAssets.Sort((lhs, rhs) => lhs.BundleName.CompareTo(rhs.BundleName));
                        else
                            bundleAssets.Sort((lhs, rhs) => rhs.BundleName.CompareTo(lhs.BundleName));
                    }
                    break;
                case 3://BundleFormatSize
                    {
                        if (ascending)
                            bundleAssets.Sort((lhs, rhs) => lhs.BundleSize.CompareTo(rhs.BundleSize));
                        else
                            bundleAssets.Sort((lhs, rhs) => rhs.BundleSize.CompareTo(lhs.BundleSize));
                    }
                    break;
                case 4://BundleKey
                    {
                        if (ascending)
                            bundleAssets.Sort((lhs, rhs) => lhs.QuarkAssetBundle.BundleKey.CompareTo(rhs.QuarkAssetBundle.BundleKey));
                        else
                            bundleAssets.Sort((lhs, rhs) => rhs.QuarkAssetBundle.BundleKey.CompareTo(lhs.QuarkAssetBundle.BundleKey));
                    }
                    break;
                case 5://BundleKey
                    {
                        if (ascending)
                            bundleAssets.Sort((lhs, rhs) => lhs.Hash.CompareTo(rhs.Hash));
                        else
                            bundleAssets.Sort((lhs, rhs) => rhs.Hash.CompareTo(lhs.Hash));
                    }
                    break;
            }
            Reload();
        }
    }
}
