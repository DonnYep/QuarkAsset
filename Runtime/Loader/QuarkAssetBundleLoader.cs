using Quark.Asset;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
namespace Quark.Loader
{
    internal class QuarkAssetBundleLoader : QuarkAssetLoader
    {
        /// <summary>
        /// bundleKey===bundleName
        /// </summary>
        readonly Dictionary<string, string> nameBundleKeyDict = new Dictionary<string, string>();
        string PersistentPath { get { return QuarkDataProxy.PersistentPath; } }
        public override void SetLoaderData(IQuarkLoaderData loaderData)
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            InitManifest(loaderData as QuarkAssetManifest);
        }
        public override T LoadAsset<T>(string assetName)
        {
            T asset = null;
            var hasObject = GetQuarkObject(assetName, out var quarkObject);
            if (!hasObject)
                return null;
            var assetBundleName = quarkObject.AssetBundleName;
            if (string.IsNullOrEmpty(assetBundleName))
                return null;
            LoadAssetBundleWithDependencies(assetBundleName);
            var hasBundleWarpper = bundleWarpperDict.TryGetValue(assetBundleName, out var bundleWarpper);
            if (!hasBundleWarpper)
                return null;
            var assetBundle = bundleWarpper.AssetBundle;
            if (assetBundle != null)
            {
                asset = assetBundle.LoadAsset<T>(assetName);
                if (asset != null)
                    OnResourceObjectLoad(quarkObject);
            }
            return asset;
        }
        public override Object LoadAsset(string assetName, Type type)
        {
            Object asset = null;
            var hasObject = GetQuarkObject(assetName, out var quarkObject);
            if (!hasObject)
                return null;
            var assetBundleName = quarkObject.AssetBundleName;
            if (string.IsNullOrEmpty(assetBundleName))
                return null;
            LoadAssetBundleWithDependencies(assetBundleName);
            var hasBundleWarpper = bundleWarpperDict.TryGetValue(assetBundleName, out var bundleWarpper);
            if (!hasBundleWarpper)
                return null;
            var assetBundle = bundleWarpper.AssetBundle;
            if (assetBundle != null)
            {
                asset = assetBundle.LoadAsset(assetName, type);
                if (asset != null)
                    OnResourceObjectLoad(quarkObject);
            }
            return asset;
        }
        public override GameObject LoadPrefab(string assetName, bool instantiate = false)
        {
            var resGo = LoadAsset<GameObject>(assetName);
            if (instantiate)
            {
                GameObject go = null;
                if (resGo != null)
                    go = GameObject.Instantiate(resGo);
                return go;
            }
            else
                return resGo;
        }
        public override T[] LoadMainAndSubAssets<T>(string assetName)
        {
            T[] assets = null;
            var hasObject = GetQuarkObject(assetName, out var quarkObject);
            if (!hasObject)
                return null;
            var assetBundleName = quarkObject.AssetBundleName;
            if (string.IsNullOrEmpty(assetBundleName))
                return null;
            LoadAssetBundleWithDependencies(assetBundleName);
            var hasBundleWarpper = bundleWarpperDict.TryGetValue(assetBundleName, out var bundleWarpper);
            if (!hasBundleWarpper)
                return null;
            var assetBundle = bundleWarpper.AssetBundle;
            if (assetBundle != null)
            {
                assets = assetBundle.LoadAssetWithSubAssets<T>(assetName);
                if (assets != null)
                    OnResourceObjectLoad(quarkObject);
            }
            return assets;
        }
        public override Object[] LoadMainAndSubAssets(string assetName, Type type)
        {
            Object[] assets = null;
            var hasObject = GetQuarkObject(assetName, out var quarkObject);
            if (!hasObject)
                return null;
            var assetBundleName = quarkObject.AssetBundleName;
            if (string.IsNullOrEmpty(assetBundleName))
                return null;
            LoadAssetBundleWithDependencies(assetBundleName);
            var hasBundleWarpper = bundleWarpperDict.TryGetValue(assetBundleName, out var bundleWarpper);
            if (!hasBundleWarpper)
                return null;
            var assetBundle = bundleWarpper.AssetBundle;
            if (assetBundle != null)
            {
                assets = assetBundle.LoadAssetWithSubAssets(assetName, type);
                if (assets != null)
                    OnResourceObjectLoad(quarkObject);
            }
            return assets;
        }
        public override Object[] LoadAllAssets(string assetBundleName)
        {
            if (string.IsNullOrEmpty(assetBundleName))
                return null;
            var hasBundleWarpper = bundleWarpperDict.TryGetValue(assetBundleName, out var bundleWarpper);
            if (!hasBundleWarpper)
                return null;
            LoadAssetBundleWithDependencies(assetBundleName);
            Object[] assets = null;
            var assetBundle = bundleWarpper.AssetBundle;
            if (assetBundle == null)
                return null;
            assets = assetBundle.LoadAllAssets();
            if (assets != null)
                OnResourceBundleAllAssetLoad(assetBundleName);
            return assets;
        }
        public override Coroutine LoadAssetAsync<T>(string assetName, Action<T> callback)
        {
            return QuarkUtility.Unity.StartCoroutine(EnumLoadAssetAsync(assetName, typeof(T), asset => { callback?.Invoke(asset as T); }));
        }
        public override Coroutine LoadAssetAsync(string assetName, Type type, Action<Object> callback)
        {
            return QuarkUtility.Unity.StartCoroutine(EnumLoadAssetAsync(assetName, type, callback));
        }
        public override Coroutine LoadPrefabAsync(string assetName, Action<GameObject> callback, bool instantiate = false)
        {
            return QuarkUtility.Unity.StartCoroutine(EnumLoadAssetAsync(assetName, typeof(GameObject), (resGo) =>
             {
                 var gameobject = resGo as GameObject;
                 if (instantiate)
                 {
                     GameObject go = null;
                     if (gameobject != null)
                         go = GameObject.Instantiate(gameobject);
                     callback.Invoke(go);
                 }
                 else
                 {
                     callback.Invoke(gameobject);
                 }
             }));
        }
        public override Coroutine LoadMainAndSubAssetsAsync<T>(string assetName, Action<T[]> callback)
        {
            return QuarkUtility.Unity.StartCoroutine(EnumLoadAssetWithSubAssetsAsync(assetName, typeof(T), assets =>
             {
                 T[] rstAssets = new T[assets.Length];
                 var length = rstAssets.Length;
                 for (int i = 0; i < length; i++)
                 {
                     rstAssets[i] = assets[i] as T;
                 }
                 callback?.Invoke(rstAssets);
             }));
        }
        public override Coroutine LoadMainAndSubAssetsAsync(string assetName, Type type, Action<Object[]> callback)
        {
            return QuarkUtility.Unity.StartCoroutine(EnumLoadAssetWithSubAssetsAsync(assetName, type, callback));
        }
        public override Coroutine LoadAllAssetAsync(string assetBundleName, Action<Object[]> callback)
        {
            return QuarkUtility.Unity.StartCoroutine(EnumLoadAllAssetAsync(assetBundleName, callback));
        }
        public override Coroutine LoadSceneAsync(string sceneName, Func<float> progressProvider, Action<float> progress, Func<bool> condition, Action callback, bool additive = false)
        {
            return QuarkUtility.Unity.StartCoroutine(EnumLoadSceneAsync(sceneName, progressProvider, progress, condition, callback, additive));
        }
        public override void ReleaseAsset(string assetName)
        {
            var hasObject = GetQuarkObject(assetName, out var quarkObject);
            if (hasObject)
            {
                if (objectWarpperDict.TryGetValue(quarkObject.AssetPath, out var objectWapper))
                    OnResoucreObjectRelease(objectWapper);
            }
        }
        public override void UnloadAsset(string assetName)
        {
            var hasObject = GetQuarkObject(assetName, out var quarkObject);
            if (hasObject)
                OnResourceObjectUnload(quarkObject);
        }
        public override void UnloadAllAssetBundle(bool unloadAllLoadedObjects = false)
        {
            //这里是卸载，保留寻址信息，清空引用计数
            foreach (var objectWarpper in objectWarpperDict.Values)
            {
                objectWarpper.ReferenceCount = 0;
            }
            foreach (var bundleWarpper in bundleWarpperDict.Values)
            {
                bundleWarpper.ReferenceCount = 0;
                bundleWarpper.AssetBundle?.Unload(unloadAllLoadedObjects);
            }
        }
        public override void UnloadAssetBundle(string assetBundleName, bool unloadAllLoadedObjects = false)
        {
            if (bundleWarpperDict.TryGetValue(assetBundleName, out var bundleWarpper))
            {
                UnloadAssetBundleWithDependencies(bundleWarpper, bundleWarpper.ReferenceCount, unloadAllLoadedObjects);
            }
        }
        public override Coroutine UnloadSceneAsync(string sceneName, Action<float> progress, Action callback)
        {
            return QuarkUtility.Unity.StartCoroutine(EnumUnloadSceneAsync(sceneName, progress, callback));
        }
        public override Coroutine UnloadAllSceneAsync(Action<float> progress, Action callback)
        {
            return QuarkUtility.Unity.StartCoroutine(EnumUnloadAllSceneAsync(progress, callback));
        }
        public override void ClearLoader()
        {
            foreach (var bundleWarpper in bundleWarpperDict.Values)
            {
                bundleWarpper.AssetBundle?.Unload(true);
            }
            loadedSceneDict.Clear();
            loadSceneList.Clear();
            bundleWarpperDict.Clear();
            objectWarpperDict.Clear();
            objectLnkDict.Clear();
            nameBundleKeyDict.Clear();
        }
        IEnumerator EnumLoadAssetWithSubAssetsAsync(string assetName, Type type, Action<Object[]> callback)
        {
            //DONE
            var hasObject = GetQuarkObject(assetName, out var quarkObject);
            if (!hasObject)
            {
                callback?.Invoke(null);
                yield break;
            }
            var assetBundleName = quarkObject.AssetBundleName;
            yield return EnumLoadAssetBundleWithDependenciesAsync(assetBundleName);
            var hasBundleWarpper = bundleWarpperDict.TryGetValue(assetBundleName, out var bundleWarpper);
            if (!hasBundleWarpper)
            {
                callback?.Invoke(null);
                yield break;
            }
            if (bundleWarpper.AssetBundle == null)
            {
                callback?.Invoke(null);
                yield break;
            }
            Object[] assets = null;
            var assetBundle = bundleWarpper.AssetBundle;
            assets = assetBundle.LoadAssetWithSubAssets(assetName, type);
            if (assets != null)
                OnResourceObjectLoad(quarkObject);
            callback?.Invoke(assets);
        }
        IEnumerator EnumLoadAssetAsync(string assetName, Type type, Action<Object> callback)
        {
            //DONE
            var hasObject = GetQuarkObject(assetName, out var quarkObject);
            if (!hasObject)
            {
                callback?.Invoke(null);
                yield break;
            }
            var assetBundleName = quarkObject.AssetBundleName;
            yield return EnumLoadAssetBundleWithDependenciesAsync(assetBundleName);
            var hasBundleWarpper = bundleWarpperDict.TryGetValue(assetBundleName, out var bundleWarpper);
            if (!hasBundleWarpper)
            {
                callback?.Invoke(null);
                yield break;
            }
            if (bundleWarpper.AssetBundle == null)
            {
                callback?.Invoke(null);
                yield break;
            }
            var assetBundle = bundleWarpper.AssetBundle;
            Object asset = null;
            asset = assetBundle.LoadAsset(assetName, type);
            if (asset != null)
                OnResourceObjectLoad(quarkObject);
            callback?.Invoke(asset);
        }
        IEnumerator EnumLoadAllAssetAsync(string bundleName, Action<Object[]> callback)
        {
            //DONE
            if (string.IsNullOrEmpty(bundleName))
                yield break;
            var hasBundleWarpper = bundleWarpperDict.TryGetValue(bundleName, out var bundleWarpper);
            if (!hasBundleWarpper)
            {
                callback?.Invoke(null);
                yield break;
            }
            yield return EnumLoadAssetBundleWithDependenciesAsync(bundleName);
            Object[] assets = null;
            var assetBundle = bundleWarpper.AssetBundle;
            if (assetBundle == null)
            {
                callback?.Invoke(null);
                yield break;
            }
            assets = assetBundle.LoadAllAssets();
            if (assets != null)
                OnResourceBundleAllAssetLoad(bundleName);
            callback?.Invoke(assets);
        }
        IEnumerator EnumLoadSceneAsync(string sceneName, Func<float> progressProvider, Action<float> progress, Func<bool> condition, Action callback, bool additive)
        {
            if (loadedSceneDict.TryGetValue(sceneName, out var loadedScene))
            {
                progress?.Invoke(1);
                SceneManager.SetActiveScene(loadedScene);
                callback?.Invoke();
                yield break;
            }
            var hasObject = GetSceneObject(sceneName, out var quarkObject);
            if (!hasObject)
            {
                progress?.Invoke(1);
                callback?.Invoke();
                yield break;
            }
            yield return EnumLoadAssetBundleWithDependenciesAsync(quarkObject.AssetBundleName);
            LoadSceneMode loadSceneMode = additive == true ? LoadSceneMode.Additive : LoadSceneMode.Single;
            var operation = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
            if (operation == null)
            {
                progress?.Invoke(1);
                callback?.Invoke();
                yield break;
            }
            loadSceneList.Add(sceneName);
            operation.allowSceneActivation = false;
            var hasProviderProgress = progressProvider != null;
            while (!operation.isDone)
            {
                if (hasProviderProgress)
                {
                    var providerProgress = progressProvider();
                    var sum = providerProgress + operation.progress;
                    if (sum >= 1.9)
                        break;
                    else
                        progress?.Invoke(sum / 2);
                }
                else
                {
                    progress?.Invoke(operation.progress);
                    if (operation.progress >= 0.9f)
                        break;
                }
                yield return null;
            }
            progress?.Invoke(1);
            yield return null;
            if (condition != null)
                yield return new WaitUntil(condition);
            operation.allowSceneActivation = true;
            yield return null;
            callback?.Invoke();
        }
        IEnumerator EnumUnloadAllSceneAsync(Action<float> progress, Action callback)
        {
            var sceneCount = loadedSceneDict.Count;
            //单位场景的百分比比率
            var unitResRatio = 100f / sceneCount;
            int currentSceneIndex = 0;
            float overallProgress = 0;
            foreach (var sceneName in loadSceneList)
            {
                var overallIndexPercent = 100 * ((float)currentSceneIndex / sceneCount);
                currentSceneIndex++;
                var ao = SceneManager.UnloadSceneAsync(sceneName);
                while (!ao.isDone)
                {
                    overallProgress = overallIndexPercent + (unitResRatio * ao.progress);
                    progress?.Invoke(overallProgress / 100);
                    yield return null;
                }
                overallProgress = overallIndexPercent + (unitResRatio * 1);
                progress?.Invoke(overallProgress / 100);
            }
            loadSceneList.Clear();
            progress?.Invoke(1);
            yield return null;
            callback?.Invoke();
        }
        IEnumerator EnumLoadAssetBundleWithDependenciesAsync(string assetBundleName)
        {
            if (string.IsNullOrEmpty(assetBundleName))
            {
                yield break;
            }
            if (bundleWarpperDict.TryGetValue(assetBundleName, out var bundleWarpper))
            {
                if (bundleWarpper.AssetBundle == null)
                {
                    var abPath = Path.Combine(PersistentPath, bundleWarpper.QuarkAssetBundle.AssetBundleKey);
                    var abReq = AssetBundle.LoadFromFileAsync(abPath, 0, QuarkDataProxy.QuarkEncryptionOffset);
                    yield return abReq;
                    bundleWarpper.AssetBundle = abReq.assetBundle;
                    if (bundleWarpper.AssetBundle != null)
                    {
                        bundleWarpper.ReferenceCount++;//AB包引用计数增加
                    }
                }
                else
                    bundleWarpper.ReferenceCount++;
                var dependentList = bundleWarpper.QuarkAssetBundle.DependentBundleKeyList;
                var dependentLength = dependentList.Count;
                for (int i = 0; i < dependentLength; i++)
                {
                    var dependentBundleKey= dependentList[i];
                    nameBundleKeyDict.TryGetValue(dependentBundleKey, out var dependentBundleName);
                    if (bundleWarpperDict.TryGetValue(dependentBundleName, out var dependentBundleWarpper))
                    {
                        if (dependentBundleWarpper.AssetBundle == null)
                        {
                            var abPath = Path.Combine(PersistentPath, dependentBundleKey);
                            var abReq = AssetBundle.LoadFromFileAsync(abPath, 0, QuarkDataProxy.QuarkEncryptionOffset);
                            yield return abReq;
                            dependentBundleWarpper.AssetBundle = abReq.assetBundle;
                            if (dependentBundleWarpper.AssetBundle != null)
                            {
                                dependentBundleWarpper.ReferenceCount++;//AB包引用计数增加
                            }
                        }
                        else
                            dependentBundleWarpper.ReferenceCount++;
                    }
                }
            }
        }
        /// <summary>
        /// 递归减少包体引用计数；
        /// </summary>
        void UnloadAssetBundleWithDependencies(QuarkBundleWarpper bundleWarpper, int count = 1, bool unloadAllLoadedObjects = false)
        {
            bundleWarpper.ReferenceCount -= count;
            if (bundleWarpper.ReferenceCount <= 0)
            {
                //当包体的引用计数小于等于0时，则表示该包已经未被依赖。
                //卸载AssetBundle；
                bundleWarpper.AssetBundle?.Unload(unloadAllLoadedObjects);
            }
            var dependentList = bundleWarpper.QuarkAssetBundle.DependentBundleKeyList;
            var dependentLength = dependentList.Count;
            //遍历查询依赖包
            for (int i = 0; i < dependentLength; i++)
            {
                var dependentBundleName = dependentList[i];
                if (!bundleWarpperDict.TryGetValue(dependentBundleName, out var dependentBundleWarpper))
                    continue;
                UnloadAssetBundleWithDependencies(dependentBundleWarpper, count, unloadAllLoadedObjects);
            }
        }
        void InitManifest(QuarkAssetManifest manifest)
        {
            var bundles = manifest.BundleInfoDict.Values; ;
            foreach (var bundle in bundles)
            {
                var objects = bundle.QuarkAssetBundle.QuarkObjects;
                foreach (var obj in objects)
                {
                    var quarkObject = obj;
                    QuarkObjectWapper wapper = new QuarkObjectWapper();
                    wapper.QuarkObject = quarkObject;
                    if (!objectLnkDict.TryGetValue(quarkObject.AssetName, out var lnkList))
                    {
                        lnkList = new LinkedList<QuarkObject>();
                        objectLnkDict.Add(quarkObject.AssetName, lnkList);
                    }
                    lnkList.AddLast(obj);
                    objectWarpperDict[obj.AssetPath] = wapper;//AssetPath与QuarkObject映射地址。理论上地址不可能重复。
                }
                var bundleName = bundle.QuarkAssetBundle.AssetBundleName;
                if (!bundleWarpperDict.ContainsKey(bundleName))
                {
                    var bundleWarpper = new QuarkBundleWarpper(bundle.QuarkAssetBundle);
                    bundleWarpperDict.Add(bundleName, bundleWarpper);
                }
                if (!nameBundleKeyDict.ContainsKey(bundle.QuarkAssetBundle.AssetBundleKey))
                {
                    nameBundleKeyDict.Add(bundle.QuarkAssetBundle.AssetBundleKey, bundleName);
                }
            }
            QuarkDataProxy.QuarkManifest = manifest;
        }
        void LoadAssetBundleWithDependencies(string assetBundleName)
        {
            if (bundleWarpperDict.TryGetValue(assetBundleName, out var bundleWarpper))
            {
                var assetBundle = bundleWarpper.AssetBundle;
                if (assetBundle == null)//判断当前warpper是否存在AssetBundle
                {
                    //同步加载AssetBundle
                    var abPath = Path.Combine(PersistentPath, bundleWarpper.QuarkAssetBundle.AssetBundleKey);
                    assetBundle = AssetBundle.LoadFromFile(abPath, 0, QuarkDataProxy.QuarkEncryptionOffset);
                    if (assetBundle != null)
                    {
                        bundleWarpper.AssetBundle = assetBundle;
                        bundleWarpper.ReferenceCount++;
                    }
                }
                else
                    bundleWarpper.ReferenceCount++;
                //这里开始加载依赖包
                var dependList = bundleWarpper.QuarkAssetBundle.DependentBundleKeyList;
                var dependLength = dependList.Count;
                for (int i = 0; i < dependLength; i++)
                {
                    //遍历依赖包
                    var dependBundleKey = dependList[i];
                    nameBundleKeyDict.TryGetValue(dependBundleKey, out var dependentBundleName);
                    if (bundleWarpperDict.TryGetValue(dependentBundleName, out var dependBundleWarpper))
                    {
                        var dependAssetBundle = dependBundleWarpper.AssetBundle;
                        if (dependAssetBundle == null)
                        {
                            var abPath = Path.Combine(PersistentPath, dependentBundleName);
                            dependAssetBundle = AssetBundle.LoadFromFile(abPath, 0, QuarkDataProxy.QuarkEncryptionOffset);
                            if (dependAssetBundle != null)
                            {
                                dependBundleWarpper.AssetBundle = dependAssetBundle;
                                dependBundleWarpper.ReferenceCount++;//AB包引用计数增加
                            }
                        }
                        else
                            dependBundleWarpper.ReferenceCount++;
                    }
                }
            }
        }
        void OnResoucreObjectRelease(QuarkObjectWapper objectWarpper)
        {
            var count = objectWarpper.ReferenceCount;
            objectWarpper.ReferenceCount = 0;
            if (bundleWarpperDict.TryGetValue(objectWarpper.QuarkObject.AssetName, out var bundleWarpper))
            {
                UnloadAssetBundleWithDependencies(bundleWarpper, count);
            }
        }
        /// <summary>
        /// 当场景被加载；
        /// </summary>
        void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            var sceneName = scene.name;
            if (loadSceneList.Contains(sceneName))
            {
                var hasObject = GetSceneObject(sceneName, out var quarkObject);
                if (hasObject)
                    OnResourceObjectLoad(quarkObject);
                loadedSceneDict[sceneName] = scene;
            }
        }
        /// <summary>
        /// 当场景被卸载；
        /// </summary>
        void OnSceneUnloaded(Scene scene)
        {
            var sceneName = scene.name;
            if (loadSceneList.Remove(sceneName))
            {
                var hasObject = GetSceneObject(sceneName, out var quarkObject);
                if (hasObject)
                    OnResourceObjectUnload(quarkObject);
            }
            loadedSceneDict.Remove(sceneName);
        }
        /// <summary>
        /// 只负责计算引用计数
        /// </summary>ram>
        void OnResourceObjectLoad(QuarkObject quarkObject)
        {
            if (!objectWarpperDict.TryGetValue(quarkObject.AssetPath, out var resourceObjectWarpper))
                return;
            resourceObjectWarpper.ReferenceCount++;
        }
        /// <summary>
        /// 当资源包中的所有对象被加载；
        /// </summary>
        void OnResourceBundleAllAssetLoad(string bundleName)
        {
            if (!bundleWarpperDict.TryGetValue(bundleName, out var bundleWarpper))
                return;
            var ResourceObjectList = bundleWarpper.QuarkAssetBundle.QuarkObjects;
            foreach (var resourceObject in ResourceObjectList)
            {
                OnResourceObjectLoad(resourceObject);
            }
        }
        /// <summary>
        /// 当资源对象被卸载；
        /// </summary>
        void OnResourceObjectUnload(QuarkObject quarkObject)
        {
            if (!objectWarpperDict.TryGetValue(quarkObject.AssetPath, out var resourceObjectWarpper))
                return;
            if (!bundleWarpperDict.TryGetValue(resourceObjectWarpper.QuarkObject.AssetBundleName, out var resourceBundleWarpper))
                return;
            resourceObjectWarpper.ReferenceCount--;
            UnloadAssetBundleWithDependencies(resourceBundleWarpper);
        }


    }
}
