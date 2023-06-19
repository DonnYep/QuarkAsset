using System.Collections;
using UnityEngine;
using UnityEditor;
using System.IO;
using Quark.Asset;
using System.Collections.Generic;
using System.Linq;
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

        public void SetAssetDatabaseTab(QuarkAssetDatabaseTab assetDatabaseTab)
        {
            this.assetDatabaseTab = assetDatabaseTab;
        }
        public void OnDisable()
        {
            QuarkEditorUtility.SaveData(AssetBundleTabDataFileName, tabData);
        }
        public void OnEnable()
        {
            buildHandlers = GetBuildHandlerNames();
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
            tabData.BuildTarget = (BuildTarget)EditorGUILayout.EnumPopup("Build target", tabData.BuildTarget);
            tabData.AssetBundleCompressType = (AssetBundleCompressType)EditorGUILayout.EnumPopup("Build compression type", tabData.AssetBundleCompressType);

            DrawBuildHanlder();

            tabData.ForceRebuildAssetBundle = EditorGUILayout.ToggleLeft("Force rebuild assetBundle", tabData.ForceRebuildAssetBundle);
            tabData.DisableWriteTypeTree = EditorGUILayout.ToggleLeft("Disable write type tree", tabData.DisableWriteTypeTree);
            if (tabData.DisableWriteTypeTree)
                tabData.IgnoreTypeTreeChanges = false;

            tabData.DeterministicAssetBundle = EditorGUILayout.ToggleLeft("Deterministic assetBundle", tabData.DeterministicAssetBundle);
            tabData.IgnoreTypeTreeChanges = EditorGUILayout.ToggleLeft("Ignore type tree changes", tabData.IgnoreTypeTreeChanges);
            if (tabData.IgnoreTypeTreeChanges)
                tabData.DisableWriteTypeTree = false;

            var buildOptions = QuarkBuildController.GetBuildAssetBundleOptions(tabData.AssetBundleCompressType,
                tabData.DisableWriteTypeTree, tabData.DeterministicAssetBundle, tabData.ForceRebuildAssetBundle, tabData.IgnoreTypeTreeChanges);
            tabData.BuildAssetBundleOptions = buildOptions;

            GUILayout.Space(16);

            GUILayout.BeginHorizontal();
            {
                tabData.BuildPath = EditorGUILayout.TextField("Build path", tabData.BuildPath.Trim());
                if (GUILayout.Button("Browse", GUILayout.MaxWidth(128f)))
                {
                    BrowseFolder();
                }
            }
            GUILayout.EndHorizontal();
            tabData.BuildVersion = EditorGUILayout.TextField("Build version", tabData.BuildVersion?.Trim());

            tabData.BuildType = (BuildType)EditorGUILayout.EnumPopup("Build type", tabData.BuildType);
            switch (tabData.BuildType)
            {
                case BuildType.Full:
                    {
                        tabData.InternalBuildVersion = EditorGUILayout.IntField("Internal build version", tabData.InternalBuildVersion);
                        if (tabData.InternalBuildVersion < 0)
                            tabData.InternalBuildVersion = 0;
                        tabData.AssetBundleOutputPath = Path.Combine(tabData.BuildPath, tabData.BuildVersion, tabData.BuildTarget.ToString(), $"{tabData.BuildVersion}_{tabData.InternalBuildVersion}").Replace("\\", "/");
                    }
                    break;
                case BuildType.Incremental:
                    {
                        tabData.AssetBundleOutputPath = Path.Combine(tabData.BuildPath, tabData.BuildVersion, tabData.BuildTarget.ToString(), tabData.BuildVersion).Replace("\\", "/");
                    }
                    break;
            }

            //tabData.AssetBundleOutputPath = Path.Combine(tabData.BuildPath, tabData.BuildVersion, tabData.BuildTarget.ToString(), $"{tabData.InternalBuildVersion}").Replace("\\", "/");
            EditorGUILayout.LabelField("Build full path", tabData.AssetBundleOutputPath);


            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Open build Path", GUILayout.MaxWidth(128f)))
                {
                    var path = tabData.AssetBundleOutputPath;
                    if (!Directory.Exists(path))
                    {
                        EditorUtility.RevealInFinder(Application.dataPath);
                    }
                    else
                    {
                        EditorUtility.RevealInFinder(path);
                    }
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(32);

            tabData.AssetBundleNameType = (AssetBundleNameType)EditorGUILayout.EnumPopup("Bundle name type", tabData.AssetBundleNameType);
            tabData.ClearOutputFolders = EditorGUILayout.ToggleLeft("ClearOutputFolders", tabData.ClearOutputFolders);


            GUILayout.BeginVertical();
            {
                tabData.CopyToStreamingAssets = EditorGUILayout.ToggleLeft("CopyToStreamingAssets", tabData.CopyToStreamingAssets);
                if (tabData.CopyToStreamingAssets)
                {
                    GUILayout.Label("Assets/StreamingAssets/[ Nullable ]");
                    tabData.StreamingRelativePath = EditorGUILayout.TextField("StreamingRelativePath", tabData.StreamingRelativePath.Trim());
                }
            }
            GUILayout.EndVertical();


            GUILayout.Space(16);

            DrawAESEncryptionForBuildInfoLable();
            DrawOffsetEncryptionForAssetBundleLable();

            GUILayout.Space(16);

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Build"))
                {
                    if (dataset != null)
                    {
                        if (tabData.UseAesEncryptionForManifest)
                        {
                            var aesKeyStr = tabData.AesEncryptionKeyForManifest;
                            var aesKeyLength = System.Text.Encoding.UTF8.GetBytes(aesKeyStr).Length;
                            if (aesKeyLength != 16 && aesKeyLength != 24 && aesKeyLength != 32)
                            {
                                QuarkUtility.LogError("QuarkAsset build aes key is invalid , key should be 16,24 or 32 bytes long !");
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
                }
            }
            GUILayout.EndHorizontal();
        }
        string[] GetBuildHandlerNames()
        {
            var srcHandlers = QuarkUtility.GetDerivedTypeNames<IQuarkBuildHandler>();
            var buildHandlerNames = new string[srcHandlers.Length + 1];
            buildHandlerNames[0] = "<NONE>";
            Array.Copy(srcHandlers, 0, buildHandlerNames, 1, srcHandlers.Length);
            return buildHandlerNames;
        }
        IQuarkBuildHandler GetBuildHandler()
        {
            IQuarkBuildHandler buildHandler = null;
            if (!string.IsNullOrEmpty(tabData.QuarkBuildHandlerName) && tabData.QuarkBuildHandlerName != QuarkConstant.NONE)
            {
                buildHandler = (IQuarkBuildHandler)QuarkUtility.GetTypeInstance(tabData.QuarkBuildHandlerName);
            }
            return buildHandler;
        }
        void DrawBuildHanlder()
        {
            tabData.QuarkBuildHandlerIndex = EditorGUILayout.Popup("Build handler", tabData.QuarkBuildHandlerIndex, buildHandlers);
            var index = tabData.QuarkBuildHandlerIndex;
            if (buildHandlers.Length > 0 && index < buildHandlers.Length)
            {
                tabData.QuarkBuildHandlerName = buildHandlers[index];
            }
        }
        void DrawAESEncryptionForBuildInfoLable()
        {
            tabData.UseAesEncryptionForManifest = EditorGUILayout.ToggleLeft("Aes encryption for buildInfo and manifest", tabData.UseAesEncryptionForManifest);
            if (tabData.UseAesEncryptionForManifest)
            {
                EditorGUILayout.LabelField("BuildInfo AES encryption key, key should be 16,24 or 32 bytes long");
                tabData.AesEncryptionKeyForManifest = EditorGUILayout.TextField("AESKey", tabData.AesEncryptionKeyForManifest);

                var aesKeyStr = tabData.AesEncryptionKeyForManifest;
                var aesKeyLength = System.Text.Encoding.UTF8.GetBytes(aesKeyStr).Length;
                EditorGUILayout.LabelField($"Current key length is:{aesKeyLength}");
                if (aesKeyLength != 16 && aesKeyLength != 24 && aesKeyLength != 32 && aesKeyLength != 0)
                {
                    EditorGUILayout.HelpBox("Key should be 16,24 or 32 bytes long", MessageType.Error);
                }
                GUILayout.Space(16);
            }
        }
        void DrawOffsetEncryptionForAssetBundleLable()
        {
            tabData.UseOffsetEncryptionForAssetBundle = EditorGUILayout.ToggleLeft("Offset encryption for asserBundle", tabData.UseOffsetEncryptionForAssetBundle);
            if (tabData.UseOffsetEncryptionForAssetBundle)
            {
                EditorGUILayout.LabelField("AssetBundle encryption offset");
                tabData.EncryptionOffsetForAssetBundle = EditorGUILayout.IntField("Encryption offset", tabData.EncryptionOffsetForAssetBundle);
                if (tabData.EncryptionOffsetForAssetBundle < 0)
                    tabData.EncryptionOffsetForAssetBundle = 0;
            }
        }
        void BrowseFolder()
        {
            var newPath = EditorUtility.OpenFolderPanel("Bundle Folder", tabData.BuildPath, string.Empty);
            if (!string.IsNullOrEmpty(newPath))
            {
                tabData.BuildPath = newPath.Replace("\\", "/");
            }
        }
        IEnumerator EnumBuildAssetBundle()
        {
            var assetBundleBuildPath = tabData.AssetBundleOutputPath;


            var buildParams = new QuarkBuildParams()
            {
                AesEncryptionKeyForManifest = tabData.AesEncryptionKeyForManifest,
                AssetBundleOutputPath = tabData.AssetBundleOutputPath,
                BuildPath = tabData.BuildPath,
                AssetBundleCompressType = tabData.AssetBundleCompressType,
                AssetBundleNameType = tabData.AssetBundleNameType,
                BuildAssetBundleOptions = tabData.BuildAssetBundleOptions,
                BuildTarget = tabData.BuildTarget,
                BuildVersion = tabData.BuildVersion,
                ClearOutputFolders = tabData.ClearOutputFolders,
                CopyToStreamingAssets = tabData.CopyToStreamingAssets,
                EncryptionOffsetForAssetBundle = tabData.EncryptionOffsetForAssetBundle,
                InternalBuildVersion = tabData.InternalBuildVersion,
                StreamingRelativePath = tabData.StreamingRelativePath,
                UseAesEncryptionForManifest = tabData.UseAesEncryptionForManifest,
                UseOffsetEncryptionForAssetBundle = tabData.UseOffsetEncryptionForAssetBundle,
                BuildType = tabData.BuildType
            };
            yield return assetDatabaseTab.BuildDataset();
            switch (tabData.BuildType)
            {
                case BuildType.Full:
                    {
                        QuarkUtility.EmptyFolder(assetBundleBuildPath);
                        FullBuild(buildParams);
                    }
                    break;
                case BuildType.Incremental:
                    QuarkUtility.CreateFolder(assetBundleBuildPath);
                    IncrementalBuild(buildParams);
                    break;
            }
            QuarkUtility.LogInfo("Quark assetbundle build done ");
        }
        /// <summary>
        /// 全量构建
        /// </summary>
        void FullBuild(QuarkBuildParams buildParams)
        {
            var buildHanlder = GetBuildHandler();
            var hasBuildHandler = buildHanlder != null;
            if (hasBuildHandler)
            {
                buildHanlder.OnBuildStart(tabData.BuildTarget, buildParams.AssetBundleOutputPath);
            }
            dataset.CacheAllBundleInfos();
            var quarkManifest = new QuarkManifest();
            QuarkBuildController.ProcessBundleInfos(dataset, quarkManifest, buildParams);
            QuarkBuildController.SetBundleDependent(dataset, quarkManifest);
            var assetBundleManifest = BuildPipeline.BuildAssetBundles(buildParams.AssetBundleOutputPath, tabData.BuildAssetBundleOptions, tabData.BuildTarget);
            QuarkBuildController.FinishBuild(assetBundleManifest, dataset, quarkManifest, buildParams);
            QuarkBuildController.OverwriteManifest(quarkManifest, buildParams);
            QuarkBuildController.CopyToStreamingAssets(buildParams);
            if (hasBuildHandler)
            {
                buildHanlder.OnBuildComplete(tabData.BuildTarget, buildParams.AssetBundleOutputPath);
            }
            //QuarkBuildController.GenerateBuildCache(quarkManifest, buildParams);
        }
        /// <summary>
        /// 增量构建
        /// </summary>
        void IncrementalBuild(QuarkBuildParams buildParams)
        {
            var buildHanlder = GetBuildHandler();
            var hasBuildHandler = buildHanlder != null;
            if (hasBuildHandler)
            {
                buildHanlder.OnBuildStart(tabData.BuildTarget, buildParams.AssetBundleOutputPath);
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
                    buildHanlder.OnBuildComplete(tabData.BuildTarget, buildParams.AssetBundleOutputPath);
                }
                var newBuildCache = new QuarkBuildCache
                {
                    BundleCacheList = info.BundleCaches,
                    BuildVerison = buildParams.BuildVersion,
                    InternalBuildVerison = buildParams.InternalBuildVersion,
                    NameType = buildParams.AssetBundleNameType
                };
                QuarkBuildController.FinishBuild(assetBundleManifest, dataset, quarkManifest, buildParams);
                QuarkBuildController.OverwriteBuildCache(newBuildCache, buildParams);
                QuarkBuildController.OverwriteManifest(quarkManifest, buildParams);
                QuarkBuildController.GenerateIncrementalBuildLog(info, buildParams);
            }
            else
            {
                QuarkBuildController.OverwriteManifest(quarkManifest, buildParams);
                QuarkUtility.LogInfo("No bundle changed !");
            }
            QuarkBuildController.CopyToStreamingAssets(buildParams);
        }
    }
}
