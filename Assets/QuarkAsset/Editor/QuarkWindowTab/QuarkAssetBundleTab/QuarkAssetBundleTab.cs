using System.Collections;
using UnityEngine;
using UnityEditor;
using System.IO;
using Quark.Asset;
using System.Collections.Generic;
using System.Linq;

namespace Quark.Editor
{
    public class QuarkAssetBundleTab
    {
        QuarkAssetBundleTabData tabData;
        const string AssetBundleTabDataFileName = "QuarkAsset_AssetBundleTabData.json";
        QuarkDataset dataset { get { return QuarkEditorDataProxy.QuarkAssetDataset; } }
        QuarkAssetDatabaseTab assetDatabaseTab;
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
            tabData.BuildAssetBundleOptions = (BuildAssetBundleOptions)EditorGUILayout.EnumPopup("Compression", tabData.BuildAssetBundleOptions);

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

            tabData.AssetBundleBuildPath = Path.Combine(tabData.BuildPath, tabData.BuildTarget.ToString(), tabData.BuildVersion).Replace("\\", "/");
            EditorGUILayout.LabelField("Build full path", tabData.AssetBundleBuildPath);

            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Open build Path", GUILayout.MaxWidth(128f)))
                {
                    var path = tabData.AssetBundleBuildPath;
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
            var assetBundleBuildPath = tabData.AssetBundleBuildPath;
            if (tabData.ClearOutputFolders)
            {
                if (Directory.Exists(assetBundleBuildPath))
                {
                    QuarkUtility.DeleteFolder(assetBundleBuildPath);
                }
            }
            if (!Directory.Exists(assetBundleBuildPath))
            {
                Directory.CreateDirectory(assetBundleBuildPath);
            }
            var quarkManifest = new QuarkManifest();
            yield return assetDatabaseTab.BuildDataset();
            yield return SetAssetBundleName(quarkManifest);
            var assetBundleManifest = BuildPipeline.BuildAssetBundles(assetBundleBuildPath, tabData.BuildAssetBundleOptions, tabData.BuildTarget);
            yield return FinishBuild(assetBundleManifest, quarkManifest);
        }
        IEnumerator SetAssetBundleName(QuarkManifest quarkManifest)
        {
            QuarkUtility.LogInfo("Start build asset bundle");
            var bundleInfos = dataset.QuarkBundleInfoList;
            foreach (var bundleInfo in bundleInfos)
            {
                //过滤空包。若文件夹被标记为bundle，且不包含内容，则unity会过滤。因此遵循unity的规范；
                if (bundleInfo.ObjectInfoList.Count <= 0)
                {
                    continue;
                }
                var bundlePath = bundleInfo.BundlePath;
                var importer = AssetImporter.GetAtPath(bundlePath);
                var nameType = tabData.AssetBundleNameType;
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
                bundle.DependentBundleKeyList.AddRange(bundleInfo.DependentBundleKeyList);
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
                var quarkBundleInfo = new QuarkManifest.QuarkBundleInfo()
                {
                    Hash = hash,
                    QuarkAssetBundle = bundle,
                    BundleName = bundleInfo.BundleName
                };
                quarkManifest.BundleInfoDict.Add(bundleName, quarkBundleInfo);
                yield return null;
            }
            for (int i = 0; i < bundleInfos.Count; i++)
            {
                var bundleInfo = bundleInfos[i];
                bundleInfo.DependentBundleKeyList.Clear();
                var importer = AssetImporter.GetAtPath(bundleInfo.BundlePath);
                bundleInfo.DependentBundleKeyList.AddRange(AssetDatabase.GetAssetBundleDependencies(importer.assetBundleName, true));
                if( quarkManifest.BundleInfoDict.TryGetValue(bundleInfo.BundleKey, out var manifestBundleInfo))
                {
                    manifestBundleInfo.QuarkAssetBundle.DependentBundleKeyList.Clear();
                    manifestBundleInfo.QuarkAssetBundle.DependentBundleKeyList.AddRange(bundleInfo.DependentBundleKeyList);
                }
            }
        }
        IEnumerator FinishBuild(AssetBundleManifest manifest, QuarkManifest quarkManifest)
        {
            var assetBundleBuildPath = tabData.AssetBundleBuildPath;
            if (manifest == null)
                yield break;
            Dictionary<string, QuarkBundleInfo> bundleKeyDict = null;
            if (tabData.AssetBundleNameType == AssetBundleNameType.HashInstead)
                bundleKeyDict = dataset.QuarkBundleInfoList.ToDictionary(b => b.BundleKey);
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
            quarkManifest.BuildVersion = tabData.BuildVersion;
            var manifestJson = QuarkUtility.ToJson(quarkManifest);
            var manifestContext = manifestJson;
            var manifestWritePath = Path.Combine(tabData.AssetBundleBuildPath, QuarkConstant.ManifestName);
            if (tabData.UseAesEncryptionForManifest)
            {
                var key = QuarkUtility.GenerateBytesAESKey(tabData.AesEncryptionKeyForManifest);
                manifestContext = QuarkUtility.AESEncryptStringToString(manifestJson, key);
            }
            QuarkUtility.WriteTextFile(manifestWritePath, manifestContext);

            yield return null;
            //删除生成文对应的主manifest文件
            var buildMainPath = Path.Combine(tabData.AssetBundleBuildPath, tabData.BuildVersion);
            var buildMainManifestPath = QuarkUtility.Append(buildMainPath, ".manifest");
            QuarkUtility.DeleteFile(buildMainPath);
            QuarkUtility.DeleteFile(buildMainManifestPath);
            var bundleInfos = dataset.QuarkBundleInfoList;
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
