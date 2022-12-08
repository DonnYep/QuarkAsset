using System.Runtime.InteropServices;

namespace Quark.Verifiy
{
    [StructLayout(LayoutKind.Auto)]
    public struct QuarkBundleVerifiyInfo
    {
        /// <summary>
        /// 包的名称；
        /// </summary>
        public string BundleName;
        /// <summary>
        /// 包的加载标记；
        /// </summary>
        public string BundleKey;
        /// <summary>
        /// 包体的Hash
        /// </summary>
        public string BundleHash;
        /// <summary>
        /// AssetBundle包大小；
        /// </summary>
        public long BundleSize;
    }
}
