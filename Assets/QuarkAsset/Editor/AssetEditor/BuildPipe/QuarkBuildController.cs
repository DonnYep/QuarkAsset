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
                var bundleKey = bundleInfo.BundleName;
                var path = Path.Combine(QuarkEditorUtility.ApplicationPath, bundlePath);
                var hash = QuarkEditorUtility.CreateDirectoryMd5(path);
                switch (nameType)
                {
                    case AssetBundleNameType.DefaultName:
                        bundleInfo.BundleKey = bundleInfo.BundleName;
                        break;
                    case AssetBundleNameType.HashInstead:
                        {
                            bundleKey = hash;
                            bundleInfo.BundleKey = hash;
                        }
                        break;
                }
                importer.assetBundleName = bundleKey;
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
                quarkManifest.BundleInfoDict.Add(bundleKey, quarkBundleInfo);
            }
        }
        public static void SetBundleDependent(QuarkDataset dataset, QuarkManifest quarkManifest)
        {
            var bundleInfos = dataset.AllCachedBundleInfos;
            var bundleDict = bundleInfos.ToDictionary(d => d.BundleKey);
            AssetDatabase.Refresh();
            for (int i = 0; i < bundleInfos.Count; i++)
            {
                var bundleInfo = bundleInfos[i];
                bundleInfo.DependentBundleKeyList.Clear();
                var importer = AssetImporter.GetAtPath(bundleInfo.BundlePath);
                var dependencies = AssetDatabase.GetAssetBundleDependencies(importer.assetBundleName, true);
                for (int j = 0; j < dependencies.Length; j++)
                {
                    var dep = dependencies[j];
                    if (bundleDict.TryGetValue(dep, out var bInfo))
                    {
                        var depInfo = new QuarkBundleDependentInfo()
                        {
                            BundleKey = dep,
                            BundleName = bInfo.BundleName
                        };
                        bundleInfo.DependentBundleKeyList.Add(depInfo);
                    }
                }
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
            //OverwriteManifest(quarkManifest, buildParams);

            //删除生成文对应的主manifest文件
            string buildMainPath = string.Empty;
            switch (buildParams.BuildType)
            {
                case BuildType.Full:
                    buildMainPath = Path.Combine(buildParams.AssetBundleOutputPath, $"{buildParams.BuildVersion}_{buildParams.InternalBuildVersion}");
                    break;
                case BuildType.Incremental:
                    buildMainPath = Path.Combine(buildParams.AssetBundleOutputPath, buildParams.BuildVersion);
                    break;
            }
            var buildMainManifestPath = QuarkUtility.Append(buildMainPath, ".manifest");
            QuarkUtility.DeleteFile(buildMainPath);
            QuarkUtility.DeleteFile(buildMainManifestPath);
            var bundleInfos = srcBundleInfos;
            var bundleInfoLength = bundleInfos.Count;

            RevertBundleDependencies(dataset);
        }
        public static void RevertBundleDependencies(QuarkDataset dataset)
        {
            var srcBundleInfos = dataset.AllCachedBundleInfos;
            var bundleDict = srcBundleInfos.ToDictionary(d => d.BundleKey);

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
                var dependencies = AssetDatabase.GetAssetBundleDependencies(importer.assetBundleName, true);
                for (int j = 0; j < dependencies.Length; j++)
                {
                    var dep = dependencies[j];
                    if (bundleDict.TryGetValue(dep, out var bInfo))
                    {
                        var depInfo = new QuarkBundleDependentInfo()
                        {
                            BundleKey = dep,
                            BundleName = bInfo.BundleName
                        };
                        bundleInfo.DependentBundleKeyList.Add(depInfo);
                    }
                }
            }

            for (int i = 0; i < bundleInfoLength; i++)
            {
                var bundle = bundleInfos[i];
                var importer = AssetImporter.GetAtPath(bundle.BundlePath);
                importer.assetBundleName = string.Empty;
            }
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            AssetDatabase.RemoveUnusedAssetBundleNames();
            System.GC.Collect();
        }
        public static void CopyToStreamingAssets(QuarkBuildParams buildParams)
        {
            if (buildParams.CopyToStreamingAssets)
            {
                var buildPath = buildParams.AssetBundleOutputPath;
                var streamingAssetPath = Path.Combine(Application.streamingAssetsPath, buildParams.StreamingRelativePath);
                if (buildParams.ClearStreamingAssetsDestinationPath)
                {
                    QuarkUtility.EmptyFolder(streamingAssetPath);
                }
                if (Directory.Exists(buildPath))
                {
                    QuarkUtility.CopyDirectory(buildPath, streamingAssetPath);
                }
            }
            AssetDatabase.Refresh();
        }
        public static void OverwriteManifest(QuarkManifest quarkManifest, QuarkBuildParams buildParams)
        {
            quarkManifest.BuildTime = System.DateTime.Now.ToString();
            quarkManifest.BuildVersion = buildParams.BuildVersion;
            quarkManifest.InternalBuildVersion = buildParams.InternalBuildVersion;
            quarkManifest.BuildHash = GUID.Generate().ToString();
            var manifestJson = QuarkUtility.ToJson(quarkManifest);
            var manifestContext = manifestJson;
            var manifestWritePath = Path.Combine(buildParams.AssetBundleOutputPath, QuarkConstant.MANIFEST_NAME);
            if (buildParams.UseAesEncryptionForManifest)
            {
                var key = QuarkUtility.GenerateBytesAESKey(buildParams.AesEncryptionKeyForManifest);
                manifestContext = QuarkUtility.AESEncryptStringToString(manifestJson, key);
            }
            QuarkUtility.OverwriteTextFile(manifestWritePath, manifestContext);
        }
        public static void OverwriteBuildCache(QuarkBuildCache buildCache, QuarkBuildParams buildParams)
        {
            var buildCacheJson = QuarkUtility.ToJson(buildCache);
            var buildCacheWritePath = Path.Combine(buildParams.BuildPath, buildParams.BuildVersion, buildParams.BuildTarget.ToString(), QuarkEditorConstant.BUILD_CACHE_NAME);
            QuarkUtility.OverwriteTextFile(buildCacheWritePath, buildCacheJson);
        }
        public static void CompareAndProcessBuildCacheFile(QuarkBuildCache buildCache, QuarkDataset dataset, QuarkBuildParams buildParams, out QuarkIncrementalBuildInfo info)
        {
            info = new QuarkIncrementalBuildInfo();

            var newlyAdded = new List<AssetCache>();
            var changed = new List<AssetCache>();
            var expired = new List<AssetCache>();
            var unchanged = new List<AssetCache>();

            var bundleCaches = new List<AssetCache>();

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
            var nameType = buildParams.AssetBundleNameType;
            var abOutputPath = buildParams.AssetBundleOutputPath;
            //路径是唯一的，可以作为key
            foreach (var cache in cacheDict.Values)
            {
                var filePath = string.Empty;
                switch (nameType)
                {
                    case AssetBundleNameType.DefaultName:
                        filePath = Path.Combine(abOutputPath, cache.BundleName);
                        break;
                    case AssetBundleNameType.HashInstead:
                        filePath = Path.Combine(abOutputPath, cache.BundleHash);
                        break;
                }
                if (!cmpDict.TryGetValue(cache.BundlePath, out var cmp))
                {
                    //现有资源不存在之前的信息，则表示为过期，应移除
                    expired.Add(cache);

                    QuarkUtility.DeleteFile(filePath);
                }
                else
                {
                    //现有资源在缓存中存在，则校验hash
                    if (cache.BundleHash != cmp.BundleHash)
                    {
                        //hash不一致
                        changed.Add(cmp);

                        QuarkUtility.DeleteFile(filePath);
                    }
                    else
                    {
                        //hash一致
                        if (File.Exists(filePath))
                        {
                            //文件存在，则表示不变
                            unchanged.Add(cmp);
                        }
                        else
                        {
                            //文件不存在，则表示变更，加入构建列表
                            changed.Add(cmp);
                        }
                    }
                    bundleCaches.Add(cmp);
                }
            }
            foreach (var cmp in cmpDict.Values)
            {
                if (!cacheDict.TryGetValue(cmp.BundlePath, out var cache))
                {
                    //缓存中不存在，则表示新增
                    newlyAdded.Add(cmp);

                    bundleCaches.Add(cmp);
                }
            }
            info.NewlyAdded = newlyAdded.ToArray();
            info.Changed = changed.ToArray();
            info.Expired = expired.ToArray();
            info.Unchanged = unchanged.ToArray();
            info.BundleCaches = bundleCaches;
        }
        public static void GenerateIncrementalBuildLog(QuarkIncrementalBuildInfo buildInfo, QuarkBuildParams buildParams)
        {
            //生成差量构建日志
            var json = QuarkUtility.ToJson(buildInfo);
            var logPath = Path.Combine(buildParams.BuildPath, buildParams.BuildVersion, buildParams.BuildTarget.ToString(), QuarkEditorConstant.BUILD_LOG_NAME);
            QuarkUtility.OverwriteTextFile(logPath, json);
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
