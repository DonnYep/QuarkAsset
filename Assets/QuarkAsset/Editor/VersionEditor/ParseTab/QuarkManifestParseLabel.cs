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

        QuarkParseBundleTreeView bundleTreeView;
        QuarkParseDependentTreeView dependentTreeView;

        TreeViewState bundleTreeViewState;
        TreeViewState dependentTreeViewState;

        SearchField bundleSearchField;
        SearchField dependentSearchField;

        long totalBundleCount;
        long totalBundleLength;
        string totalBundleFormatSize;

        long selectedTotalBundleCount;
        long selectedTotalBundleLength;
        string selectedTotalBundleFormatSize;
        public void OnEnable()
        {
            bundleSearchField = new SearchField();
            dependentSearchField = new SearchField();
            bundleTreeViewState = new TreeViewState();
            dependentTreeViewState = new TreeViewState();
            var multiColumnHeaderState = new MultiColumnHeader(QuarkEditorUtility.CreateManifestParseBundleMultiColumnHeader());
            bundleTreeView = new QuarkParseBundleTreeView(bundleTreeViewState, multiColumnHeaderState);
            bundleTreeView.onBundleSelectionChanged = OnBundleSelectionChanged;
            var depMultiColumnHeaderState = new MultiColumnHeader(QuarkEditorUtility.CreateManifestParseDependentMultiColumnHeader());
            dependentTreeView = new QuarkParseDependentTreeView(dependentTreeViewState, depMultiColumnHeaderState);
            bundleSearchField.downOrUpArrowKeyPressed += bundleTreeView.SetFocusAndEnsureSelectedItem;
            dependentSearchField.downOrUpArrowKeyPressed += dependentTreeView.SetFocusAndEnsureSelectedItem;
            ResetSelectedInfo();
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
            ResetSelectedInfo();
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
            bundleTreeView.SetManifest(manifest);
            dependentTreeView.SetManifest(manifest);
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

                GUILayout.Space(8);

                EditorGUILayout.LabelField($"Selected bundle ", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Selected bundle count: {selectedTotalBundleCount}");
                EditorGUILayout.LabelField($"Selected bundle total length: {selectedTotalBundleLength}");
                EditorGUILayout.LabelField($"Selected bundle total format size : {selectedTotalBundleFormatSize}");
            }
            GUILayout.EndVertical();
            //0.62f
        }
        void DrawTreeView(Rect rect)
        {
            GUILayout.BeginVertical(GUILayout.MaxWidth(rect.width * 0.7f));
            {
                GUILayout.BeginVertical(GUILayout.MaxHeight(rect.height * 0.62f));
                {
                    GUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("Bundles", EditorStyles.boldLabel, GUILayout.MaxWidth(92));

                        EditorGUILayout.LabelField("Search", GUILayout.MaxWidth(48));
                        bundleTreeView.searchString = bundleSearchField.OnToolbarGUI(bundleTreeView.searchString);
                    }
                    GUILayout.EndHorizontal();

                    Rect bundleViewRect = GUILayoutUtility.GetRect(32, 2048, 32, 4096);
                    bundleTreeView.OnGUI(bundleViewRect);
                }
                GUILayout.EndVertical();

                GUILayout.Space(16);

                GUILayout.BeginVertical(GUILayout.MaxHeight(rect.height * 0.38f));
                {
                    GUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("Dependencies", EditorStyles.boldLabel, GUILayout.MaxWidth(92));

                        EditorGUILayout.LabelField("Search", GUILayout.MaxWidth(48));
                        dependentTreeView.searchString = dependentSearchField.OnToolbarGUI(dependentTreeView.searchString);
                        if (GUILayout.Button("ExpandAll", EditorStyles.miniButton, GUILayout.MaxWidth(92)))
                        {
                            dependentTreeView.ExpandAll();
                        }
                        if (GUILayout.Button("CollapseAll", EditorStyles.miniButton, GUILayout.MaxWidth(92)))
                        {
                            dependentTreeView.CollapseAll();
                        }
                    }
                    GUILayout.EndHorizontal();

                    Rect dependentViewRect = GUILayoutUtility.GetRect(32, 2048, 32, 4096);
                    dependentTreeView.OnGUI(dependentViewRect);
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndVertical();
        }
        void OnBundleSelectionChanged(IEnumerable<QuarkBundleAsset> bundles)
        {
            dependentTreeView.AddSelectBundles(bundles);
            selectedTotalBundleCount = 0;
            selectedTotalBundleLength = 0;
            foreach (var b in bundles)
            {
                selectedTotalBundleCount++;
                selectedTotalBundleLength += b.BundleSize;
            }
            selectedTotalBundleFormatSize = QuarkUtility.FormatBytes(selectedTotalBundleLength);
        }
        void ResetSelectedInfo()
        {
            selectedTotalBundleCount = 0;
            selectedTotalBundleLength = 0;
            selectedTotalBundleFormatSize = QuarkUtility.FormatBytes(selectedTotalBundleLength);
        }
    }
}
