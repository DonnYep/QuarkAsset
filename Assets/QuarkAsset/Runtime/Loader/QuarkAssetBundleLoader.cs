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

        readonly Dictionary<string, AssetBundleCreateRequest> abCreateReqDict = new Dictionary<string, AssetBundleCreateRequest>();
        internal QuarkAssetBundleLoader()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        public void SetMergedManifest(QuarkMergedManifest mergedManifest)
        {
            InitMergedManifest(mergedManifest);
        }
        public void SetManifest(QuarkManifest manifest)
        {
            InitManifest(manifest);
        }
        public override T LoadAsset<T>(string assetName)
        {
            T asset = null;
            var hasObject = GetQuarkObject(assetName, out var quarkObject);
            if (!hasObject)
                return null;
            var assetBundleName = quarkObject.BundleName;
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
            var assetBundleName = quarkObject.BundleName;
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
        public override GameObject LoadPrefab(string assetName, bool instantiate)
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
            var assetBundleName = quarkObject.BundleName;
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
            var assetBundleName = quarkObject.BundleName;
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
        public override Coroutine LoadPrefabAsync(string assetName, Action<GameObject> callback, bool instantiate)
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
                if (objectWarpperDict.TryGetValue(quarkObject.ObjectPath, out var objectWapper))
                    OnResoucreObjectRelease(objectWapper);
            }
        }
        public override void UnloadAsset(string assetName)
        {
            var hasObject = GetQuarkObject(assetName, out var quarkObject);
            if (hasObject)
                OnResourceObjectUnload(quarkObject);
        }
        public override void UnloadAllAssetBundle(bool unloadAllLoadedObjects = true)
        {
            //这里是卸载，保留寻址信息，清空引用计数
            foreach (var objectWarpper in objectWarpperDict.Values)
            {
                objectWarpper.ReferenceCount = 0;
            }
            foreach (var bundleWarpper in bundleWarpperDict.Values)
            {
                bundleWarpper.ReferenceCount = 0;
                if (bundleWarpper.AssetBundle != null)
                {
                    bundleWarpper.AssetBundle?.Unload(unloadAllLoadedObjects);
                    bundleWarpper.AssetBundle = null;
                }
            }
        }
        public override void UnloadAssetBundle(string assetBundleName, bool unloadAllLoadedObjects = true)
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
        public override void ResetLoader()
        {
            foreach (var bundleWarpper in bundleWarpperDict.Values)
            {
                if (bundleWarpper.AssetBundle != null)
                {
                    bundleWarpper.AssetBundle?.Unload(true);
                    bundleWarpper.AssetBundle = null;
                }
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
            var assetBundleName = quarkObject.BundleName;
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
            var assetBundleName = quarkObject.BundleName;
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
            yield return EnumLoadAssetBundleWithDependenciesAsync(quarkObject.BundleName);
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
            yield return operation.isDone;
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
            var loadSceneArray = loadSceneList.ToArray();
            foreach (var sceneName in loadSceneArray)
            {
                var overallIndexPercent = 100 * ((float)currentSceneIndex / sceneCount);
                currentSceneIndex++;
                var operation = SceneManager.UnloadSceneAsync(sceneName);
                if (operation != null)
                {
                    while (!operation.isDone)
                    {
                        overallProgress = overallIndexPercent + (unitResRatio * operation.progress);
                        progress?.Invoke(overallProgress / 100);
                        yield return null;
                    }
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
                    var abPath = Path.Combine(bundleWarpper.BundlePersistentPath, bundleWarpper.QuarkAssetBundle.BundleKey);
                    //判断是否正在加载
                    if (abCreateReqDict.TryGetValue(abPath, out var req))
                    {
                        yield return new WaitUntil(() => { return req.isDone; });
                    }
                    else
                    {
                        var abReq = AssetBundle.LoadFromFileAsync(abPath, 0, QuarkDataProxy.QuarkEncryptionOffset);
                        abCreateReqDict.Add(abPath, abReq);
                        req = abReq;

                        yield return abReq;
                    }
                    abCreateReqDict.Remove(abPath);
                    bundleWarpper.AssetBundle = req.assetBundle;

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
                    var dependentBundleKey = dependentList[i];
                    nameBundleKeyDict.TryGetValue(dependentBundleKey.BundleKey, out var dependentBundleName);
                    if (string.IsNullOrEmpty(dependentBundleName))
                        continue;
                    if (bundleWarpperDict.TryGetValue(dependentBundleName, out var dependentBundleWarpper))
                    {
                        if (dependentBundleWarpper.AssetBundle == null)
                        {
                            var abPath = Path.Combine(dependentBundleWarpper.BundlePersistentPath, dependentBundleKey.BundleKey);
                            var assetBundle = AssetBundle.LoadFromFile(abPath, 0, QuarkDataProxy.QuarkEncryptionOffset);
                            dependentBundleWarpper.AssetBundle = assetBundle;
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
        protected override void UnloadAssetBundleWithDependencies(QuarkBundleWarpper bundleWarpper, int count = 1, bool unloadAllLoadedObjects = true)
        {
            bundleWarpper.ReferenceCount -= count;
            if (bundleWarpper.ReferenceCount <= 0)
            {
                //当包体的引用计数小于等于0时，则表示该包已经未被依赖。
                //卸载AssetBundle；
                if (bundleWarpper.AssetBundle != null)
                {
                    bundleWarpper.AssetBundle.Unload(unloadAllLoadedObjects);
                    bundleWarpper.AssetBundle = null;
                }
            }
            var dependentList = bundleWarpper.QuarkAssetBundle.DependentBundleKeyList;
            var dependentLength = dependentList.Count;
            //遍历查询依赖包
            for (int i = 0; i < dependentLength; i++)
            {
                var dependBundleKey = dependentList[i];
                nameBundleKeyDict.TryGetValue(dependBundleKey.BundleKey, out var dependentBundleName);
                if (string.IsNullOrEmpty(dependentBundleName))
                    continue;
                if (bundleWarpperDict.TryGetValue(dependentBundleName, out var dependentBundleWarpper))
                {
                    dependentBundleWarpper.ReferenceCount -= count;
                    if (dependentBundleWarpper.ReferenceCount <= 0)
                    {
                        if (dependentBundleWarpper.AssetBundle != null)
                        {
                            dependentBundleWarpper.AssetBundle.Unload(unloadAllLoadedObjects);
                            dependentBundleWarpper.AssetBundle = null;
                        }
                    }
                }
            }
        }
        void InitManifest(QuarkManifest manifest)
        {
            var bundles = manifest.BundleInfoDict.Values;
            foreach (var bundle in bundles)
            {
                var objects = bundle.QuarkAssetBundle.ObjectList;
                foreach (var obj in objects)
                {
                    var quarkObject = obj;
                    QuarkObjectWapper wapper = new QuarkObjectWapper();
                    wapper.QuarkObject = quarkObject;
                    if (!objectLnkDict.TryGetValue(quarkObject.ObjectName, out var lnkList))
                    {
                        lnkList = new LinkedList<QuarkObject>();
                        objectLnkDict.Add(quarkObject.ObjectName, lnkList);
                    }
                    lnkList.AddLast(obj);
                    objectWarpperDict[obj.ObjectPath] = wapper;//AssetPath与QuarkObject映射地址。理论上地址不可能重复。
                }
                var bundleName = bundle.QuarkAssetBundle.BundleName;
                if (!bundleWarpperDict.ContainsKey(bundleName))
                {
                    var bundleWarpper = new QuarkBundleWarpper(bundle.QuarkAssetBundle, QuarkDataProxy.PersistentPath);
                    bundleWarpperDict.Add(bundleName, bundleWarpper);
                }
                if (!nameBundleKeyDict.ContainsKey(bundle.QuarkAssetBundle.BundleKey))
                {
                    nameBundleKeyDict.Add(bundle.QuarkAssetBundle.BundleKey, bundleName);
                }
            }
            //QuarkDataProxy.QuarkManifest = manifest;
            QuarkDataProxy.BuildVersion = manifest.BuildVersion;
            QuarkDataProxy.InternalBuildVersion = manifest.InternalBuildVersion;
        }
        void InitMergedManifest(QuarkMergedManifest mergedManifest)
        {
            var bundles = mergedManifest.MergedBundles;
            foreach (var bundle in bundles)
            {
                var objects = bundle.QuarkBundleAsset.QuarkAssetBundle.ObjectList;
                foreach (var obj in objects)
                {
                    var quarkObject = obj;
                    QuarkObjectWapper wapper = new QuarkObjectWapper();
                    wapper.QuarkObject = quarkObject;
                    if (!objectLnkDict.TryGetValue(quarkObject.ObjectName, out var lnkList))
                    {
                        lnkList = new LinkedList<QuarkObject>();
                        objectLnkDict.Add(quarkObject.ObjectName, lnkList);
                    }
                    lnkList.AddLast(obj);
                    objectWarpperDict[obj.ObjectPath] = wapper;//AssetPath与QuarkObject映射地址。理论上地址不可能重复。
                }
                var bundleName = bundle.QuarkBundleAsset.QuarkAssetBundle.BundleName;
                if (!bundleWarpperDict.ContainsKey(bundleName))
                {
                    QuarkBundleWarpper bundleWarpper = null;
                    if (bundle.IsIncremental)
                    {
                        bundleWarpper = new QuarkBundleWarpper(bundle.QuarkBundleAsset.QuarkAssetBundle, QuarkDataProxy.DiffPersistentPath);
                    }
                    else
                    {
                        bundleWarpper = new QuarkBundleWarpper(bundle.QuarkBundleAsset.QuarkAssetBundle, QuarkDataProxy.PersistentPath);
                    }
                    bundleWarpperDict.Add(bundleName, bundleWarpper);
                }
                if (!nameBundleKeyDict.ContainsKey(bundle.QuarkBundleAsset.QuarkAssetBundle.BundleKey))
                {
                    nameBundleKeyDict.Add(bundle.QuarkBundleAsset.QuarkAssetBundle.BundleKey, bundleName);
                }
            }
            QuarkDataProxy.BuildVersion = mergedManifest.BuildVersion;
            QuarkDataProxy.InternalBuildVersion = mergedManifest.InternalBuildVersion;
        }
        void LoadAssetBundleWithDependencies(string assetBundleName)
        {
            if (bundleWarpperDict.TryGetValue(assetBundleName, out var bundleWarpper))
            {
                var assetBundle = bundleWarpper.AssetBundle;
                if (assetBundle == null)//判断当前warpper是否存在AssetBundle
                {
                    //同步加载AssetBundle
                    var abPath = Path.Combine(bundleWarpper.BundlePersistentPath, bundleWarpper.QuarkAssetBundle.BundleKey);
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
                    nameBundleKeyDict.TryGetValue(dependBundleKey.BundleKey, out var dependentBundleName);
                    if (string.IsNullOrEmpty(dependentBundleName))
                        continue;
                    if (bundleWarpperDict.TryGetValue(dependentBundleName, out var dependBundleWarpper))
                    {
                        var dependAssetBundle = dependBundleWarpper.AssetBundle;
                        if (dependAssetBundle == null)
                        {
                            var abPath = Path.Combine(dependBundleWarpper.BundlePersistentPath, dependBundleKey.BundleKey);
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
            if (bundleWarpperDict.TryGetValue(objectWarpper.QuarkObject.ObjectName, out var bundleWarpper))
            {
                UnloadAssetBundleWithDependencies(bundleWarpper, count);
            }
        }
        /// <summary>
        /// 当资源包中的所有对象被加载；
        /// </summary>
        void OnResourceBundleAllAssetLoad(string bundleName)
        {
            if (!bundleWarpperDict.TryGetValue(bundleName, out var bundleWarpper))
                return;
            var ResourceObjectList = bundleWarpper.QuarkAssetBundle.ObjectList;
            foreach (var resourceObject in ResourceObjectList)
            {
                OnResourceObjectLoad(resourceObject);
            }
        }
    }
}
