using UnityEngine;
using System.IO;
using Quark.Asset;
using System;

namespace Quark
{
    /// <summary>
    /// Quark配置脚本，挂载到物体上配置即可；
    /// </summary>
    public class QuarkLauncher : MonoBehaviour
    {
        [SerializeField] bool autoStartBasedOnConfig = true;
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
        /// 加载模式，分别为Editor与Build；
        /// </summary>
        [SerializeField] QuarkLoadMode loadMode;
        /// <summary>
        /// QuarkAssetLoadMode 下AssetDatabase模式所需的寻址数据；
        /// <see cref="Quark.QuarkLoadMode"/>
        /// </summary>
        [SerializeField] QuarkDataset quarkDataset;
        /// <summary>
        /// 对称加密密钥；
        /// </summary>
        [SerializeField] string manifestAesKey;
        /// <summary>
        /// 加密偏移量；
        /// </summary>
        [SerializeField] ulong encryptionOffset;
        static QuarkLauncher instance;
        public static QuarkLauncher Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = GameObject.FindObjectOfType<QuarkLauncher>();
                    if (instance == null)
                    {
                        var go = new GameObject(typeof(QuarkLauncher).Name);
                        instance = go.AddComponent<QuarkLauncher>();
                    }
                }
                return instance;
            }
        }
        /// <summary>
        /// 启动当前配置内容；
        /// </summary>
        public void LaunchWithConfig(Action onSuccess, Action<string> onFailure)
        {
            switch (loadMode)
            {
                case QuarkLoadMode.AssetDatabase:
                    {
                        QuarkResources.LaunchAssetDatabaseMode(quarkDataset, onSuccess, onFailure);
                    }
                    break;
                case QuarkLoadMode.AssetBundle:
                    {
                        var dirPath = string.Empty;
                        switch (quarkBuildPath)
                        {
                            case QuarkBuildPath.StreamingAssets:
                                {
                                    #region streamingAssetPath
                                    if (enableStreamingRelativeBuildPath)
                                        dirPath = Path.Combine(Application.streamingAssetsPath, streamingRelativeBuildPath);
                                    else
                                        dirPath = Application.streamingAssetsPath;
                                    #endregion;
                                }
                                break;
                            case QuarkBuildPath.PersistentDataPath:
                                {
                                    #region persistentPath
                                    if (enablePersistentRelativeBundlePath)
                                        dirPath = Path.Combine(Application.persistentDataPath, persistentRelativeBundlePath);
                                    else
                                        dirPath = Application.persistentDataPath;
                                    #endregion;
                                }
                                break;
                        }
                        QuarkResources.LaunchAssetBundleMode(dirPath, onSuccess, onFailure, manifestAesKey, encryptionOffset);
                    }
                    break;
            }
        }
        void Awake()
        {
            instance = this;
            if (autoStartBasedOnConfig)
            {
                LaunchWithConfig(OnLaunchSuccess, OnLaunchFailure);
            }
        }
        void OnLaunchSuccess()
        {
            QuarkUtility.LogInfo($"{loadMode} launch success");
        }
        void OnLaunchFailure(string errorMsg)
        {
            QuarkUtility.LogInfo($"{loadMode} launch fail: {errorMsg}");
        }
    }
}
