using System;

namespace Quark.Manifest
{
    /// <summary>
    /// 校验的任务；
    /// </summary>
    internal class QuarkManifestVerifyTask : IEquatable<QuarkManifestVerifyTask>
    {
        public string Url { get; private set; }
        public string ResourceBundleName { get; private set; }
        public long ResourceBundleSize { get; private set; }
        public QuarkManifestVerifyTask(string url, string resourceBundleName, long resourceBundleSize)
        {
            Url = url;
            ResourceBundleName = resourceBundleName;
            ResourceBundleSize = resourceBundleSize;
        }
        public bool Equals(QuarkManifestVerifyTask other)
        {
            return other.Url == this.Url &&
                other.ResourceBundleName == this.ResourceBundleName;
        }
    }
}
