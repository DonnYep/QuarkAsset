using Quark.Asset;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor.IMGUI.Controls;

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
        void OnMultiColumnHeaderSortingChanged(MultiColumnHeader multiColumnHeader)
        {
            var sortedColumns = multiColumnHeader.state.sortedColumns;
            if (sortedColumns.Length == 0)
                return;
            var sortedType = sortedColumns[0];
            var ascending = multiColumnHeader.IsSortedAscending(sortedType);
        }
    }
}
