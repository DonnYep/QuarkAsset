namespace Quark.Verifiy
{
    public class QuarkManifestVerifiyInfo
    {
        /// <summary>
        /// 完整的size
        /// </summary>
        public long TotalSize;
        /// <summary>
        /// 校验的bundle信息数组；
        /// </summary>
        public QuarkBundleVerifiyInfo[] BundleVerifiyInfos;
    }
}
