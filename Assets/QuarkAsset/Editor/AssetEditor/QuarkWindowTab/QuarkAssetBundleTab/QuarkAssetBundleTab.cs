using System.Collections;
using UnityEngine;
using UnityEditor;
using System.IO;
using Quark.Asset;
using System.Collections.Generic;
using System;

namespace Quark.Editor
{
    public class QuarkAssetBundleTab
    {
        QuarkAssetBundleTabData tabData;
        const string AssetBundleTabDataFileName = "QuarkAsset_AssetBundleTabData.json";
        QuarkDataset dataset { get { return QuarkEditorDataProxy.QuarkAssetDataset; } }
        QuarkAssetDatabaseTab assetDatabaseTab;
        string[] buildHandlers;
        //Texture2D createAddNewIcon;
        //Texture2D saveActiveIcon;
        internal QuarkAssetBundleTabData TabData
        {
            get { return tabData; }
        }

        QuarkAssetBundleTabNoProfileLabel noProfileLabel = new QuarkAssetBundleTabNoProfileLabel();
        QuarkAssetBundleTabProfileLabel profileLabel = new QuarkAssetBundleTabProfileLabel();

        public void SetAssetDatabaseTab(QuarkAssetDatabaseTab assetDatabaseTab)
        {
            this.assetDatabaseTab = assetDatabaseTab;
        }
        public void OnEnable()
        {
            GetTabData();
            //createAddNewIcon = QuarkEditorUtility.GetCreateAddNewIcon();
            //saveActiveIcon = QuarkEditorUtility.GetSaveActiveIcon();
            buildHandlers = QuarkEditorUtility.GetDerivedTypeHandlers<IQuarkBuildHandler>();
            noProfileLabel.OnEnable(this,buildHandlers);
            profileLabel.OnEnable(this, buildHandlers);
        }
        public void OnDisable()
        {
            noProfileLabel.OnDisable();
            profileLabel.OnDisable();
            SaveTabData();
        }
        public void OnDatasetAssign()
        {
        }
        public void OnDatasetRefresh()
        {

        }
        public void OnDatasetUnassign()
        {
        }
        public void OnGUI()
        {
            tabData.UseBuildProfile = EditorGUILayout.ToggleLeft("Use build profile", tabData.UseBuildProfile);
            EditorGUILayout.Space(16);
            if (tabData.UseBuildProfile)
            {
                profileLabel.OnGUI();
            }
            else
            {
                noProfileLabel.OnGUI();
            }

            GUILayout.Space(16);
            DrawBuildButtonLabel();
        }
        string[] GetBuildHandlerNames()
        {
            var srcHandlers = QuarkUtility.GetDerivedTypeNames<IQuarkBuildHandler>();
            var buildHandlerNames = new string[srcHandlers.Length + 1];
            buildHandlerNames[0] = "<NONE>";
            Array.Copy(srcHandlers, 0, buildHandlerNames, 1, srcHandlers.Length);
            return buildHandlerNames;
        }



        void DrawBuildButtonLabel()
        {
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Build"))
                {
                    if (dataset != null)
                    {
                        if (tabData.UseBuildProfile)
                        {
                            if (!profileLabel.HasProfile)
                            {
                                QuarkUtility.LogError("Build profile is invalid !");
                                return;
                            }
                            else
                            {
                                QuarkEditorUtility.StartCoroutine(EnumBuildAssetBundle());
                            }
                        }
                        else
                        {
                            QuarkEditorUtility.StartCoroutine(EnumBuildAssetBundle());
                        }
                    }
                    else
                        QuarkUtility.LogError("QuarkAssetDataset is invalid !");
                }
                if (GUILayout.Button("Reset"))
                {
                    tabData = new QuarkAssetBundleTabData();
                    QuarkEditorUtility.SaveData(AssetBundleTabDataFileName, tabData);
                    if (tabData.UseBuildProfile)
                    {
                        profileLabel.Reset();
                    }
                    else
                    {
                        noProfileLabel.Reset();
                    }
                }
            }
            GUILayout.EndHorizontal();
        }
        void GetTabData()
        {
            try
            {
                tabData = QuarkEditorUtility.GetData<QuarkAssetBundleTabData>(AssetBundleTabDataFileName);
            }
            catch
            {
                tabData = new QuarkAssetBundleTabData();
                QuarkEditorUtility.SaveData(AssetBundleTabDataFileName, tabData);
            }
        }
        void SaveTabData()
        {
            QuarkEditorUtility.SaveData(AssetBundleTabDataFileName, tabData);
        }
        IEnumerator EnumBuildAssetBundle()
        {
            var startTime = DateTime.Now;
            QuarkBuildParams buildParams = default;
            if (tabData.UseBuildProfile)
            {
                buildParams = profileLabel.GetBuildParams();
            }
            else
            {
                buildParams = noProfileLabel.GetBuildParams();
            }
            var assetBundleBuildPath = buildParams.AssetBundleOutputPath;
            if (buildParams.ForceRemoveAllAssetBundleNames)
            {
                QuarkCommand.ForceRemoveAllAssetBundleNames();
            }
            //yield return assetDatabaseTab.BuildDataset();
            dataset.CacheAllBundleInfos();
            QuarkBuildController.BuildDataset(dataset);
            yield return null;
            switch (buildParams.BuildType)
            {
                case QuarkBuildType.Full:
                    {
                        QuarkUtility.EmptyFolder(assetBundleBuildPath);
                        FullBuild(buildParams);
                    }
                    break;
                case QuarkBuildType.Incremental:
                    {
                        QuarkUtility.CreateFolder(assetBundleBuildPath);
                        IncrementalBuild(buildParams);
                    }
                    break;
            }
            var endTime = DateTime.Now;
            var elapsedTime = endTime - startTime;
            QuarkUtility.LogInfo($"Quark assetbundle build done , elapsed time : {elapsedTime.Hours}h:{elapsedTime.Minutes}m:{elapsedTime.Seconds}s:{elapsedTime.Milliseconds}ms");
        }
        /// <summary>
        /// 全量构建
        /// </summary>
        void FullBuild(QuarkBuildParams buildParams)
        {
            var buildHanlder = QuarkUtility.GetTypeInstance<IQuarkBuildHandler>(buildParams.BuildHandlerName);
            var hasBuildHandler = buildHanlder != null;
            if (hasBuildHandler)
            {
                buildHanlder.OnBuildStart(buildParams.BuildTarget, buildParams.AssetBundleOutputPath);
            }
            dataset.CacheAllBundleInfos();
            var quarkManifest = new QuarkManifest();
            QuarkBuildController.ProcessBundleInfos(dataset, quarkManifest, buildParams);
            QuarkBuildController.SetBundleDependent(dataset, quarkManifest);
            var assetBundleManifest = BuildPipeline.BuildAssetBundles(buildParams.AssetBundleOutputPath, buildParams.BuildAssetBundleOptions, buildParams.BuildTarget);
            QuarkBuildController.FinishBuild(assetBundleManifest, dataset, quarkManifest, buildParams);
            QuarkBuildController.OverwriteManifest(quarkManifest, buildParams);
            QuarkBuildController.CopyToStreamingAssets(buildParams);
            if (hasBuildHandler)
            {
                buildHanlder.OnBuildComplete(buildParams.BuildTarget, buildParams.AssetBundleOutputPath);
            }
            //QuarkBuildController.GenerateBuildCache(quarkManifest, buildParams);
        }
        /// <summary>
        /// 增量构建
        /// </summary>
        void IncrementalBuild(QuarkBuildParams buildParams)
        {
            var buildHanlder = QuarkUtility.GetTypeInstance<IQuarkBuildHandler>(buildParams.BuildHandlerName);
            var hasBuildHandler = buildHanlder != null;
            if (hasBuildHandler)
            {
                buildHanlder.OnBuildStart(buildParams.BuildTarget, buildParams.AssetBundleOutputPath);
            }
            dataset.CacheAllBundleInfos();
            var quarkManifest = new QuarkManifest();

            QuarkBuildController.ProcessBundleInfos(dataset, quarkManifest, buildParams);
            QuarkBuildController.SetBundleDependent(dataset, quarkManifest);

            QuarkBuildCache buildCache = default;
            try
            {
                var buildCacheWritePath = Path.Combine(buildParams.BuildPath, buildParams.BuildVersion, buildParams.BuildTarget.ToString(), QuarkEditorConstant.BUILD_CACHE_NAME);
                var cacheJson = QuarkUtility.ReadTextFileContent(buildCacheWritePath);
                buildCache = QuarkUtility.ToObject<QuarkBuildCache>(cacheJson);
            }
            catch
            {
                buildCache = new QuarkBuildCache()
                {
                    BundleCacheList = new List<AssetCache>()
                };
            }

            QuarkBuildController.CompareAndProcessBuildCacheFile(buildCache, dataset, buildParams, out var info);
            List<AssetCache> builds = new List<AssetCache>();
            builds.AddRange(info.Changed);
            builds.AddRange(info.NewlyAdded);
            var length = builds.Count;
            if (length > 0)
            {
                QuarkUtility.LogInfo($"{length } bundles has changed !");
                var abBuild = new List<AssetBundleBuild>();
                for (int i = 0; i < length; i++)
                {
                    AssetBundleBuild assetBundleBuild = default;
                    switch (buildParams.AssetBundleNameType)
                    {
                        case AssetBundleNameType.DefaultName:
                            {
                                var cache = builds[i];
                                assetBundleBuild = new AssetBundleBuild()
                                {
                                    assetBundleName = cache.BundleName,
                                    assetNames = cache.AssetNames
                                };
                            }
                            break;
                        case AssetBundleNameType.HashInstead:
                            {
                                var cache = builds[i];
                                assetBundleBuild = new AssetBundleBuild()
                                {
                                    assetBundleName = cache.BundleHash,
                                    assetNames = cache.AssetNames
                                };
                            }
                            break;
                    }
                    abBuild.Add(assetBundleBuild);
                }
                var assetBundleManifest = BuildPipeline.BuildAssetBundles(buildParams.AssetBundleOutputPath, abBuild.ToArray(), buildParams.BuildAssetBundleOptions, buildParams.BuildTarget);
                QuarkBuildController.FinishBuild(assetBundleManifest, dataset, quarkManifest, buildParams);
                if (hasBuildHandler)
                {
                    buildHanlder.OnBuildComplete(buildParams.BuildTarget, buildParams.AssetBundleOutputPath);
                }
                var newBuildCache = new QuarkBuildCache
                {
                    BundleCacheList = info.BundleCaches,
                    BuildVerison = buildParams.BuildVersion,
                    InternalBuildVerison = buildParams.InternalBuildVersion,
                    NameType = buildParams.AssetBundleNameType
                };
                QuarkBuildController.OverwriteBuildCache(newBuildCache, buildParams);
                QuarkBuildController.OverwriteManifest(quarkManifest, buildParams);
                QuarkBuildController.GenerateIncrementalBuildLog(info, buildParams);
            }
            else
            {
                QuarkBuildController.RevertBundleDependencies(dataset);
                QuarkBuildController.OverwriteManifest(quarkManifest, buildParams);
                QuarkUtility.LogInfo("No bundle changed !");
            }
            QuarkBuildController.CopyToStreamingAssets(buildParams);
        }
    }
}
