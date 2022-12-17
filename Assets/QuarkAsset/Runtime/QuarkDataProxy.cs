using Quark.Asset;
namespace Quark
{
    /// <summary>
    /// 数据代理类；
    /// </summary>
    public class QuarkDataProxy
    {
        static string quarkAESEncryptionKey;
        static byte[] quarkAESEncryptionKeyBytes;
        /// <summary>
        /// 远端存储的地址；
        /// </summary>
        public static string URL { get; set; }
        /// <summary>
        /// 本地持久化路径；
        /// </summary>
        public static string PersistentPath { get; set; }
        /// <summary>
        /// AssetBundle加密偏移量；
        /// </summary>
        public static ulong QuarkEncryptionOffset { get; set; }
        /// <summary>
        /// manifest对称加密密钥bytes；
        /// </summary>
        public static byte[] QuarkAESEncryptionKeyBytes
        {
            get
            {
                if (quarkAESEncryptionKeyBytes == null)
                {
                    quarkAESEncryptionKeyBytes = new byte[0];
                }
                return quarkAESEncryptionKeyBytes;
            }
        }
        /// <summary>
        /// manifest对称加密密钥
        /// </summary>
        public static string QuarkAESEncryptionKey
        {
            get { return quarkAESEncryptionKey; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    quarkAESEncryptionKeyBytes = QuarkUtility.GenerateBytesAESKey(value);
                    quarkAESEncryptionKey = value;
                }
            }
        }
        /// <summary>
        /// editor模式下加载寻址依据；
        /// </summary>
        public static QuarkDataset QuarkAssetDataset { get; internal set; }
        /// <summary>
        /// 存储ab包中包含的资源信息；
        /// </summary>
        public static QuarkManifest QuarkManifest { get; internal set; }
    }
}
