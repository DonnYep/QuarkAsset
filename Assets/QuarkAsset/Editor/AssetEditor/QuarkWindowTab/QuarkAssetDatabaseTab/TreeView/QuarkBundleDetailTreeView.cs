using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using Quark.Asset;
using UnityEngine;
using System.Linq;
using UnityEditor;

namespace Quark.Editor
{
    public class QuarkBundleDetailTreeView : TreeView
    {
        readonly List<QuarkBundleInfo> bundleInfoList = new List<QuarkBundleInfo>();
        Dictionary<string, IQuarkBundleInfo> bundleDict;
        public int BundleDetailCount { get { return bundleInfoList.Count; } }
        public QuarkBundleDetailTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader) : base(state, multiColumnHeader)
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
            if (QuarkEditorDataProxy.QuarkAssetDataset != null)
                bundleDict = QuarkEditorDataProxy.QuarkAssetDataset.GetCacheAllBundleInfos().ToDictionary(b => b.BundleName);
            else
                bundleDict = new Dictionary<string, IQuarkBundleInfo>();

            var root = new TreeViewItem { id = -1, depth = -1, displayName = "Root" };
            var itemList = new List<TreeViewItem>();
            var folderIcon = QuarkEditorUtility.GetFolderIcon();
            var emptyFolderIcon = QuarkEditorUtility.GetFolderEmptyIcon();
            var bundleLength = bundleInfoList.Count;
            for (int i = 0; i < bundleLength; i++)
            {
                var bundleInfo = bundleInfoList[i];
                var bundleItem = new QuarkBundleDetailTreeViewItem(i, 1, bundleInfo.BundleName)
                {
                    icon = folderIcon,
                    BundleDetailType = QuarkBundleDetailType.LabelBundle,
                    ObjectCount = bundleInfo.ObjectInfoList.Count
                };
                itemList.Add(bundleItem);

                var dependentLen = bundleInfo.DependentBundleKeyList.Count;
                var dependentRootItem = new QuarkBundleDetailTreeViewItem((i + 1) * 10000 + 1, 2, $"Dependencies: - {bundleInfo.DependentBundleKeyList.Count}");
                dependentRootItem.BundleDetailType = QuarkBundleDetailType.LabelBundle;
                var subBundleRootItem = new QuarkBundleDetailTreeViewItem((i + 1) * 10000 + 2, 2, $"SubBundles: - {bundleInfo.SubBundleInfoList.Count}");
                subBundleRootItem.BundleDetailType = QuarkBundleDetailType.LabelBundle;

                var dependentItemList = new List<TreeViewItem>();
                var subBundleItemList = new List<TreeViewItem>();

                if (bundleInfo.Splittable)
                {
                    var subBundleLength = bundleInfo.SubBundleInfoList.Count;
                    for (int j = 0; j < subBundleLength; j++)
                    {
                        var subBundle = bundleInfo.SubBundleInfoList[j];
                        int subBundleItemId = subBundleRootItem.id + j + 2 + 5000;//拆分子包区间数值
                        var subBundleItem = new QuarkBundleDetailTreeViewItem(subBundleItemId, 3, subBundle.BundleName)
                        {
                            icon = folderIcon,
                            BundleDetailType = QuarkBundleDetailType.SubBundle,
                            ObjectCount = subBundle.ObjectInfoList.Count,
                            BundlePath = subBundle.BundlePath
                        };
                        var has = bundleDict.TryGetValue(subBundle.BundleName, out var srcbundle);
                        if (has)
                        {
                            subBundleItem.BundleSize = srcbundle.BundleFormatBytes;
                        }
                        subBundleItemList.Add(subBundleItem);
                        SetupParentsAndChildrenFromDepths(subBundleRootItem, subBundleItemList);
                    }
                }

                for (int j = 0; j < dependentLen; j++)
                {
                    var dependentBundle = bundleInfo.DependentBundleKeyList[j];
                    int dependentItemId = dependentRootItem.id + j + 2;
                    var dependentItem = new QuarkBundleDetailTreeViewItem(dependentItemId, 3, dependentBundle.BundleName)
                    {
                        icon = folderIcon,
                        BundleDetailType = QuarkBundleDetailType.DependentBundle,
                    };
                    var has = bundleDict.TryGetValue(dependentBundle.BundleName, out var srcbundle);
                    if (has)
                    {
                        dependentItem.BundleSize = srcbundle.BundleFormatBytes;
                        dependentItem.ObjectCount = srcbundle.ObjectInfoList.Count;
                        dependentItem.BundlePath = srcbundle.BundlePath;
                    }
                    dependentItemList.Add(dependentItem);
                    SetupParentsAndChildrenFromDepths(dependentRootItem, dependentItemList);
                }

                var bundleSubItemList = new List<TreeViewItem>() { dependentRootItem, subBundleRootItem };
                SetupParentsAndChildrenFromDepths(bundleItem, bundleSubItemList);
            }
            SetupParentsAndChildrenFromDepths(root, itemList);
            return root;
        }
        protected override void DoubleClickedItem(int id)
        {
            if (QuarkEditorDataProxy.QuarkAssetDataset == null)
                return;
            var item = FindItemInVisibleRows(id);
            if (item != null)
            {
                if (item.BundleDetailType != QuarkBundleDetailType.LabelBundle)
                {
                    var obj = AssetDatabase.LoadAssetAtPath<Object>(item.BundlePath);
                    EditorGUIUtility.PingObject(obj);
                    Selection.activeObject = obj;
                }
            }
        }
        protected override void RowGUI(RowGUIArgs args)
        {
            var item = args.item as QuarkBundleDetailTreeViewItem;
            if (item.BundleDetailType == QuarkBundleDetailType.LabelBundle)
            {
                base.RowGUI(args);
            }
            else
            {
                var length = args.GetNumVisibleColumns();
                for (int i = 0; i < length; i++)
                {
                    DrawCellGUI(args.GetCellRect(i), args.item as QuarkBundleDetailTreeViewItem, args.GetColumn(i), ref args);
                }
            }
        }
        void DrawCellGUI(Rect cellRect, QuarkBundleDetailTreeViewItem treeView, int column, ref RowGUIArgs args)
        {
            switch (column)
            {
                case 0:
                    {
                        DefaultGUI.Label(cellRect, treeView.BundleSize, args.selected, args.focused);
                    }
                    break;
                case 1:
                    {
                        DefaultGUI.Label(cellRect, treeView.ObjectCount.ToString(), args.selected, args.focused);
                    }
                    break;
                case 2:
                    {
                        DefaultGUI.Label(cellRect, treeView.displayName, args.selected, args.focused);
                    }
                    break;
            }
        }
        QuarkBundleDetailTreeViewItem FindItemInVisibleRows(int id)
        {
            var rows = GetRows();
            foreach (var r in rows)
            {
                if (r.id == id)
                {
                    return r as QuarkBundleDetailTreeViewItem;
                }
            }
            return null;
        }

    }
}
