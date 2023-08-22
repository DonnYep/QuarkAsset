using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Quark.Manifest;
using UnityEditor;

namespace Quark.Editor
{
    public class QuarkManifestCompareTreeView : TreeView
    {
        QuarkManifestCompareResult compareResult;
        List<QuarkManifestCompareInfo> compareResultInfo = new List<QuarkManifestCompareInfo>();
        public QuarkManifestCompareTreeView(TreeViewState treeViewState, MultiColumnHeader multiColumnHeader)
: base(treeViewState, multiColumnHeader)
        {
            Reload();
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            multiColumnHeader.sortingChanged += OnMultiColumnHeaderSortingChanged;
        }
        public void SetResult(QuarkManifestCompareResult compareResult)
        {
            this.compareResult = compareResult;
            if (compareResult != null)
            {
                compareResultInfo.Clear();
                if (QuarkManifestCompareTabDataProxy.ShowChanged)
                    compareResultInfo.AddRange(compareResult.ChangedInfos);
                if (QuarkManifestCompareTabDataProxy.ShowNewlyAdded)
                    compareResultInfo.AddRange(compareResult.NewlyAddedInfos);
                if (QuarkManifestCompareTabDataProxy.ShowDeleted)
                    compareResultInfo.AddRange(compareResult.DeletedInfos);
                if (QuarkManifestCompareTabDataProxy.ShowUnchanged)
                    compareResultInfo.AddRange(compareResult.UnchangedInfos);
                Reload();
            }
        }
        public void Clear()
        {
            compareResultInfo.Clear();
            Reload();
        }
        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { id = -1, depth = -1, displayName = "Root" };

            var allItems = new List<TreeViewItem>();
            var addedIcon = EditorGUIUtility.FindTexture("CollabCreate Icon");
            var expiredIcon = EditorGUIUtility.FindTexture("CollabDeleted Icon");
            var changedIcon = EditorGUIUtility.FindTexture("CollabEdit Icon");
            var unchangedIcon = QuarkEditorUtility.GetFolderIcon();

            if (compareResult != null)
            {
                var rstInfoLength = compareResultInfo.Count;
                for (int i = 0; i < rstInfoLength; i++)
                {
                    var rstInfo = compareResultInfo[i];
                    Texture2D icon = null;
                    switch (rstInfo.BundleChangeType)
                    {
                        case QuarkBundleChangeType.Changed:
                            icon = changedIcon;
                            break;
                        case QuarkBundleChangeType.NewlyAdded:
                            icon = addedIcon;
                            break;
                        case QuarkBundleChangeType.Deleted:
                            icon = expiredIcon;
                            break;
                        case QuarkBundleChangeType.Unchanged:
                            icon = unchangedIcon;
                            break;
                    }
                    var item = new QuarkCompareInfoTreeViewItem(i, 1, rstInfo.BundleName, icon)
                    {
                        BundleFormatSize = rstInfo.BundleFormatSize,
                        BundleHash = rstInfo.BundleHash,
                        BundleKey = rstInfo.BundleKey,
                        BundleName = rstInfo.BundleName,
                        BundleSize = rstInfo.BundleSize,
                        BundleChangeType = rstInfo.BundleChangeType
                    };
                    allItems.Add(item);
                }
            }
            SetupParentsAndChildrenFromDepths(root, allItems);
            return root;
        }
        protected override void RowGUI(RowGUIArgs args)
        {
            var length = args.GetNumVisibleColumns();
            for (int i = 0; i < length; i++)
            {
                DrawCellGUI(args.GetCellRect(i), args.item as QuarkCompareInfoTreeViewItem, args.GetColumn(i), ref args);
            }
        }
        protected override void DoubleClickedItem(int id)
        {
            if (id < compareResultInfo.Count)
            {
                var rstInfo = compareResultInfo[id];
                var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(rstInfo.BundlePath);
                EditorGUIUtility.PingObject(obj);
            }
            base.DoubleClickedItem(id);
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
                case 1://BundleChangeType
                    {
                        if (ascending)
                            compareResultInfo.Sort((lhs, rhs) => lhs.BundleChangeType.CompareTo(rhs.BundleChangeType));
                        else
                            compareResultInfo.Sort((lhs, rhs) => rhs.BundleChangeType.CompareTo(lhs.BundleChangeType));
                    }
                    break;
                case 2://BundleName
                    {
                        if (ascending)
                            compareResultInfo.Sort((lhs, rhs) => lhs.BundleName.CompareTo(rhs.BundleName));
                        else
                            compareResultInfo.Sort((lhs, rhs) => rhs.BundleName.CompareTo(lhs.BundleName));
                    }
                    break;
                case 3://BundleSize
                    {
                        if (ascending)
                            compareResultInfo.Sort((lhs, rhs) => rhs.BundleSize.CompareTo(lhs.BundleSize));
                        else
                            compareResultInfo.Sort((lhs, rhs) => lhs.BundleSize.CompareTo(rhs.BundleSize));
                    }
                    break;
                case 4://BundleFormatSize
                    {
                        if (ascending)
                            compareResultInfo.Sort((lhs, rhs) => rhs.BundleSize.CompareTo(lhs.BundleSize));
                        else
                            compareResultInfo.Sort((lhs, rhs) => lhs.BundleSize.CompareTo(rhs.BundleSize));
                    }
                    break;
                case 5://BundleKey
                    {
                        if (ascending)
                            compareResultInfo.Sort((lhs, rhs) => rhs.BundleKey.CompareTo(lhs.BundleKey));
                        else
                            compareResultInfo.Sort((lhs, rhs) => lhs.BundleKey.CompareTo(rhs.BundleKey));
                    }
                    break;
                case 6://BundleHash
                    {
                        if (ascending)
                            compareResultInfo.Sort((lhs, rhs) => rhs.BundleHash.CompareTo(lhs.BundleHash));
                        else
                            compareResultInfo.Sort((lhs, rhs) => lhs.BundleHash.CompareTo(rhs.BundleHash));
                    }
                    break;
            }
            Reload();
        }
        void DrawCellGUI(Rect cellRect, QuarkCompareInfoTreeViewItem treeView, int column, ref RowGUIArgs args)
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
                        var lablCellRect = new Rect(cellRect.x + iconRect.width + 4, cellRect.y, cellRect.width - iconRect.width, cellRect.height);
                        DefaultGUI.Label(lablCellRect, treeView.BundleChangeType.ToString().ToUpper(), args.selected, args.focused);
                    }
                    break;
                case 2:
                    {
                        DefaultGUI.Label(cellRect, treeView.displayName, args.selected, args.focused);
                    }
                    break;
                case 3:
                    {
                        DefaultGUI.Label(cellRect, treeView.BundleSize.ToString(), args.selected, args.focused);
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
    }
}
