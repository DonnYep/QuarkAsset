using Quark.Asset;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Quark.Manifest
{
    public class QuarkManifestRequester
    {
        bool running = false;
        public bool Running { get { return running; } }
        Coroutine coroutine;
        readonly List<QuarkManifestRequestTask> reqTaskList;
        readonly Dictionary<string, QuarkManifestRequestTask> reqTaskDict;
        Action<long> onTaskDone;
        public event Action<long> OnTaskDone
        {
            add { onTaskDone += value; }
            remove { onTaskDone -= value; }
        }
        public bool Downloading { get; private set; }

        static int ManifestRequestTaskIndex = 0;
        internal QuarkManifestRequester()
        {
            reqTaskList = new List<QuarkManifestRequestTask>();
            reqTaskDict = new Dictionary<string, QuarkManifestRequestTask>();
        }
        public void StartRequestManifest()
        {
            if (Downloading)
                return;
            if (reqTaskList.Count == 0)
            {
                return;
            }
            coroutine = QuarkUtility.Unity.StartCoroutine(DownloadManifests());
        }
        public long AddTask(string url, byte[] aesKeyBytes, Action<QuarkManifest> onSuccess, Action<string> onFailure)
        {
            if (!reqTaskDict.ContainsKey(url))
            {
                var reqTask = new QuarkManifestRequestTask(ManifestRequestTaskIndex, url, aesKeyBytes, onSuccess, onFailure);
                reqTaskDict.Add(url, reqTask);
                reqTaskList.Add(reqTask);
                ManifestRequestTaskIndex++;
            }
            return long.MinValue;
        }
        public bool RemoveTask(string url)
        {
            if (reqTaskDict.ContainsKey(url))
            {
                var reqTask = reqTaskDict[url];
                reqTaskDict.Remove(url);
                reqTaskList.Remove(reqTask);
                return true;
            }
            return false;
        }
        public bool RemoveTask(long taskId)
        {
            var length = reqTaskList.Count;
            for (int i = 0; i < length; i++)
            {
                var reqTask = reqTaskList[i];
                if (reqTask.TaskId == taskId)
                {
                    return RemoveTask(reqTask.Url);
                }
            }
            return false;
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
            reqTaskList.Clear();
            reqTaskDict.Clear();
            Downloading = false;
        }
        IEnumerator DownloadManifests()
        {
            Downloading = true;
            while (reqTaskList.Count > 0)
            {
                var reqTask = reqTaskList[0];
                reqTaskList.RemoveAt(0);
                reqTaskDict.Remove(reqTask.Url);
                yield return DownloadSignleManifest(reqTask);
            }
            Downloading = false;
        }
        IEnumerator DownloadSignleManifest(QuarkManifestRequestTask requestTask)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(requestTask.Url))
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
                            var decrypt = requestTask.AesKeyBytes != null && requestTask.AesKeyBytes.Length > 0;
                            string srcJson = context;
                            if (decrypt)
                                srcJson = QuarkUtility.AESDecryptStringToString(context, requestTask.AesKeyBytes);
                            manifest = QuarkUtility.ToObject<QuarkManifest>(srcJson);
                            requestTask.OnSuccess?.Invoke(manifest);
                        }
                        catch (Exception e)
                        {
                            requestTask.OnFailure?.Invoke(e.ToString());
                        }
                    }
                }
                else
                {
                    requestTask.OnFailure?.Invoke(request.error);
                }
                onTaskDone?.Invoke(requestTask.TaskId);
                running = false;
            }
        }
    }
}
