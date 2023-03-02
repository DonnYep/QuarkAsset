﻿using Quark.Asset;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Object = UnityEngine.Object;
namespace Quark.Editor
{
    public class QuarkObjectTreeView : TreeView
    {
        List<QuarkObjectInfo> objectInfoList = new List<QuarkObjectInfo>();
        public QuarkObjectTreeView(TreeViewState treeViewState, MultiColumnHeader multiColumnHeader)
    : base(treeViewState, multiColumnHeader)
        {
            Reload();
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            multiColumnHeader.sortingChanged += OnMultiColumnHeaderSortingChanged; ;
        }
        public void AddPath(QuarkObjectInfo info)
        {
            objectInfoList.Add(info);
        }
        public void AddPaths(IEnumerable<QuarkObjectInfo> infos)
        {
            objectInfoList.Clear();
            objectInfoList.AddRange(infos);
        }
        public void Clear()
        {
            objectInfoList.Clear();
        }
        protected override void DoubleClickedItem(int id)
        {
            base.DoubleClickedItem(id);
            if (objectInfoList.Count < id)
                return;
            var obj = AssetDatabase.LoadAssetAtPath<Object>(objectInfoList[id].ObjectPath);
            EditorGUIUtility.PingObject(obj);
            Selection.activeObject = obj;
        }
        protected override void ContextClickedItem(int id)
        {
            var selected = GetSelection();
            GenericMenu menu = new GenericMenu();
            if (selected.Count == 1)
            {
                menu.AddItem(new GUIContent("Copy object name to clipboard"), false, CopyObjectNameToClipboard, id);
                menu.AddItem(new GUIContent("Copy object path to clipboard"), false, CopyObjectPathToClipboard, id);
            }
            menu.ShowAsContext();
        }
        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { id = -1, depth = -1, displayName = "Root" };
            var allItems = new List<TreeViewItem>();
            {
                for (int i = 0; i < objectInfoList.Count; i++)
                {
                    //var  objectIcon = EditorGUIUtility.FindTexture("console.erroricon");

                    var objectInfo = objectInfoList[i];
                    Texture2D objectIcon = null;
                    if (objectInfo.ObjectValid)
                        objectIcon = AssetDatabase.GetCachedIcon(objectInfo.ObjectPath) as Texture2D;
                    else
                        objectIcon = EditorGUIUtility.FindTexture("console.erroricon");
                    var item = new QuarkObjectTreeViewItem(i, 1, objectInfo.ObjectName, objectIcon)
                    {
                        BundleName = objectInfo.BundleName,
                        AssetPath = objectInfo.ObjectPath,
                        AssetExtension = objectInfo.ObjectExtension,
                        FormatBytes = objectInfo.ObjectFormatBytes
                    };
                    allItems.Add(item);
                }
                SetupParentsAndChildrenFromDepths(root, allItems);
                return root;
            }
        }
        protected override void RowGUI(RowGUIArgs args)
        {
            var length = args.GetNumVisibleColumns();
            for (int i = 0; i < length; i++)
            {
                DrawCellGUI(args.GetCellRect(i), args.item as QuarkObjectTreeViewItem, args.GetColumn(i), ref args);
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
                case 1://ObjectName
                    {
                        if (ascending)
                            objectInfoList.Sort((lhs, rhs) => lhs.ObjectName.CompareTo(rhs.ObjectName));
                        else
                            objectInfoList.Sort((lhs, rhs) => rhs.ObjectName.CompareTo(lhs.ObjectName));
                    }
                    break;
                case 2://Size
                    {
                        if (ascending)
                            objectInfoList.Sort((lhs, rhs) => lhs.ObjectSize.CompareTo(rhs.ObjectSize));
                        else
                            objectInfoList.Sort((lhs, rhs) => rhs.ObjectSize.CompareTo(lhs.ObjectSize));
                    }
                    break;
                case 3://Extension
                    {
                        if (ascending)
                            objectInfoList.Sort((lhs, rhs) => lhs.BundleName.CompareTo(rhs.BundleName));
                        else
                            objectInfoList.Sort((lhs, rhs) => rhs.BundleName.CompareTo(lhs.BundleName));
                    }
                    break;
                case 4://BundleName
                    {
                        if (ascending)
                            objectInfoList.Sort((lhs, rhs) => rhs.BundleName.CompareTo(lhs.BundleName));
                        else
                            objectInfoList.Sort((lhs, rhs) => lhs.BundleName.CompareTo(rhs.BundleName));
                    }
                    break;
                case 5://AssetPath
                    {
                        if (ascending)
                            objectInfoList.Sort((lhs, rhs) => rhs.ObjectPath.CompareTo(lhs.ObjectPath));
                        else
                            objectInfoList.Sort((lhs, rhs) => lhs.ObjectPath.CompareTo(rhs.ObjectPath));
                    }
                    break;
            }
            Reload();
        }

        void DrawCellGUI(Rect cellRect, QuarkObjectTreeViewItem treeView, int column, ref RowGUIArgs args)
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
                        DefaultGUI.Label(lablCellRect, treeView.displayName, args.selected, args.focused);
                    }
                    break;
                case 2:
                    {
                        DefaultGUI.Label(cellRect, treeView.FormatBytes, args.selected, args.focused);
                    }
                    break;
                case 3:
                    {
                        DefaultGUI.Label(cellRect, treeView.AssetExtension, args.selected, args.focused);
                    }
                    break;
                case 4:
                    {
                        DefaultGUI.Label(cellRect, treeView.BundleName, args.selected, args.focused);
                    }
                    break;
                case 5:
                    {
                        DefaultGUI.Label(cellRect, treeView.AssetPath, args.selected, args.focused);
                    }
                    break;
            }
        }
        void CopyObjectNameToClipboard(object context)
        {
            var id = Convert.ToInt32(context);
            var name = objectInfoList[id].ObjectName;
            GUIUtility.systemCopyBuffer = name;
        }
        void CopyObjectPathToClipboard(object context)
        {
            var id = Convert.ToInt32(context);
            var path = objectInfoList[id].ObjectPath;
            GUIUtility.systemCopyBuffer = path;
        }
    }
}
