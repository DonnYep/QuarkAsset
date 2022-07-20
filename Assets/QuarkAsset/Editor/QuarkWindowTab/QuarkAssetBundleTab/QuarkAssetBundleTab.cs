using System.Collections;
using UnityEngine;
using UnityEditor;
using System.IO;
using Quark.Asset;
namespace Quark.Editor
{
    public class QuarkAssetBundleTab
    {
        QuarkAssetBundleTabData tabData;
        const string AssetBundleTabDataFileName = "QuarkAsset_AssetBundleTabData.json";
        QuarkAssetDataset quarkAssetDataset { get { return QuarkEditorDataProxy.QuarkAssetDataset; } }
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
        public void OnDatasetUnassign()
        {
        }
        public void OnGUI()
        {
            tabData.BuildTarget = (BuildTarget)EditorGUILayout.EnumPopup("BuildTarget", tabData.BuildTarget);
            tabData.OutputPath = EditorGUILayout.TextField("OutputPath", tabData.OutputPath.Trim());
            tabData.AssetBundleBuildPath = Path.Combine(tabData.OutputPath, tabData.BuildTarget.ToString()).Replace("\\", "/");
            EditorGUILayout.LabelField("AssetBundleBuildPath", tabData.AssetBundleBuildPath);

            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Browse", GUILayout.MaxWidth(128f)))
                {
                    BrowseFolder();
                }
                if (GUILayout.Button("Reset", GUILayout.MaxWidth(128f)))
                {
                    tabData.OutputPath = Path.Combine(Path.GetFullPath("."), "AssetBundles", "QuarkAsset").Replace("\\", "/");
                    tabData.BuildTarget = BuildTarget.StandaloneWindows;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(16);

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("OpenOutputPath"))
                {
                    var path = tabData.AssetBundleBuildPath;
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    EditorUtility.RevealInFinder(path);
                }
                if (GUILayout.Button("OpenPersistentPath"))
                {
                    EditorUtility.RevealInFinder(Application.persistentDataPath);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.Space(16);


            GUILayout.BeginVertical();
            {
                tabData.WithoutManifest = EditorGUILayout.ToggleLeft("WithoutManifest", tabData.WithoutManifest);

                tabData.ClearOutputFolders = EditorGUILayout.ToggleLeft("ClearOutputFolders", tabData.ClearOutputFolders);
                tabData.CopyToStreamingAssets = EditorGUILayout.ToggleLeft("CopyToStreamingAssets", tabData.CopyToStreamingAssets);
                if (tabData.CopyToStreamingAssets)
                {
                    GUILayout.Space(16);
                    GUILayout.Label("Assets/StreamingAssets/ 下的相对路径地址，可选填 ");
                    tabData.StreamingRelativePath = EditorGUILayout.TextField("StreamingRelativePath", tabData.StreamingRelativePath.Trim());
                }
            }
            GUILayout.EndVertical();


            GUILayout.Space(16);
            GUILayout.Label("CompressedFormat  建议使用默认模式，并且请勿与NameHashType的其他类型混用，会导致AB包名混乱！");
            tabData.BuildAssetBundleOptions = (BuildAssetBundleOptions)EditorGUILayout.EnumPopup("CompressedFormat:", tabData.BuildAssetBundleOptions);
            tabData.NameHashType = (AssetBundleHashType)EditorGUILayout.EnumPopup("NameHashType", tabData.NameHashType);

            GUILayout.Space(16);

            DrawAESEncryptionForBuildInfoLable();
            DrawOffsetEncryptionForAssetBundleLable();

            GUILayout.Space(16);

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Build"))
                {
                    if (quarkAssetDataset != null)
                    {
                        if (tabData.UseAesEncryptionForBuildInfo)
                        {
                            var aesKeyStr = tabData.AesEncryptionKeyForBuildInfo;
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
            tabData.UseAesEncryptionForBuildInfo = EditorGUILayout.ToggleLeft("Aes encryption for buildInfo and manifest", tabData.UseAesEncryptionForBuildInfo);
            if (tabData.UseAesEncryptionForBuildInfo)
            {
                EditorGUILayout.LabelField("BuildInfo AES encryption key, key should be 16,24 or 32 bytes long");
                tabData.AesEncryptionKeyForBuildInfo = EditorGUILayout.TextField("AESKey", tabData.AesEncryptionKeyForBuildInfo);

                var aesKeyStr = tabData.AesEncryptionKeyForBuildInfo;
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
            var newPath = EditorUtility.OpenFolderPanel("Bundle Folder", tabData.OutputPath, string.Empty);
            if (!string.IsNullOrEmpty(newPath))
            {
                tabData.OutputPath = newPath.Replace("\\", "/");
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
            var quarkManifest = new QuarkAssetManifest();
            yield return assetDatabaseTab.BuildDataset();
            yield return SetAssetBundleName(quarkManifest);
            var assetBundleManifest = BuildPipeline.BuildAssetBundles(tabData.AssetBundleBuildPath, tabData.BuildAssetBundleOptions, tabData.BuildTarget);
            yield return FinishBuild(assetBundleManifest, quarkManifest);
        }
        IEnumerator SetAssetBundleName(QuarkAssetManifest quarkManifest)
        {
            QuarkUtility.LogInfo("Start build asset bundle");
            var abBuildPath = tabData.AssetBundleBuildPath;
            var bundles = quarkAssetDataset.QuarkAssetBundleList;
            foreach (var bundle in bundles)
            {
                var bundlePath = bundle.AssetBundlePath;
                var importer = AssetImporter.GetAtPath(bundlePath);
                var nameType = tabData.NameHashType;
                var bundleName = bundle.AssetBundleName;
                var path = Path.Combine(QuarkEditorUtility.ApplicationPath, bundlePath);
                var hash = QuarkEditorUtility.CreateDirectoryMd5(path);
                switch (nameType)
                {
                    case AssetBundleHashType.DefaultName:
                        break;
                    case AssetBundleHashType.HashInstead:
                        {
                            bundleName = hash;
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
                yield return null;
            }
            for (int i = 0; i < bundles.Count; i++)
            {
                var bundle = bundles[i];
                bundle.DependList.Clear();
                var importer = AssetImporter.GetAtPath(bundle.AssetBundlePath);
                bundle.DependList.AddRange(AssetDatabase.GetAssetBundleDependencies(importer.assetBundleName, true));
            }
        }
        IEnumerator FinishBuild(AssetBundleManifest manifest, QuarkAssetManifest quarkManifest)
        {
            var assetBundleBuildPath = tabData.AssetBundleBuildPath;
            if (manifest == null)
                yield break;
            var bundleNames = manifest.GetAllAssetBundles();
            var bundleNameLength = bundleNames.Length;
            for (int i = 0; i < bundleNameLength; i++)
            {
                var bundlePath = Path.Combine(assetBundleBuildPath, bundleNames[i]);
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
                if (tabData.WithoutManifest)
                {
                    var bundleManifestPath = QuarkUtility.Append(bundlePath, ".manifest");
                    QuarkUtility.DeleteFile(bundleManifestPath);
                }
                if (quarkManifest.BundleInfoDict.TryGetValue(bundleNames[i], out var quarkBundleInfo))
                {
                    quarkBundleInfo.BundleSize = bundleSize;
                }
            }
            quarkManifest.BuildTime = System.DateTime.Now.ToString();
            var manifestJson = QuarkUtility.ToJson(quarkManifest);
            var manifestContext = manifestJson;
            var manifestWritePath = Path.Combine(tabData.AssetBundleBuildPath, QuarkConstant.ManifestName);
            if (tabData.UseAesEncryptionForBuildInfo)
            {
                var key = QuarkUtility.GenerateBytesAESKey(tabData.AesEncryptionKeyForBuildInfo);
                manifestContext = QuarkUtility.AESEncryptStringToString(manifestJson, key);
            }
            QuarkUtility.WriteTextFile(manifestWritePath, manifestContext);

            yield return null;
            if (tabData.WithoutManifest)
            {
                //删除生成文对应的主manifest文件
                var buildMainPath = Path.Combine(tabData.AssetBundleBuildPath, tabData.BuildTarget.ToString());
                var buildMainManifestPath = QuarkUtility.Append(buildMainPath, ".manifest");
                QuarkUtility.DeleteFile(buildMainPath);
                QuarkUtility.DeleteFile(buildMainManifestPath);
            }
            var bundles = quarkAssetDataset.QuarkAssetBundleList;
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
                    QuarkUtility.Copy(buildPath, streamingAssetPath);
                }
            }
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            AssetDatabase.RemoveUnusedAssetBundleNames();
            System.GC.Collect();
        }
    }
}
