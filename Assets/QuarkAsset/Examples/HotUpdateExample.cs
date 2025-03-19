using Quark;
using Quark.Asset;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class HotUpdateExample : MonoBehaviour
{
    [SerializeField] private Text statusText;
    [SerializeField] private Text progressText;
    [SerializeField] private Image progressBar;
    [SerializeField] private Button startUpdateButton;

    [Header("更新配置")]
    [SerializeField] private string remoteUrl = "http://your-server.com/assets";
    [SerializeField] private string aesKey = "";  // 可选，如果资源使用AES加密

    private QuarkAssetUpdater updater;
    private string persistentPath;

    private void Start()
    {
        persistentPath = Path.Combine(Application.persistentDataPath, "QuarkAssets");
        if (!Directory.Exists(persistentPath))
        {
            Directory.CreateDirectory(persistentPath);
        }
        
        InitializeUpdater();
        SetupUI();
    }

    private void InitializeUpdater()
    {
        if (string.IsNullOrEmpty(aesKey))
        {
            updater = new QuarkAssetUpdater(remoteUrl, persistentPath);
        }
        else
        {
            updater = new QuarkAssetUpdater(remoteUrl, persistentPath, aesKey);
        }

        // 注册事件
        updater.OnUpdateStart += OnUpdateStart;
        updater.OnRemoteManifestDownloaded += OnRemoteManifestDownloaded;
        updater.OnRemoteManifestDownloadFailed += OnRemoteManifestDownloadFailed;
        updater.OnManifestVerified += OnManifestVerified;
        updater.OnUpdateProgress += OnUpdateProgress;
        updater.OnUpdateCompleted += OnUpdateCompleted;
    }

    private void SetupUI()
    {
        startUpdateButton.onClick.AddListener(StartUpdate);
        statusText.text = "准备就绪，点击开始更新";
        progressText.text = "0%";
        progressBar.fillAmount = 0;
    }

    public void StartUpdate()
    {
        startUpdateButton.interactable = false;
        StartCoroutine(UpdateCoroutine());
    }

    private IEnumerator UpdateCoroutine()
    {
        // 尝试加载本地清单
        QuarkManifest localManifest = null;
        var localManifestPath = Path.Combine(persistentPath, QuarkConstant.MANIFEST_NAME);
        
        if (File.Exists(localManifestPath))
        {
            try
            {
                var manifestJson = File.ReadAllText(localManifestPath);
                if (!string.IsNullOrEmpty(aesKey))
                {
                    var keyBytes = QuarkUtility.GenerateBytesAESKey(aesKey);
                    manifestJson = QuarkUtility.AESDecryptStringToString(manifestJson, keyBytes);
                }
                localManifest = QuarkUtility.ToObject<QuarkManifest>(manifestJson);
                statusText.text = "已加载本地清单";
            }
            catch (System.Exception e)
            {
                Debug.LogError($"加载本地清单失败: {e.Message}");
                statusText.text = "加载本地清单失败，将执行全新安装";
            }
        }
        else
        {
            statusText.text = "未找到本地清单，将执行全新安装";
        }

        yield return new WaitForSeconds(0.5f);

        // 开始更新
        updater.StartUpdate(localManifest);
    }

    #region 事件处理
    private void OnUpdateStart()
    {
        statusText.text = "开始更新...";
    }

    private void OnRemoteManifestDownloaded(QuarkManifest manifest)
    {
        statusText.text = $"远程清单下载成功，版本: {manifest.BuildVersion}_{manifest.InternalBuildVersion}";
    }

    private void OnRemoteManifestDownloadFailed(string errorMessage)
    {
        statusText.text = $"远程清单下载失败: {errorMessage}";
        startUpdateButton.interactable = true;
    }

    private void OnManifestVerified(Quark.Manifest.QuarkManifestVerifyResult result)
    {
        statusText.text = $"资源校验完成，{result.VerificationSuccessInfos.Length}个资源正常，{result.VerificationFailureInfos.Length}个资源需要更新";
    }

    private void OnUpdateProgress(QuarkUpdateProgressInfo info)
    {
        float progress = info.TotalProgress;
        progressBar.fillAmount = progress;
        progressText.text = $"{Mathf.FloorToInt(progress * 100)}%";
        
        statusText.text = $"正在下载: {info.CurrentDownloadIndex + 1}/{info.TotalDownloadCount} " + 
                          $"({FormatFileSize(info.DownloadedSize)}/{FormatFileSize(info.TotalRequiredDownloadSize)})";
    }

    private void OnUpdateCompleted(QuarkUpdateResult result)
    {
        if (result.IsCompleteSuccess)
        {
            statusText.text = $"更新完成! 共下载{result.SuccessedTasks.Length}个文件，总大小{FormatFileSize(result.DownloadedSize)}";
        }
        else
        {
            statusText.text = $"更新部分失败! 成功{result.SuccessedTasks.Length}个文件，失败{result.FailedTasks.Length}个文件";
        }
        
        // 保存远程清单到本地
        if (updater.IsUpdating == false && result.IsCompleteSuccess)
        {
            StartCoroutine(SaveManifestCoroutine());
        }
        
        startUpdateButton.interactable = true;
    }
    #endregion

    private IEnumerator SaveManifestCoroutine()
    {
        // 等待一帧，确保所有文件都已经写入
        yield return null;

        var operation = new DownloadRemoteManifestOperation(remoteUrl, aesKey);
        QuarkResources.EnqueueOperation(operation);
        
        while (!operation.IsDone)
        {
            yield return null;
        }

        if (operation.Status == AsyncOperationStatus.Succeeded)
        {
            var manifest = operation.Manifest;
            var manifestJson = QuarkUtility.ToJson(manifest);
            
            try
            {
                var localManifestPath = Path.Combine(persistentPath, QuarkConstant.MANIFEST_NAME);
                
                if (!string.IsNullOrEmpty(aesKey))
                {
                    var keyBytes = QuarkUtility.GenerateBytesAESKey(aesKey);
                    manifestJson = QuarkUtility.AESEncryptStringToString(manifestJson, keyBytes);
                }
                
                File.WriteAllText(localManifestPath, manifestJson);
                Debug.Log("清单已保存到本地: " + localManifestPath);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"保存清单失败: {e.Message}");
            }
        }
        else
        {
            Debug.LogError($"获取最新清单失败: {operation.Error}");
        }
    }

    // 使用异步操作的方式执行更新
    public void StartUpdateUsingOperation()
    {
        startUpdateButton.interactable = false;
        statusText.text = "开始通过Operation更新...";
        
        StartCoroutine(UpdateUsingOperationCoroutine());
    }

    private IEnumerator UpdateUsingOperationCoroutine()
    {
        // 尝试加载本地清单
        QuarkManifest localManifest = null;
        var localManifestPath = Path.Combine(persistentPath, QuarkConstant.MANIFEST_NAME);
        
        if (File.Exists(localManifestPath))
        {
            try
            {
                var manifestJson = File.ReadAllText(localManifestPath);
                if (!string.IsNullOrEmpty(aesKey))
                {
                    var keyBytes = QuarkUtility.GenerateBytesAESKey(aesKey);
                    manifestJson = QuarkUtility.AESDecryptStringToString(manifestJson, keyBytes);
                }
                localManifest = QuarkUtility.ToObject<QuarkManifest>(manifestJson);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"加载本地清单失败: {e.Message}");
            }
        }

        // 创建并启动更新操作
        var operation = new DownloadUpdateAssetOperation(remoteUrl, persistentPath, aesKey, localManifest);
        QuarkResources.EnqueueOperation(operation);
        
        // 监控进度
        while (!operation.IsDone)
        {
            progressBar.fillAmount = operation.Progress;
            progressText.text = $"{Mathf.FloorToInt(operation.Progress * 100)}%";
            
            if (!string.IsNullOrEmpty(operation.CurrentDownloadInfo))
            {
                statusText.text = $"正在下载: {operation.CurrentDownloadInfo}";
            }
            
            yield return null;
        }

        // 处理结果
        if (operation.Status == AsyncOperationStatus.Succeeded)
        {
            statusText.text = "更新成功!";
        }
        else
        {
            statusText.text = $"更新失败: {operation.Error}";
        }
        
        startUpdateButton.interactable = true;
    }

    private string FormatFileSize(long fileSize)
    {
        if (fileSize < 1024)
            return $"{fileSize}B";
        else if (fileSize < 1024 * 1024)
            return $"{fileSize / 1024f:F1}KB";
        else if (fileSize < 1024 * 1024 * 1024)
            return $"{fileSize / (1024f * 1024f):F2}MB";
        else
            return $"{fileSize / (1024f * 1024f * 1024f):F2}GB";
    }

    private void OnDestroy()
    {
        // 取消注册事件
        if (updater != null)
        {
            updater.OnUpdateStart -= OnUpdateStart;
            updater.OnRemoteManifestDownloaded -= OnRemoteManifestDownloaded;
            updater.OnRemoteManifestDownloadFailed -= OnRemoteManifestDownloadFailed;
            updater.OnManifestVerified -= OnManifestVerified;
            updater.OnUpdateProgress -= OnUpdateProgress;
            updater.OnUpdateCompleted -= OnUpdateCompleted;
        }
    }
}
