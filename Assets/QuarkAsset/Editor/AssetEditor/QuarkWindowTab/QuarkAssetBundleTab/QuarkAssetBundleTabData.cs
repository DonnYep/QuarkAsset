using System;
namespace Quark.Editor
{
    [Serializable]
    internal class QuarkAssetBundleTabData
    {
        /// <summary>
        /// 使用构建预设
        /// </summary>
        public bool UseBuildProfile;
        /// <summary>
        /// 预设地址
        /// </summary>
        public string ProfilePath;
    }
}