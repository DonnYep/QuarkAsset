using UnityEngine;
using System.IO;
using Quark.Asset;

namespace Quark
{
    /// <summary>
    /// Quark配置脚本，挂载到物体上配置即可；
    /// </summary>
    public class QuarkConfig : MonoBehaviour
    {
        /// <summary>
        /// 资源存储地址；
        /// </summary>
        public QuarkBuildPath QuarkBuildPath;
        /// <summary>
        ///启用 ab build 的相对路径；
        /// </summary>
        public bool EnableRelativeBuildPath;
        /// <summary>
        /// ab Build 的相对地址；
        /// </summary>
        public string RelativeBuildPath;


        /// <summary>
        /// 资源所在URI；
        /// </summary>
        public string Url;
        /// <summary>
        /// 是否去ping uri地址；
        /// </summary>
        public bool PingUrl;
        /// <summary>
        /// 加载模式，分别为Editor与Build；
        /// </summary>
        public QuarkLoadMode QuarkAssetLoadMode;
        /// <summary>
        /// QuarkAssetLoadMode 下AssetDatabase模式所需的寻址数据；
        /// <see cref="Quark.QuarkLoadMode"/>
        /// </summary>
        public QuarkAssetDataset QuarkAssetDataset;
        /// <summary>
        /// 资源下载到的地址；
        /// </summary>
        public QuarkDownloadedPath QuarkDownloadedPath;
        /// <summary>
        /// 使用持久化的相对路径；
        /// </summary>
        public bool EnableRelativeLoadPath;
        /// <summary>
        /// 持久化路径下的相对地址；
        /// </summary>
        public string RelativeLoadPath;
        /// <summary>
        /// 对称加密密钥；
        /// </summary>
        public string BuildInfoAESEncryptionKey;
        /// <summary>
        /// 加密偏移量；
        /// </summary>
        public ulong EncryptionOffset;
        /// <summary>
        /// QuarkPersistentPathType 枚举下的自定义持久化路径；
        /// </summary>
        public string CustomeAbsolutePath;

        /// <summary>
        /// 配置的路径都整合到此字段；
        /// </summary>
        string downloadPath;

        static QuarkConfig instance;
        public static QuarkConfig Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = GameObject.FindObjectOfType<QuarkConfig>();
                    if (instance == null)
                    {
                        var go = new GameObject(typeof(QuarkConfig).Name);
                        instance = go.AddComponent<QuarkConfig>();
                    }
                }
                return instance;
            }
        }
        void Awake()
        {
            instance = this;
            QuarkResources.QuarkEncryptionOffset = EncryptionOffset;
            QuarkResources.QuarkAssetLoadMode = QuarkAssetLoadMode;
            var keyStr = BuildInfoAESEncryptionKey;
            var aesKey = QuarkUtility.GenerateBytesAESKey(keyStr);
            QuarkResources.QuarkAESEncryptionKey = aesKey;
            switch (QuarkAssetLoadMode)
            {
                case QuarkLoadMode.AssetDatabase:
                    {
                        if (QuarkAssetDataset != null)
                        {
                            QuarkEngine.Instance.SetAssetDatabaseModeData(QuarkAssetDataset);
                            QuarkEngine.Instance.onCompareManifestSuccess?.Invoke(0);
                        }
                        else
                        {
                            QuarkEngine.Instance.onCompareManifestFailure?.Invoke(null);
                        }
                    }
                    break;
                case QuarkLoadMode.AssetBundle:
                    {
                        switch (QuarkBuildPath)
                        {
                            case QuarkBuildPath.StreamingAssets:
                                LoadStreamingManifest();
                                break;
                            case QuarkBuildPath.URL:
                                LoadURLManifest();
                                break;
                        }
                    }
                    break;
            }
        }
        void LoadURLManifest()
        {
            QuarkUtility.IsStringValid(Url, "URI is invalid !");
            if (PingUrl)
            {
                if (!QuarkUtility.PingURI(Url))
                    return;
            }
            if (QuarkDownloadedPath != QuarkDownloadedPath.Custome)
            {
                switch (QuarkDownloadedPath)
                {
                    case QuarkDownloadedPath.PersistentDataPath:
                        downloadPath = Application.persistentDataPath;
                        break;
                }
                if (EnableRelativeLoadPath)
                {
                    downloadPath = Path.Combine(downloadPath, RelativeLoadPath);
                }
            }
            else
            {
                downloadPath = CustomeAbsolutePath;
            }
            QuarkUtility.IsStringValid(downloadPath, "DownloadPath is invalid !");
            if (!Directory.Exists(downloadPath))
                Directory.CreateDirectory(downloadPath);
            QuarkEngine.Instance.Initiate(Url, downloadPath);
            QuarkEngine.Instance.RequestMainifestFromURLAsync();
        }
        void LoadStreamingManifest()
        {
            string streamingAssetPath = string.Empty;
            if (EnableRelativeBuildPath)
            {
#if UNITY_EDITOR||UNITY_ANDROID||UNITY_STANDALONE
                streamingAssetPath = Path.Combine(Application.streamingAssetsPath, RelativeBuildPath);
#elif UNITY_IPHONE && !UNITY_EDITOR
                streamingAssetPath = @"file://" + Path.Combine(Application.streamingAssetsPath, RelativeBuildPath);
#endif
            }
            else
            {
#if UNITY_EDITOR||UNITY_ANDROID||UNITY_STANDALONE
                streamingAssetPath = Application.streamingAssetsPath;
#elif UNITY_IPHONE && !UNITY_EDITOR
                streamingAssetPath = @"file://" + Application.streamingAssetsPath;
#endif
            }
            QuarkEngine.Instance.Initiate(streamingAssetPath, streamingAssetPath);
            QuarkEngine.Instance.RequestManifestFromStreamingAssetsAsync();
        }
    }
}
