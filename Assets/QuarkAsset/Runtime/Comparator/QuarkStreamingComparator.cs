using Quark.Asset;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Networking;
using UnityEngine;
using System.IO;

namespace Quark
{
    public class QuarkStreamingComparator
    {
        bool isEncrypted { get { return QuarkDataProxy.QuarkAESEncryptionKey.Length > 0; } }
        public Coroutine LoadBuildInfoAsync(string realtivePath, Action successCallback, Action<string> errorCallback)
        {
            var manifestPath = Path.Combine(Application.streamingAssetsPath, realtivePath, QuarkConstant.ManifestName);
            return QuarkUtility.Unity.StartCoroutine(EnumLoadStreamingAsset(manifestPath,  successCallback, errorCallback));
        }
        IEnumerator EnumLoadStreamingAsset(string manifestUri,  Action successCallback, Action<string> errorCallback)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(manifestUri))
            {
                QuarkAssetManifest manifest = null;
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
                                manifest = QuarkUtility.ToObject<QuarkAssetManifest>(unencryptedManifest);
                            }
                            else
                            {
                                manifest = QuarkUtility.ToObject<QuarkAssetManifest>(context);
                            }
                            QuarkDataProxy.QuarkManifest = manifest;
                            QuarkEngine.Instance.SetBuiltAssetBundleModeData(manifest);
                        }
                        catch (Exception e)
                        {
                            errorCallback?.Invoke(e.ToString());
                            yield break;
                        }
                    }
                }
            }
            successCallback?.Invoke();
        }
    }
}
