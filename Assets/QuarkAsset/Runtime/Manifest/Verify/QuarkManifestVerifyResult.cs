namespace Quark.Manifest
{
    public class QuarkManifestVerifyResult
    {
        /// <summary>
        /// 校验成功的信息；
        /// </summary>
        public QuarkManifestVerifyInfo[] VerificationSuccessInfos;
        /// <summary>
        /// 校验失败的信息；
        /// </summary>
        public QuarkManifestVerifyInfo[] VerificationFailureInfos;
    }
}
