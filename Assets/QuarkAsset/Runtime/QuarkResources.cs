using Quark.Asset;
using System;
using UnityEngine;
using Object = UnityEngine.Object;
namespace Quark
{
    public sealed class QuarkResources
    {
        public static ulong QuarkEncryptionOffset
        {
            get { return QuarkDataProxy.QuarkEncryptionOffset; }
            set { QuarkDataProxy.QuarkEncryptionOffset = value; }
        }
        public static byte[] QuarkAESEncryptionKey
        {
            get { return QuarkDataProxy.QuarkAESEncryptionKey; }
            set { QuarkDataProxy.QuarkAESEncryptionKey = value; }
        }
        public static QuarkLoadMode QuarkAssetLoadMode
        {
            get { return QuarkEngine.Instance.QuarkAssetLoadMode; }
            set { QuarkEngine.Instance.QuarkAssetLoadMode = value; }
        }
        /// <summary>
        /// 当检测到最新的；
        /// </summary>
        public static event Action<long> OnCompareManifestSuccess
        {
            add { QuarkEngine.Instance.onCompareManifestSuccess += value; }
            remove { QuarkEngine.Instance.onCompareManifestSuccess -= value; }
        }
        /// <summary>
        /// 当检测失败；
        /// </summary>
        public static event Action<string> OnCompareManifestFailure
        {
            add { QuarkEngine.Instance.onCompareManifestFailure += value; }
            remove { QuarkEngine.Instance.onCompareManifestFailure -= value; }
        }
        public static Coroutine RequestManifestFromStreamingAssetsAsync()
        {
            return QuarkEngine.Instance.RequestManifestFromStreamingAssetsAsync();
        }
        public Coroutine RequestMainifestFromURLAsync()
        {
            return QuarkEngine.Instance.RequestMainifestFromURLAsync();
        }
        public static T LoadAsset<T>(string assetName, string assetExtension = null)
where T : Object
        {
            return QuarkEngine.Instance.LoadAsset<T>(assetName, assetExtension);
        }
        public static Object LoadAsset(string assetName, Type type, string assetExtension = null)
        {
            return QuarkEngine.Instance.LoadAsset(assetName, type, assetExtension);
        }
        public static Coroutine LoadAssetAsync<T>(string assetName, Action<T> callback)
where T : Object
        {
            return QuarkEngine.Instance.LoadAssetAsync<T>(assetName, string.Empty, callback);
        }
        public static Coroutine LoadAssetAsync<T>(string assetName, string assetExtension, Action<T> callback)
where T : Object
        {
            return QuarkEngine.Instance.LoadAssetAsync<T>(assetName, assetExtension, callback);
        }
        public static Coroutine LoadAssetAsync(string assetName, Type type, Action<Object> callback)
        {
            return QuarkEngine.Instance.LoadAssetAsync(assetName, string.Empty, type, callback);
        }
        public static Coroutine LoadAssetAsync(string assetName, string assetExtension, Type type, Action<Object> callback)
        {
            return QuarkEngine.Instance.LoadAssetAsync(assetName, assetExtension, type, callback);
        }
        public static GameObject LoadPrefab(string assetName, bool instantiate = false)
        {
            return QuarkEngine.Instance.LoadPrefab(assetName, string.Empty, instantiate);
        }
        public static GameObject LoadPrefab(string assetName, string assetExtension, bool instantiate = false)
        {
            return QuarkEngine.Instance.LoadPrefab(assetName, assetExtension, instantiate);
        }
        public static T[] LoadMainAndSubAssets<T>(string assetName) where T : UnityEngine.Object
        {
            return QuarkEngine.Instance.LoadMainAndSubAssets<T>(assetName, string.Empty);
        }
        public static T[] LoadMainAndSubAssets<T>(string assetName, string assetExtension) where T : UnityEngine.Object
        {
            return QuarkEngine.Instance.LoadMainAndSubAssets<T>(assetName, assetExtension);
        }
        public static Object[] LoadMainAndSubAssets(string assetName, Type type)
        {
            return QuarkEngine.Instance.LoadMainAndSubAssets(assetName, string.Empty, type);
        }
        public static Object[] LoadMainAndSubAssets(string assetName, string assetExtension, Type type)
        {
            return QuarkEngine.Instance.LoadMainAndSubAssets(assetName, assetExtension, type);
        }
        public static Coroutine LoadPrefabAsync(string assetName, Action<GameObject> callback, bool instantiate = false)
        {
            return QuarkEngine.Instance.LoadPrefabAsync(assetName, string.Empty, callback, instantiate);
        }
        public static Coroutine LoadPrefabAsync(string assetName, string assetExtension, Action<GameObject> callback, bool instantiate = false)
        {
            return QuarkEngine.Instance.LoadPrefabAsync(assetName, assetExtension, callback, instantiate);
        }
        public static Coroutine LoadMainAndSubAssetsAsync<T>(string assetName, Action<T[]> callback) where T : UnityEngine.Object
        {
            return QuarkEngine.Instance.LoadMainAndSubAssetsAsync<T>(assetName, string.Empty, callback);
        }
        public static Coroutine LoadMainAndSubAssetsAsync<T>(string assetName, string assetExtension, Action<T[]> callback) where T : UnityEngine.Object
        {
            return QuarkEngine.Instance.LoadMainAndSubAssetsAsync<T>(assetName, assetExtension, callback);
        }
        public static Coroutine LoadMainAndSubAssetsAsync(string assetName, Type type,Action<Object[]> callback)
        {
            return QuarkEngine.Instance.LoadMainAndSubAssetsAsync(assetName, string.Empty, type,callback);
        }
        public static Coroutine LoadMainAndSubAssetsAsync(string assetName, string assetExtension, Type type,Action<Object[]> callback)
        {
            return QuarkEngine.Instance.LoadMainAndSubAssetsAsync(assetName, assetExtension, type,callback);
        }
        public static Coroutine LoadAllAssetAsync(string assetBundleName, Action<Object[]> callback)
        {
            return QuarkEngine.Instance.LoadAllAssetAsync(assetBundleName,  callback);
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
            QuarkEngine.Instance.UnloadAsset(assetName, string.Empty);
        }
        public static void UnloadAsset(string assetName, string assetExtension)
        {
            QuarkEngine.Instance.UnloadAsset(assetName, assetExtension);
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
        public static bool GetInfo<T>(string assetName, string assetExtension, out QuarkAssetObjectInfo info) where T : UnityEngine.Object
        {
            return QuarkEngine.Instance.GetInfo<T>(assetName, assetExtension, out info);
        }
        public static bool GetInfo<T>(string assetName, out QuarkAssetObjectInfo info) where T : UnityEngine.Object
        {
            return QuarkEngine.Instance.GetInfo<T>(assetName, string.Empty, out info);
        }
        public static bool GetInfo(string assetName, string assetExtension, Type type,out QuarkAssetObjectInfo info) 
        {
            return QuarkEngine.Instance.GetInfo(assetName, assetExtension, type,out info);
        }
        public static bool GetInfo(string assetName, Type type,out QuarkAssetObjectInfo info) 
        {
            return QuarkEngine.Instance.GetInfo(assetName, string.Empty,type , out info);
        }
        public static bool GetInfo(string assetName, string assetExtension, out QuarkAssetObjectInfo info)
        {
            return QuarkEngine.Instance.GetInfo(assetName, assetExtension, out info);
        }
        public static bool GetInfo(string assetName, out QuarkAssetObjectInfo info)
        {
            return QuarkEngine.Instance.GetInfo(assetName, string.Empty, out info);
        }
        public static QuarkAssetObjectInfo[] GetAllLoadedInfos()
        {
            return QuarkEngine.Instance.GetAllLoadedInfos();
        }
    }
}
