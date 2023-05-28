using Quark.Asset;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Quark.Loader
{
    internal class QuarkLoadModeProvider
    {
        Dictionary<QuarkLoadMode, QuarkAssetLoader> quarkLoaderDict;
        QuarkAssetDatabaseLoader assetDatabaseLoader;
        QuarkAssetBundleLoader assetBundleLoader;
        public QuarkLoadModeProvider()
        {
            quarkLoaderDict = new Dictionary<QuarkLoadMode, QuarkAssetLoader>();
            assetDatabaseLoader = new QuarkAssetDatabaseLoader();
            assetBundleLoader = new QuarkAssetBundleLoader();
            quarkLoaderDict[QuarkLoadMode.AssetDatabase] = assetDatabaseLoader;
            quarkLoaderDict[QuarkLoadMode.AssetBundle] = assetBundleLoader;
        }
        internal void SetAssetBundleModeMergedManifest(QuarkMergedManifest mergedManifest)
        {
            assetBundleLoader.SetMergedManifest(mergedManifest);
        }
        /// <summary>
        /// 设置Manifest；
        /// 用于assetbundle模式；
        /// </summary>
        /// <param name="manifest">Manifest文件</param>
        internal void SetAssetBundleModeManifest(QuarkManifest manifest)
        {
            assetBundleLoader.SetManifest(manifest);
        }
        /// <summary>
        /// 用于Editor开发模式；
        /// 对QuarkAssetDataset进行编码
        /// </summary>
        /// <param name="dataset">QuarkAssetDataset对象</param>
        internal void SetAssetDatabaseModeDataset(QuarkDataset dataset)
        {
            assetDatabaseLoader.SetDataset(dataset);
        }
        internal string GetBuildVersion()
        {
            string version = QuarkConstant.NONE;
            switch (QuarkDataProxy.QuarkAssetLoadMode)
            {
                case QuarkLoadMode.None:
                    version = QuarkConstant.LOAD_MODE_NONE;
                    break;
                case QuarkLoadMode.AssetDatabase:
                    version = QuarkDataProxy.QuarkAssetDataset == null ? QuarkConstant.NO_ASSET_DATASET : QuarkConstant.ASSET_DATASET;
                    break;
                case QuarkLoadMode.AssetBundle:
                    version = $"{ QuarkDataProxy.BuildVersion}_{QuarkDataProxy.InternalBuildVersion}";
                    break;
            }
            return version;
        }
        internal T LoadAsset<T>(string assetName)
where T : UnityEngine.Object
        {
            if (quarkLoaderDict.TryGetValue(QuarkDataProxy.QuarkAssetLoadMode, out var loader))
                return loader.LoadAsset<T>(assetName);
            return null;
        }
        internal Object LoadAsset(string assetName, Type type)
        {
            if (quarkLoaderDict.TryGetValue(QuarkDataProxy.QuarkAssetLoadMode, out var loader))
                return loader.LoadAsset(assetName, type);
            return null;
        }
        internal GameObject LoadPrefab(string assetName, bool instantiate)
        {
            if (quarkLoaderDict.TryGetValue(QuarkDataProxy.QuarkAssetLoadMode, out var loader))
                return loader.LoadPrefab(assetName, instantiate);
            return null;
        }
        internal T[] LoadMainAndSubAssets<T>(string assetName) where T : Object
        {
            if (quarkLoaderDict.TryGetValue(QuarkDataProxy.QuarkAssetLoadMode, out var loader))
                return loader.LoadMainAndSubAssets<T>(assetName);
            return null;
        }
        internal Object[] LoadMainAndSubAssets(string assetName, Type type)
        {
            if (quarkLoaderDict.TryGetValue(QuarkDataProxy.QuarkAssetLoadMode, out var loader))
                return loader.LoadMainAndSubAssets(assetName, type);
            return null;
        }
        internal Object[] LoadAllAssets(string assetBundleName)
        {
            if (quarkLoaderDict.TryGetValue(QuarkDataProxy.QuarkAssetLoadMode, out var loader))
                return loader.LoadAllAssets(assetBundleName);
            return null;
        }
        internal Coroutine LoadAssetAsync<T>(string assetName, Action<T> callback)
where T : UnityEngine.Object
        {
            if (quarkLoaderDict.TryGetValue(QuarkDataProxy.QuarkAssetLoadMode, out var loader))
                return loader.LoadAssetAsync(assetName, callback);
            return null;
        }
        internal Coroutine LoadAssetAsync(string assetName, Type type, Action<Object> callback)
        {
            if (quarkLoaderDict.TryGetValue(QuarkDataProxy.QuarkAssetLoadMode, out var loader))
                return loader.LoadAssetAsync(assetName, type, callback);
            return null;
        }
        internal Coroutine LoadPrefabAsync(string assetName, Action<GameObject> callback, bool instantiate)
        {
            if (quarkLoaderDict.TryGetValue(QuarkDataProxy.QuarkAssetLoadMode, out var loader))
                return loader.LoadPrefabAsync(assetName, callback, instantiate);
            return null;
        }
        internal Coroutine LoadMainAndSubAssetsAsync<T>(string assetName, Action<T[]> callback) where T : Object
        {
            if (quarkLoaderDict.TryGetValue(QuarkDataProxy.QuarkAssetLoadMode, out var loader))
                return loader.LoadMainAndSubAssetsAsync(assetName, callback);
            return null;
        }
        internal Coroutine LoadMainAndSubAssetsAsync(string assetName, Type type, Action<Object[]> callback)
        {
            if (quarkLoaderDict.TryGetValue(QuarkDataProxy.QuarkAssetLoadMode, out var loader))
                return loader.LoadMainAndSubAssetsAsync(assetName, type, callback);
            return null;
        }
        internal Coroutine LoadAllAssetAsync(string assetBundleName, Action<Object[]> callback)
        {
            if (quarkLoaderDict.TryGetValue(QuarkDataProxy.QuarkAssetLoadMode, out var loader))
                return loader.LoadAllAssetAsync(assetBundleName, callback);
            return null;
        }
        internal Coroutine LoadSceneAsync(string sceneName, Func<float> progressProvider, Action<float> progress, Func<bool> condition, Action callback, bool additive = false)
        {
            if (quarkLoaderDict.TryGetValue(QuarkDataProxy.QuarkAssetLoadMode, out var loader))
                return loader.LoadSceneAsync(sceneName, progressProvider, progress, condition, callback, additive);
            return null;
        }
        internal void UnloadAsset(string assetName)
        {
            if (quarkLoaderDict.TryGetValue(QuarkDataProxy.QuarkAssetLoadMode, out var loader))
                loader.UnloadAsset(assetName);
        }
        internal void UnloadAllAssetBundle(bool unloadAllLoadedObjects = true)
        {
            if (quarkLoaderDict.TryGetValue(QuarkDataProxy.QuarkAssetLoadMode, out var loader))
                loader.UnloadAllAssetBundle(unloadAllLoadedObjects);
        }
        internal void UnloadAssetBundle(string assetBundleName, bool unloadAllLoadedObjects = true)
        {
            if (quarkLoaderDict.TryGetValue(QuarkDataProxy.QuarkAssetLoadMode, out var loader))
                loader.UnloadAssetBundle(assetBundleName, unloadAllLoadedObjects);
        }
        internal void ResetLoader(QuarkLoadMode loadMode)
        {
            if (quarkLoaderDict.TryGetValue(loadMode, out var loader))
                loader.ResetLoader();
        }
        internal Coroutine UnloadSceneAsync(string sceneName, Action<float> progress, Action callback)
        {
            if (quarkLoaderDict.TryGetValue(QuarkDataProxy.QuarkAssetLoadMode, out var loader))
                return loader.UnloadSceneAsync(sceneName, progress, callback);
            return null;
        }
        internal Coroutine UnloadAllSceneAsync(Action<float> progress, Action callback)
        {
            if (quarkLoaderDict.TryGetValue(QuarkDataProxy.QuarkAssetLoadMode, out var loader))
                return loader.UnloadAllSceneAsync(progress, callback);
            return null;
        }
        internal bool GetInfo(string assetName, out QuarkObjectState info)
        {
            info = QuarkObjectState.None;
            if (quarkLoaderDict.TryGetValue(QuarkDataProxy.QuarkAssetLoadMode, out var loader))
                return loader.GetInfo(assetName, out info);
            return false;
        }
        internal QuarkObjectState[] GetAllLoadedInfos()
        {
            if (quarkLoaderDict.TryGetValue(QuarkDataProxy.QuarkAssetLoadMode, out var loader))
                return loader.GetAllLoadedInfos();
            return new QuarkObjectState[0];
        }
    }
}
