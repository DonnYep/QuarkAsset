using Quark.Asset;
using Quark.Networking;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Quark
{
    /// <summary>
    /// 版本控制工具类，内含的方法为更新操作的整合。
    /// </summary>
    public class QuarkVersionController
    {
        /// <summary>
        /// 比较并清理无效资产，而后再生成下载任务。
        /// <para>执行步骤： </para>
        /// <para>1、<see cref="CompareAndCleanInvalidAssets"/></para>
        /// <para>2、<see cref="CompareAndGenerateDownloadTask"/></para>
        /// </summary>
        /// <param name="src">原始的文件清单</param>
        /// <param name="diff">差异的文件清单</param>
        /// <param name="path">本地资源所在的路径</param>
        /// <param name="url">差异资源所在的地址</param>
        /// <returns>生成的下载任务</returns>
        public static List<QuarkDownloadTask> CompareAndCleanThenGenerateDownloadTask(QuarkManifest src, QuarkManifest diff, string path, string url)
        {
            CompareAndCleanInvalidAssets(src, diff, path);
            return CompareAndGenerateDownloadTask(src, diff, path, url);
        }
        /// <summary>
        /// 比较文件清单以生成下载任务
        /// </summary>
        /// <param name="src">原始的文件清单</param>
        /// <param name="diff">差异的文件清单</param>
        /// <param name="path">本地资源所在的路径</param>
        /// <param name="url">差异资源所在的地址</param>
        /// <returns>生成的下载任务</returns>
        public static List<QuarkDownloadTask> CompareAndGenerateDownloadTask(QuarkManifest src, QuarkManifest diff, string path, string url)
        {
            QuarkUtility.Manifest.MergeManifest(src, diff, out var mergedManifest);
            return CompareAndGenerateDownloadTask(mergedManifest, path, url);
        }
        /// <summary>
        /// 校验合并的文件清单以生成下载任
        /// </summary>
        /// <param name="mergedManifest">合并的文件清单</param>
        /// <param name="path">本地资源所在的路径</param>
        /// <param name="url">差异资源所在的地址</param>
        /// <returns>生成的下载任务</returns>
        public static List<QuarkDownloadTask> CompareAndGenerateDownloadTask(QuarkMergedManifest mergedManifest, string path, string url)
        {
            QuarkUtility.Intergrity.MonitoringIntegrity(mergedManifest, path, out var result);
            var downloadTasks = new List<QuarkDownloadTask>();
            var length = result.IntergrityInfos.Count;
            if (!path.EndsWith("/"))
            {
                path += "/";
            }
            if (!url.EndsWith("/"))
            {
                url += "/";
            }
            for (int i = 0; i < length; i++)
            {
                var info = result.IntergrityInfos[i];
                if (info.RecordedBundleSize > info.LocalBundleSize)
                {
                    var downloadPath = QuarkUtility.Append(path, info.BundleKey);
                    var downloadUrl = QuarkUtility.Append(url, info.BundleKey);
                    var downloadTask = new QuarkDownloadTask(downloadUrl, downloadPath, info.LocalBundleSize, info.RecordedBundleSize);
                    downloadTasks.Add(downloadTask);
                }
            }
            return downloadTasks;
        }
        /// <summary>
        /// 比较并清理失效的资产
        /// </summary>
        /// <param name="sourceManifest">原始的文件清单</param>
        /// <param name="comparisonManifest">比较的文件清单</param>
        /// <param name="path">本地持久化地址</param>
        public static void CompareAndCleanInvalidAssets(QuarkManifest sourceManifest, QuarkManifest comparisonManifest, string path)
        {
            QuarkUtility.Manifest.CompareManifest(sourceManifest, comparisonManifest, out var result);
            if (!Directory.Exists(path))
                return;
            var dirInfo = new DirectoryInfo(path);
            var fileInfos = dirInfo.GetFiles();
            var invalidFileNames = new HashSet<string>();
            var fileNames = fileInfos.Select(f => f.Name);
            var changedInfos = result.ChangedInfos;
            var deletedInfos = result.DeletedInfos;
            for (int i = 0; i < changedInfos.Length; i++)
            {
                var name = changedInfos[i].BundleKey;
                invalidFileNames.Add(name);
            }
            for (int i = 0; i < deletedInfos.Length; i++)
            {
                var name = deletedInfos[i].BundleKey;
                invalidFileNames.Add(name);
            }
            foreach (var fileInfo in fileInfos)
            {
                if (invalidFileNames.Contains(fileInfo.Name))
                    fileInfo.Delete();
            }
        }
    }
}
