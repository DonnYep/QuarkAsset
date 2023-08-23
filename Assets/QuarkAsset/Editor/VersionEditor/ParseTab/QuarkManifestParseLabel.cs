using Quark.Asset;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Quark.Editor
{
    public class QuarkManifestParseLabel
    {
        QuarkManifest manifest;

        QuarkManifestParseTreeView treeView;
        TreeViewState treeViewState;
        SearchField searchField;

        long totalBundleCount;
        long totalBundleLength;
        string totalBundleFormatSize;

        public void OnEnable()
        {
            searchField = new SearchField();
            treeViewState = new TreeViewState();
            var multiColumnHeaderState = new MultiColumnHeader(QuarkEditorUtility.CreateManifestParseMultiColumnHeader());
            treeView = new QuarkManifestParseTreeView(treeViewState, multiColumnHeaderState);
            searchField.downOrUpArrowKeyPressed += treeView.SetFocusAndEnsureSelectedItem;
        }
        public void OnGUI(Rect rect)
        {
            GUILayout.BeginVertical();
            {
                GUILayout.BeginHorizontal();
                {
                    DrawManifestDetail(rect);
                    DrawTreeView(rect);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
        public void OnDisable()
        {

        }
        public void Clear()
        {
            totalBundleLength = 0;
            totalBundleCount = 0;
            totalBundleFormatSize = QuarkUtility.FormatBytes(totalBundleLength);
        }
        public void SetManifest(QuarkManifest manifest)
        {
            totalBundleLength = 0;
            totalBundleCount = 0;
            this.manifest = manifest;
            if (manifest != null)
            {
                foreach (var bInfo in manifest.BundleInfoDict.Values)
                {
                    totalBundleLength += bInfo.BundleSize;
                }
                totalBundleCount = manifest.BundleInfoDict.Count;
            }
            totalBundleFormatSize = QuarkUtility.FormatBytes(totalBundleLength);
            treeView.SetManifest(manifest);
        }
        void DrawManifestDetail(Rect rect)
        {
            GUILayout.BeginVertical(GUILayout.MaxWidth(rect.width * 0.3f));
            {
                string buildVersion = string.Empty;
                string internalBuildVersion = string.Empty;
                string buildHash = string.Empty;
                string buildTime = string.Empty;

                if (manifest != null)
                {
                    buildVersion = manifest.BuildVersion;
                    internalBuildVersion = manifest.InternalBuildVersion.ToString();
                    buildHash = manifest.BuildHash;
                    buildTime = manifest.BuildTime;
                }
                else
                {
                    buildVersion = QuarkConstant.NONE;
                    internalBuildVersion = QuarkConstant.NONE;
                    buildHash = QuarkConstant.NONE;
                    buildTime = QuarkConstant.NONE;
                }
                GUILayout.Space(8);

                EditorGUILayout.LabelField($"Build", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Build version: {buildVersion}");
                EditorGUILayout.LabelField($"Internal build version: {internalBuildVersion}");
                EditorGUILayout.LabelField($"Build time: {buildTime}");
                EditorGUILayout.LabelField($"Build hash: {buildHash}");

                GUILayout.Space(8);

                EditorGUILayout.LabelField($"Total bundle", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Total bundle count: {totalBundleCount}");
                EditorGUILayout.LabelField($"Total bundle length: {totalBundleLength}");
                EditorGUILayout.LabelField($"Total bundle format size : {totalBundleFormatSize}");

            }
            GUILayout.EndVertical();
            //0.62f
        }
        void DrawTreeView(Rect rect)
        {
            GUILayout.BeginVertical(GUILayout.MaxWidth(rect.width * 0.7f));
            {
                GUILayout.BeginHorizontal();
                {
                    treeView.searchString = searchField.OnToolbarGUI(treeView.searchString);
                }
                GUILayout.EndHorizontal();

                Rect viewRect = GUILayoutUtility.GetRect(32, 8192, 32, 8192);
                treeView.OnGUI(viewRect);
            }
            GUILayout.EndVertical();
        }
    }
}
