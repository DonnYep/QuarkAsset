using Quark.Asset;

namespace Quark
{
    public class QuarkConvert
    {
        /// <summary>
        /// 使用配置的aesKey反序列化manifest；
        /// </summary>
        /// <param name="manifestContext">读取到的文本内容</param>
        /// <returns>反序列化后的内容</returns>
        public static QuarkAssetManifest Deserialize(string manifestContext)
        {
            QuarkAssetManifest quarkAssetManifest = null;
            try
            {
                var aesKeyBytes = QuarkDataProxy.QuarkAESEncryptionKeyBytes;
                var encrypted = aesKeyBytes.Length > 0 ? true : false;
                var unencryptedManifest = manifestContext;
                if (encrypted)
                {
                    unencryptedManifest = QuarkUtility.AESDecryptStringToString(manifestContext, aesKeyBytes);
                }
                quarkAssetManifest = QuarkUtility.ToObject<QuarkAssetManifest>(unencryptedManifest);
            }
            catch { }
            return quarkAssetManifest;
        }
        /// <summary>
        /// 使用自定义的aesKey反序列化manifest；
        /// </summary>
        /// <param name="manifestContext">读取到的文本内容</param>
        /// <param name="aesKey">对称加密密钥</param>
        /// <returns>反序列化后的内容</returns>
        public static QuarkAssetManifest Deserialize(string manifestContext,string aesKey)
        {
            QuarkAssetManifest quarkAssetManifest = null;
            try
            {
                var aesKeyBytes = QuarkUtility.GenerateBytesAESKey(aesKey);
                var encrypted = aesKeyBytes.Length > 0 ? true : false;
                var unencryptedManifest = manifestContext;
                if (encrypted)
                {
                    unencryptedManifest = QuarkUtility.AESDecryptStringToString(manifestContext, aesKeyBytes);
                }
                quarkAssetManifest = QuarkUtility.ToObject<QuarkAssetManifest>(unencryptedManifest);
            }
            catch { }
            return quarkAssetManifest;
        }
    }
}
