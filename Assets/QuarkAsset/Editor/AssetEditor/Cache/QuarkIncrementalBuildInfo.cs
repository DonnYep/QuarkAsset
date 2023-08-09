using System.Collections.Generic;

namespace Quark.Editor
{
    public class QuarkIncrementalBuildInfo
    {
        /// <summary>
        /// 新增的；
        /// </summary>
        public AssetCache[] NewlyAdded;
        /// <summary>
        /// 有改动的文件；
        /// </summary>
        public AssetCache[] Changed;
        /// <summary>
        /// 过期无效的文件；
        /// </summary>
        public AssetCache[] Expired;
        /// <summary>
        /// 未更改的文件；
        /// </summary>
        public AssetCache[] Unchanged;
        /// <summary>
        /// 文件缓存
        /// </summary>
        public List<AssetCache> BundleCaches;
    }
}
