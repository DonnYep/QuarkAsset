﻿using Quark.Asset;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
namespace Quark.Loader
{
    /// <summary>
    /// Runtime加载时的方案；
    /// <see cref="QuarkLoadMode"/>
    /// </summary>
    internal abstract class QuarkAssetLoader
    {
        /// <summary>
        /// BundleName===QuarkBundleWarpper
        /// </summary>
        protected Dictionary<string, QuarkBundleWarpper> bundleWarpperDict
            = new Dictionary<string, QuarkBundleWarpper>();
        /// <summary>
        /// AssetName===Lnk[QuarkObjectWapper]
        /// </summary>
        protected Dictionary<string, LinkedList<QuarkObject>> objectLnkDict
            = new Dictionary<string, LinkedList<QuarkObject>>();
        /// <summary>
        /// AssetPath===QuarkObjectWapper
        /// </summary>
        protected Dictionary<string, QuarkObjectWapper> objectWarpperDict
            = new Dictionary<string, QuarkObjectWapper>();
        /// <summary>
        /// 被加载的场景字典；
        /// SceneName===Scene
        /// </summary>
        protected Dictionary<string, Scene> loadedSceneDict = new Dictionary<string, Scene>();
        /// <summary>
        /// 主动加载的场景列表；
        /// </summary>
        protected List<string> loadSceneList = new List<string>();
        public abstract void SetLoaderData(IQuarkLoaderData loaderData);
        public abstract T LoadAsset<T>(string assetName) where T : Object;
        public abstract Object LoadAsset(string assetName, Type type);
        public abstract GameObject LoadPrefab(string assetName, bool instantiate = false);
        public abstract T[] LoadMainAndSubAssets<T>(string assetName) where T : Object;
        public abstract Object[] LoadMainAndSubAssets(string assetName, Type type);
        public abstract Object[] LoadAllAssets(string assetBundleName);
        public abstract Coroutine LoadAssetAsync<T>(string assetName, Action<T> callback) where T : Object;
        public abstract Coroutine LoadAssetAsync(string assetName, Type type, Action<Object> callback);
        public abstract Coroutine LoadPrefabAsync(string assetName, Action<GameObject> callback, bool instantiate = false);
        public abstract Coroutine LoadMainAndSubAssetsAsync<T>(string assetName, Action<T[]> callback) where T : Object;
        public abstract Coroutine LoadMainAndSubAssetsAsync(string assetName, Type type, Action<Object[]> callback);
        public abstract Coroutine LoadAllAssetAsync(string assetBundleName, Action<Object[]> callback);
        public abstract Coroutine LoadSceneAsync(string sceneName, Func<float> progressProvider, Action<float> progress, Func<bool> condition, Action callback, bool additive = false);
        public abstract void ReleaseAsset(string assetName);
        public abstract void UnloadAsset(string assetName);
        public abstract void UnloadAssetBundle(string assetBundleName, bool unloadAllLoadedObjects = true);
        public abstract void UnloadAllAssetBundle(bool unloadAllLoadedObjects = true);
        public abstract Coroutine UnloadSceneAsync(string sceneName, Action<float> progress, Action callback);
        public abstract Coroutine UnloadAllSceneAsync(Action<float> progress, Action callback);
        public abstract void ClearLoader();
        public bool GetInfo(string assetName, Type type, out QuarkAssetObjectInfo info)
        {
            info = QuarkAssetObjectInfo.None;
            var hasWapper = GetQuarkObject(assetName, type, out var wapper);
            if (hasWapper)
            {
                var referenceCount = objectWarpperDict[wapper.AssetPath].ReferenceCount;
                info = QuarkAssetObjectInfo.Create(wapper.AssetName, wapper.AssetPath, wapper.AssetBundleName, wapper.AssetExtension, wapper.AssetType, referenceCount);
                return true;
            }
            return false;
        }
        public bool GetInfo(string assetName, out QuarkAssetObjectInfo info)
        {
            info = QuarkAssetObjectInfo.None;
            var hasWapper = GetQuarkObject(assetName, out var wapper);
            if (hasWapper)
            {
                var referenceCount = objectWarpperDict[wapper.AssetPath].ReferenceCount;
                info = QuarkAssetObjectInfo.Create(wapper.AssetName, wapper.AssetPath, wapper.AssetBundleName, wapper.AssetExtension, wapper.AssetType, referenceCount);
                return true;
            }
            return false;
        }
        public QuarkAssetObjectInfo[] GetAllLoadedInfos()
        {
            QuarkAssetObjectInfo[] quarkAssetObjectInfos = new QuarkAssetObjectInfo[objectWarpperDict.Count];
            int idx = 0;
            foreach (var objectWarpper in objectWarpperDict.Values)
            {
                var obj = objectWarpper.QuarkObject;
                var info = QuarkAssetObjectInfo.Create(obj.AssetName, obj.AssetPath, obj.AssetBundleName, obj.AssetExtension, obj.AssetType, objectWarpper.ReferenceCount);
                quarkAssetObjectInfos[idx] = info;
                idx++;
            }
            return quarkAssetObjectInfos;
        }
        protected bool GetQuarkObject(string assetName, Type type, out QuarkObject quarkObject)
        {
            quarkObject = null;
            if (assetName.StartsWith("Assets/"))
            {
                var hasObjectWapper = objectWarpperDict.TryGetValue(assetName, out var objectWapper);
                if (hasObjectWapper)
                    quarkObject = objectWapper.QuarkObject;
                return hasObjectWapper;
            }
            var typeString = type.ToString();
            var ext = Path.GetExtension(assetName);
            if (string.IsNullOrEmpty(ext))
            {
                if (objectLnkDict.TryGetValue(assetName, out var abLnk))
                    quarkObject = abLnk.First.Value;
            }
            else
            {
                var lowerExt = ext.ToLower();
                var nameWithoutExt = Path.GetFileNameWithoutExtension(assetName);
                if (objectLnkDict.TryGetValue(nameWithoutExt, out var lnk))
                {
                    foreach (var qObject in lnk)
                    {
                        if (qObject.AssetExtension == lowerExt && qObject.AssetType == typeString)
                        {
                            quarkObject = qObject;
                            break;
                        }
                    }
                }
            }
            return quarkObject != null;
        }
        protected bool GetQuarkObject(string assetName, out QuarkObject quarkObject)
        {
            quarkObject = null;
            if (assetName.StartsWith("Assets/"))
            {
                var hasObjectWapper = objectWarpperDict.TryGetValue(assetName, out var objectWapper);
                if (hasObjectWapper)
                    quarkObject = objectWapper.QuarkObject;
                return hasObjectWapper;
            }
            var ext = Path.GetExtension(assetName);
            if (string.IsNullOrEmpty(ext))
            {
                if (objectLnkDict.TryGetValue(assetName, out var abLnk))
                    quarkObject = abLnk.First.Value;
            }
            else
            {
                var lowerExt = ext.ToLower();
                var nameWithoutExt = Path.GetFileNameWithoutExtension(assetName);
                if (objectLnkDict.TryGetValue(nameWithoutExt, out var lnk))
                {
                    foreach (var qObject in lnk)
                    {
                        if (qObject.AssetExtension == lowerExt)
                        {
                            quarkObject = qObject;
                            break;
                        }
                    }
                }
            }
            return quarkObject != null;
        }
        protected bool GetSceneObject(string assetName, out QuarkObject quarkObject)
        {
            quarkObject = null;
            if (objectLnkDict.TryGetValue(assetName, out var abLnk))
            {
                foreach (var quarkObj in abLnk)
                {
                    if (quarkObj.AssetExtension == ".unity")
                    {
                        quarkObject = quarkObj;
                        break;
                    }
                }
            }
            return quarkObject != null;
        }
        protected IEnumerator EnumUnloadSceneAsync(string sceneName, Action<float> progress, Action callback)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                QuarkUtility.LogError("Scene name is invalid!");
                progress?.Invoke(1);
                callback?.Invoke();
                yield break;
            }
            if (!loadedSceneDict.TryGetValue(sceneName, out var scene))
            {
                QuarkUtility.LogError($"Unload scene failure： {sceneName}  not loaded yet !");
                progress?.Invoke(1);
                callback?.Invoke();
                yield break;
            }
            var hasWapper = GetSceneObject(sceneName, out var wapper);
            if (!hasWapper)
            {
                QuarkUtility.LogError($"Scene：{sceneName}.unity not existed !");
                progress?.Invoke(1);
                callback?.Invoke();
                yield break;
            }
            var ao = SceneManager.UnloadSceneAsync(scene);
            if (ao != null)
            {
                while (!ao.isDone)
                {
                    progress?.Invoke(ao.progress);
                    yield return null;
                }
            }
            loadedSceneDict.Remove(sceneName);
            progress?.Invoke(1);
            callback?.Invoke();
        }
    }
}
