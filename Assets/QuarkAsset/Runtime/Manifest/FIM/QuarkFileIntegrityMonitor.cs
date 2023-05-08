using Quark.Asset;
using System.IO;

namespace Quark.Manifest
{
    /// <summary>
    /// File Integrity Monitoring, local file only
    /// </summary>
    public static class QuarkFileIntegrityMonitor
    {
        public static void MonitoringIntegrity(QuarkManifest manifest, string path, out QuarkFileIntergrityResult result)
        {
            result = new QuarkFileIntergrityResult();
            result.IntergrityInfos = new QuarkFileIntergrityInfo[manifest.BundleInfoDict.Count];
            int index = 0;
            foreach (var bundleInfo in manifest.BundleInfoDict.Values)
            {
                var bundleKey = bundleInfo.QuarkAssetBundle.BundleKey;
                var bundleName = bundleInfo.QuarkAssetBundle.BundleName;
                var filePath = Path.Combine(path, bundleKey);
                long fileLength = 0;
                if (File.Exists(filePath))
                {
                    var fileInfo = new FileInfo(filePath);
                    fileLength = fileInfo.Length;
                }
                var intergrityInfo = new QuarkFileIntergrityInfo(fileLength, bundleInfo.BundleSize, bundleKey, bundleName);
                result.IntergrityInfos[index] = intergrityInfo;
                index++;
            }
        }
    }
}
