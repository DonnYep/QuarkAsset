using System;
using System.Collections.Generic;

namespace Quark
{
    /// <summary>
    /// 资源更新结果
    /// </summary>
    [Serializable]
    public class QuarkUpdateResult
    {
        private QuarkUpdateTask[] successTasks;
        private QuarkUpdateTask[] failedTasks;
        
        /// <summary>
        /// 成功的更新任务
        /// </summary>
        public QuarkUpdateTask[] SuccessTasks
        {
            get 
            { 
                if(successTasks == null)
                    successTasks = new QuarkUpdateTask[0];
                return successTasks; 
            }
            set { successTasks = value; }
        }
        
        /// <summary>
        /// 失败的更新任务
        /// </summary>
        public QuarkUpdateTask[] FailedTasks
        {
            get 
            { 
                if(failedTasks == null)
                    failedTasks = new QuarkUpdateTask[0];
                return failedTasks; 
            }
            set { failedTasks = value; }
        }
        
        /// <summary>
        /// 更新是否完全成功（无失败任务）
        /// </summary>
        public bool IsCompleteSuccess
        {
            get { return FailedTasks.Length == 0; }
        }
        
        /// <summary>
        /// 更新是否完全失败（无成功任务）
        /// </summary>
        public bool IsCompleteFailed
        {
            get { return SuccessTasks.Length == 0; }
        }
        
        /// <summary>
        /// 更新是否部分成功（有成功任务也有失败任务）
        /// </summary>
        public bool IsPartialSuccess
        {
            get { return SuccessTasks.Length > 0 && FailedTasks.Length > 0; }
        }
        
        /// <summary>
        /// 成功任务数量
        /// </summary>
        public int SuccessCount
        {
            get { return SuccessTasks.Length; }
        }
        
        /// <summary>
        /// 失败任务数量
        /// </summary>
        public int FailedCount
        {
            get { return FailedTasks.Length; }
        }
        
        /// <summary>
        /// 总任务数量
        /// </summary>
        public int TotalCount
        {
            get { return SuccessCount + FailedCount; }
        }
        
        /// <summary>
        /// 成功率（0-1）
        /// </summary>
        public float SuccessRate
        {
            get 
            { 
                if(TotalCount == 0)
                    return 0;
                return (float)SuccessCount / TotalCount; 
            }
        }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public QuarkUpdateResult()
        {
            successTasks = new QuarkUpdateTask[0];
            failedTasks = new QuarkUpdateTask[0];
        }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="successTasks">成功的任务</param>
        /// <param name="failedTasks">失败的任务</param>
        public QuarkUpdateResult(QuarkUpdateTask[] successTasks, QuarkUpdateTask[] failedTasks)
        {
            this.successTasks = successTasks ?? new QuarkUpdateTask[0];
            this.failedTasks = failedTasks ?? new QuarkUpdateTask[0];
        }
        
        /// <summary>
        /// 从任务列表创建更新结果
        /// </summary>
        /// <param name="tasks">任务列表</param>
        /// <returns>更新结果</returns>
        public static QuarkUpdateResult FromTasks(IList<QuarkUpdateTask> tasks)
        {
            if (tasks == null || tasks.Count == 0)
                return new QuarkUpdateResult();
                
            List<QuarkUpdateTask> successList = new List<QuarkUpdateTask>();
            List<QuarkUpdateTask> failedList = new List<QuarkUpdateTask>();
            
            foreach (var task in tasks)
            {
                if (task.IsSuccessful)
                    successList.Add(task);
                else
                    failedList.Add(task);
            }
            
            return new QuarkUpdateResult(
                successList.ToArray(),
                failedList.ToArray()
            );
        }
        
        /// <summary>
        /// 获取详细的更新结果信息
        /// </summary>
        /// <returns>结果信息</returns>
        public string GetResultInfo()
        {
            string info = $"更新完成: 总计 {TotalCount} 个文件, 成功 {SuccessCount}, 失败 {FailedCount}";
            
            if (FailedCount > 0)
            {
                info += "\n失败的文件:";
                for (int i = 0; i < FailedTasks.Length; i++)
                {
                    info += $"\n  {i+1}. {FailedTasks[i].BundleName} - {FailedTasks[i].ErrorMessage}";
                }
            }
            
            return info;
        }
    }
    
    /// <summary>
    /// 资源更新任务
    /// </summary>
    [Serializable]
    public class QuarkUpdateTask
    {
        private string bundleName;
        private string downloadUri;
        private string localPath;
        private bool isSuccessful;
        private string errorMessage;
        private long fileSize;
        
        /// <summary>
        /// 资源包名称
        /// </summary>
        public string BundleName
        {
            get { return bundleName; }
            set { bundleName = value; }
        }
        
        /// <summary>
        /// 下载URI
        /// </summary>
        public string DownloadUri
        {
            get { return downloadUri; }
            set { downloadUri = value; }
        }
        
        /// <summary>
        /// 本地保存路径
        /// </summary>
        public string LocalPath
        {
            get { return localPath; }
            set { localPath = value; }
        }
        
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccessful
        {
            get { return isSuccessful; }
            set { isSuccessful = value; }
        }
        
        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage
        {
            get { return errorMessage; }
            set { errorMessage = value; }
        }
        
        /// <summary>
        /// 文件大小
        /// </summary>
        public long FileSize
        {
            get { return fileSize; }
            set { fileSize = value; }
        }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public QuarkUpdateTask()
        {
        }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="bundleName">资源包名称</param>
        /// <param name="downloadUri">下载URI</param>
        /// <param name="localPath">本地保存路径</param>
        /// <param name="fileSize">文件大小</param>
        public QuarkUpdateTask(string bundleName, string downloadUri, string localPath, long fileSize)
        {
            this.bundleName = bundleName;
            this.downloadUri = downloadUri;
            this.localPath = localPath;
            this.fileSize = fileSize;
            this.isSuccessful = false;
            this.errorMessage = string.Empty;
        }
        
        /// <summary>
        /// 标记为成功
        /// </summary>
        public void MarkAsSuccess()
        {
            isSuccessful = true;
            errorMessage = string.Empty;
        }
        
        /// <summary>
        /// 标记为失败
        /// </summary>
        /// <param name="error">错误信息</param>
        public void MarkAsFailed(string error)
        {
            isSuccessful = false;
            errorMessage = error;
        }
    }
}
