using System;
using System.IO;
using Quark.Asset;
using UnityEngine.Networking;

namespace Quark
{
    /// <summary>
    /// 下载远程清单操作
    /// </summary>
    public class DownloadRemoteManifestOperation : AsyncOperationBase
    {
        private string remoteUrl;
        private string persistentPath;
        private byte[] aesKeyBytes;
        private UnityWebRequest webRequest;
        private string savePath;
        private int timeoutSeconds = 30;
        
        /// <summary>
        /// 超时时间（秒）
        /// </summary>
        public int TimeoutSeconds
        {
            get { return timeoutSeconds; }
            set { timeoutSeconds = value > 0 ? value : 30; }
        }
        
        /// <summary>
        /// 下载的清单
        /// </summary>
        public QuarkManifest Manifest { get; private set; }
        
        /// <summary>
        /// 创建下载远程清单操作
        /// </summary>
        /// <param name="remoteUrl">远程地址</param>
        /// <param name="persistentPath">持久化路径</param>
        public DownloadRemoteManifestOperation(string remoteUrl, string persistentPath)
        {
            this.remoteUrl = remoteUrl;
            this.persistentPath = persistentPath;
            this.aesKeyBytes = new byte[0];
        }
        
        /// <summary>
        /// 创建下载远程清单操作（带AES加密）
        /// </summary>
        /// <param name="remoteUrl">远程地址</param>
        /// <param name="persistentPath">持久化路径</param>
        /// <param name="aesKey">AES加密密钥</param>
        public DownloadRemoteManifestOperation(string remoteUrl, string persistentPath, string aesKey)
        {
            this.remoteUrl = remoteUrl;
            this.persistentPath = persistentPath;
            this.aesKeyBytes = QuarkUtility.GenerateBytesAESKey(aesKey);
        }
        
        /// <summary>
        /// 创建下载远程清单操作（带AES加密）
        /// </summary>
        /// <param name="remoteUrl">远程地址</param>
        /// <param name="persistentPath">持久化路径</param>
        /// <param name="aesKeyBytes">AES加密密钥字节数组</param>
        public DownloadRemoteManifestOperation(string remoteUrl, string persistentPath, byte[] aesKeyBytes)
        {
            this.remoteUrl = remoteUrl;
            this.persistentPath = persistentPath;
            this.aesKeyBytes = aesKeyBytes;
        }
        
        internal override void OnStart()
        {
            Status = AsyncOperationStatus.Processing;
            
            try
            {
                // 构建清单URL
                string manifestUrl = QuarkUtility.WebPathCombine(remoteUrl, QuarkConstant.MANIFEST_NAME);
                savePath = Path.Combine(persistentPath, QuarkConstant.MANIFEST_NAME);
                
                // 确保目录存在
                string directory = Path.GetDirectoryName(savePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                // 创建下载请求
                webRequest = UnityWebRequest.Get(manifestUrl);
                webRequest.timeout = timeoutSeconds;
                
                // 发送请求
                var operation = webRequest.SendWebRequest();
                operation.completed += OnRequestCompleted;
            }
            catch (Exception ex)
            {
                OnError($"下载清单时出错: {ex.Message}");
            }
        }
        
        internal override void OnUpdate()
        {
            if (Status != AsyncOperationStatus.Processing || webRequest == null)
                return;
                
            // 更新进度
            Progress = webRequest.downloadProgress;
        }
        
        internal override void OnAbort()
        {
            if (webRequest != null)
            {
                webRequest.Dispose();
                webRequest = null;
            }
            
            Status = AsyncOperationStatus.Failed;
            Error = "Operation aborted";
        }
        
        private void OnRequestCompleted(UnityEngine.AsyncOperation operation)
        {
            if (webRequest == null)
                return;
                
            try
            {
                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    // 下载成功
                    string manifestText = webRequest.downloadHandler.text;
                    
                    // 保存到本地
                    File.WriteAllText(savePath, manifestText);
                    
                    // 解析清单
                    Manifest = ParseManifest(manifestText);
                    
                    if (Manifest != null)
                    {
                        Status = AsyncOperationStatus.Succeeded;
                    }
                    else
                    {
                        OnError("解析清单失败");
                    }
                }
                else
                {
                    OnError($"下载清单失败: {webRequest.error}");
                }
            }
            catch (Exception ex)
            {
                OnError($"处理清单下载响应时出错: {ex.Message}");
            }
            finally
            {
                if (webRequest != null)
                {
                    webRequest.Dispose();
                    webRequest = null;
                }
            }
        }
        
        private QuarkManifest ParseManifest(string manifestData)
        {
            try
            {
                // 如果有AES加密，先解密
                if (aesKeyBytes != null && aesKeyBytes.Length > 0)
                {
                    manifestData = QuarkUtility.AESDecryptStringToString(manifestData, aesKeyBytes);
                }
                
                // 解析JSON
                return QuarkUtility.ToObject<QuarkManifest>(manifestData);
            }
            catch (Exception ex)
            {
                QuarkUtility.LogError($"解析清单失败: {ex.Message}");
                return null;
            }
        }
        
        private void OnError(string errorMessage)
        {
            Error = errorMessage;
            Status = AsyncOperationStatus.Failed;
            
            if (webRequest != null)
            {
                webRequest.Dispose();
                webRequest = null;
            }
        }
    }
}
