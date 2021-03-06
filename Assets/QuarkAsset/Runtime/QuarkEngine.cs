using System;
using System.Collections.Generic;
using UnityEngine;
using Quark.Asset;
using Quark.Networking;
using Quark.Loader;
using System.IO;
using Object = UnityEngine.Object;
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
        QuarkComparator quarkComparator;
        Dictionary<QuarkLoadMode, QuarkAssetLoader> quarkLoaderDict;
        /// <summary>
        /// 当检测到最新的；
        /// </summary>
        public Action<long> onCompareManifestSuccess;
        /// <summary>
        /// 当检测失败；
        /// </summary>
        public Action<string> onCompareManifestFailure;
        public QuarkEngine()
        {
            quarkComparator = new QuarkComparator();
            quarkComparator.Initiate(OnCompareSuccess, OnCompareFailure);
            quarkLoaderDict = new Dictionary<QuarkLoadMode, QuarkAssetLoader>();
            quarkLoaderDict[QuarkLoadMode.AssetDatabase] = new QuarkAssetDatabaseLoader();
            quarkLoaderDict[QuarkLoadMode.AssetBundle] = new QuarkAssetBundleLoader();
        }
        /// <summary>
        /// 初始化，传入资源定位符与本地持久化路径；
        /// </summary>
        /// <param name="url">统一资源定位符</param>
        /// <param name="persistentPath">本地持久化地址</param>
        public void Initiate(string url, string persistentPath)
        {
            QuarkDataProxy.PersistentPath = persistentPath;
            QuarkDataProxy.URL = url;
        }
        public Coroutine RequestManifestFromStreamingAssetsAsync()
        {
            return quarkComparator.RequestManifestFromStreamingAssetsAsync();
        }
        public Coroutine RequestMainifestFromURLAsync()
        {
            return quarkComparator.RequestMainifestFromURLAsync();
        }
        /// <summary>
        /// 对Manifest进行编码；
        /// 用于Built assetbundle模式；
        /// </summary>
        /// <param name="manifest">unityWebRequest获取的Manifest文件对象</param>
        internal void SetBuiltAssetBundleModeData(QuarkAssetManifest manifest)
        {
            if (quarkLoaderDict.TryGetValue(QuarkLoadMode.AssetBundle, out var loader))
                loader.SetLoaderData(manifest);
        }
        /// <summary>
        /// 用于Editor开发模式；
        /// 对QuarkAssetDataset进行编码
        /// </summary>
        /// <param name="assetData">QuarkAssetDataset对象</param>
        internal void SetAssetDatabaseModeData(QuarkAssetDataset assetData)
        {
            if (quarkLoaderDict.TryGetValue(QuarkLoadMode.AssetDatabase, out var loader))
                loader.SetLoaderData(assetData);
        }
        internal T LoadAsset<T>(string assetName, string assetExtension)
where T : UnityEngine.Object
        {
            if (quarkLoaderDict.TryGetValue(QuarkAssetLoadMode, out var loader))
                return loader.LoadAsset<T>(assetName, assetExtension);
            return null;
        }
        internal Object LoadAsset(string assetName, Type type, string assetExtension)
        {
            if (quarkLoaderDict.TryGetValue(QuarkAssetLoadMode, out var loader))
                return loader.LoadAsset(assetName, assetExtension, type);
            return null;
        }
        internal GameObject LoadPrefab(string assetName, string assetExtension, bool instantiate = false)
        {
            if (quarkLoaderDict.TryGetValue(QuarkAssetLoadMode, out var loader))
                return loader.LoadPrefab(assetName, assetExtension, instantiate);
            return null;
        }
        internal T[] LoadMainAndSubAssets<T>(string assetName, string assetExtension) where T : UnityEngine.Object
        {
            if (quarkLoaderDict.TryGetValue(QuarkAssetLoadMode, out var loader))
                return loader.LoadMainAndSubAssets<T>(assetName, assetExtension);
            return null;
        }
        internal Object[] LoadMainAndSubAssets(string assetName, string assetExtension, Type type)
        {
            if (quarkLoaderDict.TryGetValue(QuarkAssetLoadMode, out var loader))
                return loader.LoadMainAndSubAssets(assetName, assetExtension, type);
            return null;
        }
        internal Coroutine LoadAssetAsync<T>(string assetName, string assetExtension, Action<T> callback)
where T : UnityEngine.Object
        {
            if (quarkLoaderDict.TryGetValue(QuarkAssetLoadMode, out var loader))
                return loader.LoadAssetAsync(assetName, assetExtension, callback);
            return null;
        }
        internal Coroutine LoadAssetAsync(string assetName, string assetExtension, Type type, Action<Object> callback)
        {
            if (quarkLoaderDict.TryGetValue(QuarkAssetLoadMode, out var loader))
                return loader.LoadAssetAsync(assetName, assetExtension, type, callback);
            return null;
        }
        internal Coroutine LoadPrefabAsync(string assetName, string assetExtension, Action<GameObject> callback, bool instantiate = false)
        {
            if (quarkLoaderDict.TryGetValue(QuarkAssetLoadMode, out var loader))
                return loader.LoadPrefabAsync(assetName, assetExtension, callback, instantiate);
            return null;
        }
        internal Coroutine LoadMainAndSubAssetsAsync<T>(string assetName, string assetExtension, Action<T[]> callback) where T : UnityEngine.Object
        {
            if (quarkLoaderDict.TryGetValue(QuarkAssetLoadMode, out var loader))
                return loader.LoadMainAndSubAssetsAsync(assetName, assetExtension, callback);
            return null;
        }
        internal Coroutine LoadMainAndSubAssetsAsync(string assetName, string assetExtension, Type type, Action<Object[]> callback)
        {
            if (quarkLoaderDict.TryGetValue(QuarkAssetLoadMode, out var loader))
                return loader.LoadMainAndSubAssetsAsync(assetName, assetExtension, type, callback);
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
        internal void UnloadAsset(string assetName, string assetExtension)
        {
            if (quarkLoaderDict.TryGetValue(QuarkAssetLoadMode, out var loader))
                loader.UnloadAsset(assetName, assetExtension);
        }
        internal void UnloadAllAssetBundle(bool unloadAllLoadedObjects = false)
        {
            if (quarkLoaderDict.TryGetValue(QuarkAssetLoadMode, out var loader))
                loader.UnloadAllAssetBundle(unloadAllLoadedObjects);
        }
        internal void UnloadAssetBundle(string assetBundleName, bool unloadAllLoadedObjects = false)
        {
            if (quarkLoaderDict.TryGetValue(QuarkAssetLoadMode, out var loader))
                loader.UnloadAssetBundle(assetBundleName, unloadAllLoadedObjects);
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
        internal bool GetInfo<T>(string assetName, string assetExtension, out QuarkAssetObjectInfo info) where T : UnityEngine.Object
        {
            info = QuarkAssetObjectInfo.None;
            if (quarkLoaderDict.TryGetValue(QuarkAssetLoadMode, out var loader))
                return loader.GetInfo<T>(assetName, assetExtension, out info);
            return false;
        }
        internal bool GetInfo(string assetName, string assetExtension, Type type, out QuarkAssetObjectInfo info)
        {
            info = QuarkAssetObjectInfo.None;
            if (quarkLoaderDict.TryGetValue(QuarkAssetLoadMode, out var loader))
                return loader.GetInfo(assetName, assetExtension, type, out info);
            return false;
        }
        internal bool GetInfo(string assetName, string assetExtension, out QuarkAssetObjectInfo info)
        {
            info = QuarkAssetObjectInfo.None;
            if (quarkLoaderDict.TryGetValue(QuarkAssetLoadMode, out var loader))
                return loader.GetInfo(assetName, assetExtension, out info);
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
            onCompareManifestSuccess?.Invoke(size);
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
