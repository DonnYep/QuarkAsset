using System;
using System.IO;

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

        public string ProfileTabAbsPath;
        /// <summary>
        /// 预设标签面ab输出的绝对目录
        /// </summary>
        public string ProfileTabAbsAssetBundleBuildPath;

        public string NoProfileTabAbsPath;
        /// <summary>
        /// 无预设标签面ab输出的绝对目录
        /// </summary>
        public string NoProfileTabAbsAssetBundleBuildPath;

        public QuarkAssetBundleTabData()
        {
            NoProfileTabAbsPath = Path.Combine(QuarkEditorUtility.ApplicationPath, QuarkEditorConstant.DEFAULT_ASSETBUNDLE_RELATIVE_PATH).Replace("\\", "/");

            ProfileTabAbsPath = Path.Combine(QuarkEditorUtility.ApplicationPath, QuarkEditorConstant.DEFAULT_ASSETBUNDLE_RELATIVE_PATH).Replace("\\", "/");
        }
    }
}