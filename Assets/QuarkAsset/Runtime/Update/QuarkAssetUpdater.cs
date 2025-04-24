using System;
using System.IO;
using System.Collections.Generic;
using Quark.Asset;
using Quark.Manifest;
using Quark.Networking;

namespace Quark
{
    /// <summary>
    /// QuarkAsset更新器
    /// 用于处理资源的下载、更新、验证等操作
    /// </summary>
    public class QuarkAssetUpdater
    {
        #region 私有字段
        private string remoteUrl;
        private string persistentPath;
        private byte[] aesKeyBytes;
        private QuarkManifest remoteManifest;
        private List<QuarkUpdateTask> updateTasks = new List<QuarkUpdateTask>();
        private QuarkManifestVerifyResult verifyResult;
        private int currentDownloadIndex = -1;
        private int retryCount = 3;
        private int downloadTimeout = 30;
        private bool deleteFailureFile = true;
        private bool isUpdating = false;
        #endregion

        #region 事件
        /// <summary>
        /// 远程清单下载完成事件
        /// </summary>
        public event Action<QuarkManifest> OnRemoteManifestDownloaded;
        
        /// <summary>
        /// 远程清单下载失败事件
        /// </summary>
        public event Action<string> OnRemoteManifestDownloadFailed;
        
        /// <summary>
        /// 清单验证完成事件
        /// </summary>
        public event Action<QuarkManifestVerifyResult> OnManifestVerified;
        
        /// <summary>
        /// 下载开始事件
        /// </summary>
        public event Action OnDownloadStarted;
        
        /// <summary>
        /// 更新进度事件
        /// </summary>
        public event Action<QuarkUpdateProgressInfo> OnUpdateProgress;
        
        /// <summary>
        /// 更新完成事件
        /// </summary>
        public event Action<QuarkUpdateResult> OnUpdateCompleted;
        
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
        #endregion

        #region 属性
        /// <summary>
        /// 重试次数
        /// </summary>
        public int RetryCount
        {
            get { return retryCount; }
            set { retryCount = Math.Max(0, value); }
        }
        
        /// <summary>
        /// 下载超时时间(秒)
        /// </summary>
        public int DownloadTimeout
        {
            get { return downloadTimeout; }
            set { downloadTimeout = Math.Max(1, value); }
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
        /// 是否正在更新
        /// </summary>
        public bool IsUpdating
        {
            get { return isUpdating; }
        }
        
        /// <summary>
        /// 远程清单
        /// </summary>
        public QuarkManifest RemoteManifest
        {
            get { return remoteManifest; }
        }
        
        /// <summary>
        /// 验证结果
        /// </summary>
        public QuarkManifestVerifyResult VerifyResult
        {
            get { return verifyResult; }
        }
        #endregion

        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="remoteUrl">远程资源URL</param>
        /// <param name="persistentPath">本地持久化路径</param>
        public QuarkAssetUpdater(string remoteUrl, string persistentPath)
        {
            this.remoteUrl = remoteUrl;
            this.persistentPath = persistentPath;
            this.aesKeyBytes = new byte[0];
        }
        
        /// <summary>
        /// 构造函数(带AES加密)
        /// </summary>
        /// <param name="remoteUrl">远程资源URL</param>
        /// <param name="persistentPath">本地持久化路径</param>
        /// <param name="aesKey">AES加密密钥</param>
        public QuarkAssetUpdater(string remoteUrl, string persistentPath, string aesKey)
        {
            this.remoteUrl = remoteUrl;
            this.persistentPath = persistentPath;
            this.aesKeyBytes = QuarkUtility.GenerateBytesAESKey(aesKey);
        }
        
        /// <summary>
        /// 构造函数(带AES加密)
        /// </summary>
        /// <param name="remoteUrl">远程资源URL</param>
        /// <param name="persistentPath">本地持久化路径</param>
        /// <param name="aesKeyBytes">AES加密密钥字节数组</param>
        public QuarkAssetUpdater(string remoteUrl, string persistentPath, byte[] aesKeyBytes)
        {
            this.remoteUrl = remoteUrl;
            this.persistentPath = persistentPath;
            this.aesKeyBytes = aesKeyBytes;
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 开始更新
        /// </summary>
        /// <param name="localManifest">本地清单(可为null)</param>
        public void StartUpdate(QuarkManifest localManifest = null)
        {
            if (isUpdating)
            {
                QuarkUtility.LogWarning("更新器已经在运行，请先停止当前更新");
                return;
            }
            
            isUpdating = true;
            
            // 重置状态
            remoteManifest = null;
            verifyResult = null;
            updateTasks.Clear();
            currentDownloadIndex = -1;
            
            // 下载远程清单
            DownloadRemoteManifest(manifestData => {
                try
                {
                    // 解析清单
                    remoteManifest = ParseManifest(manifestData);
                    
                    if (remoteManifest != null)
                    {
                        // 触发清单下载完成事件
                        OnRemoteManifestDownloaded?.Invoke(remoteManifest);
                        
                        // 验证清单
                        VerifyManifest(localManifest, remoteManifest);
                        
                        // 准备下载任务
                        PrepareUpdateTasks();
                        
                        // 开始下载
                        if (updateTasks.Count > 0)
                        {
                            OnDownloadStarted?.Invoke();
                            DownloadNextResource();
                        }
                        else
                        {
                            // 没有需要下载的资源，完成更新
                            CompleteUpdate();
                        }
                    }
                    else
                    {
                        // 清单解析失败
                        OnRemoteManifestDownloadFailed?.Invoke("远程清单解析失败");
                        isUpdating = false;
                    }
                }
                catch (Exception ex)
                {
                    // 处理异常
                    OnRemoteManifestDownloadFailed?.Invoke($"处理远程清单时出错: {ex.Message}");
                    isUpdating = false;
                }
            }, error => {
                // 下载失败
                OnRemoteManifestDownloadFailed?.Invoke(error);
                isUpdating = false;
            });
        }
        
        /// <summary>
        /// 停止更新
        /// </summary>
        public void StopUpdate()
        {
            if (!isUpdating)
                return;
                
            isUpdating = false;
            
            // 可以在这里添加取消下载的代码
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 下载远程清单
        /// </summary>
        /// <param name="onSuccess">成功回调</param>
        /// <param name="onError">失败回调</param>
        private void DownloadRemoteManifest(Action<string> onSuccess, Action<string> onError)
        {
            try
            {
                // 构建清单URL
                string manifestUrl = QuarkUtility.WebPathCombine(remoteUrl, QuarkConstant.MANIFEST_NAME);
                
                // 创建下载节点
                var node = new QuarkDownloadNode
                {
                    DownloadUri = manifestUrl,
                    SavePath = Path.Combine(persistentPath, QuarkConstant.MANIFEST_NAME),
                    Timeout = downloadTimeout
                };
                
                // 确保目录存在
                var directory = Path.GetDirectoryName(node.SavePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                // 下载清单
                // 这里应该使用实际的下载方法，以下是示例
                UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequest.Get(node.DownloadUri);
                request.timeout = node.Timeout;
                
                var asyncOperation = request.SendWebRequest();
                asyncOperation.completed += (op) => {
                    try
                    {
                        if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                        {
                            string text = request.downloadHandler.text;
                            File.WriteAllText(node.SavePath, text);
                            onSuccess(text);
                        }
                        else
                        {
                            onError($"下载清单失败: {request.error}");
                        }
                    }
                    catch (Exception ex)
                    {
                        onError($"处理清单下载时出错: {ex.Message}");
                    }
                    finally
                    {
                        request.Dispose();
                    }
                };
            }
            catch (Exception ex)
            {
                onError($"下载清单时出错: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 解析清单数据
        /// </summary>
        /// <param name="manifestData">清单数据</param>
        /// <returns>清单对象</returns>
        private QuarkManifest ParseManifest(string manifestData)
        {
            try
            {
                // 如果有AES加密，先解密
                if (aesKeyBytes.Length > 0)
                {
                    manifestData = QuarkUtility.AESDecryptStringToString(manifestData, aesKeyBytes);
                }
                
                // 解析JSON
                return QuarkUtility.ToObject<QuarkManifest>(manifestData);
            }
            catch (Exception ex)
            {
                QuarkUtility.LogError($"解析清单失败: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// 验证清单
        /// </summary>
        /// <param name="localManifest">本地清单</param>
        /// <param name="remoteManifest">远程清单</param>
        private void VerifyManifest(QuarkManifest localManifest, QuarkManifest remoteManifest)
        {
            List<QuarkManifestVerifyInfo> successList = new List<QuarkManifestVerifyInfo>();
            List<QuarkManifestVerifyInfo> failureList = new List<QuarkManifestVerifyInfo>();
            
            // 如果本地清单为空，所有远程资源都需要下载
            if (localManifest == null)
            {
                foreach (var bundle in remoteManifest.BundleInfoDict.Values)
                {
                    var info = new QuarkManifestVerifyInfo(
                        bundle.BundleName,
                        bundle.QuarkAssetBundle.BundlePath,
                        bundle.Hash,
                        bundle.BundleSize,
                        false
                    );
                    failureList.Add(info);
                }
            }
            else
            {
                // 验证每个远程资源
                foreach (var remoteBundle in remoteManifest.BundleInfoDict.Values)
                {
                    if (localManifest.BundleInfoDict.TryGetValue(remoteBundle.QuarkAssetBundle.BundleKey, out var localBundle))
                    {
                        // 本地存在此资源，验证哈希和大小
                        bool hashMatch = remoteBundle.Hash == localBundle.Hash;
                        bool sizeMatch = remoteBundle.BundleSize == localBundle.BundleSize;
                        
                        var info = new QuarkManifestVerifyInfo(
                            remoteBundle.BundleName,
                            remoteBundle.QuarkAssetBundle.BundlePath,
                            remoteBundle.Hash,
                            remoteBundle.BundleSize,
                            sizeMatch
                        );
                        
                        if (hashMatch && sizeMatch)
                        {
                            successList.Add(info);
                        }
                        else
                        {
                            failureList.Add(info);
                        }
                    }
                    else
                    {
                        // 本地不存在此资源，需要下载
                        var info = new QuarkManifestVerifyInfo(
                            remoteBundle.BundleName,
                            remoteBundle.QuarkAssetBundle.BundlePath,
                            remoteBundle.Hash,
                            remoteBundle.BundleSize,
                            false
                        );
                        failureList.Add(info);
                    }
                }
            }
            
            // 创建验证结果
            verifyResult = new QuarkManifestVerifyResult(
                successList.ToArray(),
                failureList.ToArray()
            );
            
            // 触发验证完成事件
            OnManifestVerified?.Invoke(verifyResult);
        }
        
        /// <summary>
        /// 准备更新任务
        /// </summary>
        private void PrepareUpdateTasks()
        {
            updateTasks.Clear();
            
            if (verifyResult == null || remoteManifest == null)
                return;
                
            // 为每个需要更新的资源创建任务
            foreach (var info in verifyResult.VerificationFailureInfos)
            {
                if (remoteManifest.BundleInfoDict.TryGetValue(info.ResourceBundleName, out var bundle))
                {
                    string downloadUrl = QuarkUtility.WebPathCombine(remoteUrl, bundle.QuarkAssetBundle.BundleKey);
                    string localPath = Path.Combine(persistentPath, bundle.QuarkAssetBundle.BundleKey);
                    
                    var task = new QuarkUpdateTask(
                        info.ResourceBundleName,
                        downloadUrl,
                        localPath,
                        info.ResourceBundleSize
                    );
                    
                    updateTasks.Add(task);
                }
            }
        }
        
        /// <summary>
        /// 下载下一个资源
        /// </summary>
        private void DownloadNextResource()
        {
            currentDownloadIndex++;
            
            if (currentDownloadIndex >= updateTasks.Count)
            {
                // 所有资源下载完成
                CompleteUpdate();
                return;
            }
            
            var task = updateTasks[currentDownloadIndex];
            DownloadResource(task, 0);
        }
        
        /// <summary>
        /// 下载资源
        /// </summary>
        /// <param name="task">下载任务</param>
        /// <param name="retryAttempt">重试次数</param>
        private void DownloadResource(QuarkUpdateTask task, int retryAttempt)
        {
            if (!isUpdating)
                return;
                
            try
            {
                // 创建下载节点
                var node = new QuarkDownloadNode
                {
                    DownloadUri = task.DownloadUri,
                    SavePath = task.LocalPath,
                    Timeout = downloadTimeout
                };
                
                // 确保目录存在
                var directory = Path.GetDirectoryName(node.SavePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                // 触发资源下载开始事件
                OnResourceDownloadStarted?.Invoke(task);
                
                // 这里应该使用实际的下载方法，以下是示例
                UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequest.Get(node.DownloadUri);
                request.timeout = node.Timeout;
                
                var asyncOperation = request.SendWebRequest();
                
                // 进度回调
                asyncOperation.completed += (op) => {
                    try
                    {
                        if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                        {
                            // 下载成功
                            File.WriteAllBytes(node.SavePath, request.downloadHandler.data);
                            
                            // 标记任务成功
                            task.MarkAsSuccess();
                            
                            // 触发资源下载成功事件
                            OnResourceDownloadSucceeded?.Invoke(task);
                            
                            // 触发进度事件
                            NotifyProgress(task, node, 1f);
                            
                            // 下载下一个资源
                            DownloadNextResource();
                        }
                        else
                        {
                            // 下载失败
                            string errorMessage = request.error;
                            
                            // 如果需要重试
                            if (retryAttempt < RetryCount)
                            {
                                // 删除失败文件
                                if (DeleteFailureFile && File.Exists(node.SavePath))
                                {
                                    File.Delete(node.SavePath);
                                }
                                
                                // 重试下载
                                DownloadResource(task, retryAttempt + 1);
                            }
                            else
                            {
                                // 已达到最大重试次数，标记任务失败
                                task.MarkAsFailed(errorMessage);
                                
                                // 删除失败文件
                                if (DeleteFailureFile && File.Exists(node.SavePath))
                                {
                                    File.Delete(node.SavePath);
                                }
                                
                                // 触发资源下载失败事件
                                OnResourceDownloadFailed?.Invoke(task, errorMessage);
                                
                                // 下载下一个资源
                                DownloadNextResource();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // 处理异常
                        string errorMessage = $"下载资源时出错: {ex.Message}";
                        
                        // 标记任务失败
                        task.MarkAsFailed(errorMessage);
                        
                        // 触发资源下载失败事件
                        OnResourceDownloadFailed?.Invoke(task, errorMessage);
                        
                        // 下载下一个资源
                        DownloadNextResource();
                    }
                    finally
                    {
                        request.Dispose();
                    }
                };
            }
            catch (Exception ex)
            {
                // 处理异常
                string errorMessage = $"准备下载资源时出错: {ex.Message}";
                
                // 标记任务失败
                task.MarkAsFailed(errorMessage);
                
                // 触发资源下载失败事件
                OnResourceDownloadFailed?.Invoke(task, errorMessage);
                
                // 下载下一个资源
                DownloadNextResource();
            }
        }
        
        /// <summary>
        /// 通知进度
        /// </summary>
        /// <param name="task">下载任务</param>
        /// <param name="node">下载节点</param>
        /// <param name="currentProgress">当前进度</param>
        private void NotifyProgress(QuarkUpdateTask task, QuarkDownloadNode node, float currentProgress)
        {
            if (!isUpdating)
                return;
                
            // 计算总进度
            float totalProgress = (currentDownloadIndex + currentProgress) / updateTasks.Count;
            
            // 创建进度信息
            var progressInfo = new QuarkUpdateProgressInfo(
                currentDownloadIndex,
                updateTasks.Count,
                (long)(task.FileSize * currentProgress),
                task.FileSize,
                currentProgress,
                totalProgress,
                node
            );
            
            // 触发进度事件
            OnUpdateProgress?.Invoke(progressInfo);
        }
        
        /// <summary>
        /// 完成更新
        /// </summary>
        private void CompleteUpdate()
        {
            if (!isUpdating)
                return;
                
            isUpdating = false;
            
            // 创建更新结果
            var result = QuarkUpdateResult.FromTasks(updateTasks);
            
            // 触发更新完成事件
            OnUpdateCompleted?.Invoke(result);
        }
        #endregion
    }
}
