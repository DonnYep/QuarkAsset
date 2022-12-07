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
        [SerializeField] bool autoStart = true;
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
        ///启用 ab build 的相对路径；
        /// </summary>
        [SerializeField] bool enablePersistentRelativeBundlePath;
        /// <summary>
        /// ab Build 的相对地址；
        /// </summary>
        [SerializeField] string persistentRelativeBundlePath;

        /// <summary>
        /// 资源所在URI；
        /// </summary>
        [SerializeField] string url;
        /// <summary>
        /// 加载模式，分别为Editor与Build；
        /// </summary>
        [SerializeField] QuarkLoadMode loadMode;
        /// <summary>
        /// QuarkAssetLoadMode 下AssetDatabase模式所需的寻址数据；
        /// <see cref="Quark.QuarkLoadMode"/>
        /// </summary>
        [SerializeField] QuarkAssetDataset quarkAssetDataset;
        /// <summary>
        /// 资源下载到的地址；
        /// </summary>
        [SerializeField] QuarkDownloadedPath downloadedPath;
        /// <summary>
        /// 使用持久化的相对路径；
        /// </summary>
        [SerializeField] bool enableDownloadRelativePath;
        /// <summary>
        /// 持久化路径下的相对地址；
        /// </summary>
        [SerializeField] string downloadRelativePath;
        /// <summary>
        /// 对称加密密钥；
        /// </summary>
        [SerializeField] string manifestAesEncryptKey;
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

        /// <summary>
        /// 资源所在URI；
        /// </summary>
        public string Url
        {
            get { return url; }
            set { url = value; }
        }

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
        public void LaunchAssetBundleMode()
        {

        }
        public void Initialize()
        {
            switch (loadMode)
            {
                case QuarkLoadMode.AssetDatabase:
                    {
                        if (quarkAssetDataset != null)
                        {
                            QuarkResources.SetAssetDatabaseModeDataset(quarkAssetDataset);
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
                                QuarkEngine.Instance.RequestManifestFromStreamingAssetAsync();
                                break;
                            case QuarkBuildPath.PersistentDataPath:
                                QuarkEngine.Instance.RequestManifestFromPersistentDataPathAsync();
                                break;
                            case QuarkBuildPath.URL:
                                {
                                    QuarkUtility.IsStringValid(downloadPath, "DownloadPath is invalid !");
                                    if (!Directory.Exists(downloadPath))
                                        Directory.CreateDirectory(downloadPath);
                                }
                                break;
                        }
                    }
                    break;
            }
        }
        public void SaveConfig()
        {
            #region persistentPath
            string persistentPath = string.Empty;
            if (enablePersistentRelativeBundlePath)
            {
#if UNITY_EDITOR||UNITY_ANDROID||UNITY_STANDALONE
                persistentPath = Path.Combine(Application.persistentDataPath, persistentRelativeBundlePath);
#elif UNITY_IPHONE && !UNITY_EDITOR
                persistentPath = @"file://" + Path.Combine(Application.persistentDataPath, persistentRelativeBundlePath);
#endif
            }
            else
            {
#if UNITY_EDITOR||UNITY_ANDROID||UNITY_STANDALONE
                persistentPath = Application.persistentDataPath;
#elif UNITY_IPHONE && !UNITY_EDITOR
                persistentPath = @"file://" + Application.persistentDataPath;
#endif
            }
            QuarkDataProxy.PersistentPath = persistentPath;
            #endregion;


            #region streamingAssetPath
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
            QuarkDataProxy.StreamingAssetPath = streamingAssetPath;
            #endregion;

            QuarkDataProxy.URL = url;

            #region downloadedPath
            QuarkUtility.IsStringValid(url, "URI is invalid !");
            if (downloadedPath != QuarkDownloadedPath.CustomePath)
            {
                switch (downloadedPath)
                {
                    case QuarkDownloadedPath.PersistentDataPath:
                        downloadPath = Application.persistentDataPath;
                        break;
                }
                if (enableDownloadRelativePath)
                {
                    downloadPath = Path.Combine(downloadPath, downloadRelativePath);
                }
            }
            else
            {
                downloadPath = customeAbsolutePath;
            }
            QuarkDataProxy.PersistentPath = downloadPath;
            #endregion;
        }
        void Awake()
        {
            instance = this;
            QuarkDataProxy.QuarkEncryptionOffset = encryptionOffset;
            QuarkResources.QuarkAssetLoadMode = loadMode;
            QuarkDataProxy.QuarkAESEncryptionKey = manifestAesEncryptKey;
            SaveConfig();
            if (autoStart)
                Initialize();
        }
    }
}
