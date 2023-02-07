using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
        Action<QuarkDownloadOverallProgressEventArgs> onDownloadOverall;
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
        public event Action<QuarkDownloadOverallProgressEventArgs> OnDownloadOverall
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
        public string URL { get { return QuarkDataProxy.URL; } }
        public int DownloadTimeout { get; private set; }
        public bool DeleteFailureFile { get; set; }

        /// <summary>
        /// 是否正在下载；
        /// </summary>
        public bool Downloading { get; private set; }
        /// <summary>
        /// 下载中的资源总数；
        /// </summary>
        public int DownloadingCount { get { return pendingtasks.Count; } }

        List<QuarkDownloadTask> pendingtasks = new List<QuarkDownloadTask>();
        Dictionary<string, QuarkDownloadTask> pendingTaskDict = new Dictionary<string, QuarkDownloadTask>();
        List<QuarkDownloadCompletedInfo> successURIs = new List<QuarkDownloadCompletedInfo>();
        List<QuarkDownloadCompletedInfo> failureURIs = new List<QuarkDownloadCompletedInfo>();

        DateTime downloadStartTime;
        DateTime downloadEndTime;

        UnityWebRequest unityWebRequest;

        /// <summary>
        /// 单位资源的百分比比率；
        /// </summary>
        float UnitResRatio { get { return 100f / downloadCount; } }
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
        /// <summary>
        /// 移除下载文件；
        /// </summary>
        /// <param name="fileName">文件名</param>
        public void RemoveDownloadFile(string uri)
        {
            if (pendingTaskDict.ContainsKey(uri))
            {
                var downloadTask = pendingTaskDict[uri];
                pendingTaskDict.Remove(uri);
                pendingtasks.Remove(downloadTask);
                downloadCount--;
            }
        }
        /// <summary>
        /// 添加下载文件；
        /// </summary>
        /// <param name="fileName">文件名</param>
        public void AddDownloadFile(string uri, string downloadPath)
        {
            if (!pendingTaskDict.ContainsKey(uri))
            {
                var task = new QuarkDownloadTask(uri, downloadPath);
                pendingTaskDict.Add(uri, task);
                pendingtasks.Add(task);
                downloadCount++;
            }
        }
        /// <summary>
        /// 启动下载；
        /// </summary>
        public void LaunchDownload()
        {
            canDownload = true;
            if (pendingtasks.Count == 0 || !canDownload)
            {
                canDownload = false;
                return;
            }
            Downloading = true;
            downloadStartTime = DateTime.Now;
            QuarkUtility.Unity.StartCoroutine(EnumDownloadMultipleFiles());
        }
        /// <summary>
        /// 移除所有下载；
        /// </summary>
        public void RemoveAllDownload()
        {
            OnCancelDownload();
        }
        /// <summary>
        /// 终止下载，谨慎使用；
        /// </summary>
        public void CancelDownload()
        {
            OnCancelDownload();
        }
        public void Release()
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
            while (pendingtasks.Count > 0)
            {
                var task = pendingtasks[0];
                pendingtasks.RemoveAt(0);
                currentDownloadIndex = downloadCount - pendingtasks.Count - 1;
                var fileDownloadPath = Path.Combine(task.DownloadPath, task.URI);
                var remoteUri = Path.Combine(URL, task.URI);
                yield return EnumDownloadSingleFile(remoteUri, fileDownloadPath);
                pendingTaskDict.Remove(task.URI);
            }
            OnDownloadedPendingFiles();
        }
        IEnumerator EnumDownloadSingleFile(string uri, string downloadPath)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(uri))
            {
                var fileDownloadStartTime = DateTime.Now;
                Downloading = true;
#if UNITY_2019_1_OR_NEWER
                request.downloadHandler = new DownloadHandlerFile(downloadPath, true);
#elif UNITY_2018_1_OR_NEWER
                request.downloadHandler = new DownloadHandlerFile(downloadPath);
#endif
                unityWebRequest = request;
                var timeout = Convert.ToInt32(DownloadTimeout);
                if (timeout > 0)
                    request.timeout = timeout;
                var startEventArgs = QuarkDownloadStartEventArgs.Create(uri, downloadPath);
                onDownloadStart?.Invoke(startEventArgs);
                QuarkDownloadStartEventArgs.Release(startEventArgs);

                //增量下载实现
                //下载的路径是可IO的
                var fileInfo = new FileInfo(downloadPath);
                request.SetRequestHeader("Range", "bytes=" + fileInfo.Length + "-");

                var operation = request.SendWebRequest();
                while (!operation.isDone && canDownload)
                {
                    OnFileDownloading(uri, QuarkDataProxy.PersistentPath, request.downloadProgress, request.downloadedBytes);
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
                        Downloading = false;
                        var fileDownloadEndTime = DateTime.Now;
                        var timeSpan = fileDownloadEndTime - fileDownloadStartTime;
                        var completedInfo = new QuarkDownloadCompletedInfo(uri, downloadPath, (long)request.downloadedBytes, timeSpan);
                        var successEventArgs = QuarkDownloadSuccessEventArgs.Create(completedInfo);
                        onDownloadSuccess?.Invoke(successEventArgs);
                        QuarkDownloadSuccessEventArgs.Release(successEventArgs);
                        OnFileDownloading(uri, QuarkDataProxy.PersistentPath, 1, request.downloadedBytes);
                        successURIs.Add(completedInfo);
                    }
                }
                else
                {
                    Downloading = false;
                    var fileDownloadEndTime = DateTime.Now;
                    var timeSpan = fileDownloadEndTime - fileDownloadStartTime;
                    var completedInfo = new QuarkDownloadCompletedInfo(uri, downloadPath, (long)request.downloadedBytes, timeSpan);
                    var failureEventArgs = QuarkDownloadFailureEventArgs.Create(completedInfo, request.error);
                    onDownloadFailure?.Invoke(failureEventArgs);
                    QuarkDownloadFailureEventArgs.Release(failureEventArgs);

                    failureURIs.Add(completedInfo);
                    OnFileDownloading(uri, QuarkDataProxy.PersistentPath, 1, request.downloadedBytes);
                    if (DeleteFailureFile)
                    {
                        QuarkUtility.DeleteFile(downloadPath);
                    }
                }
                unityWebRequest = null;
            }
        }
        /// <summary>
        /// 处理整体进度；
        /// individualPercent 0~1；
        /// </summary>
        /// <param name="uri">资源地址</param>
        /// <param name="downloadPath">下载到本地的目录</param>
        /// <param name="individualPercent">资源个体百分比0~1</param>
        void OnFileDownloading(string uri, string downloadPath, float individualPercent, ulong downloadedBytes)
        {
            var overallIndexPercent = 100 * ((float)currentDownloadIndex / downloadCount);
            var overallProgress = overallIndexPercent + (UnitResRatio * individualPercent);
            var eventArgs = QuarkDownloadOverallProgressEventArgs.Create(uri, downloadPath, overallProgress, individualPercent, downloadedBytes);
            onDownloadOverall?.Invoke(eventArgs);
            QuarkDownloadOverallProgressEventArgs.Release(eventArgs);
        }
        void OnDownloadedPendingFiles()
        {
            canDownload = false;
            Downloading = false;
            downloadEndTime = DateTime.Now;
            var eventArgs = QuarkAllDownloadCompletedEventArgs.Create(successURIs.ToArray(), failureURIs.ToArray(), downloadEndTime - downloadStartTime);
            onAllDownloadFinish?.Invoke(eventArgs);
            QuarkAllDownloadCompletedEventArgs.Release(eventArgs);
            pendingtasks.Clear();
            failureURIs.Clear();
            successURIs.Clear();
            downloadCount = 0;
        }
        void OnCancelDownload()
        {
            unityWebRequest?.Abort();
            downloadCount = 0;
            pendingtasks.Clear();
            failureURIs.Clear();
            successURIs.Clear();
            canDownload = false;
        }
    }
}
