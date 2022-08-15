using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Object = UnityEngine.Object;
namespace Quark.Editor
{
    public class QuarkAssetObjectTreeView : TreeView
    {
        List<QuarkObjectItem> objectItemList = new List<QuarkObjectItem>();
        public QuarkAssetObjectTreeView(TreeViewState treeViewState, MultiColumnHeader multiColumnHeader)
    : base(treeViewState, multiColumnHeader)
        {
            Reload();
            showAlternatingRowBackgrounds = true;
            showBorder = true;
        }
        public void AddPath(QuarkObjectItem item)
        {
            objectItemList.Add(item);
        }
        public void AddPaths(IEnumerable<QuarkObjectItem> items)
        {
            objectItemList.Clear();
            objectItemList.AddRange(items);
            Reload();
        }
        public void Clear()
        {
            objectItemList.Clear();
            Reload();
        }
        protected override void SingleClickedItem(int id)
        {
            base.SingleClickedItem(id);
            if (objectItemList.Count < id)
                return;
            var obj = AssetDatabase.LoadAssetAtPath<Object>(objectItemList[id].AssetPath);
            EditorGUIUtility.PingObject(obj);
            Selection.activeObject = obj;
        }
        protected override void DoubleClickedItem(int id)
        {
            base.DoubleClickedItem(id);
            if (objectItemList.Count < id)
                return;
            var obj = AssetDatabase.LoadAssetAtPath<Object>(objectItemList[id].AssetPath);
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
            Texture2D objectIcon = null;
            var allItems = new List<TreeViewItem>();
            {
                for (int i = 0; i < objectItemList.Count; i++)
                {
                    var obj = AssetDatabase.LoadAssetAtPath(objectItemList[i].AssetPath, typeof(Object));
                    bool isValidAsset = obj != null;
                    if (isValidAsset)
                    {
                        objectIcon = QuarkEditorUtility.ToTexture2D(EditorGUIUtility.ObjectContent(obj, obj.GetType()).image);
                    }
                    else
                    {
                        objectIcon = EditorGUIUtility.FindTexture("console.erroricon");
                    }
                    var objItem = objectItemList[i];
                    var item = new QuarkObjectTreeViewItem(i, 1, objItem.AssetName, objectIcon)
                    { BundleName = objItem.AssetBundleName, AssetPath = objItem.AssetPath, AssetExtension = objItem.AssetExtension };
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
                        DefaultGUI.Label(cellRect, treeView.AssetExtension, args.selected, args.focused);
                    }
                    break;
                case 3:
                    {
                        DefaultGUI.Label(cellRect, treeView.BundleName, args.selected, args.focused);
                    }
                    break;
                case 4:
                    {
                        DefaultGUI.Label(cellRect, treeView.AssetPath, args.selected, args.focused);
                    }
                    break;
            }
        }
        void CopyObjectNameToClipboard(object context)
        {
            var id = Convert.ToInt32(context);
            var name = objectItemList[id].AssetName;
            GUIUtility.systemCopyBuffer = name;
        }
        void CopyObjectPathToClipboard(object context)
        {
            var id = Convert.ToInt32(context);
            var path = objectItemList[id].AssetPath;
            GUIUtility.systemCopyBuffer = path;
        }
    }
}
