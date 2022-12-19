﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Quark.Asset;

namespace Quark.Editor
{
    public class QuarkBundleTreeView : TreeView
    {
        readonly List<QuarkBundleInfo> bundleInfoList = new List<QuarkBundleInfo>();

        string originalName;
        /// <summary>
        /// 正在重命名的itemId
        /// </summary>
        int renamingItemId = -1;
        /// <summary>
        /// 上一行的cellRect
        /// </summary>
        Rect latestBundleCellRect;
        public Action<IList<int>> onSelectionChanged;
        public Action<IList<int>> onDelete;
        public Action onAllDelete;
        public QuarkBundleTreeView(TreeViewState treeViewState, MultiColumnHeader multiColumnHeader)
      : base(treeViewState, multiColumnHeader)
        {
            Reload();
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            multiColumnHeader.sortingChanged += OnSortingChanged;
        }
        public void Clear()
        {
            bundleInfoList.Clear();
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
        public void RemoveBundleByName(string bundleName)
        {
            try
            {
                if (QuarkEditorDataProxy.QuarkAssetDataset == null)
                    return;
                QuarkBundleInfo removedBundle = null;
                var bundleInfos = QuarkEditorDataProxy.QuarkAssetDataset.QuarkBundleInfoList;
                var length = bundleInfos.Count;
                int removeIndex = -1;
                for (int i = 0; i < length; i++)
                {
                    if (bundleInfos[i].BundleName == bundleName)
                    {
                        removeIndex = i;
                        removedBundle = bundleInfos[i];
                        break;
                    }
                }
                if (removeIndex != -1)
                {
                    QuarkEditorDataProxy.QuarkAssetDataset.QuarkBundleInfoList.RemoveAt(removeIndex);
                    bundleInfoList.Remove(removedBundle);
                }
            }
            catch (Exception e)
            {
                QuarkUtility.LogError(e);
            }
            Reload();
        }
        protected override void DoubleClickedItem(int id)
        {
            if (QuarkEditorDataProxy.QuarkAssetDataset == null)
                return;
            var bundles = QuarkEditorDataProxy.QuarkAssetDataset.QuarkBundleInfoList;
            var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(bundles[id].BundlePath);
            EditorGUIUtility.PingObject(obj);
            Selection.activeObject = obj;
            base.DoubleClickedItem(id);
        }
        protected override Rect GetRenameRect(Rect rowRect, int row, TreeViewItem item)
        {
            return new Rect(latestBundleCellRect.x, latestBundleCellRect.height * row, latestBundleCellRect.width, latestBundleCellRect.height);
        }
        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { id = -1, depth = -1, displayName = "Root" };
            var allItems = new List<TreeViewItem>();
            var assetIcon = EditorGUIUtility.FindTexture("PreMatCube");
            for (int i = 0; i < bundleInfoList.Count; i++)
            {
                var item = new QuarkBundleTreeViewItem(i, 1, bundleInfoList[i].BundleName, assetIcon)
                {
                    ObjectCount = bundleInfoList[i].ObjectInfoList.Count,
                    BundleSize = bundleInfoList[i].BundleFormatBytes
                };
                allItems.Add(item);
            }
            SetupParentsAndChildrenFromDepths(root, allItems);
            return root;
        }
        protected override void SelectionChanged(IList<int> selectedIds)
        {
            base.SelectionChanged(selectedIds);
            onSelectionChanged?.Invoke(selectedIds);
        }
        protected override void RenameEnded(RenameEndedArgs args)
        {
            if (QuarkEditorDataProxy.QuarkAssetDataset == null)
                return;
            var item = FindItem(args.itemID, rootItem);
            var bundles = QuarkEditorDataProxy.QuarkAssetDataset.QuarkBundleInfoList;
            if (!string.IsNullOrEmpty(args.newName))
            {
                //防止重名
                var canUse = true;
                var newName = args.newName;
                var length = bundles.Count;
                for (int i = 0; i < length; i++)
                {
                    if (bundles[i].BundleName == newName)
                    {
                        canUse = false;
                        break;
                    }
                }
                if (canUse)
                {
                    var bundle = bundles[args.itemID];
                    bundle.BundleName = QuarkUtility.FormatAssetBundleName(newName);
                    item.displayName = bundle.BundleName;
                }
                else
                {
                    item.displayName = originalName;
                }
            }
            else
            {
                item.displayName = originalName;
            }
            EditorUtility.SetDirty(QuarkEditorDataProxy.QuarkAssetDataset);
            base.RenameEnded(args);
            renamingItemId = -1;
        }
        protected override bool CanRename(TreeViewItem item)
        {
            originalName = item.displayName;
            renamingItemId = item.id;
            BeginRename(item);
            return item != null;
        }
        protected override void ContextClickedItem(int id)
        {
            var selected = GetSelection();
            GenericMenu menu = new GenericMenu();
            if (selected.Count > 1)
            {
                menu.AddItem(new GUIContent("Delete bundle"), false, Delete, selected);
                menu.AddItem(new GUIContent("Delete all bundles"), false, DeleteAll);
                menu.AddItem(new GUIContent("Reset the names of all bundles"), false, ResetAllBundlesName);
            }
            if (selected.Count == 1)
            {
                menu.AddItem(new GUIContent("Delete bundle"), false, Delete, selected);
                menu.AddItem(new GUIContent("Delete all bundles"), false, DeleteAll);
                menu.AddItem(new GUIContent("Reset bundle name"), false, ResetBundleName, id);
                menu.AddItem(new GUIContent("Reset the names of all bundles"), false, ResetAllBundlesName);
                menu.AddItem(new GUIContent("Copy bundle name to clipboard"), false, CopyBundleNameToClipboard, id);
                menu.AddItem(new GUIContent("Copy bundle path to clipboard"), false, CopyBundlePathToClipboard, id);
            }
            menu.ShowAsContext();
        }
        protected override void RowGUI(RowGUIArgs args)
        {
            var length = args.GetNumVisibleColumns();
            for (int i = 0; i < length; i++)
            {
                DrawCellGUI(args.GetCellRect(i), args.item as QuarkBundleTreeViewItem, args.GetColumn(i), ref args);
            }
        }
        //public override void OnGUI(Rect rect)
        //{
        //var tRect = this.treeViewRect;
        //var newRect = tRect;
        //EditorGUIUtility.AddCursorRect(tRect, MouseCursor.ResizeVertical);
        //if (Event.current.type == EventType.MouseDown && tRect.Contains(Event.current.mousePosition))
        //{
        //   var m_VerticalSplitterPercentRight = Mathf.Clamp(Event.current.mousePosition.y / tRect.height, 0.2f, 0.98f);
        //    newRect.x = newRect.height* m_VerticalSplitterPercentRight;
        //}
        //base.OnGUI(newRect);
        //}
        void OnSortingChanged(MultiColumnHeader multiColumnHeader)
        {
            if (QuarkEditorDataProxy.QuarkAssetDataset == null)
                return;
            var quarkBundleInfoList = QuarkEditorDataProxy.QuarkAssetDataset.QuarkBundleInfoList;
            var sortedColumns = multiColumnHeader.state.sortedColumns;
            if (sortedColumns.Length == 0)
                return;
            var sortedType = sortedColumns[0];
            var ascending = multiColumnHeader.IsSortedAscending(sortedType);
            switch (sortedType)
            {
                case 0://index
                    break;
                case 1://size
                    {
                        if (ascending)
                            quarkBundleInfoList.Sort((lhs, rhs) => lhs.BundleSize.CompareTo(rhs.BundleSize));
                        else
                            quarkBundleInfoList.Sort((lhs, rhs) => rhs.BundleSize.CompareTo(lhs.BundleSize));
                    }
                    break;
                case 2://count
                    {
                        if (ascending)
                            quarkBundleInfoList.Sort((lhs, rhs) => lhs.ObjectInfoList.Count.CompareTo(rhs.ObjectInfoList.Count));
                        else
                            quarkBundleInfoList.Sort((lhs, rhs) => rhs.ObjectInfoList.Count.CompareTo(lhs.ObjectInfoList.Count));
                    }
                    break;
                case 3://bundle
                    {
                        if (ascending)
                            quarkBundleInfoList.Sort((lhs, rhs) => rhs.BundleName.CompareTo(lhs.BundleName));
                        else
                            quarkBundleInfoList.Sort((lhs, rhs) => lhs.BundleName.CompareTo(rhs.BundleName));
                    }
                    break;
            }
            bundleInfoList.Clear();
            bundleInfoList.AddRange(quarkBundleInfoList);
            EditorUtility.SetDirty(QuarkEditorDataProxy.QuarkAssetDataset);
#if UNITY_2021_1_OR_NEWER
            AssetDatabase.SaveAssetIfDirty(QuarkEditorDataProxy.QuarkAssetDataset);
#elif UNITY_2019_1_OR_NEWER
            AssetDatabase.SaveAssets();
#endif
            AssetDatabase.Refresh();
            Reload();
        }
        void DrawCellGUI(Rect cellRect, QuarkBundleTreeViewItem treeView, int column, ref RowGUIArgs args)
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
                        DefaultGUI.Label(cellRect, treeView.BundleSize, args.selected, args.focused);
                    }
                    break;
                case 2:
                    {
                        DefaultGUI.Label(cellRect, treeView.ObjectCount.ToString(), args.selected, args.focused);
                    }
                    break;
                case 3:
                    {
                        var iconRect = new Rect(cellRect.x + 4, cellRect.y, cellRect.height, cellRect.height);
                        if (treeView.icon != null)
                            GUI.DrawTexture(iconRect, treeView.icon, ScaleMode.ScaleToFit);
                        var labelCellRect = new Rect(cellRect.x + iconRect.width + 4, cellRect.y, cellRect.width - iconRect.width, cellRect.height);
                        if (treeView.id != renamingItemId)
                            DefaultGUI.Label(labelCellRect, treeView.displayName, args.selected, args.focused);
                        latestBundleCellRect = labelCellRect;
                    }
                    break;
            }
        }
        void DeleteAll()
        {
            Reload();
            onAllDelete?.Invoke();
        }
        void ResetAllBundlesName()
        {
            if (QuarkEditorDataProxy.QuarkAssetDataset == null)
                return;
            var bundles = QuarkEditorDataProxy.QuarkAssetDataset.QuarkBundleInfoList;
            for (int i = 0; i < bundles.Count; i++)
            {
                var bundle = bundles[i];
                bundle.BundleName = QuarkUtility.FormatAssetBundleName(bundle.BundlePath);
            }
            EditorUtility.SetDirty(QuarkEditorDataProxy.QuarkAssetDataset);
            Reload();
        }
        void ResetBundleName(object context)
        {
            var id = Convert.ToInt32(context);
            if (QuarkEditorDataProxy.QuarkAssetDataset == null)
                return;
            var bundle = QuarkEditorDataProxy.QuarkAssetDataset.QuarkBundleInfoList[id];
            bundle.BundleName = QuarkUtility.FormatAssetBundleName(bundle.BundlePath);
            EditorUtility.SetDirty(QuarkEditorDataProxy.QuarkAssetDataset);
            Reload();
        }
        void CopyBundleNameToClipboard(object context)
        {
            var id = Convert.ToInt32(context);
            if (QuarkEditorDataProxy.QuarkAssetDataset == null)
                return;
            var name = QuarkEditorDataProxy.QuarkAssetDataset.QuarkBundleInfoList[id].BundleName;
            GUIUtility.systemCopyBuffer = name;
        }
        void CopyBundlePathToClipboard(object context)
        {
            var id = Convert.ToInt32(context);
            if (QuarkEditorDataProxy.QuarkAssetDataset == null)
                return;
            var path = QuarkEditorDataProxy.QuarkAssetDataset.QuarkBundleInfoList[id].BundlePath;
            GUIUtility.systemCopyBuffer = path;
        }
        void Delete(object context)
        {
            try
            {
                var list = context as IList<int>;
                var length = list.Count;
                var rmBundleInfos = new QuarkBundleInfo[length];
                for (int i = 0; i < length; i++)
                {
                    var rmId = list[i];
                    rmBundleInfos[i] = bundleInfoList[rmId];
                }
                for (int i = 0; i < length; i++)
                {
                    bundleInfoList.Remove(rmBundleInfos[i]);
                }
                onDelete?.Invoke(list);

            }
            catch (Exception e)
            {
                QuarkUtility.LogError(e);
            }
        }
    }
}
