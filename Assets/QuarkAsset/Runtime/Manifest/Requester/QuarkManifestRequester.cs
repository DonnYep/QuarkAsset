using Quark.Asset;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Quark
{
    public class QuarkManifestRequester
    {
        Action<QuarkManifest> onManifestAcquireSuccess;
        Action<string> onManifestAcquireFailure;
        bool running = false;
        public bool Running { get { return running; } }
        Coroutine coroutine;
        /// <summary>
        /// 从本地下载文件清单；
        /// </summary>
        /// <param name="url">地址</param>
        /// <param name="aesKeyBytes">密钥</param>
        public Coroutine RequestManifestAsync(string url, byte[] aesKeyBytes)
        {
            if (running)
                return coroutine;
            coroutine = QuarkUtility.Unity.StartCoroutine(RequestManifest(url, aesKeyBytes));
            return coroutine;
        }
        public QuarkManifestRequester OnManifestAcquireSuccess(Action<QuarkManifest> onSuccess)
        {
            onManifestAcquireSuccess = onSuccess;
            return this;
        }
        public QuarkManifestRequester OnManifestAcquireFailure(Action<string> onFailure)
        {
            onManifestAcquireFailure = onFailure;
            return this;
        }
        /// <summary>
        /// 停止请求文件清单；
        /// </summary>
        public void StopRequestManifest()
        {
            if (coroutine != null)
            {
                QuarkUtility.Unity.StopCoroutine(coroutine);
            }
        }
        IEnumerator RequestManifest(string manifestUrl, byte[] aesKeyBytes = null)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(manifestUrl))
            {
                running = true;
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
                        QuarkManifest manifest = null;
                        try
                        {
                            var isEncrypted = aesKeyBytes != null && aesKeyBytes.Length > 0;
                            string srcJson = context;
                            if (isEncrypted)
                                srcJson = QuarkUtility.AESDecryptStringToString(context, aesKeyBytes);
                            manifest = QuarkUtility.ToObject<QuarkManifest>(srcJson);
                            onManifestAcquireSuccess?.Invoke(manifest);
                        }
                        catch (Exception e)
                        {
                            onManifestAcquireFailure?.Invoke(e.ToString());
                            yield break;
                        }
                    }
                }
                else
                {
                    onManifestAcquireFailure?.Invoke(request.error);
                }
                running = false;
                onManifestAcquireSuccess = null;
                onManifestAcquireFailure = null;
            }
        }
    }
}
