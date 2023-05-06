using Quark.Asset;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Quark.Manifest
{
    //Integrity
    public class QuarkManifestVerifier
    {
        Coroutine coroutine;
        Action<QuarkManifestVerifyResult> onVerifyDone;
        public event Action<QuarkManifestVerifyResult> OnVerifyDone
        {
            add { onVerifyDone += value; }
            remove { onVerifyDone -= value; }
        }
        List<QuarkManifestVerifyTask> tasks = new List<QuarkManifestVerifyTask>();
        List<QuarkManifestVerifyInfo> verificationSuccessInfos = new List<QuarkManifestVerifyInfo>();
        List<QuarkManifestVerifyInfo> verificationFailureInfos = new List<QuarkManifestVerifyInfo>();
        bool verificationInProgress;
        public bool VerificationInProgress
        {
            get { return verificationInProgress; }
        }
        internal QuarkManifestVerifier() { }
        /// <summary>
        /// 校验文件清单；
        /// </summary>
        /// <param name="manifest">文件清单</param>
        /// <param name="path">持久化路径</param>
        public void VerifyManifest(QuarkManifest manifest, string path)
        {
            if (verificationInProgress)
                return;
            verificationInProgress = true;
            tasks.Clear();
            verificationSuccessInfos.Clear();
            verificationFailureInfos.Clear();
            foreach (var bundleBuildInfo in manifest.BundleInfoDict.Values)
            {
                var url = Path.Combine(path, bundleBuildInfo.QuarkAssetBundle.BundleKey);
                tasks.Add(new QuarkManifestVerifyTask(url, bundleBuildInfo.QuarkAssetBundle.BundleName, bundleBuildInfo.BundleSize));
            }
            coroutine = QuarkUtility.Unity.StartCoroutine(MultipleVerify());
        }
        /// <summary>
        /// 停止校验；
        /// </summary>
        public void StopVerify()
        {
            if (coroutine != null)
                QuarkUtility.Unity.StopCoroutine(coroutine);
            var length = tasks.Count;
            for (int i = 0; i < length; i++)
            {
                var task = tasks[i];
                verificationFailureInfos.Add(new QuarkManifestVerifyInfo(task.Url, task.ResourceBundleName, task.ResourceBundleSize, false, 0));
            }
            var result = new QuarkManifestVerifyResult()
            {
                VerificationFailureInfos = verificationFailureInfos.ToArray(),
                VerificationSuccessInfos = verificationSuccessInfos.ToArray()
            };
            verificationInProgress = false;
            onVerifyDone?.Invoke(result);
            tasks.Clear();
            verificationSuccessInfos.Clear();
            verificationFailureInfos.Clear();
        }
        IEnumerator MultipleVerify()
        {
            while (tasks.Count > 0)
            {
                var task = tasks[0];
                tasks.RemoveAt(0);
                yield return VerifyContentLength(task);
            }
            var result = new QuarkManifestVerifyResult()
            {
                VerificationFailureInfos = verificationFailureInfos.ToArray(),
                VerificationSuccessInfos = verificationSuccessInfos.ToArray()
            };
            verificationInProgress = false;
            onVerifyDone?.Invoke(result);
        }
        IEnumerator VerifyContentLength(QuarkManifestVerifyTask task)
        {
            using (UnityWebRequest request = UnityWebRequest.Head(task.Url))
            {
                yield return request.SendWebRequest();
                var size = request.GetRequestHeader("Content-Length");
#if UNITY_2020_1_OR_NEWER
                if (request.result != UnityWebRequest.Result.ConnectionError && request.result != UnityWebRequest.Result.ProtocolError)
#elif UNITY_2018_1_OR_NEWER
                if (!request.isNetworkError && !request.isHttpError)
#endif
                {
                    var bundleLength = Convert.ToInt64(size);
                    bool bundleLengthMatched = false;
                    if (task.ResourceBundleSize == bundleLength)
                    {
                        bundleLengthMatched = true;
                    }
                    verificationSuccessInfos.Add(new QuarkManifestVerifyInfo(task.Url, task.ResourceBundleName, task.ResourceBundleSize, bundleLengthMatched, bundleLength));
                }
                else
                {
                    verificationFailureInfos.Add(new QuarkManifestVerifyInfo(task.Url, task.ResourceBundleName, task.ResourceBundleSize, false, 0));
                }
            }
        }
    }
}
