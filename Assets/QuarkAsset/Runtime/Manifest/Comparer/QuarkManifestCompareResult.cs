using System;

namespace Quark.Manifest
{
    [Serializable]
    public class QuarkManifestCompareResult
    {
        /// <summary>
        /// 有改动的文件；
        /// </summary>
        public QuarkManifestCompareInfo[] ChangedInfos;
        /// <summary>
        /// 新增的文件；
        /// </summary>
        public QuarkManifestCompareInfo[] NewlyAddedInfos;
        /// <summary>
        /// 过期无效的文件；
        /// </summary>
        public QuarkManifestCompareInfo[] ExpiredInfos;
        /// <summary>
        /// 未更改的文件；
        /// </summary>
        public QuarkManifestCompareInfo[] UnchangedInfos;
    }
}
