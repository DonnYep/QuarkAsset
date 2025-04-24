using System;

namespace Quark.Manifest
{
    /// <summary>
    /// 清单验证结果
    /// </summary>
    [Serializable]
    public class QuarkManifestVerifyResult
    {
        private QuarkManifestVerifyInfo[] verificationSuccessInfos;
        private QuarkManifestVerifyInfo[] verificationFailureInfos;
        
        /// <summary>
        /// 验证成功的资源信息
        /// </summary>
        public QuarkManifestVerifyInfo[] VerificationSuccessInfos
        {
            get 
            { 
                if(verificationSuccessInfos == null)
                    verificationSuccessInfos = new QuarkManifestVerifyInfo[0];
                return verificationSuccessInfos; 
            }
            set { verificationSuccessInfos = value; }
        }
        
        /// <summary>
        /// 验证失败的资源信息
        /// </summary>
        public QuarkManifestVerifyInfo[] VerificationFailureInfos
        {
            get 
            { 
                if(verificationFailureInfos == null)
                    verificationFailureInfos = new QuarkManifestVerifyInfo[0];
                return verificationFailureInfos; 
            }
            set { verificationFailureInfos = value; }
        }
        
        /// <summary>
        /// 是否全部验证成功
        /// </summary>
        public bool IsAllVerified
        {
            get { return VerificationFailureInfos.Length == 0; }
        }
        
        /// <summary>
        /// 获取需要更新的资源数量
        /// </summary>
        public int RequireUpdateCount
        {
            get { return VerificationFailureInfos.Length; }
        }
        
        /// <summary>
        /// 获取验证成功的资源数量
        /// </summary>
        public int SuccessCount
        {
            get { return VerificationSuccessInfos.Length; }
        }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public QuarkManifestVerifyResult()
        {
            verificationSuccessInfos = new QuarkManifestVerifyInfo[0];
            verificationFailureInfos = new QuarkManifestVerifyInfo[0];
        }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="successInfos">验证成功的资源信息</param>
        /// <param name="failureInfos">验证失败的资源信息</param>
        public QuarkManifestVerifyResult(QuarkManifestVerifyInfo[] successInfos, QuarkManifestVerifyInfo[] failureInfos)
        {
            verificationSuccessInfos = successInfos ?? new QuarkManifestVerifyInfo[0];
            verificationFailureInfos = failureInfos ?? new QuarkManifestVerifyInfo[0];
        }
        
        /// <summary>
        /// 获取需要更新的总大小
        /// </summary>
        /// <returns>需要更新的字节数</returns>
        public long GetTotalUpdateSize()
        {
            long totalSize = 0;
            for (int i = 0; i < VerificationFailureInfos.Length; i++)
            {
                totalSize += VerificationFailureInfos[i].ResourceBundleSize;
            }
            return totalSize;
        }
        
        /// <summary>
        /// 获取格式化的总更新大小字符串
        /// </summary>
        /// <returns>格式化的大小字符串</returns>
        public string GetFormatTotalUpdateSize()
        {
            return QuarkUtility.FormatBytes(GetTotalUpdateSize());
        }
        
        /// <summary>
        /// 检查资源是否需要更新
        /// </summary>
        /// <param name="bundleName">资源包名称</param>
        /// <returns>是否需要更新</returns>
        public bool CheckRequireUpdate(string bundleName)
        {
            for (int i = 0; i < VerificationFailureInfos.Length; i++)
            {
                if (VerificationFailureInfos[i].ResourceBundleName == bundleName)
                {
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// 获取JSON格式的验证结果
        /// </summary>
        /// <returns>JSON字符串</returns>
        public string ToJson()
        {
            return QuarkUtility.ToJson(this);
        }
    }
}
