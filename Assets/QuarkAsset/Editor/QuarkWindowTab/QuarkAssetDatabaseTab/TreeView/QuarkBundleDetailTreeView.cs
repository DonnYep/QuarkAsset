using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using Quark.Asset;

namespace Quark.Editor
{
    public class QuarkBundleDetailTreeView : TreeView
    {
        readonly List<QuarkBundleInfo> bundleInfoList = new List<QuarkBundleInfo>();
        public float TreeViewRowHeight
        {
            get { return rowHeight; }
            set { rowHeight = value; }
        }
        public int BundleDetailCount { get { return bundleInfoList.Count; } }
        public QuarkBundleDetailTreeView(TreeViewState state) : base(state)
        {
            Reload();
            showBorder = true;
            showAlternatingRowBackgrounds = true;
        }
        public bool AddBundle(QuarkBundleInfo bundleInfo)
        {
            if (!bundleInfoList.Contains(bundleInfo))
            {
                bundleInfoList.Add(bundleInfo);
                return true;
            }
            return false;
        }
        public void RemoveBundle(QuarkBundleInfo bundleInfo)
        {
            if (bundleInfoList.Contains(bundleInfo))
            {
                bundleInfoList.Remove(bundleInfo);
                Reload();
            }
        }
        public void Clear()
        {
            bundleInfoList.Clear();
        }
        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { id = -1, depth = -1, displayName = "Root" };
            var itemList = new List<TreeViewItem>();
            var folderIcon = QuarkEditorUtility.GetFolderIcon();
            var emptyFolderIcon = QuarkEditorUtility.GetFolderEmptyIcon();

            var bundleLength = bundleInfoList.Count;
            for (int i = 0; i < bundleLength; i++)
            {
                var bundleInfo = bundleInfoList[i];
                var bundleItem = new TreeViewItem(i, 1, bundleInfo.BundleName) { icon = folderIcon };
                itemList.Add(bundleItem);
                var dependentLen = bundleInfo.DependentBundleKeyList.Count;
                var dependentItemList = new List<TreeViewItem>();
                var dependentRootItem = new TreeViewItem((i + 1) * 10000 + 1, 2, $"Dependencies: - {bundleInfo.DependentBundleKeyList.Count}");
                var subBundleRootItem = new TreeViewItem((i + 1) * 10000 + 2, 2, $"SubBundles: - {bundleInfo.SubBundleInfoList.Count}");

                var subBundleItemList = new List<TreeViewItem>();

                if (bundleInfo.Splittable)
                {
                    var subBundleLength = bundleInfo.SubBundleInfoList.Count;
                    for (int j = 0; j < subBundleLength; j++)
                    {
                        var subBundle = bundleInfo.SubBundleInfoList[j];
                        int subBundleItemId = subBundleRootItem.id + j + 2 + 5000;//拆分子包区间数值
                        var subBundleItem = new TreeViewItem(subBundleItemId, 3, subBundle.BundleName) 
                        { 
                            icon=folderIcon
                        };
    
                        subBundleItemList.Add(subBundleItem);
                        SetupParentsAndChildrenFromDepths(subBundleRootItem, subBundleItemList);
                    }
                }

                for (int j = 0; j < dependentLen; j++)
                {
                    var bundleKey = bundleInfo.DependentBundleKeyList[j];
                    int dependentItemId = dependentRootItem.id + j + 2;
                    var dependentItem = new TreeViewItem(dependentItemId, 3, bundleKey)
                    {
                        icon = folderIcon
                    };
                    dependentItemList.Add(dependentItem);
                    SetupParentsAndChildrenFromDepths(dependentRootItem, dependentItemList);
                }

                var bundleSubItemList = new List<TreeViewItem>() { dependentRootItem, subBundleRootItem };
                SetupParentsAndChildrenFromDepths(bundleItem, bundleSubItemList);
            }
            SetupParentsAndChildrenFromDepths(root, itemList);
            return root;
        }
    }
}
