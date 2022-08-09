﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Quark.Asset;

namespace Quark.Editor
{
    public class QuarkAssetBundleTreeView : TreeView
    {
        string originalName;
        public QuarkAssetBundleTreeView(TreeViewState treeViewState, MultiColumnHeader multiColumnHeader)
      : base(treeViewState, multiColumnHeader)
        {
            Reload();
            showAlternatingRowBackgrounds = true;
            showBorder = true;
        }
        public void Clear()
        {
            Reload();
        }
        public void AddPath(string path)
        {
            if (QuarkEditorDataProxy.QuarkAssetDataset == null)
                return;
            var bundles = QuarkEditorDataProxy.QuarkAssetDataset.QuarkAssetBundleList;
            var length = bundles.Count;
            bool existed = false;
            for (int i = 0; i < length; i++)
            {
                if (bundles[i].AssetBundlePath == path)
                {
                    existed = true;
                    break;
                }
            }
            if (!existed)
            {
                var bundle = new QuarkAssetBundle()
                {
                    AssetBundleName = QuarkUtility.FormatAssetBundleName(path),
                    AssetBundlePath = path
                };
                QuarkEditorDataProxy.QuarkAssetDataset.QuarkAssetBundleList.Add(bundle);
            }
            Reload();
        }
        public void RemoveBundleByName(string bundleName)
        {
            try
            {
                if (QuarkEditorDataProxy.QuarkAssetDataset == null)
                    return;
                var bundles = QuarkEditorDataProxy.QuarkAssetDataset.QuarkAssetBundleList;
                var length = bundles.Count;
                int removeindex = -1;
                for (int i = 0; i < length; i++)
                {
                    if (bundles[i].AssetBundleName == bundleName)
                    {
                        removeindex = i;
                        break;
                    }
                }
                if (removeindex != -1)
                {
                    QuarkEditorDataProxy.QuarkAssetDataset.QuarkAssetBundleList.RemoveAt(removeindex);
                }
            }
            catch (Exception e)
            {
                QuarkUtility.LogError(e);
            }
            Reload();
        }
        protected override void SingleClickedItem(int id)
        {
            if (QuarkEditorDataProxy.QuarkAssetDataset == null)
                return;
            var bundles = QuarkEditorDataProxy.QuarkAssetDataset.QuarkAssetBundleList;
            var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(bundles[id].AssetBundlePath);
            EditorGUIUtility.PingObject(obj);
            Selection.activeObject = obj;
            base.SingleClickedItem(id);
        }
        protected override void DoubleClickedItem(int id)
        {
            if (QuarkEditorDataProxy.QuarkAssetDataset == null)
                return;
            var bundles = QuarkEditorDataProxy.QuarkAssetDataset.QuarkAssetBundleList;
            var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(bundles[id].AssetBundlePath);
            EditorGUIUtility.PingObject(obj);
            Selection.activeObject = obj;
            base.DoubleClickedItem(id);
        }
        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { id = -1, depth = -1, displayName = "Root" };
            var allItems = new List<TreeViewItem>();
            if (QuarkEditorDataProxy.QuarkAssetDataset == null)
            {
                SetupParentsAndChildrenFromDepths(root, allItems);
                return root;
            }
            var bundles = QuarkEditorDataProxy.QuarkAssetDataset.QuarkAssetBundleList;
            var assetIcon = EditorGUIUtility.FindTexture("PreMatCube");
            for (int i = 0; i < bundles.Count; i++)
            {
                var item = new TreeViewItem { id = i, depth = 1, displayName = bundles[i].AssetBundleName, icon = assetIcon };
                allItems.Add(item);
            }
            SetupParentsAndChildrenFromDepths(root, allItems);
            return root;
        }
        protected override void RenameEnded(RenameEndedArgs args)
        {
            if (QuarkEditorDataProxy.QuarkAssetDataset == null)
                return;
            var item = FindItem(args.itemID, rootItem);
            var bundles = QuarkEditorDataProxy.QuarkAssetDataset.QuarkAssetBundleList;
            if (!string.IsNullOrEmpty(args.newName))
            {
                //防止重名
                var canUse = true;
                var newName = args.newName;
                var length = bundles.Count;
                for (int i = 0; i < length; i++)
                {
                    if (bundles[i].AssetBundleName == newName)
                    {
                        canUse = false;
                        break;
                    }
                }
                if (canUse)
                {
                    var bundle = bundles[args.itemID];
                    bundle.AssetBundleName = QuarkUtility.FormatAssetBundleName(newName);
                    item.displayName = bundle.AssetBundleName;
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
        }
        protected override bool CanRename(TreeViewItem item)
        {
            originalName = item.displayName;
            item.displayName = null;
            BeginRename(item);
            return item != null;
        }
        protected override void ContextClickedItem(int id)
        {
            List<string> selectedNodes = new List<string>();
            var selected = GetSelection();
            foreach (var nodeID in selected)
            {
                selectedNodes.Add(FindItem(nodeID, rootItem).displayName);
            }
            GenericMenu menu = new GenericMenu();
            if (selectedNodes.Count > 1)
            {
                menu.AddItem(new GUIContent("Delete "), false, Delete, selectedNodes);
                menu.AddItem(new GUIContent("Delete all bundles"), false, DeleteAll);
                menu.AddItem(new GUIContent("Reset the names of all bundles"), false, ResetAllBundlesName);
            }
            if (selectedNodes.Count == 1)
            {
                menu.AddItem(new GUIContent("Delete"), false, Delete, selectedNodes);
                menu.AddItem(new GUIContent("Reset bundle name"), false, ResetBundleName, id);
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
        void DeleteAll()
        {
            Reload();
        }
        void ResetAllBundlesName()
        {
            if (QuarkEditorDataProxy.QuarkAssetDataset == null)
                return;
            var bundles = QuarkEditorDataProxy.QuarkAssetDataset.QuarkAssetBundleList;
            for (int i = 0; i < bundles.Count; i++)
            {
                var bundle = bundles[i];
                bundle.AssetBundleName = QuarkUtility.FormatAssetBundleName(bundle.AssetBundlePath);
            }
            EditorUtility.SetDirty(QuarkEditorDataProxy.QuarkAssetDataset);
            Reload();
        }
        void ResetBundleName(object context)
        {
            var id = Convert.ToInt32(context);
            if (QuarkEditorDataProxy.QuarkAssetDataset == null)
                return;
            var bundle = QuarkEditorDataProxy.QuarkAssetDataset.QuarkAssetBundleList[id];
            bundle.AssetBundleName = QuarkUtility.FormatAssetBundleName(bundle.AssetBundlePath);
            EditorUtility.SetDirty(QuarkEditorDataProxy.QuarkAssetDataset);
            Reload();
        }
        void CopyBundleNameToClipboard(object context)
        {
            var id = Convert.ToInt32(context);
            if (QuarkEditorDataProxy.QuarkAssetDataset == null)
                return;
            var name = QuarkEditorDataProxy.QuarkAssetDataset.QuarkAssetBundleList[id].AssetBundleName;
            GUIUtility.systemCopyBuffer = name;
        }
        void CopyBundlePathToClipboard(object context)
        {
            var id = Convert.ToInt32(context);
            if (QuarkEditorDataProxy.QuarkAssetDataset == null)
                return;
            var path = QuarkEditorDataProxy.QuarkAssetDataset.QuarkAssetBundleList[id].AssetBundlePath;
            GUIUtility.systemCopyBuffer = path;
        }
        void Delete(object context)
        {
            try
            {
                var list = context as List<string>;
                for (int i = 0; i < list.Count; i++)
                {
                    RemoveBundleByName(list[i]);
                }
                Reload();
            }
            catch (Exception e)
            {
                QuarkUtility.LogError(e);
            }
        }
    }
}
