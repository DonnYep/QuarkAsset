using Quark.Asset;
namespace Quark
{
    public partial class QuarkUtility
    {
        public class Manifest
        {
            public static string Serialize(QuarkManifest manifest, string aesKey)
            {
                var aesKeyBytes = QuarkUtility.GenerateBytesAESKey(aesKey);
                return Serialize(manifest, aesKeyBytes);
            }
            public static string Serialize(QuarkManifest manifest, byte[] aesKeyBytes)
            {
                var manifestJson = QuarkUtility.ToJson(manifest);
                var encrypt = aesKeyBytes != null && aesKeyBytes.Length > 0;
                string serializedContext = string.Empty;
                if (encrypt)
                    serializedContext = QuarkUtility.AESEncryptStringToString(manifestJson, aesKeyBytes);
                else
                    serializedContext = manifestJson;
                return serializedContext;
            }
            /// <summary>
            /// 使用配置的aesKey反序列化manifest；
            /// </summary>
            /// <param name="manifestContext">读取到的文本内容</param>
            /// <returns>反序列化后的内容</returns>
            public static QuarkManifest Deserialize(string manifestContext)
            {
                return Deserialize(manifestContext, QuarkDataProxy.QuarkAesEncryptionKey);
            }
            /// <summary>
            /// 使用自定义的aesKey反序列化manifest；
            /// </summary>
            /// <param name="manifestContext">读取到的文本内容</param>
            /// <param name="aesKey">对称加密密钥</param>
            /// <returns>反序列化后的内容</returns>
            public static QuarkManifest Deserialize(string manifestContext, string aesKey)
            {
                var aesKeyBytes = QuarkUtility.GenerateBytesAESKey(aesKey);
                return Deserialize(manifestContext, aesKeyBytes);
            }
            public static QuarkManifest Deserialize(string manifestContext, byte[] aesKeyBytes)
            {
                QuarkManifest quarkAssetManifest = null;
                try
                {
                    var encrypted = aesKeyBytes.Length > 0 ? true : false;
                    var unencryptedManifest = manifestContext;
                    if (encrypted)
                    {
                        unencryptedManifest = QuarkUtility.AESDecryptStringToString(manifestContext, aesKeyBytes);
                    }
                    quarkAssetManifest = QuarkUtility.ToObject<QuarkManifest>(unencryptedManifest);
                }
                catch { }
                return quarkAssetManifest;
            }
        }
    }
}