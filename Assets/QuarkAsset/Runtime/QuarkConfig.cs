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
        [SerializeField] bool autoStart=true;
        /// <summary>
        /// 资源存储地址；
        /// </summary>
        [SerializeField] QuarkBuildPath quarkBuildPath;
        /// <summary>
        ///启用 ab build 的相对路径；
        /// </summary>
        [SerializeField] bool enableStreamingRelativeBuildPath;
        /// <summary>
        /// ab Build 的相对地址；
        /// </summary>
        [SerializeField] string streamingRelativeBuildPath;

        /// <summary>
        /// 资源所在URI；
        /// </summary>
        [SerializeField] string url;
        /// <summary>
        /// 是否去ping uri地址；
        /// </summary>
        [SerializeField] bool pingUrl;
        /// <summary>
        /// 加载模式，分别为Editor与Build；
        /// </summary>
        [SerializeField] QuarkLoadMode quarkAssetLoadMode;
        /// <summary>
        /// QuarkAssetLoadMode 下AssetDatabase模式所需的寻址数据；
        /// <see cref="Quark.QuarkLoadMode"/>
        /// </summary>
        [SerializeField] QuarkAssetDataset quarkAssetDataset;
        /// <summary>
        /// 资源下载到的地址；
        /// </summary>
        [SerializeField] QuarkDownloadedPath quarkDownloadedPath;
        /// <summary>
        /// 使用持久化的相对路径；
        /// </summary>
        [SerializeField] bool enableRelativeLoadPath;
        /// <summary>
        /// 持久化路径下的相对地址；
        /// </summary>
        [SerializeField] string relativeLoadPath;
        /// <summary>
        /// 对称加密密钥；
        /// </summary>
        [SerializeField] string buildInfoAESEncryptionKey;
        /// <summary>
        /// 加密偏移量；
        /// </summary>
        [SerializeField] ulong encryptionOffset;
        /// <summary>
        /// QuarkPersistentPathType 枚举下的自定义持久化路径；
        /// </summary>
        [SerializeField] string customeAbsolutePath;

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
        public void InitQuark()
        {
            var keyStr = buildInfoAESEncryptionKey;
            var aesKey = QuarkUtility.GenerateBytesAESKey(keyStr);
            QuarkResources.QuarkAESEncryptionKey = aesKey;
            switch (quarkAssetLoadMode)
            {
                case QuarkLoadMode.AssetDatabase:
                    {
                        if (quarkAssetDataset != null)
                        {
                            QuarkEngine.Instance.SetAssetDatabaseModeData(quarkAssetDataset);
                            QuarkEngine.Instance.onCompareManifestSuccess?.Invoke(null, 0);
                        }
                        else
                        {
                            QuarkEngine.Instance.onCompareManifestFailure?.Invoke(null);
                        }
                    }
                    break;
                case QuarkLoadMode.AssetBundle:
                    {
                        switch (quarkBuildPath)
                        {
                            case QuarkBuildPath.StreamingAssets:
                                LoadStreamingManifest();
                                break;
                            case QuarkBuildPath.Remote:
                                LoadURLManifest();
                                break;
                        }
                    }
                    break;
            }
        }
        void Awake()
        {
            instance = this;
            QuarkResources.QuarkEncryptionOffset = encryptionOffset;
            QuarkResources.QuarkAssetLoadMode = quarkAssetLoadMode;
            if (autoStart)
                InitQuark();
        }
        void LoadURLManifest()
        {
            QuarkUtility.IsStringValid(url, "URI is invalid !");
            if (pingUrl)
            {
                if (!QuarkUtility.PingURI(url))
                    return;
            }
            if (quarkDownloadedPath != QuarkDownloadedPath.Custome)
            {
                switch (quarkDownloadedPath)
                {
                    case QuarkDownloadedPath.PersistentDataPath:
                        downloadPath = Application.persistentDataPath;
                        break;
                }
                if (enableRelativeLoadPath)
                {
                    downloadPath = Path.Combine(downloadPath, relativeLoadPath);
                }
            }
            else
            {
                downloadPath = customeAbsolutePath;
            }
            QuarkUtility.IsStringValid(downloadPath, "DownloadPath is invalid !");
            if (!Directory.Exists(downloadPath))
                Directory.CreateDirectory(downloadPath);
            QuarkEngine.Instance.Initiate(url, downloadPath);
            QuarkEngine.Instance.RequestMainifestFromURLAsync();
        }
        void LoadStreamingManifest()
        {
            string streamingAssetPath = string.Empty;
            if (enableStreamingRelativeBuildPath)
            {
#if UNITY_EDITOR||UNITY_ANDROID||UNITY_STANDALONE
                streamingAssetPath = Path.Combine(Application.streamingAssetsPath, streamingRelativeBuildPath);
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
            QuarkEngine.Instance.RequestManifestFromStreamingAssetAsync();
        }
    }
}
