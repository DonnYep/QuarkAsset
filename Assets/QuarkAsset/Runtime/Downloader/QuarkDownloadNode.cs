using System;
using System.Collections.Generic;
using System.IO;

namespace Quark
{
    /// <summary>
    /// 资源下载节点
    /// </summary>
    [Serializable]
    public class QuarkDownloadNode
    {
        /// <summary>
        /// 下载URI
        /// </summary>
        public string DownloadUri { get; set; }
        
        /// <summary>
        /// 保存路径
        /// </summary>
        public string SavePath { get; set; }
        
        /// <summary>
        /// 下载超时时间（秒）
        /// </summary>
        public int Timeout { get; set; } = 30;
        
        /// <summary>
        /// 是否使用断点续传
        /// </summary>
        public bool UseResumeDownload { get; set; } = false;
        
        /// <summary>
        /// 下载任务ID
        /// </summary>
        public long TaskId { get; set; } = 0;
        
        /// <summary>
        /// 下载状态
        /// </summary>
        public DownloadStatus Status { get; set; } = DownloadStatus.None;
        
        /// <summary>
        /// 下载进度（0-1）
        /// </summary>
        public float Progress { get; set; } = 0f;
        
        /// <summary>
        /// 已下载字节数
        /// </summary>
        public long DownloadedBytes { get; set; } = 0;
        
        /// <summary>
        /// 文件总字节数
        /// </summary>
        public long TotalBytes { get; set; } = 0;
        
        /// <summary>
        /// 下载的文件类型
        /// </summary>
        public DownloadFileType FileType { get; set; } = DownloadFileType.File;
        
        /// <summary>
        /// 下载完成回调
        /// </summary>
        public Action<QuarkDownloadNode> OnCompleted { get; set; }
        
        /// <summary>
        /// 下载失败回调
        /// </summary>
        public Action<QuarkDownloadNode, string> OnFailed { get; set; }
        
        /// <summary>
        /// 下载进度回调
        /// </summary>
        public Action<QuarkDownloadNode, float> OnProgress { get; set; }
        
        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage { get; set; }
        
        /// <summary>
        /// 用户自定义数据
        /// </summary>
        public object UserData { get; set; }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public QuarkDownloadNode()
        {
        }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="downloadUri">下载URI</param>
        /// <param name="savePath">保存路径</param>
        public QuarkDownloadNode(string downloadUri, string savePath)
        {
            DownloadUri = downloadUri;
            SavePath = savePath;
        }
        
        /// <summary>
        /// 获取文件名
        /// </summary>
        /// <returns>文件名</returns>
        public string GetFileName()
        {
            return Path.GetFileName(SavePath);
        }
        
        /// <summary>
        /// 获取格式化的已下载大小
        /// </summary>
        /// <returns>格式化的大小字符串</returns>
        public string GetFormattedDownloadedSize()
        {
            return QuarkUtility.FormatBytes(DownloadedBytes);
        }
        
        /// <summary>
        /// 获取格式化的总大小
        /// </summary>
        /// <returns>格式化的大小字符串</returns>
        public string GetFormattedTotalSize()
        {
            return QuarkUtility.FormatBytes(TotalBytes);
        }
    }
    
    /// <summary>
    /// 下载状态
    /// </summary>
    public enum DownloadStatus
    {
        /// <summary>
        /// 未开始
        /// </summary>
        None,
        
        /// <summary>
        /// 等待中
        /// </summary>
        Waiting,
        
        /// <summary>
        /// 下载中
        /// </summary>
        Downloading,
        
        /// <summary>
        /// 已暂停
        /// </summary>
        Paused,
        
        /// <summary>
        /// 已完成
        /// </summary>
        Completed,
        
        /// <summary>
        /// 已失败
        /// </summary>
        Failed
    }
    
    /// <summary>
    /// 下载文件类型
    /// </summary>
    public enum DownloadFileType
    {
        /// <summary>
        /// 普通文件
        /// </summary>
        File,
        
        /// <summary>
        /// 清单文件
        /// </summary>
        Manifest,
        
        /// <summary>
        /// 资源包
        /// </summary>
        AssetBundle,
        
        /// <summary>
        /// 其他类型
        /// </summary>
        Other
    }
}
