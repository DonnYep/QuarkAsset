﻿﻿﻿using Quark.Asset;
using Quark.Manifest;
using System;

namespace Quark
{
    /// <summary>
    /// 下载更新资源的操作
    /// 用于完成资源的热更新，包括下载清单、校验资源、下载更新等步骤
    /// </summary>
    public class DownloadUpdateAssetOperation : AsyncOperationBase
    {
        private string remoteUrl;
        private string persistentPath;
        private byte[] aesKeyBytes;
        
        private QuarkAssetUpdater updater;
        private QuarkManifest localManifest;
        
        /// <summary>
        /// 远程清单
        /// </summary>
        public QuarkManifest RemoteManifest { get; private set; }
        
        /// <summary>
        /// 资源校验结果
        /// </summary>
        public QuarkManifestVerifyResult VerifyResult { get; private set; }
        
        /// <summary>
        /// 下载进度 (0-1)
        /// </summary>
        public float DownloadProgress { get; private set; }
        
        /// <summary>
        /// 当前下载文件信息
        /// </summary>
        public string CurrentDownloadInfo { get; private set; }
        
        /// <summary>
        /// 更新结果
        /// </summary>
        public QuarkUpdateResult UpdateResult { get; private set; }
        
        /// <summary>
        /// 下载超时时间(秒)
        /// </summary>
        public int DownloadTimeout
        {
            get => updater.DownloadTimeout;
            set => updater.DownloadTimeout = value;
        }
        
        /// <summary>
        /// 下载失败是否删除文件
        /// </summary>
        public bool DeleteFailureFile
        {
            get => updater.DeleteFailureFile;
            set => updater.DeleteFailureFile = value;
        }

        /// <summary>
        /// 创建资源更新操作
        /// </summary>
        /// <param name="remoteUrl">远程资源URL</param>
        /// <param name="persistentPath">本地持久化路径</param>
        /// <param name="localManifest">本地清单（可为null，表示首次安装）</param>
        public DownloadUpdateAssetOperation(string remoteUrl, string persistentPath, QuarkManifest localManifest = null)
        {
            this.remoteUrl = remoteUrl;
            this.persistentPath = persistentPath;
            this.localManifest = localManifest;
            this.aesKeyBytes = new byte[0];
            
            this.updater = new QuarkAssetUpdater(remoteUrl, persistentPath);
            RegisterEvents();
        }
        
        /// <summary>
        /// 创建资源更新操作（带AES加密）
        /// </summary>
        /// <param name="remoteUrl">远程资源URL</param>
        /// <param name="persistentPath">本地持久化路径</param>
        /// <param name="aesKey">AES加密密钥</param>
        /// <param name="localManifest">本地清单（可为null，表示首次安装）</param>
        public DownloadUpdateAssetOperation(string remoteUrl, string persistentPath, string aesKey, QuarkManifest localManifest = null)
        {
            this.remoteUrl = remoteUrl;
            this.persistentPath = persistentPath;
            this.localManifest = localManifest;
            this.aesKeyBytes = QuarkUtility.GenerateBytesAESKey(aesKey);
            
            this.updater = new QuarkAssetUpdater(remoteUrl, persistentPath, aesKey);
            RegisterEvents();
        }
        
        /// <summary>
        /// 创建资源更新操作（带AES加密）
        /// </summary>
        /// <param name="remoteUrl">远程资源URL</param>
        /// <param name="persistentPath">本地持久化路径</param>
        /// <param name="aesKeyBytes">AES加密密钥字节数组</param>
        /// <param name="localManifest">本地清单（可为null，表示首次安装）</param>
        public DownloadUpdateAssetOperation(string remoteUrl, string persistentPath, byte[] aesKeyBytes, QuarkManifest localManifest = null)
        {
            this.remoteUrl = remoteUrl;
            this.persistentPath = persistentPath;
            this.localManifest = localManifest;
            this.aesKeyBytes = aesKeyBytes;
            
            this.updater = new QuarkAssetUpdater(remoteUrl, persistentPath, aesKeyBytes);
            RegisterEvents();
        }

        internal override void OnStart()
        {
            Status = AsyncOperationStatus.Processing;
            updater.StartUpdate(localManifest);
        }

        internal override void OnUpdate()
        {
            // 操作系统会在每帧调用此方法，无需在此处实现额外逻辑
            // 所有状态更新通过事件处理
        }

        internal override void OnAbort()
        {
            updater.StopUpdate();
            Status = AsyncOperationStatus.Failed;
            Error = "Operation aborted";
        }
        
        private void RegisterEvents()
        {
            updater.OnRemoteManifestDownloaded += OnRemoteManifestDownloaded;
            updater.OnRemoteManifestDownloadFailed += OnRemoteManifestDownloadFailed;
            updater.OnManifestVerified += OnManifestVerified;
            updater.OnUpdateProgress += OnUpdateProgress;
            updater.OnUpdateCompleted += OnUpdateCompleted;
        }
        
        private void OnRemoteManifestDownloaded(QuarkManifest manifest)
        {
            RemoteManifest = manifest;
        }
        
        private void OnRemoteManifestDownloadFailed(string errorMessage)
        {
            Error = $"下载远程清单失败: {errorMessage}";
            Status = AsyncOperationStatus.Failed;
        }
        
        private void OnManifestVerified(QuarkManifestVerifyResult result)
        {
            VerifyResult = result;
        }
        
        private void OnUpdateProgress(QuarkUpdateProgressInfo info)
        {
            DownloadProgress = info.TotalProgress;
            CurrentDownloadInfo = $"{info.CurrentDownloadIndex+1}/{info.TotalDownloadCount} {info.Node.DownloadUri}";
            
            // 更新进度
            Progress = DownloadProgress;
        }
        
        private void OnUpdateCompleted(QuarkUpdateResult result)
        {
            UpdateResult = result;
            
            if (result.IsCompleteSuccess)
            {
                Status = AsyncOperationStatus.Succeeded;
            }
            else
            {
                Error = $"资源更新失败: {result.FailedTasks.Length} 个文件下载失败";
                Status = AsyncOperationStatus.Failed;
            }
        }
    }
}
