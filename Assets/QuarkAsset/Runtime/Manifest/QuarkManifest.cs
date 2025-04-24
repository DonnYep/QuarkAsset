using System;
using System.Collections.Generic;
using Quark.Asset;

namespace Quark.Manifest
{
    /// <summary>
    /// Quark资源清单
    /// </summary>
    [Serializable]
    public class QuarkManifest
    {
        /// <summary>
        /// 构建版本
        /// </summary>
        public string BuildVersion { get; set; }
        
        /// <summary>
        /// 内部构建版本号
        /// </summary>
        public int InternalBuildVersion { get; set; }
        
        /// <summary>
        /// 构建时间
        /// </summary>
        public string BuildTime { get; set; }
        
        /// <summary>
        /// 构建哈希值
        /// </summary>
        public string BuildHash { get; set; }
        
        /// <summary>
        /// 资源包信息字典
        /// </summary>
        public Dictionary<string, QuarkBundleAsset> BundleInfoDict { get; set; } = new Dictionary<string, QuarkBundleAsset>();
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public QuarkManifest()
        {
            BuildVersion = "1.0.0";
            InternalBuildVersion = 1;
            BuildTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            BuildHash = Guid.NewGuid().ToString();
            BundleInfoDict = new Dictionary<string, QuarkBundleAsset>();
        }
        
        /// <summary>
        /// 获取资源包数量
        /// </summary>
        /// <returns>资源包数量</returns>
        public int GetBundleCount()
        {
            return BundleInfoDict.Count;
        }
        
        /// <summary>
        /// 检查资源包是否存在
        /// </summary>
        /// <param name="bundleKey">资源包键</param>
        /// <returns>是否存在</returns>
        public bool ContainsBundle(string bundleKey)
        {
            return BundleInfoDict.ContainsKey(bundleKey);
        }
        
        /// <summary>
        /// 获取资源包
        /// </summary>
        /// <param name="bundleKey">资源包键</param>
        /// <returns>资源包信息</returns>
        public QuarkBundleAsset GetBundle(string bundleKey)
        {
            if (BundleInfoDict.TryGetValue(bundleKey, out var asset))
            {
                return asset;
            }
            return null;
        }
        
        /// <summary>
        /// 添加资源包
        /// </summary>
        /// <param name="bundleKey">资源包键</param>
        /// <param name="asset">资源包信息</param>
        public void AddBundle(string bundleKey, QuarkBundleAsset asset)
        {
            if (!string.IsNullOrEmpty(bundleKey) && asset != null)
            {
                if (BundleInfoDict.ContainsKey(bundleKey))
                {
                    BundleInfoDict[bundleKey] = asset;
                }
                else
                {
                    BundleInfoDict.Add(bundleKey, asset);
                }
            }
        }
        
        /// <summary>
        /// 移除资源包
        /// </summary>
        /// <param name="bundleKey">资源包键</param>
        /// <returns>是否成功移除</returns>
        public bool RemoveBundle(string bundleKey)
        {
            if (!string.IsNullOrEmpty(bundleKey))
            {
                return BundleInfoDict.Remove(bundleKey);
            }
            return false;
        }
        
        /// <summary>
        /// 合并清单
        /// </summary>
        /// <param name="other">其他清单</param>
        /// <param name="overwrite">是否覆盖</param>
        public void Merge(QuarkManifest other, bool overwrite = true)
        {
            if (other == null)
                return;
                
            foreach (var pair in other.BundleInfoDict)
            {
                if (!BundleInfoDict.ContainsKey(pair.Key))
                {
                    BundleInfoDict.Add(pair.Key, pair.Value);
                }
                else if (overwrite)
                {
                    BundleInfoDict[pair.Key] = pair.Value;
                }
            }
        }
        
        /// <summary>
        /// 转换为JSON字符串
        /// </summary>
        /// <returns>JSON字符串</returns>
        public string ToJson()
        {
            return QuarkUtility.ToJson(this);
        }
    }
}
