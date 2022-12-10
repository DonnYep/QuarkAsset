using System;
using System.Collections.Generic;
using UnityEngine;
using Quark.Asset;
using Quark.Networking;
using Quark.Loader;
using System.IO;
using Object = UnityEngine.Object;
using Quark.Verifiy;

namespace Quark
{
    //================================================
    /*
    *1、QuarkAsset是一款unity资源管理的解决方案。摒弃了Resources原生的
    * 加载模式，主要针对AssetBundle在Editor模式与Runtime模式加载方式的
    * 统一。
    * 
    *2、Editor模式下，加载方式主要依靠unity生成的gid进行资源寻址。通过
    * gid可以忽略由于更改文件夹地址导致的加载失败问题。
    * 
    *3、加载资源可直接通过资源名进行加载，无需通过相对地址或者完整路径
    *名。若文件资源相同，则可通过后缀名、相对于unity的assetType、以及完整
    *路径规避。
    *
    *4、Quark设计方向是插件化，即插即用，因此内置了很多常用工具函数；
    *
    *5、使用流程：1>先初始化调用Initiate函数;
    *                        2>比较远端与本地的文件清单，调用CompareManifest；
    *                        3>下载差异文件，调用LaunchDownload；
    */
    //================================================
    internal class QuarkEngine
    {
        static QuarkEngine instance;
        public static QuarkEngine Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new QuarkEngine();
                }
                return instance;
            }
        }
        public QuarkLoadMode QuarkAssetLoadMode { get; set; }
        public ulong QuarkEncryptionOffset
        {
            get { return QuarkDataProxy.QuarkEncryptionOffset; }
            set { QuarkDataProxy.QuarkEncryptionOffset = value; }
        }
        public QuarkComparator quarkComparator;
        /// <summary>
        /// 当检测到最新的；
        /// LatestURLS---Size
        /// </summary>

        internal Action<string[], long> onCompareManifestSuccess;

        Dictionary<QuarkLoadMode, QuarkAssetLoader> quarkLoaderDict;
        /// <summary>
        /// 当检测失败；
        /// ErrorMessage
        /// </summary>
        public Action<string> onCompareManifestFailure;

        #region quark downloader
        public QuarkDownloader quarkDownloader;
        #endregion
        public QuarkManifestVerifier quarkManifestVerifier;
        public QuarkEngine()
        {
            quarkComparator = new QuarkComparator();
            quarkDownloader = new QuarkDownloader();
            quarkManifestVerifier = new QuarkManifestVerifier();
            quarkComparator.Initiate(OnCompareSuccess, OnCompareFailure);
            quarkLoaderDict = new Dictionary<QuarkLoadMode, QuarkAssetLoader>();
            quarkLoaderDict[QuarkLoadMode.AssetDatabase] = new QuarkAssetDatabaseLoader();
            quarkLoaderDict[QuarkLoadMode.AssetBundle] = new QuarkAssetBundleLoader();
        }
        public Coroutine RequestManifestFromLocalAssetAsync()
        {
            return quarkComparator.RequestManifestFromLocalAssetAsync();
        }
        public Coroutine RequestMainifestFromURLAsync()
        {
            return quarkComparator.RequestMainifestFromURLAsync();
        }
        /// <summary>
        /// 设置Manifest；
        /// 用于assetbundle模式；
        /// </summary>
        /// <param name="manifest">Manifest文件</param>
        internal void SetAssetBundleModeManifest(QuarkAssetManifest manifest)
        {
            if (quarkLoaderDict.TryGetValue(QuarkLoadMode.AssetBundle, out var loader))
                loader.SetLoaderData(manifest);
        }
        /// <summary>
        /// 用于Editor开发模式；
        /// 对QuarkAssetDataset进行编码
        /// </summary>
        /// <param name="assetData">QuarkAssetDataset对象</param>
        internal void SetAssetDatabaseModeDataset(QuarkAssetDataset assetData)
        {
            if (quarkLoaderDict.TryGetValue(QuarkLoadMode.AssetDatabase, out var loader))
                loader.SetLoaderData(assetData);
        }
        internal T LoadAsset<T>(string assetName)
where T : UnityEngine.Object
        {
            if (quarkLoaderDict.TryGetValue(QuarkAssetLoadMode, out var loader))
                return loader.LoadAsset<T>(assetName);
            return null;
        }
        internal Object LoadAsset(string assetName, Type type)
        {
            if (quarkLoaderDict.TryGetValue(QuarkAssetLoadMode, out var loader))
                return loader.LoadAsset(assetName, type);
            return null;
        }
        internal GameObject LoadPrefab(string assetName, bool instantiate = false)
        {
            if (quarkLoaderDict.TryGetValue(QuarkAssetLoadMode, out var loader))
                return loader.LoadPrefab(assetName, instantiate);
            return null;
        }
        internal T[] LoadMainAndSubAssets<T>(string assetName) where T : Object
        {
            if (quarkLoaderDict.TryGetValue(QuarkAssetLoadMode, out var loader))
                return loader.LoadMainAndSubAssets<T>(assetName);
            return null;
        }
        internal Object[] LoadMainAndSubAssets(string assetName, Type type)
        {
            if (quarkLoaderDict.TryGetValue(QuarkAssetLoadMode, out var loader))
                return loader.LoadMainAndSubAssets(assetName, type);
            return null;
        }
        internal Object[] LoadAllAssets(string assetBundleName)
        {
            if (quarkLoaderDict.TryGetValue(QuarkAssetLoadMode, out var loader))
                return loader.LoadAllAssets(assetBundleName);
            return null;
        }
        internal Coroutine LoadAssetAsync<T>(string assetName, Action<T> callback)
where T : UnityEngine.Object
        {
            if (quarkLoaderDict.TryGetValue(QuarkAssetLoadMode, out var loader))
                return loader.LoadAssetAsync(assetName, callback);
            return null;
        }
        internal Coroutine LoadAssetAsync(string assetName, Type type, Action<Object> callback)
        {
            if (quarkLoaderDict.TryGetValue(QuarkAssetLoadMode, out var loader))
                return loader.LoadAssetAsync(assetName, type, callback);
            return null;
        }
        internal Coroutine LoadPrefabAsync(string assetName, Action<GameObject> callback, bool instantiate = false)
        {
            if (quarkLoaderDict.TryGetValue(QuarkAssetLoadMode, out var loader))
                return loader.LoadPrefabAsync(assetName, callback, instantiate);
            return null;
        }
        internal Coroutine LoadMainAndSubAssetsAsync<T>(string assetName, Action<T[]> callback) where T : Object
        {
            if (quarkLoaderDict.TryGetValue(QuarkAssetLoadMode, out var loader))
                return loader.LoadMainAndSubAssetsAsync(assetName, callback);
            return null;
        }
        internal Coroutine LoadMainAndSubAssetsAsync(string assetName, Type type, Action<Object[]> callback)
        {
            if (quarkLoaderDict.TryGetValue(QuarkAssetLoadMode, out var loader))
                return loader.LoadMainAndSubAssetsAsync(assetName, type, callback);
            return null;
        }
        internal Coroutine LoadAllAssetAsync(string assetBundleName, Action<Object[]> callback)
        {
            if (quarkLoaderDict.TryGetValue(QuarkAssetLoadMode, out var loader))
                return loader.LoadAllAssetAsync(assetBundleName, callback);
            return null;
        }
        internal Coroutine LoadSceneAsync(string sceneName, Func<float> progressProvider, Action<float> progress, Func<bool> condition, Action callback, bool additive = false)
        {
            if (quarkLoaderDict.TryGetValue(QuarkAssetLoadMode, out var loader))
                return loader.LoadSceneAsync(sceneName, progressProvider, progress, condition, callback, additive);
            return null;
        }
        internal void UnloadAsset(string assetName)
        {
            if (quarkLoaderDict.TryGetValue(QuarkAssetLoadMode, out var loader))
                loader.UnloadAsset(assetName);
        }
        internal void UnloadAllAssetBundle(bool unloadAllLoadedObjects = true)
        {
            if (quarkLoaderDict.TryGetValue(QuarkAssetLoadMode, out var loader))
                loader.UnloadAllAssetBundle(unloadAllLoadedObjects);
        }
        internal void UnloadAssetBundle(string assetBundleName, bool unloadAllLoadedObjects = true)
        {
            if (quarkLoaderDict.TryGetValue(QuarkAssetLoadMode, out var loader))
                loader.UnloadAssetBundle(assetBundleName, unloadAllLoadedObjects);
        }
        internal void ClearLoader()
        {
            if (quarkLoaderDict.TryGetValue(QuarkAssetLoadMode, out var loader))
                loader.ClearLoader();
        }
        internal Coroutine UnloadSceneAsync(string sceneName, Action<float> progress, Action callback)
        {
            if (quarkLoaderDict.TryGetValue(QuarkAssetLoadMode, out var loader))
                return loader.UnloadSceneAsync(sceneName, progress, callback);
            return null;
        }
        internal Coroutine UnloadAllSceneAsync(Action<float> progress, Action callback)
        {
            if (quarkLoaderDict.TryGetValue(QuarkAssetLoadMode, out var loader))
                return loader.UnloadAllSceneAsync(progress, callback);
            return null;
        }
        internal bool GetInfo(string assetName, Type type, out QuarkAssetObjectInfo info)
        {
            info = QuarkAssetObjectInfo.None;
            if (quarkLoaderDict.TryGetValue(QuarkAssetLoadMode, out var loader))
                return loader.GetInfo(assetName, type, out info);
            return false;
        }
        internal bool GetInfo(string assetName, out QuarkAssetObjectInfo info)
        {
            info = QuarkAssetObjectInfo.None;
            if (quarkLoaderDict.TryGetValue(QuarkAssetLoadMode, out var loader))
                return loader.GetInfo(assetName, out info);
            return false;
        }
        internal QuarkAssetObjectInfo[] GetAllLoadedInfos()
        {
            if (quarkLoaderDict.TryGetValue(QuarkAssetLoadMode, out var loader))
                return loader.GetAllLoadedInfos();
            return new QuarkAssetObjectInfo[0];
        }
        /// <summary>
        /// 获取比较manifest成功；
        /// </summary>
        /// <param name="latest">最新的</param>
        /// <param name="expired">过期的</param>
        /// <param name="size">整体文件大小</param>
        void OnCompareSuccess(string[] latest, string[] expired, long size)
        {
            var length = expired.Length;
            for (int i = 0; i < length; i++)
            {
                try
                {
                    var expiredPath = Path.Combine(QuarkDataProxy.PersistentPath, expired[i]);
                    QuarkUtility.DeleteFile(expiredPath);
                }
                catch { }
            }
            onCompareManifestSuccess?.Invoke(latest, size);
        }
        /// <summary>
        /// 当比较失败；
        /// </summary>
        /// <param name="errorMessage">错误信息</param>
        void OnCompareFailure(string errorMessage)
        {
            onCompareManifestFailure?.Invoke(errorMessage);
        }
    }
}
