using Quark.Asset;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace Quark.Editor
{
    public class QuarkBuildPipeline
    {
        static string QuarkDatasetPath = "Assets/QuarkAssetDataset.asset";
        static QuarkAssetBundleTabData tabData;
        static QuarkAssetDataset dataset;
        /// <summary>
        /// 默认不拷贝到StreamingAssets文件夹；
        /// </summary>
        [MenuItem("Window/QuarkAsset/Build/ActivePlatform")]
        public static void BuildActivePlatformAssetBundle()
        {
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            BuildAssetBundle(buildTarget);
        }
        /// <summary>
        /// 默认不拷贝到StreamingAssets文件夹；
        /// </summary>
        [MenuItem("Window/QuarkAsset/Build/ActivePlatformNameByHash")]
        public static void BuildActivePlatformAssetBundleNameByHash()
        {
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            BuildAssetBundle(buildTarget, true);
        }
        /// <summary>
        /// 默认不拷贝到StreamingAssets文件夹；
        /// </summary>
        [MenuItem("Window/QuarkAsset/Build/Android")]
        public static void BuildAndroidAssetBundle()
        {
            BuildAssetBundle(BuildTarget.Android);
        }
        /// <summary>
        /// 默认不拷贝到StreamingAssets文件夹；
        /// </summary>
        [MenuItem("Window/QuarkAsset/Build/AndroidNameByHash")]
        public static void BuildAndroidAssetBundleNameByHash()
        {
            BuildAssetBundle(BuildTarget.Android, true);
        }
        /// <summary>
        /// 默认不拷贝到StreamingAssets文件夹；
        /// </summary>
        [MenuItem("Window/QuarkAsset/Build/iOS")]
        public static void BuildiOSAssetBundle()
        {
            BuildAssetBundle(BuildTarget.iOS);
        }
        /// <summary>
        /// 默认不拷贝到StreamingAssets文件夹；
        /// </summary>
        [MenuItem("Window/QuarkAsset/Build/iOSNameByHash")]
        public static void BuildiOSAssetBundleNameByHash()
        {
            BuildAssetBundle(BuildTarget.iOS, true);
        }
        /// <summary>
        /// 默认不拷贝到StreamingAssets文件夹；
        /// </summary>
        [MenuItem("Window/QuarkAsset/Build/StandaloneWindows")]
        public static void BuildStandaloneWindowsAssetBundle()
        {
            BuildAssetBundle(BuildTarget.StandaloneWindows);
        }
        /// <summary>
        /// 默认不拷贝到StreamingAssets文件夹；
        /// </summary>
        [MenuItem("Window/QuarkAsset/Build/StandaloneWindowsNameByHash")]
        public static void BuildStandaloneWindowsAssetBundleNameByHash()
        {
            BuildAssetBundle(BuildTarget.StandaloneWindows, true);
        }
        /// <summary>
        /// 构建assetbundle
        /// </summary>
        /// <param name="buildTarget">目标平台</param>
        /// <param name="nameByHash">是否以hash命名bundle</param>
        /// <param name="copyToStreamingAssets">拷贝到StreamingAssets目录</param>
        /// <returns>生成后的地址</returns>
        public static string BuildAssetBundle(BuildTarget buildTarget, bool nameByHash = false, bool copyToStreamingAssets = false)
        {
            dataset = AssetDatabase.LoadAssetAtPath<QuarkAssetDataset>(QuarkDatasetPath);
            if (dataset == null)
            {
                QuarkUtility.LogError($"Path: {QuarkDatasetPath} invalid !");
                return string.Empty;
            }
            tabData = new QuarkAssetBundleTabData();
            tabData.BuildTarget = buildTarget;
            if (nameByHash)
                tabData.AssetBundleNameType = AssetBundleNameType.HashInstead;
            else
                tabData.AssetBundleNameType = AssetBundleNameType.DefaultName;
            tabData.CopyToStreamingAssets = copyToStreamingAssets;
            tabData.AssetBundleBuildPath = Path.Combine(tabData.BuildPath, tabData.BuildTarget.ToString(), tabData.BuildVersion).Replace("\\", "/");
            tabData.StreamingRelativePath = buildTarget.ToString().ToLower();
            OnBuildAssetBundle(dataset, tabData);
            return tabData.AssetBundleBuildPath;
        }
        /// <summary>
        /// 加密构建assetBundle；
        /// </summary>
        /// <param name="buildTarget">目标平台</param>
        /// <param name="aseKey">manifest加密的aes密钥</param>
        /// <param name="offset">bundle byte向右偏移量</param>
        /// <param name="nameByHash">是否以hash命名bundle</param>
        /// <param name="copyToStreamingAssets">拷贝到StreamingAssets目录</param>
        /// <returns>生成后的地址</returns>
        public static string EncryptBuildAssetBundle(BuildTarget buildTarget, string aseKey, int offset, bool nameByHash = false, bool copyToStreamingAssets = false)
        {
            dataset = AssetDatabase.LoadAssetAtPath<QuarkAssetDataset>(QuarkDatasetPath);
            if (dataset == null)
            {
                QuarkUtility.LogError($"Path: {QuarkDatasetPath} invalid !");
                return string.Empty;
            }
            tabData = new QuarkAssetBundleTabData();
            tabData.BuildTarget = buildTarget;
            if (!string.IsNullOrEmpty(aseKey))
            {
                var aesKeyLength = System.Text.Encoding.UTF8.GetBytes(aseKey).Length;
                if (aesKeyLength != 16 && aesKeyLength != 24 && aesKeyLength != 32)
                {
                    QuarkUtility.LogError("QuarkAsset build aes key is invalid , key should be 16,24 or 32 bytes long !");
                    return string.Empty;
                }
                tabData.UseAesEncryptionForManifest = true;
                tabData.AesEncryptionKeyForManifest = aseKey;
            }
            else
            {
                tabData.UseAesEncryptionForManifest = false;
            }
            tabData.UseOffsetEncryptionForAssetBundle = true;
            tabData.EncryptionOffsetForAssetBundle = offset;
            if (tabData.EncryptionOffsetForAssetBundle < 0)
                tabData.EncryptionOffsetForAssetBundle = 0;
            if (nameByHash)
                tabData.AssetBundleNameType = AssetBundleNameType.HashInstead;
            else
                tabData.AssetBundleNameType = AssetBundleNameType.DefaultName;
            tabData.CopyToStreamingAssets = copyToStreamingAssets;
            tabData.AssetBundleBuildPath = Path.Combine(tabData.BuildPath, tabData.BuildTarget.ToString(), tabData.BuildVersion).Replace("\\", "/");
            tabData.StreamingRelativePath = buildTarget.ToString().ToLower();
            OnBuildAssetBundle(dataset, tabData);
            return tabData.AssetBundleBuildPath;
        }
        public static string[] GetBuildScenePath()
        {
            dataset = AssetDatabase.LoadAssetAtPath<QuarkAssetDataset>(QuarkDatasetPath);
            if (dataset == null)
            {
                QuarkUtility.LogError($"Path: {QuarkDatasetPath} invalid !");
                return new string[0];
            }
            return dataset.QuarkSceneList.Select(s => s.AssetPath).ToArray();
        }
        static void OnBuildAssetBundle(QuarkAssetDataset dataset, QuarkAssetBundleTabData tabData)
        {
            QuarkUtility.LogInfo("Quark build pipeline start");
            var assetBundleBuildPath = tabData.AssetBundleBuildPath;
            if (Directory.Exists(assetBundleBuildPath))
            {
                QuarkUtility.DeleteFolder(assetBundleBuildPath);
            }
            if (!Directory.Exists(assetBundleBuildPath))
            {
                Directory.CreateDirectory(assetBundleBuildPath);
            }
            var quarkManifest = new QuarkAssetManifest();
            OnBuildDataset(dataset);
            OnSetAssetBundleName(quarkManifest, dataset);
            var assetBundleManifest = BuildPipeline.BuildAssetBundles(assetBundleBuildPath, tabData.BuildAssetBundleOptions, tabData.BuildTarget);
            OnFinishBuild(assetBundleManifest, quarkManifest, dataset);
            QuarkUtility.LogInfo("Quark build pipeline done");
        }
        static void OnBuildDataset(QuarkAssetDataset dataset)
        {
            if (dataset == null)
            {
                QuarkUtility.LogError("QuarkAssetDataset is invalid !");
                return;
            }
            EditorUtility.ClearProgressBar();
            var bundles = dataset.QuarkAssetBundleList;
            var extensions = dataset.QuarkAssetExts;
            List<QuarkObject> quarkAssetList = new List<QuarkObject>();
            List<QuarkObject> quarkSceneList = new List<QuarkObject>();
            List<QuarkAssetBundle> validBundleList = new List<QuarkAssetBundle>();
            var sceneAssetFullName = typeof(SceneAsset).FullName;
            int currentBundleIndex = 0;
            int bundleCount = bundles.Count;
            foreach (var bundle in bundles)
            {
                currentBundleIndex++;
                var bundlePath = bundle.AssetBundlePath;
                if (!AssetDatabase.IsValidFolder(bundlePath))
                    continue;
                validBundleList.Add(bundle);
                bundle.QuarkObjects.Clear();
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
                        var assetObject = new QuarkObject()
                        {
                            AssetName = Path.GetFileNameWithoutExtension(filePath),
                            AssetExtension = lowerExt,
                            AssetBundleName = bundle.AssetBundleName,
                            AssetPath = lowerExtFilePath,
                            AssetType = AssetDatabase.LoadAssetAtPath(filePath, typeof(UnityEngine.Object)).GetType().FullName
                        };
                        quarkAssetList.Add(assetObject);
                        if (assetObject.AssetType == sceneAssetFullName)
                        {
                            quarkSceneList.Add(assetObject);
                        }
                        bundle.QuarkObjects.Add(assetObject);
                    }
                }
                EditorUtility.DisplayCancelableProgressBar("QuarkAsset", "QuarkDataset Building", currentBundleIndex / (float)bundleCount);
            }
            EditorUtility.ClearProgressBar();
            dataset.QuarkObjectList.Clear();
            dataset.QuarkObjectList.AddRange(quarkAssetList);
            dataset.QuarkAssetBundleList.Clear();
            dataset.QuarkAssetBundleList.AddRange(validBundleList);
            dataset.QuarkSceneList.Clear();
            dataset.QuarkSceneList.AddRange(quarkSceneList);

            EditorUtility.SetDirty(dataset);
            AssetDatabase.SaveAssets();
        }
        static void OnSetAssetBundleName(QuarkAssetManifest quarkManifest, QuarkAssetDataset dataset)
        {
            var bundles = dataset.QuarkAssetBundleList;
            foreach (var bundle in bundles)
            {
                var bundlePath = bundle.AssetBundlePath;
                var importer = AssetImporter.GetAtPath(bundlePath);
                var nameType = tabData.AssetBundleNameType;
                var bundleName = bundle.AssetBundleName;
                var path = Path.Combine(QuarkEditorUtility.ApplicationPath, bundlePath);
                var hash = QuarkEditorUtility.CreateDirectoryMd5(path);
                switch (nameType)
                {
                    case AssetBundleNameType.DefaultName:
                        bundle.AssetBundleKey = bundle.AssetBundleName;
                        break;
                    case AssetBundleNameType.HashInstead:
                        {
                            bundleName = hash;
                            bundle.AssetBundleKey = hash;
                        }
                        break;
                }
                importer.assetBundleName = bundleName;
                var bundleInfo = new QuarkAssetManifest.QuarkBundleInfo()
                {
                    Hash = hash,
                    QuarkAssetBundle = bundle,
                    BundleName = bundle.AssetBundleName
                };
                quarkManifest.BundleInfoDict.Add(bundleName, bundleInfo);
            }
            for (int i = 0; i < bundles.Count; i++)
            {
                var bundle = bundles[i];
                bundle.DependentBundleKeyList.Clear();
                var importer = AssetImporter.GetAtPath(bundle.AssetBundlePath);
                bundle.DependentBundleKeyList.AddRange(AssetDatabase.GetAssetBundleDependencies(importer.assetBundleName, true));
            }
        }
        static void OnFinishBuild(AssetBundleManifest manifest, QuarkAssetManifest quarkManifest, QuarkAssetDataset dataset)
        {
            var assetBundleBuildPath = tabData.AssetBundleBuildPath;
            if (manifest == null)
                return;
            Dictionary<string, QuarkAssetBundle> bundleKeyDict = null;
            if (tabData.AssetBundleNameType == AssetBundleNameType.HashInstead)
                bundleKeyDict = dataset.QuarkAssetBundleList.ToDictionary(b => b.AssetBundleKey);
            var bundleKeys = manifest.GetAllAssetBundles();
            var bundleKeyLength = bundleKeys.Length;
            for (int i = 0; i < bundleKeyLength; i++)
            {
                var bundleKey = bundleKeys[i];

                var bundlePath = Path.Combine(assetBundleBuildPath, bundleKey);
                long bundleSize = 0;
                if (tabData.UseOffsetEncryptionForAssetBundle)
                {
                    var bundleBytes = File.ReadAllBytes(bundlePath);
                    var offset = tabData.EncryptionOffsetForAssetBundle;
                    QuarkUtility.AppendAndWriteAllBytes(bundlePath, new byte[offset], bundleBytes);
                    bundleSize = offset + bundleBytes.Length;
                }
                else
                {
                    var bundleBytes = File.ReadAllBytes(bundlePath);
                    bundleSize = bundleBytes.LongLength;
                }

                var bundleName = string.Empty;
                switch (tabData.AssetBundleNameType)
                {
                    case AssetBundleNameType.DefaultName:
                        {
                            bundleName = bundleKey;
                        }
                        break;
                    case AssetBundleNameType.HashInstead:
                        {
                            if (bundleKeyDict.TryGetValue(bundleKey, out var bundle))
                                bundleName = bundle.AssetBundleKey;
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
            var manifestJson = QuarkUtility.ToJson(quarkManifest);
            var manifestContext = manifestJson;
            var manifestWritePath = Path.Combine(tabData.AssetBundleBuildPath, QuarkConstant.ManifestName);
            if (tabData.UseAesEncryptionForManifest)
            {
                var key = QuarkUtility.GenerateBytesAESKey(tabData.AesEncryptionKeyForManifest);
                manifestContext = QuarkUtility.AESEncryptStringToString(manifestJson, key);
            }
            QuarkUtility.WriteTextFile(manifestWritePath, manifestContext);

            //删除生成文对应的主manifest文件
            var buildMainPath = Path.Combine(tabData.AssetBundleBuildPath, tabData.BuildVersion);
            var buildMainManifestPath = QuarkUtility.Append(buildMainPath, ".manifest");
            QuarkUtility.DeleteFile(buildMainPath);
            QuarkUtility.DeleteFile(buildMainManifestPath);
            var bundles = dataset.QuarkAssetBundleList;
            var bundleLength = bundles.Count;
            for (int i = 0; i < bundleLength; i++)
            {
                var bundle = bundles[i];
                var importer = AssetImporter.GetAtPath(bundle.AssetBundlePath);
                importer.assetBundleName = string.Empty;
            }
            if (tabData.CopyToStreamingAssets)
            {
                var buildPath = tabData.AssetBundleBuildPath;
                if (Directory.Exists(buildPath))
                {
                    var streamingAssetPath = Path.Combine(Application.streamingAssetsPath, tabData.StreamingRelativePath);
                    QuarkUtility.CopyDirectory(buildPath, streamingAssetPath);
                }
            }
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            AssetDatabase.RemoveUnusedAssetBundleNames();
            System.GC.Collect();
        }
    }
}
