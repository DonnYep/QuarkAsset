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
        QuarkBuildProfile buildProfile;
        Texture2D createAddNewIcon;
        Texture2D saveActiveIcon;

        public void SetAssetDatabaseTab(QuarkAssetDatabaseTab assetDatabaseTab)
        {
            this.assetDatabaseTab = assetDatabaseTab;
        }
        public void OnDisable()
        {
            QuarkEditorUtility.SaveScriptableObject(buildProfile);
            if (buildProfile != null)
            {
                tabData.BuildProfilePath = AssetDatabase.GetAssetPath(buildProfile);
            }
            QuarkEditorUtility.SaveData(AssetBundleTabDataFileName, tabData);
        }
        public void OnEnable()
        {
            createAddNewIcon = QuarkEditorUtility.GetCreateAddNewIcon();
            saveActiveIcon = QuarkEditorUtility.GetSaveActiveIcon();

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
            if (!string.IsNullOrEmpty(tabData.BuildProfilePath))
            {
                buildProfile = AssetDatabase.LoadAssetAtPath<QuarkBuildProfile>(tabData.BuildProfilePath);
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

            tabData.ForceRemoveAllAssetBundleNames = EditorGUILayout.ToggleLeft("Force remove all assetbundle names before build", tabData.ForceRemoveAllAssetBundleNames);

            GUILayout.Space(16);
            tabData.UseBuildProfile = EditorGUILayout.ToggleLeft("Use build profile", tabData.UseBuildProfile);
            GUILayout.Space(16);
            if (tabData.UseBuildProfile)
            {
                DrawProfileBuildSettingLabel();
            }
            else
            {
                DrawBuildSettingLabel();
                DrawAESEncryptionForBuildInfoLable();
                DrawOffsetEncryptionForAssetBundleLable();
            }

            GUILayout.Space(16);

            DrawBuildPathLabel();

            GUILayout.Space(16);

            DrawCopyToStreamingLabel();

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
        void DrawProfileBuildSettingLabel()
        {
            EditorGUILayout.BeginHorizontal();
            {
                buildProfile = (QuarkBuildProfile)EditorGUILayout.ObjectField("Build profile", buildProfile, typeof(QuarkBuildProfile), false);
                if (GUILayout.Button(createAddNewIcon, GUILayout.MaxWidth(28)))
                {
                    buildProfile = CreateBuildProfile();
                }
                if (GUILayout.Button(saveActiveIcon, GUILayout.MaxWidth(28)))
                {
                    QuarkEditorUtility.SaveScriptableObject(buildProfile);
                }
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(9);

            EditorGUILayout.BeginVertical("box");
            {
                if (buildProfile != null)
                {
                    buildProfile.ProfileDescription = EditorGUILayout.TextField("Description", buildProfile.ProfileDescription);
                    buildProfile.BuildVersion = EditorGUILayout.TextField("Build version", buildProfile.BuildVersion);
                    buildProfile.BuildType = (BuildType)EditorGUILayout.EnumPopup("Build type", buildProfile.BuildType);
                    switch (buildProfile.BuildType)
                    {
                        case BuildType.Full:
                            {
                                buildProfile.InternalBuildVersion = EditorGUILayout.IntField("Internal build version", buildProfile.InternalBuildVersion);
                                if (buildProfile.InternalBuildVersion < 0)
                                    buildProfile.InternalBuildVersion = 0;
                                var buildVersion = buildProfile.BuildVersion;
                                if (!string.IsNullOrEmpty(buildVersion))
                                {
                                    tabData.AssetBundleOutputPath = Path.Combine(tabData.BuildPath, buildVersion, tabData.BuildTarget.ToString(), $"{buildVersion}_{buildProfile.InternalBuildVersion}").Replace("\\", "/");
                                }
                                else
                                {
                                    tabData.AssetBundleOutputPath = Path.Combine(tabData.BuildPath, tabData.BuildTarget.ToString(), $"_{buildProfile.InternalBuildVersion}").Replace("\\", "/");
                                }
                            }
                            break;
                        case BuildType.Incremental:
                            {
                                tabData.AssetBundleOutputPath = Path.Combine(tabData.BuildPath, buildProfile.BuildVersion, tabData.BuildTarget.ToString(), buildProfile.BuildVersion).Replace("\\", "/");
                            }
                            break;
                    }
                    buildProfile.AssetBundleNameType = (AssetBundleNameType)EditorGUILayout.EnumPopup("Bundle name type", buildProfile.AssetBundleNameType);

                    buildProfile.UseAesEncryptionForManifest = EditorGUILayout.ToggleLeft("Aes encryption for buildInfo and manifest", buildProfile.UseAesEncryptionForManifest);
                    if (buildProfile.UseAesEncryptionForManifest)
                    {
                        EditorGUILayout.LabelField("BuildInfo AES encryption key, key should be 16,24 or 32 bytes long");
                        buildProfile.AesEncryptionKeyForManifest = EditorGUILayout.TextField("AESKey", buildProfile.AesEncryptionKeyForManifest);

                        var aesKeyStr = buildProfile.AesEncryptionKeyForManifest;
                        int aesKeyLength = 0;
                        if (string.IsNullOrEmpty(aesKeyStr))
                        {
                            aesKeyLength = 0;
                        }
                        else
                        {
                            aesKeyLength = System.Text.Encoding.UTF8.GetBytes(aesKeyStr).Length;
                        }
                        EditorGUILayout.LabelField($"Current key length is:{aesKeyLength}");
                        if (aesKeyLength != 16 && aesKeyLength != 24 && aesKeyLength != 32 && aesKeyLength != 0)
                        {
                            EditorGUILayout.HelpBox("Key should be 16,24 or 32 bytes long", MessageType.Error);
                        }
                        GUILayout.Space(16);
                    }

                    buildProfile.UseOffsetEncryptionForAssetBundle = EditorGUILayout.ToggleLeft("Offset encryption for asserBundle", buildProfile.UseOffsetEncryptionForAssetBundle);
                    if (buildProfile.UseOffsetEncryptionForAssetBundle)
                    {
                        EditorGUILayout.LabelField("AssetBundle encryption offset");
                        buildProfile.EncryptionOffsetForAssetBundle = EditorGUILayout.IntField("Encryption offset", buildProfile.EncryptionOffsetForAssetBundle);
                        if (buildProfile.EncryptionOffsetForAssetBundle < 0)
                            buildProfile.EncryptionOffsetForAssetBundle = 0;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        void DrawCopyToStreamingLabel()
        {
            GUILayout.BeginVertical();
            {
                tabData.CopyToStreamingAssets = EditorGUILayout.ToggleLeft("CopyToStreamingAssets", tabData.CopyToStreamingAssets);
                if (tabData.CopyToStreamingAssets)
                {
                    tabData.ClearStreamingAssetsDestinationPath = EditorGUILayout.ToggleLeft("Clear streaming assets destination path", tabData.ClearStreamingAssetsDestinationPath);

                    var streamingRelativePath = tabData.StreamingRelativePath.Trim();
                    if (string.IsNullOrEmpty(streamingRelativePath))
                    {
                        GUILayout.Label("Assets/StreamingAssets/[ Nullable ]");
                    }
                    else
                    {
                        GUILayout.Label($"Assets/StreamingAssets/{streamingRelativePath}");
                    }
                    tabData.StreamingRelativePath = EditorGUILayout.TextField("StreamingRelativePath", streamingRelativePath);
                }
            }
            GUILayout.EndVertical();
        }
        void DrawBuildButtonLabel()
        {
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
        void DrawBuildSettingLabel()
        {
            tabData.BuildType = (BuildType)EditorGUILayout.EnumPopup("Build type", tabData.BuildType);
            tabData.AssetBundleNameType = (AssetBundleNameType)EditorGUILayout.EnumPopup("Bundle name type", tabData.AssetBundleNameType);
            tabData.BuildVersion = EditorGUILayout.TextField("Build version", tabData.BuildVersion?.Trim());

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
        }
        void DrawBuildPathLabel()
        {
            GUILayout.BeginHorizontal();
            {
                tabData.BuildPath = EditorGUILayout.TextField("Build path", tabData.BuildPath.Trim());
                if (GUILayout.Button("Browse", GUILayout.MaxWidth(128f)))
                {
                    BrowseFolder();
                }
            }
            GUILayout.EndHorizontal();
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
        }
        QuarkBuildProfile CreateBuildProfile()
        {
            var so = QuarkEditorUtility.CreateScriptableObject<QuarkBuildProfile>("Assets/Editor/NewQuarkBuildProfile.asset");
            return so;
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
            var startTime = DateTime.Now;
            var assetBundleBuildPath = tabData.AssetBundleOutputPath;
            QuarkBuildParams buildParams = new QuarkBuildParams()
            {
                AssetBundleOutputPath = tabData.AssetBundleOutputPath,
                BuildPath = tabData.BuildPath,
                AssetBundleCompressType = tabData.AssetBundleCompressType,
                BuildAssetBundleOptions = tabData.BuildAssetBundleOptions,
                BuildTarget = tabData.BuildTarget,
                CopyToStreamingAssets = tabData.CopyToStreamingAssets,
                ClearStreamingAssetsDestinationPath = tabData.ClearStreamingAssetsDestinationPath,
                StreamingRelativePath = tabData.StreamingRelativePath
            };
            if (tabData.UseBuildProfile)
            {
                if (buildProfile != null)
                {
                    buildParams.BuildType = buildProfile.BuildType;
                    buildParams.BuildVersion = buildProfile.BuildVersion;
                    buildParams.InternalBuildVersion = buildProfile.InternalBuildVersion;
                    buildParams.AssetBundleNameType = buildProfile.AssetBundleNameType;
                    buildParams.UseOffsetEncryptionForAssetBundle = buildProfile.UseOffsetEncryptionForAssetBundle;
                    buildParams.EncryptionOffsetForAssetBundle = buildProfile.EncryptionOffsetForAssetBundle;
                    buildParams.UseAesEncryptionForManifest = buildProfile.UseAesEncryptionForManifest;
                    buildParams.AesEncryptionKeyForManifest = buildProfile.AesEncryptionKeyForManifest;
                }
                else
                {
                    QuarkUtility.LogError("Quark build profile is invalid !");

                    yield break;
                }
            }
            else
            {
                buildParams.BuildType = tabData.BuildType;
                buildParams.BuildVersion = tabData.BuildVersion;
                buildParams.InternalBuildVersion = tabData.InternalBuildVersion;
                buildParams.AssetBundleNameType = tabData.AssetBundleNameType;
                buildParams.UseOffsetEncryptionForAssetBundle = tabData.UseOffsetEncryptionForAssetBundle;
                buildParams.EncryptionOffsetForAssetBundle = tabData.EncryptionOffsetForAssetBundle;
                buildParams.UseAesEncryptionForManifest = tabData.UseAesEncryptionForManifest;
                buildParams.AesEncryptionKeyForManifest = tabData.AesEncryptionKeyForManifest;
            }

            if (tabData.ForceRemoveAllAssetBundleNames)
            {
                QuarkCommand.ForceRemoveAllAssetBundleNames();
            }
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
