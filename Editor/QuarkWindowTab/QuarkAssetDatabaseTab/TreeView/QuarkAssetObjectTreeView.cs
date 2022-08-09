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
        List<string> pathList = new List<string>();
        public QuarkAssetObjectTreeView(TreeViewState treeViewState, MultiColumnHeader multiColumnHeader)
    : base(treeViewState, multiColumnHeader)
        {
            Reload();
            showAlternatingRowBackgrounds = true;
            showBorder = true;
        }
        public void AddPath(string path)
        {
            pathList.Add(path);
        }
        public void AddPaths(IEnumerable<string> paths)
        {
            pathList.Clear();
            pathList.AddRange(paths);
            Reload();
        }
        public void Clear()
        {
            pathList.Clear();
            Reload();
        }
        protected override void SingleClickedItem(int id)
        {
            base.SingleClickedItem(id);
            if (pathList.Count < id)
                return;
            var obj = AssetDatabase.LoadAssetAtPath<Object>(pathList[id]);
            EditorGUIUtility.PingObject(obj);
            Selection.activeObject = obj;
        }
        protected override void DoubleClickedItem(int id)
        {
            base.DoubleClickedItem(id);
            if (pathList.Count < id)
                return;
            var obj = AssetDatabase.LoadAssetAtPath<Object>(pathList[id]);
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
                for (int i = 0; i < pathList.Count; i++)
                {
                    var obj = AssetDatabase.LoadAssetAtPath(pathList[i], typeof(Object));
                    bool isValidAsset = obj != null;
                    if (isValidAsset)
                    {
                        objectIcon = QuarkEditorUtility.ToTexture2D(EditorGUIUtility.ObjectContent(obj, obj.GetType()).image);
                    }
                    else
                    {
                        objectIcon = EditorGUIUtility.FindTexture("console.erroricon");
                    }
                    var item = new TreeViewItem { id = i, depth = 1, displayName = pathList[i], icon = objectIcon };
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
                DrawCellGUI(args.GetCellRect(i), args.item, args.GetColumn(i), ref args);
            }
        }
        void DrawCellGUI(Rect cellRect, TreeViewItem treeView, int column, ref RowGUIArgs args)
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
            }
        }
        void CopyObjectNameToClipboard(object context)
        {
            var id = Convert.ToInt32(context);
            var path = pathList[id];
            var name = Path.GetFileNameWithoutExtension(path);
            GUIUtility.systemCopyBuffer = name;
        }
        void CopyObjectPathToClipboard(object context)
        {
            var id = Convert.ToInt32(context);
            var path = pathList[id];
            GUIUtility.systemCopyBuffer = path;
        }
    }
}
