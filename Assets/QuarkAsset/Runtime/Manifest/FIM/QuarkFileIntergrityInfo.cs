namespace Quark.Manifest
{
    public struct QuarkFileIntergrityInfo
    {
        /// <summary>
        /// bundle size on local path
        /// </summary>
        public long LocalBundleSize { get; private set; }
        /// <summary>
        /// bundle size on manifest
        /// </summary>
        public long RecordedBundleSize { get; private set; }
        public string BundleKey { get; private set; }
        public string BundleName { get; private set; }
        public QuarkFileIntergrityInfo(long localBundleSize,long recordedBundleSize, string bundleKey, string bundleName)
        {
            LocalBundleSize = localBundleSize;
            RecordedBundleSize = recordedBundleSize;
            BundleKey = bundleKey;
            BundleName = bundleName;
        }
    }
}