using Quark.Asset;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
namespace Quark.Loader
{
    internal class QuarkAssetDatabaseLoader : QuarkAssetLoader
    {
        /// <summary>
        /// bundleKey===bundleName
        /// </summary>
        readonly Dictionary<string, string> nameBundleKeyDict = new Dictionary<string, string>();
        public QuarkAssetDatabaseLoader()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        public void SetDataset(QuarkDataset dataset)
        {
            InitDataset(dataset);
        }
        public override T LoadAsset<T>(string assetName)
        {
            if (string.IsNullOrEmpty(assetName))
                throw new ArgumentNullException("Asset name is invalid!");
            T asset = null;
#if UNITY_EDITOR
            var hasObject = GetQuarkObject(assetName, out var quarkObject);
            if (hasObject)
            {
                asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(quarkObject.ObjectPath);
                if (asset != null)
                    OnResourceObjectLoad(quarkObject);
            }
#endif
            return asset;
        }
        public override Object LoadAsset(string assetName, Type type)
        {
            if (string.IsNullOrEmpty(assetName))
                throw new ArgumentNullException("Asset name is invalid!");
            Object asset = null;
#if UNITY_EDITOR
            var hasObject = GetQuarkObject(assetName, out var quarkObject);
            if (hasObject)
            {
                asset = UnityEditor.AssetDatabase.LoadAssetAtPath(quarkObject.ObjectPath, type);
                if (asset != null)
                    OnResourceObjectLoad(quarkObject);
            }
#endif
            return asset;
        }
        public override GameObject LoadPrefab(string assetName, bool instantiate)
        {
            var resGGo = LoadAsset<GameObject>(assetName);
            if (instantiate)
                return GameObject.Instantiate(resGGo);
            else
                return resGGo;
        }
        public override T[] LoadMainAndSubAssets<T>(string assetName)
        {
            if (string.IsNullOrEmpty(assetName))
                throw new ArgumentNullException("Asset name is invalid!");
            T[] assets = null;
#if UNITY_EDITOR
            var hasObject = GetQuarkObject(assetName, out var quarkObject);
            if (hasObject)
            {
                var assetObj = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(quarkObject.ObjectPath);
                var length = assetObj.Length;
                assets = new T[length];
                for (int i = 0; i < length; i++)
                {
                    assets[i] = assetObj[i] as T;
                }
                if (assetObj != null)
                    OnResourceObjectLoad(quarkObject);
            }
#endif
            return assets;
        }
        public override Object[] LoadMainAndSubAssets(string assetName, Type type)
        {
            if (string.IsNullOrEmpty(assetName))
                throw new ArgumentNullException("Asset name is invalid!");
            Object[] assets = null;
#if UNITY_EDITOR
            var hasObject = GetQuarkObject(assetName, out var quarkObject);
            if (hasObject)
            {
                var assetObj = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(quarkObject.ObjectPath);
                var length = assetObj.Length;
                assets = new Object[length];
                for (int i = 0; i < length; i++)
                {
                    assets[i] = assetObj[i];
                }
                if (assetObj != null)
                    OnResourceObjectLoad(quarkObject);
                return assets;
            }
#endif
            return assets;
        }
        public override Object[] LoadAllAssets(string assetBundleName)
        {
            if (string.IsNullOrEmpty(assetBundleName))
                return null;
            List<Object> assetList = new List<Object>();
#if UNITY_EDITOR
            LoadAssetBundleWithDepend(assetBundleName);
            if (bundleWarpperDict.TryGetValue(assetBundleName, out var bundleWarpper))
            {
                var bundle = bundleWarpper.QuarkAssetBundle;
                var warppers = bundle.ObjectList;
                foreach (var w in warppers)
                {
                    var asset = UnityEditor.AssetDatabase.LoadAssetAtPath(w.ObjectPath, typeof(Object));
                    assetList.Add(asset);
                }
            }
            OnResourceBundleAllAssetLoad(assetBundleName);
#endif
            return assetList.ToArray();
        }
        public override Coroutine LoadPrefabAsync(string assetName, Action<GameObject> callback, bool instantiate)
        {
            return QuarkUtility.Unity.StartCoroutine(EnumLoadAssetAsync(assetName, typeof(GameObject), (resGo) =>
            {
                var gameobject = resGo as GameObject;
                if (instantiate)
                {
                    var go = GameObject.Instantiate(gameobject);
                    callback.Invoke(go);
                }
                else
                {
                    callback.Invoke(gameobject);
                }
            }));
        }
        public override Coroutine LoadSceneAsync(string sceneName, Func<float> progressProvider, Action<float> progress, Func<bool> condition, Action callback, bool additive = false)
        {
            return QuarkUtility.Unity.StartCoroutine(EnumLoadSceneAsync(sceneName, progressProvider, progress, condition, callback, additive));
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
        public override Coroutine LoadAssetAsync<T>(string assetName, Action<T> callback)
        {
            return QuarkUtility.Unity.StartCoroutine(EnumLoadAssetAsync(assetName, typeof(T), asset => { callback?.Invoke(asset as T); }));
        }
        public override Coroutine LoadAssetAsync(string assetName, Type type, Action<Object> callback)
        {
            return QuarkUtility.Unity.StartCoroutine(EnumLoadAssetAsync(assetName, type, callback));
        }
        public override Coroutine LoadAllAssetAsync(string assetBundleName, Action<Object[]> callback)
        {
            return QuarkUtility.Unity.StartCoroutine(EnumLoadAllAssetAsync(assetBundleName, callback));
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
            loadedSceneDict.Clear();
            loadSceneList.Clear();
            bundleWarpperDict.Clear();
            objectWarpperDict.Clear();
            objectLnkDict.Clear();
            nameBundleKeyDict.Clear();
        }
        IEnumerator EnumLoadAssetWithSubAssetsAsync(string assetName, Type type, Action<Object[]> callback)
        {
            var assets = LoadMainAndSubAssets(assetName, type);
            yield return null;
            callback?.Invoke(assets);
        }
        IEnumerator EnumLoadAssetAsync(string assetName, Type type, Action<Object> callback)
        {
            var asset = LoadAsset(assetName, type);
            yield return null;
            callback?.Invoke(asset);
        }
        IEnumerator EnumLoadAllAssetAsync(string assetBundleName, Action<Object[]> callback)
        {
            if (string.IsNullOrEmpty(assetBundleName))
                yield break;
            List<Object> assetList = new List<Object>();
# if UNITY_EDITOR
            yield return EnumLoadAssetBundleWithDependenciesAsync(assetBundleName);
            if (bundleWarpperDict.TryGetValue(assetBundleName, out var bundleWarpper))
            {
                var bundle = bundleWarpper.QuarkAssetBundle;
                var warppers = bundle.ObjectList;
                foreach (var w in warppers)
                {
                    var asset = UnityEditor.AssetDatabase.LoadAssetAtPath(w.ObjectPath, typeof(Object));
                    assetList.Add(asset);
                }
            }
            OnResourceBundleAllAssetLoad(assetBundleName);
#endif
            callback?.Invoke(assetList.ToArray());
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
            LoadSceneMode loadSceneMode = additive == true ? LoadSceneMode.Additive : LoadSceneMode.Single;
            AsyncOperation operation = null;
#if UNITY_EDITOR
            operation = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(quarkObject.ObjectPath, new LoadSceneParameters(loadSceneMode));
#else
            operation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
#endif
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
                    progress?.Invoke(sum / 2);
                    if (sum >= 1.9)
                        break;
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
        IEnumerator EnumLoadAssetBundleWithDependenciesAsync(string bundleName)
        {
            var hasBundle = bundleWarpperDict.TryGetValue(bundleName, out var bundleWarpper);
            if (!hasBundle)
                yield break; //若bundle信息为空，则终止；
            bundleWarpper.ReferenceCount++; //AB包引用计数增加
            var dependList = bundleWarpper.QuarkAssetBundle.DependentBundleKeyList;
            var length = dependList.Count;
            for (int i = 0; i < length; i++)
            {
                var dependentBundleKey = dependList[i];
                nameBundleKeyDict.TryGetValue(dependentBundleKey.BundleKey, out var dependentBundleName);
                if (string.IsNullOrEmpty(dependentBundleName))
                    continue;
                if (bundleWarpperDict.TryGetValue(dependentBundleName, out var dependBundleWarpper))
                {
                    dependBundleWarpper.ReferenceCount++; //AB包引用计数增加
                }
            }
        }
        /// <summary>
        /// 递归减少包体引用计数；
        /// </summary>
        /// <param name="bundleWarpper">资源包的壳</param>
        /// <param name="count">减少的数量</param>
        protected override void UnloadAssetBundleWithDependencies(QuarkBundleWarpper bundleWarpper, int count = 1, bool unloadAllLoadedObjects = true)
        {
            bundleWarpper.ReferenceCount -= count;
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
                }
            }
        }
        void InitDataset(QuarkDataset assetDataset)
        {
            var bundleInfos = assetDataset.QuarkBundleInfoList;
            foreach (var bundleInfo in bundleInfos)
            {
                var objectInfos = bundleInfo.ObjectInfoList;
                var bundle = new QuarkBundle()
                {
                    BundleKey = bundleInfo.BundleKey,
                    BundleName = bundleInfo.BundleName,
                    BundlePath = bundleInfo.BundlePath
                };
                bundle.DependentBundleKeyList.AddRange(bundleInfo.DependentBundleKeyList);
                foreach (var objectInfo in objectInfos)
                {
                    var quarkObject = new QuarkObject()
                    {
                        BundleName = objectInfo.BundleName,
                        ObjectExtension = objectInfo.ObjectExtension,
                        ObjectName = objectInfo.ObjectName,
                        ObjectPath = objectInfo.ObjectPath,
                        ObjectType = objectInfo.ObjectType
                    };
                    bundle.ObjectList.Add(quarkObject);
                    QuarkObjectWapper wapper = new QuarkObjectWapper();
                    wapper.QuarkObject = quarkObject;

                    if (!objectLnkDict.TryGetValue(quarkObject.ObjectName, out var lnkList))
                    {
                        lnkList = new LinkedList<QuarkObject>();
                        objectLnkDict.Add(quarkObject.ObjectName, lnkList);
                    }
                    lnkList.AddLast(quarkObject);
                    objectWarpperDict[objectInfo.ObjectPath] = wapper;//AssetPath与QuarkObject映射地址。理论上地址不可能重复。
                }
                var bundleName = bundleInfo.BundleName;
                if (!bundleWarpperDict.ContainsKey(bundleName))
                {
                    var bundleWarpper = new QuarkBundleWarpper(bundle, string.Empty);
                    bundleWarpperDict.Add(bundleName, bundleWarpper);
                }
                if (!nameBundleKeyDict.ContainsKey(bundleInfo.BundleKey))
                {
                    nameBundleKeyDict.Add(bundleInfo.BundleKey, bundleName);
                }
            }
        }
        void LoadAssetBundleWithDepend(string assetBundleName)
        {
            var hasBundle = bundleWarpperDict.TryGetValue(assetBundleName, out var bundleWarpper);
            if (!hasBundle)
                return; //若bundle信息为空，则终止；
            bundleWarpper.ReferenceCount++; //AB包引用计数增加
            var dependList = bundleWarpper.QuarkAssetBundle.DependentBundleKeyList;
            var length = dependList.Count;
            for (int i = 0; i < length; i++)
            {
                var dependentBundleKey = dependList[i];
                nameBundleKeyDict.TryGetValue(dependentBundleKey.BundleKey, out var dependentBundleName);
                if (string.IsNullOrEmpty(dependentBundleName))
                    continue;
                if (bundleWarpperDict.TryGetValue(dependentBundleName, out var dependBundleWarpper))
                {
                    dependBundleWarpper.ReferenceCount++;
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
