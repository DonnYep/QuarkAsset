using Quark.Asset;
using Quark.Loader;
using Quark.Manifest;
using Quark.Networking;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;
namespace Quark
{
    public static partial class QuarkResources
    {
        static QuarkAssetDownloader quarkAssetDownloader;
        static QuarkManifestVerifier quarkManifestVerifier;
        static QuarkManifestRequester quarkManifestRequester;
        static QuarkLoadModeProvider quarkLoadModeProvider
            = new QuarkLoadModeProvider();
        public static QuarkLoadMode QuarkLoadMode
        {
            get { return QuarkDataProxy.QuarkAssetLoadMode; }
            set { QuarkDataProxy.QuarkAssetLoadMode = value; }
        }
        public static QuarkAssetDownloader QuarkAssetDownloader
        {
            get
            {
                if (quarkAssetDownloader == null)
                    quarkAssetDownloader = new QuarkAssetDownloader();
                return quarkAssetDownloader;
            }
        }
        /// <summary>
        /// 文件校验器；
        /// </summary>
        public static QuarkManifestVerifier QuarkManifestVerifier
        {
            get
            {
                if (quarkManifestVerifier == null)
                    quarkManifestVerifier = new QuarkManifestVerifier();
                return quarkManifestVerifier;
            }
        }
        /// <summary>
        /// 文件清单获取器
        /// </summary>
        public static QuarkManifestRequester QuarkManifestRequester
        {
            get
            {
                if (quarkManifestRequester == null)
                    quarkManifestRequester = new QuarkManifestRequester();
                return quarkManifestRequester;
            }
        }
        public static void LaunchAssetBundleMode(QuarkMergedManifest mergedManifest, string persistentPath, string diffPersistentPath, string manifestAesKey = "", ulong encryptionOffset = 0)
        {
            QuarkLoadMode = QuarkLoadMode.AssetBundle;
            QuarkDataProxy.QuarkEncryptionOffset = encryptionOffset;
            QuarkDataProxy.QuarkAesEncryptionKey = manifestAesKey;
            QuarkDataProxy.PersistentPath = persistentPath;
            QuarkDataProxy.DiffPersistentPath = diffPersistentPath;
            quarkLoadModeProvider.SetAssetBundleModeMergedManifest(mergedManifest);
        }
        public static void LaunchAssetBundleMode(QuarkManifest manifest, string persistentPath, QuarkManifest diffManifest, string diffPersistentPath, string manifestAesKey = "", ulong encryptionOffset = 0)
        {
            QuarkLoadMode = QuarkLoadMode.AssetBundle;
            QuarkDataProxy.QuarkEncryptionOffset = encryptionOffset;
            QuarkDataProxy.QuarkAesEncryptionKey = manifestAesKey;
            QuarkDataProxy.PersistentPath = persistentPath;
            QuarkDataProxy.DiffPersistentPath = diffPersistentPath;
            QuarkUtility.Manifest.MergeManifest(manifest, diffManifest, out var mergeManifest);
            quarkLoadModeProvider.SetAssetBundleModeMergedManifest(mergeManifest);
        }
        /// <summary>
        /// launch quark assetBundle mode, local file only!
        /// </summary>
        /// <param name="persistentPath">directory path</param>
        /// <param name="manifestAesKey">ase key for manifest file</param>
        /// <param name="encryptionOffset">bundle encryption offset</param>
        public static void LaunchAssetBundleMode(string persistentPath, Action onSuccess, Action<string> onFailure, string manifestAesKey = "", ulong encryptionOffset = 0)
        {
            QuarkLoadMode = QuarkLoadMode.AssetBundle;
            QuarkDataProxy.QuarkEncryptionOffset = encryptionOffset;
            QuarkDataProxy.QuarkAesEncryptionKey = manifestAesKey;
            var aesKeyBytes = QuarkUtility.GenerateBytesAESKey(manifestAesKey);
            string uri = string.Empty;
            uri = QuarkUtility.PlatformPerfix + Path.Combine(persistentPath, QuarkConstant.MANIFEST_NAME);
            QuarkDataProxy.PersistentPath = persistentPath;
            QuarkManifestRequester.AddTask(uri, aesKeyBytes, (manifest) =>
            {
                quarkLoadModeProvider.SetAssetBundleModeManifest(manifest);
                onSuccess?.Invoke();
            },
            (error) =>
            {
                onFailure?.Invoke(error);
            });
            QuarkManifestRequester.StartRequestManifest();
        }
        /// <summary>
        /// launch quark assetBundle mode, local file only!
        /// </summary>
        /// <param name="manifest">quark manifest</param>
        /// <param name="persistentPath">directory path</param>
        /// <param name="manifestAesKey">ase key for manifest file</param>
        /// <param name="encryptionOffset">bundle encryption offset</param>
        public static void LaunchAssetBundleMode(QuarkManifest manifest, string persistentPath, string manifestAesKey = "", ulong encryptionOffset = 0)
        {
            QuarkLoadMode = QuarkLoadMode.AssetBundle;
            QuarkDataProxy.QuarkEncryptionOffset = encryptionOffset;
            QuarkDataProxy.QuarkAesEncryptionKey = manifestAesKey;
            QuarkDataProxy.PersistentPath = persistentPath;
            quarkLoadModeProvider.SetAssetBundleModeManifest(manifest);
        }
        /// <summary>
        /// launch quark assetDatabase mode, editor only!
        /// </summary>
        /// <param name="quarkDataset">dataset</param>
        public static void LaunchAssetDatabaseMode(QuarkDataset quarkDataset, Action onSuccess, Action<string> onFailure)
        {
            QuarkLoadMode = QuarkLoadMode.AssetDatabase;
            if (quarkDataset == null)
            {
                onFailure?.Invoke("quarkDataset is null !");
                return;
            }
            SetAssetDatabaseModeDataset(quarkDataset);
            onSuccess?.Invoke();
        }
        /// <summary>
        /// 设置Manifest；
        /// 用于assetbundle模式；
        /// </summary>
        /// <param name="manifest">Manifest文件</param>
        /// <param name="persistentPath">local persistent path</param>
        public static void SetAssetBundleModeManifest(QuarkManifest manifest, string persistentPath)
        {
            QuarkDataProxy.PersistentPath = persistentPath;
            quarkLoadModeProvider.SetAssetBundleModeManifest(manifest);
        }
        /// <summary>
        /// 用于Editor开发模式；
        /// 对QuarkAssetDataset进行编码
        /// </summary>
        /// <param name="dataset">QuarkAssetDataset对象</param>
        public static void SetAssetDatabaseModeDataset(QuarkDataset dataset)
        {
            quarkLoadModeProvider.SetAssetDatabaseModeDataset(dataset);
        }
        public static string GetBuildVersion()
        {
            return quarkLoadModeProvider.GetBuildVersion();
        }
        public static T LoadAsset<T>(string assetName)
where T : Object
        {
            return quarkLoadModeProvider.LoadAsset<T>(assetName);
        }
        public static Object LoadAsset(string assetName, Type type)
        {
            return quarkLoadModeProvider.LoadAsset(assetName, type);
        }
        public static Coroutine LoadAssetAsync<T>(string assetName, Action<T> callback)
where T : Object
        {
            return quarkLoadModeProvider.LoadAssetAsync<T>(assetName, callback);
        }
        public static Coroutine LoadAssetAsync(string assetName, Type type, Action<Object> callback)
        {
            return quarkLoadModeProvider.LoadAssetAsync(assetName, type, callback);
        }
        public static GameObject LoadPrefab(string assetName, bool instantiate = false)
        {
            return quarkLoadModeProvider.LoadPrefab(assetName, instantiate);
        }
        public static T[] LoadMainAndSubAssets<T>(string assetName) where T : Object
        {
            return quarkLoadModeProvider.LoadMainAndSubAssets<T>(assetName);
        }
        public static Object[] LoadMainAndSubAssets(string assetName, Type type)
        {
            return quarkLoadModeProvider.LoadMainAndSubAssets(assetName, type);
        }
        public static Object[] LoadAllAssets(string assetBundleName)
        {
            return quarkLoadModeProvider.LoadAllAssets(assetBundleName);
        }
        public static Coroutine LoadPrefabAsync(string assetName, Action<GameObject> callback, bool instantiate = false)
        {
            return quarkLoadModeProvider.LoadPrefabAsync(assetName, callback, instantiate);
        }
        public static Coroutine LoadMainAndSubAssetsAsync<T>(string assetName, Action<T[]> callback) where T : UnityEngine.Object
        {
            return quarkLoadModeProvider.LoadMainAndSubAssetsAsync<T>(assetName, callback);
        }
        public static Coroutine LoadMainAndSubAssetsAsync(string assetName, Type type, Action<Object[]> callback)
        {
            return quarkLoadModeProvider.LoadMainAndSubAssetsAsync(assetName, type, callback);
        }
        public static Coroutine LoadAllAssetAsync(string assetBundleName, Action<Object[]> callback)
        {
            return quarkLoadModeProvider.LoadAllAssetAsync(assetBundleName, callback);
        }
        public static Coroutine LoadSceneAsync(string sceneName, Action<float> progress, Action callback, bool additive = false)
        {
            return quarkLoadModeProvider.LoadSceneAsync(sceneName, null, progress, null, callback, additive);
        }
        public static Coroutine LoadSceneAsync(string sceneName, Action<float> progress, Func<bool> condition, Action callback, bool additive = false)
        {
            return quarkLoadModeProvider.LoadSceneAsync(sceneName, null, progress, condition, callback, additive);
        }
        public static Coroutine LoadSceneAsync(string sceneName, Func<float> progressProvider, Action<float> progress, Func<bool> condition, Action callback, bool additive = false)
        {
            return quarkLoadModeProvider.LoadSceneAsync(sceneName, progressProvider, progress, condition, callback, additive);
        }
        public static void UnloadAsset(string assetName)
        {
            quarkLoadModeProvider.UnloadAsset(assetName);
        }
        public static void UnloadAllAssetBundle(bool unloadAllLoadedObjects = true)
        {
            quarkLoadModeProvider.UnloadAllAssetBundle(unloadAllLoadedObjects);
        }
        public static void UnloadAssetBundle(string assetBundleName, bool unloadAllLoadedObjects = true)
        {
            quarkLoadModeProvider.UnloadAssetBundle(assetBundleName, unloadAllLoadedObjects);
        }
        public static Coroutine UnloadSceneAsync(string sceneName, Action<float> progress, Action callback)
        {
            return quarkLoadModeProvider.UnloadSceneAsync(sceneName, progress, callback);
        }
        public static Coroutine UnloadAllSceneAsync(Action<float> progress, Action callback)
        {
            return quarkLoadModeProvider.UnloadAllSceneAsync(progress, callback);
        }
        /// <summary>
        /// 重置清空寻址信息。
        /// </summary>
        public static void ResetLoader(QuarkLoadMode loadMode)
        {
            quarkLoadModeProvider.ResetLoader(loadMode);
        }
        public static bool GetObjectInfo(string assetName, out QuarkObjectState info)
        {
            return quarkLoadModeProvider.GetObjectInfo(assetName, out info);
        }
        public static bool GetBundleInfo(string bundleName, out QuarkBundleState info)
        {
            return quarkLoadModeProvider.GetBundleInfo(bundleName, out info);
        }
        public static QuarkObjectState[] GetAllLoadedObjectInfo()
        {
            return quarkLoadModeProvider.GetAllLoadedObjectInfo();
        }
        public static QuarkBundleState[] GetAllBundleInfo()
        {
            return quarkLoadModeProvider.GetAllBundleInfo();
        }
        public static async Task<T> LoadAssetAsync<T>(string assetName) where T : Object
        {
            return await new QuarkLoadAwaiter<T>(assetName);
        }

    }
}
