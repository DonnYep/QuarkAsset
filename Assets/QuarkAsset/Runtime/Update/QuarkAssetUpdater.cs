using Quark.Asset;
using Quark.Manifest;
using Quark.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Quark
{
    /// <summary>
    /// QuarkAsset资源更新管理器
    /// 提供统一的资源热更新流程，包括下载清单、校验资源、下载更新等功能
    /// </summary>
    public class QuarkAssetUpdater
    {
        #region 事件
        /// <summary>
        /// 更新开始事件
        /// </summary>
        public event Action OnUpdateStart;
        
        /// <summary>
        /// 远程清单下载成功事件
        /// </summary>
        public event Action<QuarkManifest> OnRemoteManifestDownloaded;
        
        /// <summary>
        /// 远程清单下载失败事件
        /// </summary>
        public event Action<string> OnRemoteManifestDownloadFailed;
        
        /// <summary>
        /// 资源校验完成事件
        /// </summary>
        public event Action<QuarkManifestVerifyResult> OnManifestVerified;
        
        /// <summary>
        /// 资源更新进度事件
        /// </summary>
        public event Action<QuarkUpdateProgressInfo> OnUpdateProgress;
        
        /// <summary>
        /// 资源更新完成事件
        /// </summary>
        public event Action<QuarkUpdateResult> OnUpdateCompleted;

        /// <summary>
        /// 下载任务准备完成事件
        /// </summary>
        public event Action<int> OnDownloadTasksPrepared;
        #endregion

        private string remoteUrl;
        private string persistentPath;
        private byte[] aesKeyBytes;
        private int downloadTimeout = 30;
        private bool deleteFailureFile = true;
        private int maxRetryCount = 3;
        private float retryDelay = 1f;
        
        private QuarkManifest localManifest;
        private QuarkManifest remoteManifest;
        private QuarkManifestVerifyResult verifyResult;
        private QuarkAssetDownloader assetDownloader;
        
        private bool isUpdating = false;
        private List<QuarkDownloadTask> downloadTasks = new List<QuarkDownloadTask>();
        private Dictionary<string, int> retryCountDict = new Dictionary<string, int>();

        /// <summary>
        /// 是否正在更新
        /// </summary>
        public bool IsUpdating => isUpdating;

        /// <summary>
        /// 下载超时时间(秒)
        /// </summary>
        public int DownloadTimeout
        {
            get => downloadTimeout;
            set => downloadTimeout = value > 0 ? value : 30;
        }

        /// <summary>
        /// 下载失败是否删除文件
        /// </summary>
        public bool DeleteFailureFile
        {
            get => deleteFailureFile;
            set => deleteFailureFile = value;
        }

        /// <summary>
        /// 最大重试次数
        /// </summary>
        public int MaxRetryCount
        {
            get => maxRetryCount;
            set => maxRetryCount = value >= 0 ? value : 0;
        }

        /// <summary>
        /// 重试延迟时间(秒)
        /// </summary>
        public float RetryDelay
        {
            get => retryDelay;
            set => retryDelay = value >= 0 ? value : 0;
        }

        /// <summary>
        /// 创建资源更新管理器
        /// </summary>
        /// <param name="remoteUrl">远程资源URL</param>
        /// <param name="persistentPath">本地持久化路径</param>
        public QuarkAssetUpdater(string remoteUrl, string persistentPath)
        {
            this.remoteUrl = remoteUrl;
            this.persistentPath = persistentPath;
            this.aesKeyBytes = new byte[0];
            this.assetDownloader = new QuarkAssetDownloader();
            
            InitDownloader();
        }

        /// <summary>
        /// 创建资源更新管理器（带AES加密）
        /// </summary>
        /// <param name="remoteUrl">远程资源URL</param>
        /// <param name="persistentPath">本地持久化路径</param>
        /// <param name="aesKey">AES加密密钥</param>
        public QuarkAssetUpdater(string remoteUrl, string persistentPath, string aesKey)
        {
            this.remoteUrl = remoteUrl;
            this.persistentPath = persistentPath;
            this.aesKeyBytes = QuarkUtility.GenerateBytesAESKey(aesKey);
            this.assetDownloader = new QuarkAssetDownloader();
            
            InitDownloader();
        }

        /// <summary>
        /// 创建资源更新管理器（带AES加密）
        /// </summary>
        /// <param name="remoteUrl">远程资源URL</param>
        /// <param name="persistentPath">本地持久化路径</param>
        /// <param name="aesKeyBytes">AES加密密钥字节数组</param>
        public QuarkAssetUpdater(string remoteUrl, string persistentPath, byte[] aesKeyBytes)
        {
            this.remoteUrl = remoteUrl;
            this.persistentPath = persistentPath;
            this.aesKeyBytes = aesKeyBytes;
            this.assetDownloader = new QuarkAssetDownloader();
            
            InitDownloader();
        }

        /// <summary>
        /// 开始资源更新流程
        /// </summary>
        /// <param name="localManifest">本地清单（如果为null，则表示首次安装）</param>
        public void StartUpdate(QuarkManifest localManifest = null)
        {
            if (isUpdating)
                return;

            isUpdating = true;
            this.localManifest = localManifest;
            OnUpdateStart?.Invoke();
            
            // 开始下载远程清单
            DownloadRemoteManifest();
        }

        /// <summary>
        /// 停止资源更新流程
        /// </summary>
        public void StopUpdate()
        {
            if (!isUpdating)
                return;

            isUpdating = false;
            QuarkResources.QuarkManifestRequester.StopRequestManifest();
            QuarkResources.QuarkManifestVerifier.StopVerify();
            assetDownloader.StopDownload();
            
            // 清理资源
            retryCountDict.Clear();
            downloadTasks.Clear();
        }

        #region 私有方法
        private void InitDownloader()
        {
            assetDownloader.DownloadTimeout = downloadTimeout;
            assetDownloader.DeleteFailureFile = deleteFailureFile;
            
            assetDownloader.OnDownloadUpdate += (args) => {
                var progressInfo = new QuarkUpdateProgressInfo(
                    args.CurrentDownloadNode,
                    args.CurrentDownloadIndex,
                    args.TotalDownloadCount,
                    args.CompletedDownloadSize,
                    args.TotalRequiredDownloadSize
                );
                OnUpdateProgress?.Invoke(progressInfo);
            };
            
            assetDownloader.OnDownloadFailure += (args) => {
                var downloadUri = args.CurrentDownloadNode.DownloadUri;
                var downloadPath = args.CurrentDownloadNode.DownloadPath;
                
                // 获取当前重试次数
                if (!retryCountDict.TryGetValue(downloadUri, out int retryCount))
                {
                    retryCount = 0;
                    retryCountDict[downloadUri] = retryCount;
                }
                
                // 如果还有重试次数，则重新添加到下载队列
                if (retryCount < maxRetryCount)
                {
                    retryCountDict[downloadUri] = retryCount + 1;
                    
                    // 使用协程延迟重试
                    QuarkUtility.Unity.StartCoroutine(DelayRetry(downloadUri, downloadPath));
                }
            };
            
            assetDownloader.OnDownloadStart += (args) => {
                // 记录下载开始
                QuarkUtility.LogInfo($"开始下载: {args.CurrentDownloadNode.DownloadUri}");
            };
            
            assetDownloader.OnDownloadSuccess += (args) => {
                // 记录下载成功
                QuarkUtility.LogInfo($"下载成功: {args.CurrentDownloadNode.DownloadUri}");
            };
            
            assetDownloader.OnDownloadAllTasksFinish += (args) => {
                var updateResult = new QuarkUpdateResult(
                    args.DownloadSuccessedTasks,
                    args.DownloadFailedTasks,
                    args.DownloadSuccessedNodes,
                    args.DownloadFailedNodes,
                    args.DownloadedSize,
                    args.DownloadAllTasksCompletedTimeSpan
                );
                isUpdating = false;
                retryCountDict.Clear();
                OnUpdateCompleted?.Invoke(updateResult);
            };
        }

        private IEnumerator DelayRetry(string downloadUri, string downloadPath)
        {
            // 延迟一段时间后重试
            yield return new WaitForSeconds(retryDelay);
            
            if (!isUpdating)
                yield break;
                
            // 获取远程清单中的资源信息
            foreach (var bundleInfo in remoteManifest.BundleInfoDict.Values)
            {
                var bundleKey = bundleInfo.QuarkAssetBundle.BundleKey;
                var bundleUri = QuarkUtility.WebPathCombine(remoteUrl, bundleKey);
                
                if (bundleUri == downloadUri)
                {
                    // 创建新的下载任务
                    var task = new QuarkDownloadTask(downloadUri, downloadPath, bundleInfo.BundleSize);
                    assetDownloader.AddDownload(task);
                    assetDownloader.StartDownload();
                    break;
                }
            }
        }

        private void DownloadRemoteManifest()
        {
            var operation = new DownloadRemoteManifestOperation(remoteUrl, aesKeyBytes);
            operation.Completed += (op) => {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    remoteManifest = operation.Manifest;
                    OnRemoteManifestDownloaded?.Invoke(remoteManifest);
                    
                    // 验证资源
                    VerifyAssets();
                }
                else
                {
                    isUpdating = false;
                    OnRemoteManifestDownloadFailed?.Invoke(op.Error);
                }
            };
            QuarkResources.EnqueueOperation(operation);
        }

        private void VerifyAssets()
        {
            if (remoteManifest == null)
            {
                isUpdating = false;
                OnRemoteManifestDownloadFailed?.Invoke("Remote manifest is null");
                return;
            }

            // 如果是首次安装，不需要验证资源，直接下载所有资源
            if (localManifest == null)
            {
                PrepareDownloadTasks(remoteManifest, new QuarkManifestVerifyResult());
                return;
            }

            // 验证本地资源
            QuarkResources.QuarkManifestVerifier.OnVerifyDone += OnVerifyComplete;
            QuarkResources.QuarkManifestVerifier.VerifyManifest(remoteManifest, persistentPath);
        }

        private void OnVerifyComplete(QuarkManifestVerifyResult result)
        {
            QuarkResources.QuarkManifestVerifier.OnVerifyDone -= OnVerifyComplete;
            verifyResult = result;
            OnManifestVerified?.Invoke(result);
            
            // 准备下载任务
            PrepareDownloadTasks(remoteManifest, result);
        }

        private void PrepareDownloadTasks(QuarkManifest manifest, QuarkManifestVerifyResult verifyResult)
        {
            downloadTasks.Clear();
            
            // 确保持久化目录存在
            if (!Directory.Exists(persistentPath))
            {
                try
                {
                    Directory.CreateDirectory(persistentPath);
                }
                catch (Exception e)
                {
                    isUpdating = false;
                    OnUpdateCompleted?.Invoke(new QuarkUpdateResult(
                        new QuarkDownloadTask[0],
                        new QuarkDownloadTask[0],
                        new QuarkDownloadNode[0],
                        new QuarkDownloadNode[0],
                        0,
                        TimeSpan.Zero
                    ));
                    OnRemoteManifestDownloadFailed?.Invoke($"Failed to create persistent directory: {e.Message}");
                    return;
                }
            }
            
            // 如果是首次安装，或者本地资源不完整，则需要下载所有资源
            if (localManifest == null)
            {
                // 添加所有资源到下载列表
                foreach (var bundleInfo in manifest.BundleInfoDict.Values)
                {
                    var bundleKey = bundleInfo.QuarkAssetBundle.BundleKey;
                    var bundleSize = bundleInfo.BundleSize;
                    var downloadUri = QuarkUtility.WebPathCombine(remoteUrl, bundleKey);
                    var downloadPath = Path.Combine(persistentPath, bundleKey);
                    
                    // 确保目录存在
                    var directory = Path.GetDirectoryName(downloadPath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        try
                        {
                            Directory.CreateDirectory(directory);
                        }
                        catch (Exception e)
                        {
                            QuarkUtility.LogWarning($"Failed to create directory {directory}: {e.Message}");
                            continue;
                        }
                    }
                    
                    downloadTasks.Add(new QuarkDownloadTask(downloadUri, downloadPath, bundleSize));
                }
            }
            else
            {
                // 比较本地和远程清单，仅下载需要更新的资源
                foreach (var bundleInfo in manifest.BundleInfoDict.Values)
                {
                    var bundleKey = bundleInfo.QuarkAssetBundle.BundleKey;
                    var bundleSize = bundleInfo.BundleSize;
                    
                    bool needDownload = !localManifest.BundleInfoDict.ContainsKey(bundleKey);
                    
                    if (!needDownload)
                    {
                        var localBundleInfo = localManifest.BundleInfoDict[bundleKey];
                        
                        // 比较Hash或者大小，判断是否需要更新
                        if (localBundleInfo.BundleHash != bundleInfo.BundleHash || 
                            localBundleInfo.BundleSize != bundleInfo.BundleSize)
                        {
                            needDownload = true;
                        }
                    }
                    
                    // 检查验证失败的资源
                    if (verifyResult.VerificationFailureInfos != null)
                    {
                        foreach (var failInfo in verifyResult.VerificationFailureInfos)
                        {
                            if (failInfo.ResourceBundleName == bundleInfo.QuarkAssetBundle.BundleName)
                            {
                                needDownload = true;
                                break;
                            }
                        }
                    }
                    
                    // 检查验证成功但大小不匹配的资源
                    if (verifyResult.VerificationSuccessInfos != null)
                    {
                        foreach (var successInfo in verifyResult.VerificationSuccessInfos)
                        {
                            if (successInfo.ResourceBundleName == bundleInfo.QuarkAssetBundle.BundleName && 
                                !successInfo.ResourceBundleSizeMatched)
                            {
                                needDownload = true;
                                break;
                            }
                        }
                    }
                    
                    if (needDownload)
                    {
                        var downloadUri = QuarkUtility.WebPathCombine(remoteUrl, bundleKey);
                        var downloadPath = Path.Combine(persistentPath, bundleKey);
                        
                        // 确保目录存在
                        var directory = Path.GetDirectoryName(downloadPath);
                        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                        {
                            try
                            {
                                Directory.CreateDirectory(directory);
                            }
                            catch (Exception e)
                            {
                                QuarkUtility.LogWarning($"Failed to create directory {directory}: {e.Message}");
                                continue;
                            }
                        }
                        
                        downloadTasks.Add(new QuarkDownloadTask(downloadUri, downloadPath, bundleSize));
                    }
                }
            }
            
            // 通知准备完成
            OnDownloadTasksPrepared?.Invoke(downloadTasks.Count);
            
            // 开始下载资源
            if (downloadTasks.Count > 0)
            {
                assetDownloader.AddDownloads(downloadTasks);
                assetDownloader.StartDownload();
            }
            else
            {
                // 没有需要更新的资源，直接完成
                isUpdating = false;
                OnUpdateCompleted?.Invoke(new QuarkUpdateResult(
                    new QuarkDownloadTask[0],
                    new QuarkDownloadTask[0],
                    new QuarkDownloadNode[0],
                    new QuarkDownloadNode[0],
                    0,
                    TimeSpan.Zero
                ));
            }
        }
        
        /// <summary>
        /// 获取当前下载的进度信息
        /// </summary>
        /// <returns>下载进度信息</returns>
        public QuarkUpdateProgressInfo GetUpdateProgressInfo()
        {
            if (!isUpdating)
                return new QuarkUpdateProgressInfo();
                
            // 获取当前进度
            var currentNode = new QuarkDownloadNode();
            var currentIndex = 0;
            var totalCount = downloadTasks.Count;
            var downloadedSize = 0L;
            var totalSize = 0L;
            
            foreach (var task in downloadTasks)
            {
                totalSize += task.RecordedBundleSize;
            }
            
            return new QuarkUpdateProgressInfo(currentNode, currentIndex, totalCount, downloadedSize, totalSize);
        }
        #endregion
    }

    /// <summary>
    /// 资源更新进度信息
    /// </summary>
    public struct QuarkUpdateProgressInfo
    {
        /// <summary>
        /// 当前下载节点
        /// </summary>
        public QuarkDownloadNode Node { get; private set; }
        
        /// <summary>
        /// 当前下载索引
        /// </summary>
        public int CurrentDownloadIndex { get; private set; }
        
        /// <summary>
        /// 总下载数量
        /// </summary>
        public int TotalDownloadCount { get; private set; }
        
        /// <summary>
        /// 已下载大小
        /// </summary>
        public long DownloadedSize { get; private set; }
        
        /// <summary>
        /// 总需下载大小
        /// </summary>
        public long TotalRequiredDownloadSize { get; private set; }
        
        /// <summary>
        /// 总进度 (0-1)
        /// </summary>
        public float TotalProgress
        {
            get
            {
                if (TotalRequiredDownloadSize <= 0)
                    return 1f;
                return (float)DownloadedSize / TotalRequiredDownloadSize;
            }
        }

        public QuarkUpdateProgressInfo(QuarkDownloadNode node, int currentIndex, int totalCount, 
            long downloadedSize, long totalSize)
        {
            Node = node;
            CurrentDownloadIndex = currentIndex;
            TotalDownloadCount = totalCount;
            DownloadedSize = downloadedSize;
            TotalRequiredDownloadSize = totalSize;
        }
    }

    /// <summary>
    /// 资源更新结果
    /// </summary>
    public struct QuarkUpdateResult
    {
        /// <summary>
        /// 成功的下载任务
        /// </summary>
        public QuarkDownloadTask[] SuccessedTasks { get; private set; }
        
        /// <summary>
        /// 失败的下载任务
        /// </summary>
        public QuarkDownloadTask[] FailedTasks { get; private set; }
        
        /// <summary>
        /// 成功的下载节点
        /// </summary>
        public QuarkDownloadNode[] SuccessedNodes { get; private set; }
        
        /// <summary>
        /// 失败的下载节点
        /// </summary>
        public QuarkDownloadNode[] FailedNodes { get; private set; }
        
        /// <summary>
        /// 已下载大小
        /// </summary>
        public long DownloadedSize { get; private set; }
        
        /// <summary>
        /// 下载耗时
        /// </summary>
        public TimeSpan DownloadTimeSpan { get; private set; }
        
        /// <summary>
        /// 是否完全成功
        /// </summary>
        public bool IsCompleteSuccess => FailedTasks == null || FailedTasks.Length == 0;

        public QuarkUpdateResult(QuarkDownloadTask[] successTasks, QuarkDownloadTask[] failTasks, 
            QuarkDownloadNode[] successNodes, QuarkDownloadNode[] failNodes, 
            long downloadedSize, TimeSpan timeSpan)
        {
            SuccessedTasks = successTasks;
            FailedTasks = failTasks;
            SuccessedNodes = successNodes;
            FailedNodes = failNodes;
            DownloadedSize = downloadedSize;
            DownloadTimeSpan = timeSpan;
        }
        
        /// <summary>
        /// 获取失败任务信息字符串
        /// </summary>
        /// <returns>失败任务信息</returns>
        public string GetFailedTasksInfo()
        {
            if (IsCompleteSuccess)
                return "No failed tasks";
                
            var result = $"Failed {FailedTasks.Length} tasks:\n";
            for (int i = 0; i < FailedTasks.Length; i++)
            {
                result += $"  {i+1}. {FailedTasks[i].DownloadUri}\n";
            }
            return result;
        }
    }
}
