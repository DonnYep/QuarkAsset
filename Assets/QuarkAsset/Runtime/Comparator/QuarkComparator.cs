using Quark.Asset;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
namespace Quark.Networking
{
    /// <summary>
    /// Quark Manifest比较器；
    /// </summary>
    public class QuarkComparator
    {
        /// <summary>
        /// 比较失败，传入ErrorMessage；
        /// </summary>
        Action<string> onCompareFailure;
        /// <summary>
        /// Latest===Expired==OverallSize(需要下载的大小)
        /// </summary>
        Action<string[], string[], long> onCompareSuccess;
        //名字相同，但是HASH不同，则认为资源有作修改，需要加入到最新队列中；
        List<string> latest = new List<string>();
        //本地有但是远程没有，则标记为可过期文件；
        List<string> expired = new List<string>();
        /// <summary>
        /// 本地持久化路径；
        /// </summary>
        public string PersistentPath { get { return QuarkDataProxy.PersistentPath; } }
        /// <summary>
        /// 远程资源地址；
        /// </summary>
        public string URL { get { return QuarkDataProxy.URL; } }
        bool isEncrypted { get { return QuarkDataProxy.QuarkAESEncryptionKey.Length > 0; } }
        QuarkAssetManifest localManifest = null;
        QuarkAssetManifest remoteManifest = null;
        public void Initiate(Action<string[], string[], long> onCompareSuccess, Action<string> onCompareFailure)
        {
            this.onCompareSuccess = onCompareSuccess;
            this.onCompareFailure = onCompareFailure;
        }
        /// <summary>
        /// 检查更新；
        /// 比较remote与local的manifest文件；
        /// </summary>
        public Coroutine RequestMainifestFromURLAsync()
        {
            var uriManifestPath = Path.Combine(URL, QuarkConstant.ManifestName);
            return QuarkUtility.Unity.StartCoroutine(EnumRequestManifestFromURL(uriManifestPath));
        }
        public Coroutine RequestManifestFromStreamingAssetAsync()
        {
            var localManifestPath = Path.Combine(URL, QuarkConstant.ManifestName);
            return QuarkUtility.Unity.StartCoroutine(EnumRequestManifestFromStreamingAsset(localManifestPath));
        }
        public void Clear()
        {
            latest.Clear();
            expired.Clear();
            onCompareSuccess = null;
            onCompareFailure = null;
        }
        IEnumerator EnumRequestManifestFromURL(string uri)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(uri))
            {
                yield return request.SendWebRequest();
#if UNITY_2020_1_OR_NEWER
                if (request.result != UnityWebRequest.Result.ConnectionError && request.result != UnityWebRequest.Result.ProtocolError)
#elif UNITY_2018_1_OR_NEWER
                if (!request.isNetworkError && !request.isHttpError)
#endif
                {
                    if (request.isDone)
                    {
                        OnUriManifestSuccess(request.downloadHandler.text);
                    }
                }
                else
                {
                    onCompareFailure?.Invoke(request.error);
                }
            }
        }
        IEnumerator EnumRequestManifestFromStreamingAsset(string manifestUri)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(manifestUri))
            {
                yield return request.SendWebRequest();
#if UNITY_2020_1_OR_NEWER
                if (request.result != UnityWebRequest.Result.ConnectionError && request.result != UnityWebRequest.Result.ProtocolError)
#elif UNITY_2018_1_OR_NEWER
                if (!request.isNetworkError && !request.isHttpError)
#endif
                {
                    if (request.isDone)
                    {
                        var context = request.downloadHandler.text;
                        try
                        {
                            if (isEncrypted)
                            {
                                var unencryptedManifest = QuarkUtility.AESDecryptStringToString(context, QuarkDataProxy.QuarkAESEncryptionKey);
                                localManifest = QuarkUtility.ToObject<QuarkAssetManifest>(unencryptedManifest);
                            }
                            else
                            {
                                localManifest = QuarkUtility.ToObject<QuarkAssetManifest>(context);
                            }
                        }
                        catch (Exception e)
                        {
                            onCompareFailure(e.ToString());
                            QuarkUtility.LogError(e);
                            yield break;
                        }
                    }
                }
            }
            QuarkEngine.Instance.SetBuiltAssetBundleModeData(localManifest);
            onCompareSuccess(new string[0], new string[0], 0);
        }
        void OnUriManifestSuccess(string remoteManifestContext)
        {
            latest.Clear();
            expired.Clear();
            //获取本地manifest
            var localManifestPath = Path.Combine(PersistentPath, QuarkConstant.ManifestName);
            string localManifestContext = string.Empty;
            long overallSize = 0;
            var aesKey = QuarkDataProxy.QuarkAESEncryptionKey;
            try
            {
                //解析加密的manifest
                if (isEncrypted)
                {
                    //todo 这段必须改成webrequest 
                    var encryptedManifest = QuarkUtility.ReadTextFileContent(localManifestPath);
                    localManifestContext = QuarkUtility.AESDecryptStringToString(encryptedManifest, aesKey);
                }
                else
                {
                    localManifestContext = QuarkUtility.ReadTextFileContent(localManifestPath);
                }
                localManifest = QuarkUtility.ToObject<QuarkAssetManifest>(localManifestContext);
            }
            catch { }
            try
            {
                //解析加密的manifest
                if (isEncrypted)
                {
                    var unencryptedManifest = QuarkUtility.AESDecryptStringToString(remoteManifestContext, aesKey);
                    remoteManifest = QuarkUtility.ToObject<QuarkAssetManifest>(unencryptedManifest);
                }
                else
                {
                    remoteManifest = QuarkUtility.ToObject<QuarkAssetManifest>(remoteManifestContext);
                }
            }
            catch (Exception e)
            {
                //解析remote失败，则返回
                QuarkUtility.LogError(e);
                onCompareFailure?.Invoke(e.ToString());
                return;
            }
            if (localManifest == null)
            {
                //若本地的Manifest为空，远端的Manifest不为空，则将需要下载的资源url缓存到latest;
                latest.AddRange(remoteManifest.BundleInfoDict.Keys.ToList());
                foreach (var bundleInfo in remoteManifest.BundleInfoDict.Values)
                {
                    overallSize += bundleInfo.BundleSize;
                }
            }
            else
            {
                //若本地的Manifest不为空，远端的Manifest不为空，则对比二者之间的差异；
                //远端有本地没有，则缓存至latest；
                //远端没有本地有，则缓存至expired；
                foreach (var remoteBuildInfo in remoteManifest.BundleInfoDict)
                {
                    var remoteBundleName = remoteBuildInfo.Key;
                    var remoteBundleBuildInfo = remoteBuildInfo.Value;
                    if (localManifest.BundleInfoDict.TryGetValue(remoteBundleName, out var localBuildInfo))
                    {
                        if (localBuildInfo.Hash != remoteBundleBuildInfo.Hash)
                        {
                            overallSize += remoteBundleBuildInfo.BundleSize;
                            latest.Add(remoteBundleName);
                            expired.Add(remoteBundleName);
                        }
                        else
                        {
                            //检测远端包体与本地包体的大小是否相同。
                            //在Hash相同的情况下，若包体不同，则可能是本地的包不完整，因此需要重新加入下载队列。
                            var localBundlePath = Path.Combine(PersistentPath, localBuildInfo.BundleName);
                            var localBundleSize = QuarkUtility.GetFileSize(localBundlePath);
                            var remoteBundleSize = remoteBundleBuildInfo.BundleSize;
                            if (remoteBundleSize != localBundleSize)
                            {
                                var remainBundleSize = remoteBundleSize - localBundleSize;
                                overallSize += remainBundleSize;
                                latest.Add(remoteBundleName);
                                if (remoteBundleSize < localBundleSize)//若本地包体大于远端包体，则表示为本地包为过期包
                                {
                                    expired.Add(localBuildInfo.BundleName);
                                }
                            }
                        }
                    }
                    else
                    {
                        overallSize += remoteBundleBuildInfo.BundleSize;
                        latest.Add(remoteBundleName);
                    }
                    foreach (var localMF in localManifest.BundleInfoDict)
                    {
                        if (!remoteManifest.BundleInfoDict.ContainsKey(localMF.Key))
                        {
                            expired.Add(localMF.Key);
                        }
                    }
                }
            }
            var latesetArray = latest.ToArray();
            var expiredArray = expired.ToArray();
            latest.Clear();
            expired.Clear();
            QuarkUtility.OverwriteTextFile(localManifestPath, remoteManifestContext);
            QuarkEngine.Instance.SetBuiltAssetBundleModeData(remoteManifest);
            onCompareSuccess?.Invoke(latesetArray, expiredArray, overallSize);
        }
    }
}
