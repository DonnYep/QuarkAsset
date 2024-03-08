using System.Collections.Generic;

namespace Quark.Asset
{
    public interface IQuarkBundleInfo
    {
        string BundleName { get; set; }
        string BundlePath { get; set; }
        string BundleKey { get; set; }
        long BundleSize { get; set; }
        string BundleFormatBytes { get; set; }
        /// <summary>
        /// 标记所有资源个体为独立bundle包
        /// </summary>
        bool Extract { get; set; }
        List<QuarkObjectInfo> ObjectInfoList { get; set; }
        List<QuarkBundleDependentInfo> DependentBundleKeyList { get; set; }
    }
}
