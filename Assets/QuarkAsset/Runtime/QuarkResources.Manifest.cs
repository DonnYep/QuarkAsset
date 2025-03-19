﻿namespace Quark
{
    public static partial class QuarkResources
    {
        /// <summary>
        /// 下载远程清单
        /// </summary>
        /// <param name="url">远程URL</param>
        /// <returns>下载远程清单的操作</returns>
        public static DownloadRemoteManifestOperation DownloadRemoteManifest(string url)
        {
            var op = new DownloadRemoteManifestOperation(url);
            QuarkUtility.Unity.CheckCoroutineProvider();
            OperationSystem.StartOperation(op);
            return op;
        }
        
        /// <summary>
        /// 下载远程清单（使用AES加密）
        /// </summary>
        /// <param name="url">远程URL</param>
        /// <param name="aesKey">AES加密密钥</param>
        /// <returns>下载远程清单的操作</returns>
        public static DownloadRemoteManifestOperation DownloadRemoteManifest(string url, string aesKey)
        {
            var op = new DownloadRemoteManifestOperation(url, aesKey);
            QuarkUtility.Unity.CheckCoroutineProvider();
            OperationSystem.StartOperation(op);
            return op;
        }
        
        /// <summary>
        /// 下载远程清单（使用AES加密）
        /// </summary>
        /// <param name="url">远程URL</param>
        /// <param name="aesKeyBytes">AES加密密钥字节数组</param>
        /// <returns>下载远程清单的操作</returns>
        public static DownloadRemoteManifestOperation DownloadRemoteManifest(string url, byte[] aesKeyBytes)
        {
            var op = new DownloadRemoteManifestOperation(url, aesKeyBytes);
            QuarkUtility.Unity.CheckCoroutineProvider();
            OperationSystem.StartOperation(op);
            return op;
        }
        
        /// <summary>
        /// 下载更新资源
        /// </summary>
        /// <param name="remoteUrl">远程资源URL</param>
        /// <param name="persistentPath">本地持久化路径</param>
        /// <param name="localManifest">本地清单（可为null，表示首次安装）</param>
        /// <returns>下载更新资源的操作</returns>
        public static DownloadUpdateAssetOperation DownloadUpdateAsset(string remoteUrl, string persistentPath, Asset.QuarkManifest localManifest = null)
        {
            var op = new DownloadUpdateAssetOperation(remoteUrl, persistentPath, localManifest);
            QuarkUtility.Unity.CheckCoroutineProvider();
            OperationSystem.StartOperation(op);
            return op;
        }
        
        /// <summary>
        /// 下载更新资源（使用AES加密）
        /// </summary>
        /// <param name="remoteUrl">远程资源URL</param>
        /// <param name="persistentPath">本地持久化路径</param>
        /// <param name="aesKey">AES加密密钥</param>
        /// <param name="localManifest">本地清单（可为null，表示首次安装）</param>
        /// <returns>下载更新资源的操作</returns>
        public static DownloadUpdateAssetOperation DownloadUpdateAsset(string remoteUrl, string persistentPath, string aesKey, Asset.QuarkManifest localManifest = null)
        {
            var op = new DownloadUpdateAssetOperation(remoteUrl, persistentPath, aesKey, localManifest);
            QuarkUtility.Unity.CheckCoroutineProvider();
            OperationSystem.StartOperation(op);
            return op;
        }
        
        /// <summary>
        /// 下载更新资源（使用AES加密）
        /// </summary>
        /// <param name="remoteUrl">远程资源URL</param>
        /// <param name="persistentPath">本地持久化路径</param>
        /// <param name="aesKeyBytes">AES加密密钥字节数组</param>
        /// <param name="localManifest">本地清单（可为null，表示首次安装）</param>
        /// <returns>下载更新资源的操作</returns>
        public static DownloadUpdateAssetOperation DownloadUpdateAsset(string remoteUrl, string persistentPath, byte[] aesKeyBytes, Asset.QuarkManifest localManifest = null)
        {
            var op = new DownloadUpdateAssetOperation(remoteUrl, persistentPath, aesKeyBytes, localManifest);
            QuarkUtility.Unity.CheckCoroutineProvider();
            OperationSystem.StartOperation(op);
            return op;
        }
        
        /// <summary>
        /// 创建合并清单
        /// 用于增量更新，将本地清单与远程清单合并成一个合并清单
        /// </summary>
        /// <param name="srcManifest">源清单（本地清单）</param>
        /// <param name="diffManifest">差异清单（远程清单）</param>
        /// <returns>合并后的清单</returns>
        public static Asset.QuarkMergedManifest CreateMergedManifest(Asset.QuarkManifest srcManifest, Asset.QuarkManifest diffManifest)
        {
            if (srcManifest == null || diffManifest == null)
                return null;
                
            Asset.QuarkMergedManifest mergedManifest;
            QuarkUtility.Manifest.MergeManifest(srcManifest, diffManifest, out mergedManifest);
            return mergedManifest;
        }
        
        /// <summary>
        /// 创建合并清单
        /// 用于增量更新，将本地合并清单与远程清单合并成一个新的合并清单
        /// </summary>
        /// <param name="srcMergedManifest">源合并清单（本地合并清单）</param>
        /// <param name="diffManifest">差异清单（远程清单）</param>
        /// <returns>合并后的清单</returns>
        public static Asset.QuarkMergedManifest CreateMergedManifest(Asset.QuarkMergedManifest srcMergedManifest, Asset.QuarkManifest diffManifest)
        {
            if (srcMergedManifest == null || diffManifest == null)
                return null;
                
            Asset.QuarkMergedManifest mergedManifest;
            QuarkUtility.Manifest.MergeManifest(srcMergedManifest, diffManifest, out mergedManifest);
            return mergedManifest;
        }
        
        /// <summary>
        /// 创建合并清单
        /// 用于增量更新，将本地合并清单与远程合并清单合并成一个新的合并清单
        /// </summary>
        /// <param name="srcMergedManifest">源合并清单（本地合并清单）</param>
        /// <param name="diffMergedManifest">差异合并清单（远程合并清单）</param>
        /// <returns>合并后的清单</returns>
        public static Asset.QuarkMergedManifest CreateMergedManifest(Asset.QuarkMergedManifest srcMergedManifest, Asset.QuarkMergedManifest diffMergedManifest)
        {
            if (srcMergedManifest == null || diffMergedManifest == null)
                return null;
                
            Asset.QuarkMergedManifest mergedManifest;
            QuarkUtility.Manifest.MergeManifest(srcMergedManifest, diffMergedManifest, out mergedManifest);
            return mergedManifest;
        }
        
        /// <summary>
        /// 比较两个清单
        /// </summary>
        /// <param name="sourceManifest">源清单</param>
        /// <param name="comparisonManifest">比较清单</param>
        /// <returns>比较结果</returns>
        public static Manifest.QuarkManifestCompareResult CompareManifest(Asset.QuarkManifest sourceManifest, Asset.QuarkManifest comparisonManifest)
        {
            if (sourceManifest == null || comparisonManifest == null)
                return null;
                
            Manifest.QuarkManifestCompareResult result;
            QuarkUtility.Manifest.CompareManifest(sourceManifest, comparisonManifest, out result);
            return result;
        }
        
        /// <summary>
        /// 按Bundle名称比较两个清单
        /// </summary>
        /// <param name="sourceManifest">源清单</param>
        /// <param name="comparisonManifest">比较清单</param>
        /// <returns>比较结果</returns>
        public static Manifest.QuarkManifestCompareResult CompareManifestByBundleName(Asset.QuarkManifest sourceManifest, Asset.QuarkManifest comparisonManifest)
        {
            if (sourceManifest == null || comparisonManifest == null)
                return null;
                
            Manifest.QuarkManifestCompareResult result;
            QuarkUtility.Manifest.CompareManifestByBundleName(sourceManifest, comparisonManifest, out result);
            return result;
        }
        
        /// <summary>
        /// 将操作添加到操作系统队列中
        /// </summary>
        /// <param name="operation">异步操作</param>
        public static void EnqueueOperation(AsyncOperationBase operation)
        {
            QuarkUtility.Unity.CheckCoroutineProvider();
            OperationSystem.StartOperation(operation);
        }
    }
}
