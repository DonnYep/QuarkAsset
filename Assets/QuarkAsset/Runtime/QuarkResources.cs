using Quark.Asset;
using Quark.Compare;
using Quark.Loader;
using Quark.Networking;
using Quark.Verify;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;
namespace Quark
{
    public static class QuarkResources
    {
        static QuarkDownloader quarkDownloader;
        static QuarkManifestVerifier quarkManifestVerifier;
        static QuarkManifestRequester quarkManifestRequester;
        static QuarlManifestComparer quarlManifestComparer;
        static QuarkLoadModeProvider quarkLoadModeProvider
            = new QuarkLoadModeProvider();
        public static QuarkLoadMode QuarkLoadMode
        {
            get { return QuarkDataProxy.QuarkAssetLoadMode; }
            set { QuarkDataProxy.QuarkAssetLoadMode = value; }
        }
        /// <summary>
        /// 文件下载器；
        /// </summary>
        public static QuarkDownloader QuarkDownloader
        {
            get
            {
                if (quarkDownloader == null)
                    quarkDownloader = new QuarkDownloader();
                return quarkDownloader;
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
        /// <summary>
        /// 文件清单比较器
        /// </summary>
        public static QuarlManifestComparer QuarlManifestComparer
        {
            get
            {
                if (quarlManifestComparer == null)
                    quarlManifestComparer = new QuarlManifestComparer();
                return quarlManifestComparer;
            }
        }
        /// <summary>
        /// launch quark assetBundle mode, local file only!
        /// </summary>
        /// <param name="path">directory path</param>
        /// <param name="manifestAesKey">ase key for manifest file</param>
        /// <param name="encryptionOffset">bundle encryption offset</param>
        public static void LaunchAssetBundleMode(string path, Action onSuccess, Action<string> onFailure, string manifestAesKey = "", ulong encryptionOffset = 0)
        {
            QuarkLoadMode = QuarkLoadMode.AssetBundle;
            QuarkDataProxy.QuarkEncrytionData.QuarkEncryptionOffset = encryptionOffset;
            QuarkDataProxy.QuarkEncrytionData.QuarkAesEncryptionKey = manifestAesKey;
            var aesKeyBytes = QuarkUtility.GenerateBytesAESKey(manifestAesKey);
            string manifestPerfixPath = string.Empty;
            string persistentPath = string.Empty;
            string uri = string.Empty;
            if (!string.IsNullOrEmpty(QuarkUtility.PlatformPerfix))
            {
                //若平台宏字符串不为空，则判断是否以平台宏前缀开始
                if (!path.StartsWith(QuarkUtility.PlatformPerfix))
                {
                    manifestPerfixPath = QuarkUtility.PlatformPerfix + path;
                    persistentPath = path;
                }
                else
                {
                    //若dirPath包含了平台宏
                    manifestPerfixPath = path;
                    //持久化路径需要移除平台宏前缀
                    persistentPath = path.Remove(0, QuarkUtility.PlatformPerfix.Length);
                }
            }
            else
            {
                //若平台宏字符串为空，则直接使用地址
                manifestPerfixPath = path;
                persistentPath = path;
            }

            uri = Path.Combine(manifestPerfixPath, QuarkConstant.MANIFEST_NAME);
            QuarkDataProxy.PersistentPath = persistentPath;
            QuarkManifestRequester.onManifestAcquireSuccess = ((manifest) =>
              {
                  SetAssetBundleModeManifest(manifest);
                  onSuccess?.Invoke();
              });
            QuarkManifestRequester.onManifestAcquireFailure = onFailure;
            QuarkManifestRequester.RequestManifestAsync(uri, aesKeyBytes);
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
        public static void SetAssetBundleModeManifest(QuarkManifest manifest)
        {
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
            return QuarkDataProxy.QuarkManifest != null ? QuarkDataProxy.QuarkManifest.BuildVersion : "<NONE>";
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
        /// 重置清空寻址信息；
        /// </summary>
        public static void ResetLoader(QuarkLoadMode loadMode)
        {
            quarkLoadModeProvider.ResetLoader(loadMode);
        }
        public static bool GetInfo(string assetName, out QuarkObjectState info)
        {
            return quarkLoadModeProvider.GetInfo(assetName, out info);
        }
        public static QuarkObjectState[] GetAllLoadedInfos()
        {
            return quarkLoadModeProvider.GetAllLoadedInfos();
        }


        public static async Task<T> LoadAssetAsync<T>(string assetName) where T : Object
        {
            return await new QuarkLoadAwaiter<T>(assetName);
        }
    }
}
