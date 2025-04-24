using System;
using UnityEngine;

namespace Quark.Editor
{
    /// <summary>
    /// QuarkAsset环境配置
    /// 定义不同环境（如开发、测试、生产）的下载地址、加密设置等
    /// </summary>
    public class QuarkProfile : ScriptableObject
    {
        [SerializeField]
        private string profileName = "New Profile";
        
        [SerializeField]
        private string downloadURL = "http://localhost:8080/quarkassets";
        
        [SerializeField]
        private bool useAesEncryption = false;
        
        [SerializeField]
        private string aesEncryptionKey = "QuarkAssetKey";
        
        [SerializeField]
        private bool useOffsetEncryption = false;
        
        [SerializeField]
        private int offsetEncryptionValue = 32;
        
        [SerializeField]
        private string customDescription = "";
        
        /// <summary>
        /// 环境配置名称
        /// </summary>
        public string ProfileName
        {
            get { return profileName; }
            set { profileName = value; }
        }
        
        /// <summary>
        /// 资源下载地址
        /// </summary>
        public string DownloadURL
        {
            get { return downloadURL; }
            set { downloadURL = value; }
        }
        
        /// <summary>
        /// 是否使用AES加密
        /// </summary>
        public bool UseAesEncryption
        {
            get { return useAesEncryption; }
            set { useAesEncryption = value; }
        }
        
        /// <summary>
        /// AES加密密钥
        /// </summary>
        public string AesEncryptionKey
        {
            get { return aesEncryptionKey; }
            set { aesEncryptionKey = value; }
        }
        
        /// <summary>
        /// 是否使用偏移加密
        /// </summary>
        public bool UseOffsetEncryption
        {
            get { return useOffsetEncryption; }
            set { useOffsetEncryption = value; }
        }
        
        /// <summary>
        /// 偏移加密值
        /// </summary>
        public int OffsetEncryptionValue
        {
            get { return offsetEncryptionValue; }
            set { offsetEncryptionValue = value; }
        }
        
        /// <summary>
        /// 自定义描述
        /// </summary>
        public string CustomDescription
        {
            get { return customDescription; }
            set { customDescription = value; }
        }
        
        /// <summary>
        /// 获取完整的环境描述
        /// </summary>
        /// <returns>环境描述</returns>
        public string GetDescription()
        {
            var desc = $"{profileName} - {downloadURL}";
            
            if (!string.IsNullOrEmpty(customDescription))
            {
                desc += $" - {customDescription}";
            }
            
            return desc;
        }
        
        /// <summary>
        /// 复制当前配置
        /// </summary>
        /// <returns>新的配置实例</returns>
        public QuarkProfile Clone()
        {
            var clone = CreateInstance<QuarkProfile>();
            clone.profileName = this.profileName + " (Copy)";
            clone.downloadURL = this.downloadURL;
            clone.useAesEncryption = this.useAesEncryption;
            clone.aesEncryptionKey = this.aesEncryptionKey;
            clone.useOffsetEncryption = this.useOffsetEncryption;
            clone.offsetEncryptionValue = this.offsetEncryptionValue;
            clone.customDescription = this.customDescription;
            
            return clone;
        }
    }
}
