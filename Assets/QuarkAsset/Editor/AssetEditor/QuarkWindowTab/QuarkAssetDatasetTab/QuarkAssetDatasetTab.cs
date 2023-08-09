using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;
using UnityEngine;
namespace Quark.Editor
{
    public class QuarkAssetDatasetTab
    {
        ReorderableList reorderableList;
        Vector2 scrollPosition;
        SearchField searchField;
        string searchText;
        List<string> extensionList = new List<string>();
        bool datasetAssigned = false;
        public void OnDisable()
        {
            datasetAssigned = false;
            extensionList.Clear();
        }
        public void OnEnable()
        {
            searchField = new SearchField();
            reorderableList = new ReorderableList(extensionList, typeof(string), true, true, true, true);
            reorderableList.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, $"Extension count : {extensionList.Count}");
            reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                rect.y += 2f;
                rect.height = EditorGUIUtility.singleLineHeight;
                var srcExt = extensionList[index];

                int rectOffset = 48;
                var lableRect = rect;
                lableRect.width = rectOffset;
                EditorGUI.LabelField(lableRect, $"ID: {index + 1}");
                var textRect = rect;
                textRect.x += rectOffset;
                textRect.width -= rectOffset;
                var newExt = EditorGUI.TextField(textRect, srcExt);

                if (string.IsNullOrEmpty(newExt))
                    return;
                var lowerStr = newExt.ToLower();
                if (!extensionList.Contains(lowerStr))
                {
                    if (!lowerStr.StartsWith("."))
                        return;
                    extensionList[index] = lowerStr;
                    if (!datasetAssigned)
                        return;
                    var datasetExtList = QuarkEditorDataProxy.QuarkAssetDataset.QuarkAssetExts;
                    datasetExtList[index] = lowerStr;
                    EditorUtility.SetDirty(QuarkEditorDataProxy.QuarkAssetDataset);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            };
            if (QuarkEditorDataProxy.QuarkAssetDataset != null)
            {
                datasetAssigned = true;
                extensionList.AddRange(QuarkEditorDataProxy.QuarkAssetDataset.QuarkAssetExts);
            }
            reorderableList.onAddCallback = (list) =>
            {
                list.list.Add("<none>");
            };
            reorderableList.onChangedCallback = (list) =>
            {
                if (!datasetAssigned)
                    return;
                var datasetExtList = QuarkEditorDataProxy.QuarkAssetDataset.QuarkAssetExts;
                datasetExtList.Clear();
                datasetExtList.AddRange(extensionList);
                EditorUtility.SetDirty(QuarkEditorDataProxy.QuarkAssetDataset);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            };
            reorderableList.onRemoveCallback = (list) =>
            {
                var removeIndex = list.index;
                list.list.RemoveAt(removeIndex);
            };
        }
        public void OnDatasetAssign()
        {
            datasetAssigned = true;
            extensionList.AddRange(QuarkEditorDataProxy.QuarkAssetDataset.QuarkAssetExts);
        }
        public void OnDatasetRefresh()
        {
            if (!datasetAssigned)
                return;
            extensionList.AddRange(QuarkEditorDataProxy.QuarkAssetDataset.QuarkAssetExts);
        }
        public void OnDatasetUnassign()
        {
            datasetAssigned = false;
            extensionList.Clear();
        }
        public void OnGUI()
        {
            //EditorGUILayout.LabelField("Quark Available Extenison", EditorStyles.boldLabel);
            GUILayout.Space(8);
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Clear extensions"))
                {
                    if (!datasetAssigned)
                        return;
                    extensionList.Clear();
                    QuarkEditorDataProxy.QuarkAssetDataset.QuarkAssetExts.Clear();
                    EditorUtility.SetDirty(QuarkEditorDataProxy.QuarkAssetDataset);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
                if (GUILayout.Button("Reset extensions"))
                {
                    if (!datasetAssigned)
                        return;
                    var datasetExtensionList = QuarkEditorDataProxy.QuarkAssetDataset.QuarkAssetExts;
                    extensionList.Clear();
                    extensionList.AddRange(QuarkEditorConstant.Extensions);
                    datasetExtensionList.Clear();
                    datasetExtensionList.AddRange(QuarkEditorConstant.Extensions);
                    EditorUtility.SetDirty(QuarkEditorDataProxy.QuarkAssetDataset);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            {
                //EditorGUILayout.LabelField("Search extension", EditorStyles.boldLabel, GUILayout.MaxWidth(128));
                GUILayout.Label("Search extension", GUILayout.MaxWidth(128));
                searchText = searchField.OnToolbarGUI(searchText);
                DrawSearchList();
            }
            GUILayout.EndHorizontal();
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            GUILayout.BeginVertical("box");
            {
                reorderableList.DoLayoutList();
            }
            GUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }
        void DrawSearchList()
        {
            if (string.IsNullOrEmpty(searchText))
            {
                if (!datasetAssigned)
                    return;
                extensionList.Clear();
                extensionList.AddRange(QuarkEditorDataProxy.QuarkAssetDataset.QuarkAssetExts);
            }
            else
            {
                if (!datasetAssigned)
                    return;
                extensionList.Clear();
                var datasetExtList = QuarkEditorDataProxy.QuarkAssetDataset.QuarkAssetExts;
                var length = datasetExtList.Count;
                for (int i = 0; i < length; i++)
                {
                    var ext = datasetExtList[i];
                    if (ext.Contains(searchText.ToLower()))
                    {
                        extensionList.Add(ext);
                    }
                }
            }
        }
    }
}
