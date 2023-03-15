using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Quark.Networking
{
    /// <summary>
    /// Quark资源下载器；
    /// 资源被下载到本地持久化路径后，再由Qurk加载器进行资源加载；
    /// </summary>
    public class QuarkDownloader
    {
        #region events
        Action<QuarkDownloadStartEventArgs> onDownloadStart;
        Action<QuarkDownloadSuccessEventArgs> onDownloadSuccess;
        Action<QuarkDownloadFailureEventArgs> onDownloadFailure;
        Action<QuarkDownloadingEventArgs> onDownloadOverall;
        Action<QuarkAllDownloadCompletedEventArgs> onAllDownloadFinish;
        /// <summary>
        /// URL---DownloadPath
        /// </summary>
        public event Action<QuarkDownloadStartEventArgs> OnDownloadStart
        {
            add { onDownloadStart += value; }
            remove { onDownloadStart -= value; }
        }
        /// <summary>
        /// URL---DownloadPath
        /// </summary>
        public event Action<QuarkDownloadSuccessEventArgs> OnDownloadSuccess
        {
            add { onDownloadSuccess += value; }
            remove { onDownloadSuccess -= value; }
        }
        /// <summary>
        /// URL---DownloadPath---ErrorMessage
        /// </summary>
        public event Action<QuarkDownloadFailureEventArgs> OnDownloadFailure
        {
            add { onDownloadFailure += value; }
            remove { onDownloadFailure -= value; }
        }
        /// <summary>
        /// URL---DownloadPath---OverallProgress(0~100%)---IndividualProgress(0~100%)
        /// </summary>
        public event Action<QuarkDownloadingEventArgs> OnDownloadOverall
        {
            add { onDownloadOverall += value; }
            remove { onDownloadOverall -= value; }
        }
        /// <summary>
        /// SuccessURIs---FailureURIs---TimeSpan
        /// </summary>
        public event Action<QuarkAllDownloadCompletedEventArgs> OnAllDownloadFinish
        {
            add { onAllDownloadFinish += value; }
            remove { onAllDownloadFinish -= value; }
        }
        #endregion
        int downloadTimeout = 30;
        public int DownloadTimeout
        {
            get { return downloadTimeout; }
            set
            {
                downloadTimeout = value;
                if (downloadTimeout < 0)
                    downloadTimeout = 0;
            }
        }
        public bool DeleteFailureFile { get; set; }
        /// <summary>
        /// 是否正在下载；
        /// </summary>
        public bool Downloading { get; private set; }
        /// <summary>
        /// 下载中的资源总数；
        /// </summary>
        public int DownloadingCount { get { return pendingTasks.Count; } }

        List<QuarkDownloadTask> pendingTasks = new List<QuarkDownloadTask>();
        Dictionary<string, QuarkDownloadTask> pendingTaskDict = new Dictionary<string, QuarkDownloadTask>();
        List<QuarkDownloadNode> successedNodeList = new List<QuarkDownloadNode>();
        List<QuarkDownloadNode> failedNodeList = new List<QuarkDownloadNode>();

        DateTime downloadStartTime;
        DateTime downloadEndTime;

        UnityWebRequest unityWebRequest;

        Coroutine coroutine;
        /// <summary>
        /// 总共需要下载的文件大小
        /// </summary>
        long totalRequiredDownloadSize;
        /// <summary>
        /// 已经下载的文件大小
        /// </summary>
        long completedDownloadSize;
        /// <summary>
        /// 当前下载的序号；
        /// </summary>
        int currentDownloadIndex = 0;
        /// <summary>
        /// 当前是否可下载；
        /// </summary>
        bool canDownload;
        /// <summary>
        /// 下载任务数量；
        /// </summary>
        int downloadCount = 0;
        internal QuarkDownloader() { }
        /// <summary>
        /// 添加下载文件；
        /// </summary>
        public void AddDownload(QuarkDownloadInfo info)
        {
            if (Downloading)
                return;
            if (!pendingTaskDict.ContainsKey(info.DownloadUri))
            {
                var task = new QuarkDownloadTask(info.DownloadUri, info.DownloadPath, info.RequiredDownloadSize);
                pendingTaskDict.Add(task.DownloadUri, task);
                pendingTasks.Add(task);
                downloadCount++;
                totalRequiredDownloadSize += info.RequiredDownloadSize;
            }
        }
        /// <summary>
        /// 启动下载；
        /// </summary>
        public void StartDownload()
        {
            if (Downloading)
                return;
            canDownload = true;
            if (pendingTasks.Count == 0 || !canDownload)
            {
                canDownload = false;
                return;
            }
            Downloading = true;
            downloadStartTime = DateTime.Now;
            coroutine = QuarkUtility.Unity.StartCoroutine(EnumDownloadMultipleFiles());
        }
        public void StopDownload()
        {
            if (coroutine != null)
                QuarkUtility.Unity.StopCoroutine(coroutine);
            unityWebRequest?.Abort();
            downloadCount = 0;
            pendingTasks.Clear();
            failedNodeList.Clear();
            successedNodeList.Clear();
            canDownload = false;
        }
        public void ClearEvents()
        {
            onDownloadStart = null;
            onDownloadSuccess = null;
            onDownloadFailure = null;
            onDownloadOverall = null;
            onAllDownloadFinish = null;
            downloadCount = 0;
        }
        IEnumerator EnumDownloadMultipleFiles()
        {
            while (pendingTasks.Count > 0)
            {
                var task = pendingTasks[0];
                pendingTasks.RemoveAt(0);
                currentDownloadIndex = downloadCount - pendingTasks.Count - 1;
                yield return EnumDownloadSingleFile(task.DownloadUri, task.DownloadPath);
                pendingTaskDict.Remove(task.DownloadUri);
            }
            OnDownloadedPendingFiles();
            Downloading = false;
        }
        IEnumerator EnumDownloadSingleFile(string downloadUri, string downloadPath)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(downloadUri))
            {
                var fileDownloadStartTime = DateTime.Now;
#if UNITY_2019_1_OR_NEWER
                request.downloadHandler = new DownloadHandlerFile(downloadPath, true);
#elif UNITY_2018_1_OR_NEWER
                request.downloadHandler = new DownloadHandlerFile(downloadPath);
#endif
                unityWebRequest = request;
                request.timeout = downloadTimeout;

                {
                    var now = DateTime.Now;
                    var timeSpan = now - fileDownloadStartTime;
                    var downloadNode = new QuarkDownloadNode(downloadUri, downloadPath, (long)request.downloadedBytes,0, timeSpan);
                    var startEventArgs = QuarkDownloadStartEventArgs.Create(downloadNode);
                    onDownloadStart?.Invoke(startEventArgs);
                    QuarkDownloadStartEventArgs.Release(startEventArgs);
                }

                //增量下载实现
                //下载的路径是可IO的
                var fileInfo = new FileInfo(downloadPath);
                request.SetRequestHeader("Range", "bytes=" + fileInfo.Length + "-");

                var operation = request.SendWebRequest();
                while (!operation.isDone && canDownload)
                {
                    var now = DateTime.Now;
                    var timeSpan = now - fileDownloadStartTime;
                    var downloadNode = new QuarkDownloadNode(downloadUri, downloadPath, (long)request.downloadedBytes, operation.progress,timeSpan);
                    OnDownloading(downloadNode, completedDownloadSize + downloadNode.DownloadedBytes);
                    yield return null;
                }
#if UNITY_2020_1_OR_NEWER
                if (request.result != UnityWebRequest.Result.ConnectionError && request.result != UnityWebRequest.Result.ProtocolError && canDownload)
#elif UNITY_2018_1_OR_NEWER
                if (!request.isNetworkError && !request.isHttpError && canDownload)
#endif
                {
                    if (request.isDone)
                    {
                        var fileDownloadEndTime = DateTime.Now;
                        var timeSpan = fileDownloadEndTime - fileDownloadStartTime;
                        var downloadNode = new QuarkDownloadNode(downloadUri, downloadPath, (long)request.downloadedBytes, 1,timeSpan);
                        completedDownloadSize += downloadNode.DownloadedBytes;
                        var successEventArgs = QuarkDownloadSuccessEventArgs.Create(downloadNode);
                        onDownloadSuccess?.Invoke(successEventArgs);
                        QuarkDownloadSuccessEventArgs.Release(successEventArgs);
                        successedNodeList.Add(downloadNode);
                        OnDownloading(downloadNode, completedDownloadSize);
                    }
                }
                else
                {
                    var fileDownloadEndTime = DateTime.Now;
                    var timeSpan = fileDownloadEndTime - fileDownloadStartTime;
                    var downloadNode = new QuarkDownloadNode(downloadUri, downloadPath, (long)request.downloadedBytes, operation.progress,timeSpan);
                    completedDownloadSize += downloadNode.DownloadedBytes;
                    var failureEventArgs = QuarkDownloadFailureEventArgs.Create(downloadNode, request.error);
                    onDownloadFailure?.Invoke(failureEventArgs);
                    QuarkDownloadFailureEventArgs.Release(failureEventArgs);
                    failedNodeList.Add(downloadNode);
                    OnDownloading(downloadNode, completedDownloadSize);
                    if (DeleteFailureFile)
                    {
                        QuarkUtility.DeleteFile(downloadPath);
                    }
                }
                unityWebRequest = null;
            }
        }
        void OnDownloading(QuarkDownloadNode node, long downloadedSize)
        {
            var eventArgs = QuarkDownloadingEventArgs.Create(node, currentDownloadIndex, downloadCount, downloadedSize, totalRequiredDownloadSize);
            onDownloadOverall?.Invoke(eventArgs);
            QuarkDownloadingEventArgs.Release(eventArgs);
        }
        void OnDownloadedPendingFiles()
        {
            canDownload = false;
            Downloading = false;
            downloadEndTime = DateTime.Now;
            var eventArgs = QuarkAllDownloadCompletedEventArgs.Create(successedNodeList.ToArray(), failedNodeList.ToArray(), downloadEndTime - downloadStartTime);
            onAllDownloadFinish?.Invoke(eventArgs);
            QuarkAllDownloadCompletedEventArgs.Release(eventArgs);
            pendingTasks.Clear();
            failedNodeList.Clear();
            successedNodeList.Clear();
            downloadCount = 0;
        }
    }
}
