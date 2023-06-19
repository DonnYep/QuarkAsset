using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
namespace Quark.Networking
{
    /// <summary>
    /// QuarkDownloader的替代方案
    /// </summary>
    public class QuarkAssetDownloader
    {
        #region events
        Action<QuarkDownloadStartEventArgs> onDownloadStart;
        Action<QuarkDownloadSuccessEventArgs> onDownloadSuccess;
        Action<QuarkDownloadFailureEventArgs> onDownloadFailure;
        Action<QuarkDownloadUpdateEventArgs> onDownloadUpdate;
        Action<QuarkDownloadAllTasksCompletedEventArgs> onDownloadAllTasksFinish;
        public event Action<QuarkDownloadStartEventArgs> OnDownloadStart
        {
            add { onDownloadStart += value; }
            remove { onDownloadStart -= value; }
        }
        public event Action<QuarkDownloadSuccessEventArgs> OnDownloadSuccess
        {
            add { onDownloadSuccess += value; }
            remove { onDownloadSuccess -= value; }
        }
        public event Action<QuarkDownloadFailureEventArgs> OnDownloadFailure
        {
            add { onDownloadFailure += value; }
            remove { onDownloadFailure -= value; }
        }
        public event Action<QuarkDownloadUpdateEventArgs> OnDownloadUpdate
        {
            add { onDownloadUpdate += value; }
            remove { onDownloadUpdate -= value; }
        }
        public event Action<QuarkDownloadAllTasksCompletedEventArgs> OnDownloadAllTasksFinish
        {
            add { onDownloadAllTasksFinish += value; }
            remove { onDownloadAllTasksFinish -= value; }
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
        int redirectLimit = 32;
        public int RedirectLimit
        {
            get { return redirectLimit; }
            set { redirectLimit = value; }
        }
        public bool DeleteFailureFile { get; set; }
        List<QuarkDownloadTask> pendingTasks = new List<QuarkDownloadTask>();
        Dictionary<string, QuarkDownloadTask> pendingTaskDict = new Dictionary<string, QuarkDownloadTask>();

        List<QuarkDownloadNode> successedNodeList = new List<QuarkDownloadNode>();
        List<QuarkDownloadNode> failedNodeList = new List<QuarkDownloadNode>();

        List<QuarkDownloadTask> successedTaskList = new List<QuarkDownloadTask>();
        List<QuarkDownloadTask> failedTaskList = new List<QuarkDownloadTask>();

        DateTime downloadStartTime;
        DateTime downloadEndTime;

        QuarkDownloadTask downloadingTask;

        UnityWebRequest unityWebRequest;
        Coroutine coroutine;
        /// <summary>
        /// 是否正在下载；
        /// </summary>
        public bool Downloading { get; private set; }
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
        public int DownloadCount { get { return downloadCount; } }

        internal QuarkAssetDownloader() { }
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
            coroutine = QuarkUtility.Unity.StartCoroutine(RunDownloadMultipleFiles());
        }
        public void AddDownloads(IEnumerable<QuarkDownloadTask> tasks)
        {
            if (Downloading)
                return;
            foreach (var task in tasks)
            {
                AddDownload(task);
            }
        }
        public void AddDownload(QuarkDownloadTask task)
        {
            if (Downloading)
                return;
            if (!pendingTaskDict.ContainsKey(task.DownloadUri))
            {
                pendingTaskDict.Add(task.DownloadUri, task);
                pendingTasks.Add(task);
                downloadCount++;
                var remainSize = task.RecordedBundleSize - task.LocalBundleSize;
                if (remainSize < 0)
                    remainSize = 0;
                totalRequiredDownloadSize += remainSize;
            }
        }
        public void RemoveDownload(string downloadUri)
        {
            if (pendingTaskDict.TryGetValue(downloadUri, out var task))
            {
                if (downloadingTask.DownloadUri == downloadUri)
                {
                    unityWebRequest?.Abort();
                    downloadCount--;
                }
                else
                {
                    pendingTaskDict.Remove(downloadUri);
                    pendingTasks.Remove(task);
                    downloadCount--;
                    var remainSize = task.RecordedBundleSize - task.LocalBundleSize;
                    if (remainSize < 0)
                        remainSize = 0;
                    totalRequiredDownloadSize -= remainSize;
                }
            }
        }
        public void StopDownload()
        {
            downloadCount = 0;
            pendingTaskDict.Clear();
            pendingTasks.Clear();
            unityWebRequest?.Abort();
            if (coroutine != null)
                QuarkUtility.Unity.StopCoroutine(coroutine);
        }
        IEnumerator RunDownloadMultipleFiles()
        {
            downloadStartTime = DateTime.Now;
            while (pendingTasks.Count > 0)
            {
                downloadingTask = pendingTasks[0];
                pendingTasks.RemoveAt(0);
                currentDownloadIndex = downloadCount - pendingTasks.Count - 1;
                yield return RunDownloadSingleFile(downloadingTask);
                pendingTaskDict.Remove(downloadingTask.DownloadUri);
            }
            OnAllPendingTasksCompleted();
        }
        IEnumerator RunDownloadSingleFile(QuarkDownloadTask task)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(task.DownloadUri))
            {
                var fileDownloadStartTime = DateTime.Now;
#if UNITY_2019_1_OR_NEWER
                request.downloadHandler = new DownloadHandlerFile(task.DownloadPath, true);
#elif UNITY_2018_1_OR_NEWER
                request.downloadHandler = new DownloadHandlerFile(downloadPath);
#endif
                unityWebRequest = request;
                request.timeout = downloadTimeout;
                request.redirectLimit = redirectLimit;
                {
                    var now = DateTime.Now;
                    var timeSpan = now - fileDownloadStartTime;
                    var downloadNode = new QuarkDownloadNode(task.DownloadUri, task.DownloadPath, (long)request.downloadedBytes, 0, timeSpan);
                    var startEventArgs = QuarkDownloadStartEventArgs.Create(downloadNode);
                    onDownloadStart?.Invoke(startEventArgs);
                    QuarkDownloadStartEventArgs.Release(startEventArgs);
                }
                //增量下载实现
                //下载的路径是可IO的
                long fileInfoLength = 0;
                if (File.Exists(task.DownloadPath))
                {
                    var fileInfo = new FileInfo(task.DownloadPath);
                    fileInfoLength = fileInfo.Length;
                }
                request.SetRequestHeader("Range", "bytes=" + fileInfoLength + "-");

                var operation = request.SendWebRequest();
                while (!operation.isDone && canDownload)
                {
                    var now = DateTime.Now;
                    var timeSpan = now - fileDownloadStartTime;
                    var downloadNode = new QuarkDownloadNode(task.DownloadUri, task.DownloadPath, (long)request.downloadedBytes, operation.progress, timeSpan);
                    OnDownloadingHandler(downloadNode, completedDownloadSize + downloadNode.DownloadedBytes);
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
                        var downloadNode = new QuarkDownloadNode(task.DownloadUri, task.DownloadPath, (long)request.downloadedBytes, 1, timeSpan);
                        completedDownloadSize += downloadNode.DownloadedBytes;
                        var successEventArgs = QuarkDownloadSuccessEventArgs.Create(downloadNode);
                        onDownloadSuccess?.Invoke(successEventArgs);
                        QuarkDownloadSuccessEventArgs.Release(successEventArgs);
                        successedNodeList.Add(downloadNode);
                        successedTaskList.Add(task);
                        OnDownloadingHandler(downloadNode, completedDownloadSize);
                    }
                }
                else
                {
                    var fileDownloadEndTime = DateTime.Now;
                    var timeSpan = fileDownloadEndTime - fileDownloadStartTime;
                    QuarkDownloadNode downloadNode;
                    if (DeleteFailureFile)
                    {
                        QuarkUtility.DeleteFile(task.DownloadPath);
                        downloadNode = new QuarkDownloadNode(task.DownloadUri, task.DownloadPath, 0, operation.progress, timeSpan);
                    }
                    else
                    {
                        downloadNode = new QuarkDownloadNode(task.DownloadUri, task.DownloadPath, (long)request.downloadedBytes, operation.progress, timeSpan);
                        completedDownloadSize += downloadNode.DownloadedBytes;
                    }
                    var failureEventArgs = QuarkDownloadFailureEventArgs.Create(downloadNode, request.error);
                    onDownloadFailure?.Invoke(failureEventArgs);
                    QuarkDownloadFailureEventArgs.Release(failureEventArgs);
                    failedNodeList.Add(downloadNode);
                    failedTaskList.Add(task);
                    OnDownloadingHandler(downloadNode, completedDownloadSize);
                }
                unityWebRequest = null;
            }
        }
        void OnDownloadingHandler(QuarkDownloadNode node, long downloadedSize)
        {
            var eventArgs = QuarkDownloadUpdateEventArgs.Create(node, currentDownloadIndex, downloadCount, downloadedSize, totalRequiredDownloadSize);
            onDownloadUpdate?.Invoke(eventArgs);
            QuarkDownloadUpdateEventArgs.Release(eventArgs);
        }
        void OnAllPendingTasksCompleted()
        {
            canDownload = false;
            Downloading = false;
            downloadEndTime = DateTime.Now;
            var successedTaskArray = successedTaskList.ToArray();
            var failedTaskArray = failedTaskList.ToArray();
            var successedNodeArray = successedNodeList.ToArray();
            var failedNodeArray = failedNodeList.ToArray();
            var downloadTimeSpan = downloadEndTime - downloadStartTime;
            var eventArgs = QuarkDownloadAllTasksCompletedEventArgs.Create(successedTaskArray, failedTaskArray, successedNodeArray, failedNodeArray, completedDownloadSize, downloadTimeSpan);
            onDownloadAllTasksFinish?.Invoke(eventArgs);
            QuarkDownloadAllTasksCompletedEventArgs.Release(eventArgs);
            pendingTasks.Clear();
            successedTaskList.Clear();
            failedTaskList.Clear();
            successedNodeList.Clear();
            failedNodeList.Clear();
            downloadCount = 0;
            currentDownloadIndex = 0;
            totalRequiredDownloadSize = 0;
            completedDownloadSize = 0;
        }
    }
}
