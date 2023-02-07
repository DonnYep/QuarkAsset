namespace Quark.Encrypt
{
    public class QuarkEncrytionData
    {
        string quarkAesEncryptionKey;
        byte[] quarkAesEncryptionKeyBytes;
        /// <summary>
        /// AssetBundle加密偏移量；
        /// </summary>
        public ulong QuarkEncryptionOffset { get; set; }
        /// <summary>
        /// manifest对称加密密钥bytes；
        /// </summary>
        public byte[] QuarkAesEncryptionKeyBytes
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
        public string QuarkAesEncryptionKey
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
    }
}
