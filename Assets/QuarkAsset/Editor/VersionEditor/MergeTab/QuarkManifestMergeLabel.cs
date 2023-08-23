using Quark.Asset;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Quark.Editor
{
    public class QuarkManifestMergeLabel
    {
        QuarkManifestMergeTreeView treeView;
        TreeViewState treeViewState;
        SearchField searchField;
        Rect buttonRect;
        QuarkManifestMergeTab owner;
        long totalBundleCount;
        long totalBundleLength;
        string totalBundleFormatSize;

        int builtinBundleCount;
        long totalBuiltInBundleLength;
        string totalBuiltInBundleFormatSize;

        int incrementalBundleCount;
        long totalIncrementalBundleLength;
        string totalIncrementalBundleFormatSize;

        QuarkMergedManifest mergedManifest;
        public QuarkManifestMergeLabel(QuarkManifestMergeTab owner)
        {
            this.owner = owner;
        }
        public void OnEnable()
        {
            searchField = new SearchField();
            treeViewState = new TreeViewState();
            var multiColumnHeaderState = new MultiColumnHeader(QuarkEditorUtility.CreateManifestMergeMultiColumnHeader());
            treeView = new QuarkManifestMergeTreeView(treeViewState, multiColumnHeaderState);
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
        public void Clear()
        {
            treeView.Clear();
            totalBundleLength = 0;
            incrementalBundleCount = 0;
            builtinBundleCount = 0;
            totalIncrementalBundleLength = 0;
            totalBuiltInBundleLength = 0;
            totalBundleFormatSize = QuarkUtility.FormatBytes(totalBundleLength);
            totalIncrementalBundleFormatSize = QuarkUtility.FormatBytes(totalIncrementalBundleLength);
            totalBuiltInBundleFormatSize = QuarkUtility.FormatBytes(totalBuiltInBundleLength);
            this.mergedManifest = null;
        }
        public void SetManifest(QuarkMergedManifest mergedManifest)
        {
            this.mergedManifest = mergedManifest;
            totalBundleLength = 0;
            incrementalBundleCount = 0;
            builtinBundleCount = 0;
            totalIncrementalBundleLength = 0;
            totalBuiltInBundleLength = 0;
            totalBundleCount = 0;
            if (mergedManifest != null)
            {
                foreach (var mb in mergedManifest.MergedBundles)
                {
                    var bundleSize = mb.QuarkBundleAsset.BundleSize;
                    totalBundleLength += bundleSize;
                    if (mb.IsIncremental)
                    {
                        totalIncrementalBundleLength += bundleSize;
                        incrementalBundleCount++;
                    }
                    else
                    {
                        totalBuiltInBundleLength += bundleSize;
                        builtinBundleCount++;
                    }
                }
                totalBundleCount = mergedManifest.MergedBundles.Count;
            }
            totalBundleFormatSize = QuarkUtility.FormatBytes(totalBundleLength);
            totalIncrementalBundleFormatSize = QuarkUtility.FormatBytes(totalIncrementalBundleLength);
            totalBuiltInBundleFormatSize = QuarkUtility.FormatBytes(totalBuiltInBundleLength);
            treeView.SetManifest(mergedManifest);
        }
        void DrawManifestDetail(Rect rect)
        {
            GUILayout.BeginVertical(GUILayout.MaxWidth(rect.width * 0.3f));
            {
                string buildVersion = string.Empty;
                string internalBuildVersion = string.Empty;
                string buildHash = string.Empty;
                string buildTime = string.Empty;

                if (mergedManifest != null)
                {
                    buildVersion = mergedManifest.BuildVersion;
                    internalBuildVersion = mergedManifest.InternalBuildVersion.ToString();
                    buildHash = mergedManifest.BuildHash;
                    buildTime = mergedManifest.BuildTime;
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

                GUILayout.Space(8);

                EditorGUILayout.LabelField($"Incremental bundle", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Incremental bundle count: {incrementalBundleCount}");
                EditorGUILayout.LabelField($"Incremental bundle length: {totalIncrementalBundleLength}");
                EditorGUILayout.LabelField($"Incremental bundle Format size : {totalIncrementalBundleFormatSize}");

                GUILayout.Space(8);
                EditorGUILayout.LabelField($"Built-In bundle", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Built-In bundle count: {builtinBundleCount}");
                EditorGUILayout.LabelField($"Built-In bundle length: {totalBuiltInBundleLength}");
                EditorGUILayout.LabelField($"Built-In bundles Format size : {totalBuiltInBundleFormatSize}");
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
                    if (GUILayout.Button("Bundle merge type filter", EditorStyles.toolbarPopup, GUILayout.MaxWidth(192)))
                    {
                        var popup = new MergeTypePopup();
                        popup.onClose = () => owner.RefreshMergedManifest();
                        PopupWindow.Show(buttonRect, popup);
                    }
                    if (Event.current.type == EventType.Repaint)
                        buttonRect = GUILayoutUtility.GetLastRect();
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
