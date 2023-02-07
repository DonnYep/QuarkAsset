using Quark.Asset;
using Quark.Encrypt;

namespace Quark
{
    /// <summary>
    /// 数据代理类；
    /// </summary>
    internal class QuarkDataProxy
    {
        static QuarkEncrytionData quarkEncrytionData;
        /// <summary>
        /// 加密数据；
        /// </summary>
        public static QuarkEncrytionData QuarkEncrytionData
        {
            get
            {
                if (quarkEncrytionData == null)
                    quarkEncrytionData = new QuarkEncrytionData();
                return quarkEncrytionData;
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
        public static QuarkDataset QuarkAssetDataset { get; internal set; }
        public static QuarkManifest QuarkManifest { get; internal set; }
        public static QuarkLoadMode QuarkAssetLoadMode { get; internal set; }
    }
}
