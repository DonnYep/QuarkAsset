using Quark.Asset;
namespace Quark
{
    /// <summary>
    /// 数据代理类；
    /// </summary>
    internal class QuarkDataProxy
    {
        static string quarkAesEncryptionKey;
        static byte[] quarkAesEncryptionKeyBytes;
        /// <summary>
        /// AssetBundle加密偏移量；
        /// </summary>
        public static ulong QuarkEncryptionOffset { get; set; }
        /// <summary>
        /// manifest对称加密密钥bytes；
        /// </summary>
        public static byte[] QuarkAesEncryptionKeyBytes
        {
            get
            {
                if (quarkAesEncryptionKeyBytes == null)
                {
                    quarkAesEncryptionKeyBytes = new byte[0];
                }
                return quarkAesEncryptionKeyBytes;
            }
        }
        /// <summary>
        /// manifest对称加密密钥
        /// </summary>
        public static string QuarkAesEncryptionKey
        {
            get { return quarkAesEncryptionKey; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    quarkAesEncryptionKeyBytes = QuarkUtility.GenerateBytesAESKey(value);
                    quarkAesEncryptionKey = value;
                }
            }
        }
        /// <summary>
        /// 远端存储的地址；
        /// </summary>
        public static string URL { get; set; }
        /// <summary>
        /// 本地持久化路径；
        /// </summary>
        public static string PersistentPath { get; set; }
        /// <summary>
        /// 差异文件持久优化路径
        /// </summary>
        public static string DiffPersistentPath { get; set; }
        public static QuarkDataset QuarkAssetDataset { get; internal set; }
        public static QuarkManifest QuarkManifest { get; internal set; }
        public static string BuildVersion { get; internal set; }
        public static int InternalBuildVersion { get; internal set; }
        public static QuarkLoadMode QuarkAssetLoadMode { get; internal set; }
    }
}
