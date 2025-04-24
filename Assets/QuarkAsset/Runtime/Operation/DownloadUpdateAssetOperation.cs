﻿using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine.Networking;
using Quark.Manifest;

namespace Quark
{
    /// <summary>
    /// 下载更新资源操作
    /// </summary>
    public class DownloadUpdateAssetOperation : AsyncOperationBase
    {
        private string remoteUrl;
        private string persistentPath;
        private QuarkManifestVerifyResult verifyResult;
        private List<QuarkUpdateTask> updateTasks = new List<QuarkUpdateTask>();
        private List<QuarkUpdateTask> successTasks = new List<QuarkUpdateTask>();
        private List<QuarkUpdateTask> failedTasks = new List<QuarkUpdateTask>();
        private int currentTaskIndex = -1;
        private int retryCount = 3;
        private int timeoutSeconds = 30;
        private bool deleteFailureFile = true;
        private int currentRetryAttempt = 0;
        private UnityWebRequest webRequest;
        private QuarkUpdateTask currentTask;
        
        /// <summary>
        /// 更新进度事件
        /// </summary>
        public event Action<QuarkUpdateProgressInfo> OnUpdateProgress;
        
        /// <summary>
        /// 单个资源下载开始事件
        /// </summary>
        public event Action<QuarkUpdateTask> OnResourceDownloadStarted;
        
        /// <summary>
        /// 单个资源下载成功事件
        /// </summary>
        public event Action<QuarkUpdateTask> OnResourceDownloadSucceeded;
        
        /// <summary>
        /// 单个资源下载失败事件
        /// </summary>
        public event Action<QuarkUpdateTask, string> OnResourceDownloadFailed;
        
        /// <summary>
        /// 更新任务列表
        /// </summary>
        public List<QuarkUpdateTask> UpdateTasks => updateTasks;
        
        /// <summary>
        /// 成功的更新任务列表
        /// </summary>
        public List<QuarkUpdateTask> SuccessTasks => successTasks;
        
        /// <summary>
        /// 失败的更新任务列表
        /// </summary>
        public List<QuarkUpdateTask> FailedTasks => failedTasks;
        
        /// <summary>
        /// 更新结果
        /// </summary>
        public QuarkUpdateResult UpdateResult { get; private set; }
        
        /// <summary>
        /// 重试次数
        /// </summary>
        public int RetryCount
        {
            get { return retryCount; }
            set { retryCount = Math.Max(0, value); }
        }
        
        /// <summary>
        /// 超时时间（秒）
        /// </summary>
        public int TimeoutSeconds
        {
            get { return timeoutSeconds; }
            set { timeoutSeconds = Math.Max(1, value); }
        }
        
        /// <summary>
        /// 下载失败是否删除文件
        /// </summary>
        public bool DeleteFailureFile
        {
            get { return deleteFailureFile; }
            set { deleteFailureFile = value; }
        }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="remoteUrl">远程URL</param>
        /// <param name="persistentPath">持久化路径</param>
        /// <param name="verifyResult">验证结果</param>
        public DownloadUpdateAssetOperation(string remoteUrl, string persistentPath, QuarkManifestVerifyResult verifyResult)
        {
            this.remoteUrl = remoteUrl;
            this.persistentPath = persistentPath;
            this.verifyResult = verifyResult;
            
            // 准备更新任务
            PrepareUpdateTasks();
        }
        
        internal override void OnStart()
        {
            Status = AsyncOperationStatus.Processing;
            
            if (updateTasks.Count == 0)
            {
                // 没有需要更新的资源，直接完成
                CompleteOperation();
                return;
            }
            
            // 开始下载第一个资源
            currentTaskIndex = -1;
            DownloadNextResource();
        }
        
        internal override void OnUpdate()
        {
            if (Status != AsyncOperationStatus.Processing)
                return;
                
            if (webRequest != null && currentTask != null)
            {
                // 更新进度
                var downloadProgress = webRequest.downloadProgress;
                var progressInfo = CreateProgressInfo(currentTask, downloadProgress);
                
                // 更新总进度
                Progress = (currentTaskIndex + downloadProgress) / updateTasks.Count;
                
                // 触发进度事件
                OnUpdateProgress?.Invoke(progressInfo);
            }
        }
        
        internal override void OnAbort()
        {
            if (webRequest != null)
            {
                webRequest.Dispose();
                webRequest = null;
            }
            
            Status = AsyncOperationStatus.Failed;
            Error = "Operation aborted";
        }
        
        /// <summary>
        /// 准备更新任务
        /// </summary>
        private void PrepareUpdateTasks()
        {
            updateTasks.Clear();
            successTasks.Clear();
            failedTasks.Clear();
            
            if (verifyResult == null)
                return;
                
            // 为每个需要更新的资源创建任务
            foreach (var info in verifyResult.VerificationFailureInfos)
            {
                string downloadUrl = QuarkUtility.WebPathCombine(remoteUrl, info.ResourcePath);
                string localPath = Path.Combine(persistentPath, info.ResourcePath);
                
                var task = new QuarkUpdateTask(
                    info.ResourceBundleName,
                    downloadUrl,
                    localPath,
                    info.ResourceBundleSize
                );
                
                updateTasks.Add(task);
            }
        }
        
        /// <summary>
        /// 下载下一个资源
        /// </summary>
        private void DownloadNextResource()
        {
            if (Status != AsyncOperationStatus.Processing)
                return;
                
            currentTaskIndex++;
            currentRetryAttempt = 0;
            
            if (currentTaskIndex >= updateTasks.Count)
            {
                // 所有资源下载完成
                CompleteOperation();
                return;
            }
            
            currentTask = updateTasks[currentTaskIndex];
            DownloadResource();
        }
        
        /// <summary>
        /// 下载资源
        /// </summary>
        private void DownloadResource()
        {
            if (Status != AsyncOperationStatus.Processing)
                return;
                
            try
            {
                // 确保目录存在
                string directory = Path.GetDirectoryName(currentTask.LocalPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                // 触发资源下载开始事件
                OnResourceDownloadStarted?.Invoke(currentTask);
                
                // 创建下载请求
                webRequest = UnityWebRequest.Get(currentTask.DownloadUri);
                webRequest.timeout = timeoutSeconds;
                
                // 发送请求
                var operation = webRequest.SendWebRequest();
                operation.completed += OnRequestCompleted;
            }
            catch (Exception ex)
            {
                HandleDownloadError($"准备下载资源时出错: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 处理下载错误
        /// </summary>
        /// <param name="errorMessage">错误信息</param>
        private void HandleDownloadError(string errorMessage)
        {
            if (Status != AsyncOperationStatus.Processing)
                return;
                
            // 释放请求
            if (webRequest != null)
            {
                webRequest.Dispose();
                webRequest = null;
            }
            
            // 如果可以重试
            if (currentRetryAttempt < retryCount)
            {
                currentRetryAttempt++;
                
                // 删除失败文件
                if (deleteFailureFile && File.Exists(currentTask.LocalPath))
                {
                    File.Delete(currentTask.LocalPath);
                }
                
                // 重试下载
                DownloadResource();
            }
            else
            {
                // 达到最大重试次数，标记任务失败
                currentTask.MarkAsFailed(errorMessage);
                failedTasks.Add(currentTask);
                
                // 删除失败文件
                if (deleteFailureFile && File.Exists(currentTask.LocalPath))
                {
                    File.Delete(currentTask.LocalPath);
                }
                
                // 触发资源下载失败事件
                OnResourceDownloadFailed?.Invoke(currentTask, errorMessage);
                
                // 下载下一个资源
                DownloadNextResource();
            }
        }
        
        /// <summary>
        /// 请求完成回调
        /// </summary>
        /// <param name="operation">异步操作</param>
        private void OnRequestCompleted(UnityEngine.AsyncOperation operation)
        {
            if (Status != AsyncOperationStatus.Processing || webRequest == null || currentTask == null)
                return;
                
            try
            {
                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    // 下载成功，保存文件
                    File.WriteAllBytes(currentTask.LocalPath, webRequest.downloadHandler.data);
                    
                    // 标记任务成功
                    currentTask.MarkAsSuccess();
                    successTasks.Add(currentTask);
                    
                    // 触发资源下载成功事件
                    OnResourceDownloadSucceeded?.Invoke(currentTask);
                    
                    // 触发进度事件
                    var progressInfo = CreateProgressInfo(currentTask, 1f);
                    OnUpdateProgress?.Invoke(progressInfo);
                    
                    // 释放请求
                    webRequest.Dispose();
                    webRequest = null;
                    
                    // 下载下一个资源
                    DownloadNextResource();
                }
                else
                {
                    // 下载失败
                    HandleDownloadError($"下载资源失败: {webRequest.error}");
                }
            }
            catch (Exception ex)
            {
                // 处理异常
                HandleDownloadError($"保存资源时出错: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 创建进度信息
        /// </summary>
        /// <param name="task">当前任务</param>
        /// <param name="currentProgress">当前进度</param>
        /// <returns>进度信息</returns>
        private QuarkUpdateProgressInfo CreateProgressInfo(QuarkUpdateTask task, float currentProgress)
        {
            // 创建下载节点
            var node = new QuarkDownloadNode
            {
                DownloadUri = task.DownloadUri,
                SavePath = task.LocalPath,
                Timeout = timeoutSeconds,
                Progress = currentProgress,
                DownloadedBytes = (long)(task.FileSize * currentProgress),
                TotalBytes = task.FileSize,
                Status = currentProgress >= 1f ? DownloadStatus.Completed : DownloadStatus.Downloading
            };
            
            // 计算总进度
            float totalProgress = (currentTaskIndex + currentProgress) / updateTasks.Count;
            
            // 创建进度信息
            return new QuarkUpdateProgressInfo(
                currentTaskIndex,
                updateTasks.Count,
                node.DownloadedBytes,
                node.TotalBytes,
                currentProgress,
                totalProgress,
                node
            );
        }
        
        /// <summary>
        /// 完成操作
        /// </summary>
        private void CompleteOperation()
        {
            // 创建更新结果
            UpdateResult = new QuarkUpdateResult(
                successTasks.ToArray(),
                failedTasks.ToArray()
            );
            
            // 设置状态
            Status = UpdateResult.FailedCount > 0 ? AsyncOperationStatus.Failed : AsyncOperationStatus.Succeeded;
            
            if (Status == AsyncOperationStatus.Failed)
            {
                Error = $"有 {UpdateResult.FailedCount} 个资源下载失败";
            }
            
            // 更新进度为100%
            Progress = 1f;
            
            // 设置完成
            SetFinish();
        }
    }
}
