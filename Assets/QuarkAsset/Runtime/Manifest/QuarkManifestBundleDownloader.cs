using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
namespace Quark.Networking
{
    /// <summary>
    /// QuarkDownloader的替代方案，根据manifest下载文件
    /// </summary>
    public class QuarkManifestBundleDownloader
    {
        List<QuarkDownloadTask> pendingtasks = new List<QuarkDownloadTask>();
        Dictionary<string, QuarkDownloadTask> pendingTaskDict = new Dictionary<string, QuarkDownloadTask>();
        List<QuarkDownloadNode> successURIs = new List<QuarkDownloadNode>();
        List<QuarkDownloadNode> failureURIs = new List<QuarkDownloadNode>();
        UnityWebRequest unityWebRequest;

        public void LaunchDownload()
        {

        }
        public void AddDownloadFile()
        {

        }
//        IEnumerator EnumDownloadMultipleFiles()
//        {
//            while (pendingtasks.Count > 0)
//            {
//                var task = pendingtasks[0];
//                pendingtasks.RemoveAt(0);
//                currentDownloadIndex = downloadCount - pendingtasks.Count - 1;
//                var fileDownloadPath = Path.Combine(task.DownloadPath, task.URI);
//                var remoteUri = Path.Combine(URL, task.URI);
//                yield return EnumDownloadSingleFile(remoteUri, fileDownloadPath);
//                pendingTaskDict.Remove(task.URI);
//            }
//            OnDownloadedPendingFiles();
//        }
//        IEnumerator EnumDownloadSingleFile(string uri, string downloadPath)
//        {
//            using (UnityWebRequest request = UnityWebRequest.Get(uri))
//            {
//                var fileDownloadStartTime = DateTime.Now;
//                Downloading = true;
//#if UNITY_2019_1_OR_NEWER
//                request.downloadHandler = new DownloadHandlerFile(downloadPath, true);
//#elif UNITY_2018_1_OR_NEWER
//                request.downloadHandler = new DownloadHandlerFile(downloadPath);
//#endif
//                unityWebRequest = request;

//                var startEventArgs = QuarkDownloadStartEventArgs.Create(uri, downloadPath);
//                onDownloadStart?.Invoke(startEventArgs);
//                QuarkDownloadStartEventArgs.Release(startEventArgs);

//                //增量下载实现
//                //下载的路径是可IO的
//                var fileInfo = new FileInfo(downloadPath);
//                request.SetRequestHeader("Range", "bytes=" + fileInfo.Length + "-");

//                var operation = request.SendWebRequest();
//                while (!operation.isDone && canDownload)
//                {
//                    OnFileDownloading(uri, QuarkDataProxy.PersistentPath, request.downloadProgress, request.downloadedBytes);
//                    yield return null;
//                }
//#if UNITY_2020_1_OR_NEWER
//                if (request.result != UnityWebRequest.Result.ConnectionError && request.result != UnityWebRequest.Result.ProtocolError && canDownload)
//#elif UNITY_2018_1_OR_NEWER
//                if (!request.isNetworkError && !request.isHttpError && canDownload)
//#endif
//                {
//                    if (request.isDone)
//                    {
//                        Downloading = false;
//                        var fileDownloadEndTime = DateTime.Now;
//                        var timeSpan = fileDownloadEndTime - fileDownloadStartTime;
//                        var completedInfo = new QuarkDownloadCompletedInfo(uri, downloadPath, (long)request.downloadedBytes, timeSpan);
//                        var successEventArgs = QuarkDownloadSuccessEventArgs.Create(completedInfo);
//                        onDownloadSuccess?.Invoke(successEventArgs);
//                        QuarkDownloadSuccessEventArgs.Release(successEventArgs);
//                        OnFileDownloading(uri, QuarkDataProxy.PersistentPath, 1, request.downloadedBytes);
//                        successURIs.Add(completedInfo);
//                    }
//                }
//                else
//                {
//                    Downloading = false;
//                    var fileDownloadEndTime = DateTime.Now;
//                    var timeSpan = fileDownloadEndTime - fileDownloadStartTime;
//                    var completedInfo = new QuarkDownloadCompletedInfo(uri, downloadPath, (long)request.downloadedBytes, timeSpan);
//                    var failureEventArgs = QuarkDownloadFailureEventArgs.Create(completedInfo, request.error);
//                    onDownloadFailure?.Invoke(failureEventArgs);
//                    QuarkDownloadFailureEventArgs.Release(failureEventArgs);

//                    failureURIs.Add(completedInfo);
//                    OnFileDownloading(uri, QuarkDataProxy.PersistentPath, 1, request.downloadedBytes);
//                    if (DeleteFailureFile)
//                    {
//                        QuarkUtility.DeleteFile(downloadPath);
//                    }
//                }
//                unityWebRequest = null;
//            }
//        }
    }
}
