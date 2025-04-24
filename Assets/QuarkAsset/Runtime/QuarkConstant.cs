using System;
using System.IO;
using UnityEngine;

namespace Quark
{
    /// <summary>
    /// Quark相关常量
    /// </summary>
    public static class QuarkConstant
    {
        /// <summary>
        /// 系统名称
        /// </summary>
        public const string SYSTEM_NAME = "QuarkAsset";
        
        /// <summary>
        /// 系统版本
        /// </summary>
        public const string SYSTEM_VERSION = "1.0.0";
        
        /// <summary>
        /// 清单文件名
        /// </summary>
        public const string MANIFEST_NAME = "QuarkManifest.json";
        
        /// <summary>
        /// 默认的资源目录名
        /// </summary>
        public const string DEFAULT_RESOURCES_DIRECTORY = "QuarkResources";
        
        /// <summary>
        /// 默认的持久化目录名
        /// </summary>
        public const string DEFAULT_PERSISTENT_DIRECTORY = "QuarkPersistent";
        
        /// <summary>
        /// 默认的流式目录名
        /// </summary>
        public const string DEFAULT_STREAMING_DIRECTORY = "QuarkStreaming";
        
        /// <summary>
        /// 默认的资源路径（编辑器模式下）
        /// </summary>
        public const string DEFAULT_EDITOR_RESOURCES_PATH = "Assets/QuarkResources";
        
        /// <summary>
        /// 默认的构建输出路径
        /// </summary>
        public const string DEFAULT_BUILD_OUTPUT_PATH = "QuarkBuild";
        
        /// <summary>
        /// 默认的缓存路径（持久化数据）
        /// </summary>
        public static string DefaultPersistentPath
        {
            get
            {
                return Path.Combine(Application.persistentDataPath, DEFAULT_PERSISTENT_DIRECTORY);
            }
        }
        
        /// <summary>
        /// 默认的流式路径（只读数据）
        /// </summary>
        public static string DefaultStreamingPath
        {
            get
            {
                return Path.Combine(Application.streamingAssetsPath, DEFAULT_STREAMING_DIRECTORY);
            }
        }
        
        /// <summary>
        /// 默认资源版本
        /// </summary>
        public const string DEFAULT_VERSION = "1.0.0";
        
        /// <summary>
        /// 默认内部构建版本
        /// </summary>
        public const int DEFAULT_INTERNAL_VERSION = 1;
        
        /// <summary>
        /// 默认超时时间（秒）
        /// </summary>
        public const int DEFAULT_TIMEOUT = 30;
        
        /// <summary>
        /// 默认重试次数
        /// </summary>
        public const int DEFAULT_RETRY_COUNT = 3;
        
        /// <summary>
        /// 默认并行下载数量
        /// </summary>
        public const int DEFAULT_PARALLEL_DOWNLOAD_COUNT = 3;
        
        /// <summary>
        /// 默认AES加密密钥
        /// </summary>
        public const string DEFAULT_AES_KEY = "QuarkAssetDefaultKey";
        
        /// <summary>
        /// 默认偏移加密值
        /// </summary>
        public const int DEFAULT_OFFSET_VALUE = 64;
    }
}
