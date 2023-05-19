using Quark.Asset;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Quark.Editor
{
    public class QuarkBuildController
    {
        public static void BuildDataset(QuarkDataset dataset)
        {
            if (dataset == null)
            {
                QuarkUtility.LogError("QuarkAssetDataset is invalid !");
                return;
            }
            var bundleInfos = dataset.AllCachedBundleInfos;
            var extensions = dataset.QuarkAssetExts;
            List<QuarkObjectInfo> quarkSceneList = new List<QuarkObjectInfo>();
            List<IQuarkBundleInfo> invalidBundleInfos = new List<IQuarkBundleInfo>();
            var sceneAssetFullName = typeof(SceneAsset).FullName;
            int currentBundleIndex = 0;
            foreach (var bundleInfo in bundleInfos)
            {
                currentBundleIndex++;
                var bundlePath = bundleInfo.BundlePath;
                if (!AssetDatabase.IsValidFolder(bundlePath))
                {
                    invalidBundleInfos.Add(bundleInfo);
                    continue;
                }
                bundleInfo.ObjectInfoList.Clear();
                bundleInfo.BundleSize = QuarkEditorUtility.GetUnityDirectorySize(bundlePath, dataset.QuarkAssetExts);
                bundleInfo.BundleFormatBytes = EditorUtility.FormatBytes(bundleInfo.BundleSize);
                bundleInfo.BundleKey = bundleInfo.BundleName;
                var filePaths = Directory.GetFiles(bundlePath, ".", SearchOption.AllDirectories);
                var fileLength = filePaths.Length;
                for (int i = 0; i < fileLength; i++)
                {
                    //强制将文件的后缀名统一成小写
                    var filePath = filePaths[i].Replace("\\", "/");
                    var srcFileExt = Path.GetExtension(filePath);
                    var lowerExt = srcFileExt.ToLower();
                    var lowerExtFilePath = filePath.Replace(srcFileExt, lowerExt);
                    if (extensions.Contains(lowerExt))
                    {
                        var objectInfo = new QuarkObjectInfo()
                        {
                            ObjectName = Path.GetFileNameWithoutExtension(filePath),
                            ObjectExtension = lowerExt,
                            BundleName = bundleInfo.BundleName,
                            ObjectPath = lowerExtFilePath,
                            ObjectType = AssetDatabase.LoadAssetAtPath(filePath, typeof(Object)).GetType().FullName,
                            ObjectSize = QuarkUtility.GetFileSize(filePath)
                        };
                        objectInfo.ObjectValid = AssetDatabase.LoadMainAssetAtPath(objectInfo.ObjectPath) != null;
                        objectInfo.ObjectFormatBytes = EditorUtility.FormatBytes(objectInfo.ObjectSize);
                        if (objectInfo.ObjectType == sceneAssetFullName)
                        {
                            quarkSceneList.Add(objectInfo);
                        }
                        bundleInfo.ObjectInfoList.Add(objectInfo);
                    }
                }
            }
            for (int i = 0; i < invalidBundleInfos.Count; i++)
            {
                bundleInfos.Remove(invalidBundleInfos[i]);
            }
            dataset.QuarkSceneList.Clear();
            dataset.QuarkSceneList.AddRange(quarkSceneList);

            EditorUtility.SetDirty(dataset);
            AssetDatabase.SaveAssets();
        }
        public static void ProcessBundleInfos(QuarkDataset dataset, QuarkManifest quarkManifest, QuarkBuildParams buildParams)
        {
            var bundleInfos = dataset.AllCachedBundleInfos;
            foreach (var bundleInfo in bundleInfos)
            {
                //过滤空包。若文件夹被标记为bundle，且不包含内容，则unity会过滤。因此遵循unity的规范；
                if (bundleInfo.ObjectInfoList.Count <= 0)
                {
                    continue;
                }
                var bundlePath = bundleInfo.BundlePath;
                var importer = AssetImporter.GetAtPath(bundlePath);
                var nameType = buildParams.AssetBundleNameType;
                var bundleName = bundleInfo.BundleName;
                var path = Path.Combine(QuarkEditorUtility.ApplicationPath, bundlePath);
                var hash = QuarkEditorUtility.CreateDirectoryMd5(path);
                switch (nameType)
                {
                    case AssetBundleNameType.DefaultName:
                        bundleInfo.BundleKey = bundleInfo.BundleName;
                        break;
                    case AssetBundleNameType.HashInstead:
                        {
                            bundleName = hash;
                            bundleInfo.BundleKey = hash;
                        }
                        break;
                }
                importer.assetBundleName = bundleName;
                var bundle = new QuarkBundle()
                {
                    BundleKey = bundleInfo.BundleKey,
                    BundleName = bundleInfo.BundleName,
                    BundlePath = bundleInfo.BundlePath
                };
                var objectInfoList = bundleInfo.ObjectInfoList;
                var objectInfoLength = objectInfoList.Count;
                for (int j = 0; j < objectInfoLength; j++)
                {
                    var objectInfo = objectInfoList[j];
                    var quarkObject = new QuarkObject()
                    {
                        ObjectName = objectInfo.ObjectName,
                        ObjectPath = objectInfo.ObjectPath,
                        BundleName = objectInfo.BundleName,
                        ObjectExtension = objectInfo.ObjectExtension,
                        ObjectType = objectInfo.ObjectType
                    };
                    bundle.ObjectList.Add(quarkObject);
                }
                var quarkBundleInfo = new QuarkBundleAsset()
                {
                    Hash = hash,
                    QuarkAssetBundle = bundle,
                    BundleName = bundleInfo.BundleName
                };
                quarkManifest.BundleInfoDict.Add(bundleName, quarkBundleInfo);
            }
        }
        public static void SetBundleDependent(QuarkDataset dataset, QuarkManifest quarkManifest)
        {
            var bundleInfos = dataset.AllCachedBundleInfos;
            AssetDatabase.Refresh();
            for (int i = 0; i < bundleInfos.Count; i++)
            {
                var bundleInfo = bundleInfos[i];
                bundleInfo.DependentBundleKeyList.Clear();
                var importer = AssetImporter.GetAtPath(bundleInfo.BundlePath);
                bundleInfo.DependentBundleKeyList.AddRange(AssetDatabase.GetAssetBundleDependencies(importer.assetBundleName, true));
                if (quarkManifest.BundleInfoDict.TryGetValue(bundleInfo.BundleKey, out var manifestBundleInfo))
                {
                    manifestBundleInfo.QuarkAssetBundle.DependentBundleKeyList.Clear();
                    manifestBundleInfo.QuarkAssetBundle.DependentBundleKeyList.AddRange(bundleInfo.DependentBundleKeyList);
                }
            }
        }
        public static void FinishBuild(AssetBundleManifest manifest, QuarkDataset dataset, QuarkManifest quarkManifest, QuarkBuildParams buildParams)
        {
            var assetBundleBuildPath = buildParams.AssetBundleOutputPath;
            if (manifest == null)
                return;
            var srcBundleInfos = dataset.AllCachedBundleInfos;
            Dictionary<string, IQuarkBundleInfo> bundleKeyDict = null;
            if (buildParams.AssetBundleNameType == AssetBundleNameType.HashInstead)
                bundleKeyDict = srcBundleInfos.ToDictionary(b => b.BundleKey);
            var bundleKeys = manifest.GetAllAssetBundles();
            var bundleKeyLength = bundleKeys.Length;
            for (int i = 0; i < bundleKeyLength; i++)
            {
                var bundleKey = bundleKeys[i];

                var bundlePath = Path.Combine(assetBundleBuildPath, bundleKey);
                long bundleSize = 0;
                if (buildParams.UseOffsetEncryptionForAssetBundle)
                {
                    var bundleBytes = File.ReadAllBytes(bundlePath);
                    var offset = buildParams.EncryptionOffsetForAssetBundle;
                    QuarkUtility.AppendAndWriteAllBytes(bundlePath, new byte[offset], bundleBytes);
                    bundleSize = offset + bundleBytes.Length;
                }
                else
                {
                    var bundleBytes = File.ReadAllBytes(bundlePath);
                    bundleSize = bundleBytes.LongLength;
                }

                var bundleName = string.Empty;
                switch (buildParams.AssetBundleNameType)
                {
                    case AssetBundleNameType.DefaultName:
                        {
                            bundleName = bundleKey;
                        }
                        break;
                    case AssetBundleNameType.HashInstead:
                        {
                            if (bundleKeyDict.TryGetValue(bundleKey, out var bundle))
                                bundleName = bundle.BundleKey;
                        }
                        break;
                }
                if (quarkManifest.BundleInfoDict.TryGetValue(bundleName, out var quarkBundleInfo))
                {
                    quarkBundleInfo.BundleSize = bundleSize;
                }
                var bundleManifestPath = QuarkUtility.Append(bundlePath, ".manifest");
                QuarkUtility.DeleteFile(bundleManifestPath);
            }
            quarkManifest.BuildTime = System.DateTime.Now.ToString();
            quarkManifest.BuildVersion = buildParams.BuildVersion;
            quarkManifest.InternalBuildVersion = buildParams.InternalBuildVersion;
            var manifestJson = QuarkUtility.ToJson(quarkManifest);
            var manifestContext = manifestJson;
            var manifestWritePath = Path.Combine(buildParams.AssetBundleOutputPath, QuarkConstant.MANIFEST_NAME);
            if (buildParams.UseAesEncryptionForManifest)
            {
                var key = QuarkUtility.GenerateBytesAESKey(buildParams.AesEncryptionKeyForManifest);
                manifestContext = QuarkUtility.AESEncryptStringToString(manifestJson, key);
            }
            QuarkUtility.WriteTextFile(manifestWritePath, manifestContext);

            //删除生成文对应的主manifest文件
            var buildMainPath = Path.Combine(buildParams.AssetBundleOutputPath, $"{buildParams.BuildVersion}_{buildParams.InternalBuildVersion}");
            var buildMainManifestPath = QuarkUtility.Append(buildMainPath, ".manifest");
            QuarkUtility.DeleteFile(buildMainPath);
            QuarkUtility.DeleteFile(buildMainManifestPath);
            var bundleInfos = srcBundleInfos;
            var bundleInfoLength = bundleInfos.Count;

            //这段还原dataset在editor模式的依赖
            for (int i = 0; i < bundleInfoLength; i++)
            {
                var bundleInfo = bundleInfos[i];
                var importer = AssetImporter.GetAtPath(bundleInfo.BundlePath);
                importer.assetBundleName = bundleInfo.BundleName;
                bundleInfo.BundleKey = bundleInfo.BundleName;
            }
            for (int i = 0; i < bundleInfoLength; i++)
            {
                var bundleInfo = bundleInfos[i];
                var importer = AssetImporter.GetAtPath(bundleInfo.BundlePath);
                bundleInfo.DependentBundleKeyList.Clear();
                bundleInfo.DependentBundleKeyList.AddRange(AssetDatabase.GetAssetBundleDependencies(importer.assetBundleName, true));
            }

            for (int i = 0; i < bundleInfoLength; i++)
            {
                var bundle = bundleInfos[i];
                var importer = AssetImporter.GetAtPath(bundle.BundlePath);
                importer.assetBundleName = string.Empty;
            }
            if (buildParams.CopyToStreamingAssets)
            {
                var buildPath = buildParams.AssetBundleOutputPath;
                if (Directory.Exists(buildPath))
                {
                    var streamingAssetPath = Path.Combine(Application.streamingAssetsPath, buildParams.StreamingRelativePath);
                    QuarkUtility.CopyDirectory(buildPath, streamingAssetPath);
                }
            }
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            AssetDatabase.RemoveUnusedAssetBundleNames();
            System.GC.Collect();
        }
        public static void GenerateBuildCache(QuarkManifest quarkManifest, QuarkBuildParams buildParams)
        {
            List<AssetCache> bundleCacheList = new List<AssetCache>();
            foreach (var bundleInfo in quarkManifest.BundleInfoDict.Values)
            {
                var assetNames = bundleInfo.QuarkAssetBundle.ObjectList.Select(o => o.ObjectPath).ToArray();

                var bundleCache = new AssetCache()
                {
                    BundleHash = bundleInfo.Hash,
                    BundleName = bundleInfo.BundleName,
                    //BundleSize = bundleInfo.BundleSize,
                    BundlePath = bundleInfo.QuarkAssetBundle.BundlePath,
                    AssetNames = assetNames
                };
                bundleCacheList.Add(bundleCache);
            }
            var buildCache = new QuarkBuildCache()
            {
                BuildVerison = buildParams.BuildVersion,
                InternalBuildVerison = buildParams.InternalBuildVersion,
                BundleCacheList = bundleCacheList
            };
            var buildCacheJson = QuarkUtility.ToJson(buildCache);
            var buildCacheWritePath = Path.Combine(buildParams.BuildPath, buildParams.BuildVersion, buildParams.BuildTarget.ToString(), QuarkEditorConstant.BUILD_CACHE_NAME);
            QuarkUtility.OverwriteTextFile(buildCacheWritePath, buildCacheJson);
        }
        public static void OverwriteBuildCache(QuarkBuildCache buildCache, QuarkBuildParams buildParams)
        {
            var buildCacheJson = QuarkUtility.ToJson(buildCache);
            var buildCacheWritePath = Path.Combine(buildParams.BuildPath, buildParams.BuildVersion, buildParams.BuildTarget.ToString(), QuarkEditorConstant.BUILD_CACHE_NAME);
            QuarkUtility.OverwriteTextFile(buildCacheWritePath, buildCacheJson);
        }
        /// <summary>
        /// 比较缓存 
        /// </summary>
        public static void CompareBuildCache(QuarkBuildCache buildCache, QuarkDataset dataset, out List<AssetCache> changed)
        {
            changed = new List<AssetCache>();
            List<AssetBundleBuild> assetBundleBuildList = new List<AssetBundleBuild>();
            var bundleCacheList = buildCache.BundleCacheList;
            var cacheDict = bundleCacheList.ToDictionary((b) => b.BundlePath);
            BuildDataset(dataset);
            dataset.CacheAllBundleInfos();
            var bundleInfos = dataset.AllCachedBundleInfos;
            List<AssetCache> compareInfoList = new List<AssetCache>();
            foreach (var bundleInfo in bundleInfos)
            {
                //过滤空包。若文件夹被标记为bundle，且不包含内容，则unity会过滤。因此遵循unity的规范；
                if (bundleInfo.ObjectInfoList.Count <= 0)
                {
                    continue;
                }
                var bundlePath = bundleInfo.BundlePath;
                var bundleName = bundleInfo.BundleName;
                var path = Path.Combine(QuarkEditorUtility.ApplicationPath, bundlePath);
                var hash = QuarkEditorUtility.CreateDirectoryMd5(path);
                var assetNames = bundleInfo.ObjectInfoList.Select(o => o.ObjectPath).ToArray();
                var cmpInfo = new AssetCache()
                {
                    BundleHash = hash,
                    BundleName = bundleInfo.BundleName,
                    BundlePath = bundleInfo.BundlePath,
                    AssetNames = assetNames
                };
                compareInfoList.Add(cmpInfo);
            }
            var cmpDict = compareInfoList.ToDictionary(b => b.BundlePath);
            //路径是唯一的，可以作为key
            foreach (var cache in cacheDict.Values)
            {
                if (!cmpDict.TryGetValue(cache.BundlePath, out var cmp))
                {
                    //现有资源不存在之前的信息，则表示为过期，应移除
                    //changed.Add(cache);
                }
                else
                {
                    //现有资源在缓存中存在，则校验hash
                    if (cache.BundleHash != cmp.BundleHash)
                    {
                        //hash不一致
                        changed.Add(cmp);
                    }
                }
            }
            foreach (var cmp in cmpDict.Values)
            {
                if (!cacheDict.TryGetValue(cmp.BundlePath, out var cache))
                {
                    //缓存中不存在，则表示新增
                    changed.Add(cmp);
                }
            }
            QuarkUtility.LogInfo($"{changed.Count} bundles has changed !");
        }
        public static void GenerateDifferenceFile(QuarkManifest quarkManifest, List<AssetCache> changedAssets, QuarkBuildParams buildParams)
        {
            var quarkDiffManifest = new QuarkDiffManifest();
            var srcPathDict = quarkManifest.BundleInfoDict.Values.ToDictionary(b => b.QuarkAssetBundle.BundlePath);
            var cmpPathDict = changedAssets.ToDictionary(b => b.BundlePath);
            var diffList = new List<QuarkBundleAsset>();
            foreach (var srcPath in srcPathDict)
            {
                var srcBundlePath = srcPath.Value.QuarkAssetBundle.BundlePath;
                if (cmpPathDict.ContainsKey(srcBundlePath))
                {
                    diffList.Add(srcPath.Value);
                }
            }
            quarkDiffManifest.BundleInfoDict = diffList.ToDictionary(b => b.QuarkAssetBundle.BundleKey);
            quarkDiffManifest.BuildVersion = buildParams.BuildVersion;
            quarkDiffManifest.InternalBuildVersion = buildParams.InternalBuildVersion;
            quarkDiffManifest.BuildTime = System.DateTime.Now.ToString();

            var diffManifestJson = QuarkUtility.ToJson(quarkDiffManifest);
            var diffManifestContext = diffManifestJson;
            var diffManifestWritePath = Path.Combine(buildParams.AssetBundleOutputPath, QuarkConstant.DIFF_MANIFEST_NAME);
            if (buildParams.UseAesEncryptionForManifest)
            {
                var key = QuarkUtility.GenerateBytesAESKey(buildParams.AesEncryptionKeyForManifest);
                diffManifestContext = QuarkUtility.AESEncryptStringToString(diffManifestJson, key);
            }
            QuarkUtility.WriteTextFile(diffManifestWritePath, diffManifestContext);

            var manifestWritePath = Path.Combine(buildParams.AssetBundleOutputPath, QuarkConstant.MANIFEST_NAME);
            QuarkUtility.DeleteFile(manifestWritePath);

        }
        public static void CompareAndUpdateBuildCache(QuarkBuildCache buildCache, QuarkDataset dataset, out List<AssetCache> newBundleCacheList, out List<AssetCache> changed)
        {
            changed = new List<AssetCache>();
            newBundleCacheList = new List<AssetCache>();
            List<AssetBundleBuild> assetBundleBuildList = new List<AssetBundleBuild>();
            var bundleCacheList = buildCache.BundleCacheList;
            var cacheDict = bundleCacheList.ToDictionary((b) => b.BundlePath);
            BuildDataset(dataset);
            dataset.CacheAllBundleInfos();
            var bundleInfos = dataset.AllCachedBundleInfos;
            List<AssetCache> cmpList = new List<AssetCache>();
            foreach (var bundleInfo in bundleInfos)
            {
                //过滤空包。若文件夹被标记为bundle，且不包含内容，则unity会过滤。因此遵循unity的规范；
                if (bundleInfo.ObjectInfoList.Count <= 0)
                {
                    continue;
                }
                var bundlePath = bundleInfo.BundlePath;
                var bundleName = bundleInfo.BundleName;
                var path = Path.Combine(QuarkEditorUtility.ApplicationPath, bundlePath);
                var hash = QuarkEditorUtility.CreateDirectoryMd5(path);
                var assetNames = bundleInfo.ObjectInfoList.Select(o => o.ObjectPath).ToArray();
                var cmpInfo = new AssetCache()
                {
                    BundleHash = hash,
                    BundleName = bundleInfo.BundleName,
                    BundlePath = bundleInfo.BundlePath,
                    AssetNames = assetNames,
                };
                cmpList.Add(cmpInfo);
            }
            var cmpDict = cmpList.ToDictionary(b => b.BundlePath);
            //路径是唯一的，可以作为key
            foreach (var cache in cacheDict.Values)
            {
                if (!cmpDict.TryGetValue(cache.BundlePath, out var cmp))
                {
                    //现有资源不存在之前的信息，则表示为过期，应移除
                }
                else
                {
                    //现有资源在缓存中存在，则校验hash
                    if (cache.BundleHash != cmp.BundleHash)
                    {
                        //hash不一致
                        changed.Add(cmp);
                    }
                    newBundleCacheList.Add(cmp);
                }
            }
            foreach (var cmp in cmpDict.Values)
            {
                if (!cacheDict.TryGetValue(cmp.BundlePath, out var cache))
                {
                    //缓存中不存在，则表示新增
                    changed.Add(cmp);
                    newBundleCacheList.Add(cmp);
                }
                else
                {
                    //cmp.BundleSize = cache.BundleSize;
                }
            }
        }
        public static BuildAssetBundleOptions GetBuildAssetBundleOptions(AssetBundleCompressType compressType, bool disableWriteTypeTree, bool deterministicAssetBundle, bool forceRebuildAssetBundle, bool ignoreTypeTreeChanges)
        {
            BuildAssetBundleOptions options = BuildAssetBundleOptions.None;
            switch (compressType)
            {
                case AssetBundleCompressType.Uncompressed:
                    options |= BuildAssetBundleOptions.UncompressedAssetBundle;
                    break;
                case AssetBundleCompressType.StandardCompression_LZMA:
                    //None=StandardCompression_LZMA
                    break;
                case AssetBundleCompressType.ChunkBasedCompression_LZ4:
                    options |= BuildAssetBundleOptions.ChunkBasedCompression;
                    break;
            }
            if (disableWriteTypeTree)
                options |= BuildAssetBundleOptions.DisableWriteTypeTree;
            if (deterministicAssetBundle)
                options |= BuildAssetBundleOptions.DeterministicAssetBundle;
            if (forceRebuildAssetBundle)
                options |= BuildAssetBundleOptions.ForceRebuildAssetBundle;
            if (ignoreTypeTreeChanges)
                options |= BuildAssetBundleOptions.IgnoreTypeTreeChanges;
            return options;
        }

    }
}
