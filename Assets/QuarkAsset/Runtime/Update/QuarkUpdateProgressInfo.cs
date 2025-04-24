using System;

namespace Quark
{
    /// <summary>
    /// 资源更新进度信息
    /// </summary>
    [Serializable]
    public class QuarkUpdateProgressInfo
    {
        private int currentDownloadIndex;
        private int totalDownloadCount;
        private long currentDownloadedBytes;
        private long currentTotalBytes;
        private float currentProgress;
        private float totalProgress;
        private QuarkDownloadNode node;
        
        /// <summary>
        /// 当前下载索引（从0开始）
        /// </summary>
        public int CurrentDownloadIndex
        {
            get { return currentDownloadIndex; }
            set { currentDownloadIndex = value; }
        }
        
        /// <summary>
        /// 总下载数量
        /// </summary>
        public int TotalDownloadCount
        {
            get { return totalDownloadCount; }
            set { totalDownloadCount = value; }
        }
        
        /// <summary>
        /// 当前已下载字节数
        /// </summary>
        public long CurrentDownloadedBytes
        {
            get { return currentDownloadedBytes; }
            set { currentDownloadedBytes = value; }
        }
        
        /// <summary>
        /// 当前文件总字节数
        /// </summary>
        public long CurrentTotalBytes
        {
            get { return currentTotalBytes; }
            set { currentTotalBytes = value; }
        }
        
        /// <summary>
        /// 当前文件下载进度（0-1）
        /// </summary>
        public float CurrentProgress
        {
            get { return currentProgress; }
            set { currentProgress = value; }
        }
        
        /// <summary>
        /// 总体下载进度（0-1）
        /// </summary>
        public float TotalProgress
        {
            get { return totalProgress; }
            set { totalProgress = value; }
        }
        
        /// <summary>
        /// 当前下载节点
        /// </summary>
        public QuarkDownloadNode Node
        {
            get { return node; }
            set { node = value; }
        }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public QuarkUpdateProgressInfo()
        {
        }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="currentIndex">当前下载索引</param>
        /// <param name="totalCount">总下载数量</param>
        /// <param name="downloadedBytes">已下载字节数</param>
        /// <param name="totalBytes">总字节数</param>
        /// <param name="current">当前进度</param>
        /// <param name="total">总进度</param>
        /// <param name="node">下载节点</param>
        public QuarkUpdateProgressInfo(
            int currentIndex, 
            int totalCount, 
            long downloadedBytes, 
            long totalBytes, 
            float current, 
            float total, 
            QuarkDownloadNode node)
        {
            this.currentDownloadIndex = currentIndex;
            this.totalDownloadCount = totalCount;
            this.currentDownloadedBytes = downloadedBytes;
            this.currentTotalBytes = totalBytes;
            this.currentProgress = current;
            this.totalProgress = total;
            this.node = node;
        }
        
        /// <summary>
        /// 获取已下载字节数的格式化字符串
        /// </summary>
        /// <returns>格式化后的字符串</returns>
        public string GetFormattedDownloadedBytes()
        {
            return QuarkUtility.FormatBytes(currentDownloadedBytes);
        }
        
        /// <summary>
        /// 获取总字节数的格式化字符串
        /// </summary>
        /// <returns>格式化后的字符串</returns>
        public string GetFormattedTotalBytes()
        {
            return QuarkUtility.FormatBytes(currentTotalBytes);
        }
        
        /// <summary>
        /// 获取进度描述
        /// </summary>
        /// <returns>进度描述字符串</returns>
        public string GetProgressDescription()
        {
            return $"下载中: {currentDownloadIndex + 1}/{totalDownloadCount} " +
                   $"{GetFormattedDownloadedBytes()}/{GetFormattedTotalBytes()} " +
                   $"({currentProgress:P2})";
        }
    }
}
