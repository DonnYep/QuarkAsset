using Quark.Asset;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Quark.Editor
{
    public class QuarkParseDependentTreeView : TreeView
    {
        QuarkManifest manifest;
        List<QuarkBundleAsset> selectedBundles = new List<QuarkBundleAsset>();
        public QuarkParseDependentTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader) : base(state, multiColumnHeader)
        {
            Reload();
            showAlternatingRowBackgrounds = true;
            showBorder = true;
        }
        public void SetManifest(QuarkManifest manifest)
        {
            this.manifest = manifest;
        }
        public void AddSelectBundles(IEnumerable<QuarkBundleAsset> bundles)
        {
            selectedBundles.Clear();
            selectedBundles.AddRange(bundles);
            Reload();
        }
        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { id = -1, depth = -1, displayName = "Root" };
            var allItems = new List<TreeViewItem>();
            var defaultIcon = QuarkEditorUtility.GetFolderIcon();
            var linkIcon = QuarkEditorUtility.GetFindDependenciesIcon();
            var length = selectedBundles.Count;
            for (int i = 0; i < length; i++)
            {
                var ba = selectedBundles[i];
                var rootNode = new QuarkParseDependentTreeViewItem(i, 1, ba.BundleName)
                {
                    IsLabelNode = true,
                    icon = defaultIcon
                };
                allItems.Add(rootNode);
                var dependents = ba.QuarkAssetBundle.DependentBundleKeyList;

                var labelNode = new QuarkParseDependentTreeViewItem((i + 1) * 10000 + 1, 2, $"Dependencies: - {dependents.Count}")
                {
                    IsLabelNode = true,
                    icon = linkIcon
                };
                allItems.Add(rootNode);

                var labelItems = new List<TreeViewItem>() { labelNode };

                SetupParentsAndChildrenFromDepths(rootNode, labelItems);

                var depItems = new List<TreeViewItem>();
                for (int j = 0; j < dependents.Count; j++)
                {
                    var depKey = dependents[j].BundleKey;
                    var hasDepBundle = manifest.BundleInfoDict.TryGetValue(depKey, out var depBundle);
                    if (hasDepBundle)
                    {
                        var dependentItem = new QuarkParseDependentTreeViewItem(labelNode.id + j + 1, 3, dependents[j].BundleName)
                        {
                            BundleFormatSize = QuarkUtility.FormatBytes(depBundle.BundleSize),
                            BundleHash = depBundle.Hash,
                            BundleSize = depBundle.BundleSize,
                            ObjectCount = depBundle.QuarkAssetBundle.ObjectList.Count,
                            icon = defaultIcon
                        };
                        depItems.Add(dependentItem);
                    }
                }
                SetupParentsAndChildrenFromDepths(labelNode, depItems);
            }
            SetupParentsAndChildrenFromDepths(root, allItems);
            return root;
        }
        protected override void RowGUI(RowGUIArgs args)
        {
            var item = args.item as QuarkParseDependentTreeViewItem;
            if (item.IsLabelNode)
            {
                base.RowGUI(args);
            }
            else
            {
                var length = args.GetNumVisibleColumns();
                for (int i = 0; i < length; i++)
                {
                    DrawCellGUI(args.GetCellRect(i), args.item as QuarkParseDependentTreeViewItem, args.GetColumn(i), ref args);
                }
            }
        }
        void DrawCellGUI(Rect cellRect, QuarkParseDependentTreeViewItem treeView, int column, ref RowGUIArgs args)
        {
            switch (column)
            {
                case 0:
                    {
                        DefaultGUI.Label(cellRect, treeView.BundleFormatSize, args.selected, args.focused);
                    }
                    break;
                case 1:
                    {
                        var iconRect = new Rect(cellRect.x + 4, cellRect.y, cellRect.height, cellRect.height);
                        if (treeView.icon != null)
                            GUI.DrawTexture(iconRect, treeView.icon, ScaleMode.ScaleToFit);
                        var labelCellRect = new Rect(cellRect.x + iconRect.width + 4, cellRect.y, cellRect.width - iconRect.width, cellRect.height);
                        DefaultGUI.Label(labelCellRect, treeView.displayName, args.selected, args.focused);
                    }
                    break;
                case 2:
                    {
                        DefaultGUI.Label(cellRect, treeView.ObjectCount.ToString(), args.selected, args.focused);
                    }
                    break;
                case 3:
                    {
                        DefaultGUI.Label(cellRect, treeView.BundleHash, args.selected, args.focused);
                    }
                    break;
            }
        }
    }
}
