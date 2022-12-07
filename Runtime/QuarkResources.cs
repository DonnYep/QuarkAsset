using Quark.Asset;
using Quark.Networking;
using Quark.Verifiy;
using System;
using UnityEngine;
using Object = UnityEngine.Object;
namespace Quark
{
    public static class QuarkResources
    {
        public static string ManifestBuildVersion
        {
            get
            {
                return QuarkDataProxy.QuarkManifest != null ? QuarkDataProxy.QuarkManifest.BuildVersion : "<NONE>";
            }
        }
        public static QuarkLoadMode QuarkAssetLoadMode
        {
            get { return QuarkEngine.Instance.QuarkAssetLoadMode; }
            set { QuarkEngine.Instance.QuarkAssetLoadMode = value; }
        }
        /// <summary>
        /// 当检测到最新的；
        /// LatestURLS---Size
        /// </summary>
        public static event Action<string[], long> OnCompareManifestSuccess
        {
            add { QuarkEngine.Instance.onCompareManifestSuccess += value; }
            remove { QuarkEngine.Instance.onCompareManifestSuccess -= value; }
        }
        /// <summary>
        /// 当检测失败；
        /// ErrorMessage
        /// </summary>
        public static event Action<string> OnCompareManifestFailure
        {
            add { QuarkEngine.Instance.onCompareManifestFailure += value; }
            remove { QuarkEngine.Instance.onCompareManifestFailure -= value; }
        }
        /// <summary>
        /// 文件下载器；
        /// </summary>
        public static QuarkDownloader QuarkDownloader
        {
            get { return QuarkEngine.Instance.quarkDownloader; }
        }
        /// <summary>
        /// 文件校验器；
        /// </summary>
        public static QuarkManifestVerifier QuarkManifestVerifier
        {
            get { return QuarkEngine.Instance.quarkManifestVerifier; }
        }
        /// <summary>
        /// 文件比较器；
        /// </summary>
        public static QuarkComparator QuarkComparator
        {
            get { return QuarkEngine.Instance.quarkComparator; }
        }
        /// <summary>
        /// 设置Manifest；
        /// 用于assetbundle模式；
        /// </summary>
        /// <param name="manifest">Manifest文件</param>
        public static void SetAssetBundleModeManifest(QuarkAssetManifest manifest)
        {
            QuarkEngine.Instance.SetAssetBundleModeManifest(manifest);
        }
        /// <summary>
        /// 用于Editor开发模式；
        /// 对QuarkAssetDataset进行编码
        /// </summary>
        /// <param name="dataset">QuarkAssetDataset对象</param>
        public static void SetAssetDatabaseModeDataset(QuarkAssetDataset dataset)
        {
            QuarkEngine.Instance.SetAssetDatabaseModeDataset(dataset);
        }
        public static T LoadAsset<T>(string assetName)
where T : Object
        {
            return QuarkEngine.Instance.LoadAsset<T>(assetName);
        }
        public static Object LoadAsset(string assetName, Type type)
        {
            return QuarkEngine.Instance.LoadAsset(assetName, type);
        }
        public static Coroutine LoadAssetAsync<T>(string assetName, Action<T> callback)
where T : Object
        {
            return QuarkEngine.Instance.LoadAssetAsync<T>(assetName, callback);
        }
        public static Coroutine LoadAssetAsync(string assetName, Type type, Action<Object> callback)
        {
            return QuarkEngine.Instance.LoadAssetAsync(assetName, type, callback);
        }
        public static GameObject LoadPrefab(string assetName, bool instantiate = false)
        {
            return QuarkEngine.Instance.LoadPrefab(assetName, instantiate);
        }
        public static T[] LoadMainAndSubAssets<T>(string assetName) where T : Object
        {
            return QuarkEngine.Instance.LoadMainAndSubAssets<T>(assetName);
        }
        public static Object[] LoadMainAndSubAssets(string assetName, Type type)
        {
            return QuarkEngine.Instance.LoadMainAndSubAssets(assetName, type);
        }
        public static Object[] LoadAllAssets(string assetBundleName)
        {
            return QuarkEngine.Instance.LoadAllAssets(assetBundleName);
        }
        public static Coroutine LoadPrefabAsync(string assetName, Action<GameObject> callback, bool instantiate = false)
        {
            return QuarkEngine.Instance.LoadPrefabAsync(assetName, callback, instantiate);
        }
        public static Coroutine LoadMainAndSubAssetsAsync<T>(string assetName, Action<T[]> callback) where T : UnityEngine.Object
        {
            return QuarkEngine.Instance.LoadMainAndSubAssetsAsync<T>(assetName, callback);
        }
        public static Coroutine LoadMainAndSubAssetsAsync(string assetName, Type type, Action<Object[]> callback)
        {
            return QuarkEngine.Instance.LoadMainAndSubAssetsAsync(assetName, type, callback);
        }
        public static Coroutine LoadAllAssetAsync(string assetBundleName, Action<Object[]> callback)
        {
            return QuarkEngine.Instance.LoadAllAssetAsync(assetBundleName, callback);
        }
        public static Coroutine LoadSceneAsync(string sceneName, Action<float> progress, Action callback, bool additive = false)
        {
            return QuarkEngine.Instance.LoadSceneAsync(sceneName, null, progress, null, callback, additive);
        }
        public static Coroutine LoadSceneAsync(string sceneName, Action<float> progress, Func<bool> condition, Action callback, bool additive = false)
        {
            return QuarkEngine.Instance.LoadSceneAsync(sceneName, null, progress, condition, callback, additive);
        }
        public static Coroutine LoadSceneAsync(string sceneName, Func<float> progressProvider, Action<float> progress, Func<bool> condition, Action callback, bool additive = false)
        {
            return QuarkEngine.Instance.LoadSceneAsync(sceneName, progressProvider, progress, condition, callback, additive);
        }
        public static void UnloadAsset(string assetName)
        {
            QuarkEngine.Instance.UnloadAsset(assetName);
        }
        public static void UnloadAllAssetBundle(bool unloadAllLoadedObjects = false)
        {
            QuarkEngine.Instance.UnloadAllAssetBundle(unloadAllLoadedObjects);
        }
        public static void UnloadAssetBundle(string assetBundleName, bool unloadAllLoadedObjects = false)
        {
            QuarkEngine.Instance.UnloadAssetBundle(assetBundleName, unloadAllLoadedObjects);
        }
        public static Coroutine UnloadSceneAsync(string sceneName, Action<float> progress, Action callback)
        {
            return QuarkEngine.Instance.UnloadSceneAsync(sceneName, progress, callback);
        }
        public static Coroutine UnloadAllSceneAsync(Action<float> progress, Action callback)
        {
            return QuarkEngine.Instance.UnloadAllSceneAsync(progress, callback);
        }
        /// <summary>
        /// 谨慎使用，清理加载器；
        /// </summary>
        public static void ClearLoader()
        {
            QuarkEngine.Instance.ClearLoader();
        }
        public static bool GetInfo<T>(string assetName, out QuarkAssetObjectInfo info) where T : Object
        {
            return QuarkEngine.Instance.GetInfo(assetName, typeof(T), out info);
        }
        public static bool GetInfo(string assetName, Type type, out QuarkAssetObjectInfo info)
        {
            return QuarkEngine.Instance.GetInfo(assetName, type, out info);
        }
        public static bool GetInfo(string assetName, out QuarkAssetObjectInfo info)
        {
            return QuarkEngine.Instance.GetInfo(assetName, out info);
        }
        public static QuarkAssetObjectInfo[] GetAllLoadedInfos()
        {
            return QuarkEngine.Instance.GetAllLoadedInfos();
        }
    }
}
