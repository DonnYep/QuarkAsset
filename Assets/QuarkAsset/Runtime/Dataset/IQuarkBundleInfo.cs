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
        List<QuarkObjectInfo> ObjectInfoList { get; set; }
        List<QuarkBundleDependentInfo> DependentBundleKeyList { get; set; }
    }
}
