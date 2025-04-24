using System;
using System.Collections.Generic;

namespace Quark.Asset
{
    /// <summary>
    /// Quark资源包
    /// </summary>
    [Serializable]
    public class QuarkBundle
    {
        /// <summary>
        /// 资源包键名
        /// </summary>
        public string BundleKey;
        
        /// <summary>
        /// 资源包名称
        /// </summary>
        public string BundleName;
        
        /// <summary>
        /// 资源包路径
        /// </summary>
        public string BundlePath;
        
        /// <summary>
        /// 对象列表
        /// </summary>
        public List<QuarkObject> ObjectList = new List<QuarkObject>();
        
        /// <summary>
        /// 依赖的资源包键名列表
        /// </summary>
        public List<QuarkBundleDependentInfo> DependentBundleKeyList = new List<QuarkBundleDependentInfo>();
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public QuarkBundle()
        {
            ObjectList = new List<QuarkObject>();
            DependentBundleKeyList = new List<QuarkBundleDependentInfo>();
        }
        
        /// <summary>
        /// 对象数量
        /// </summary>
        public int ObjectCount => ObjectList.Count;
        
        /// <summary>
        /// 依赖数量
        /// </summary>
        public int DependentCount => DependentBundleKeyList.Count;
        
        /// <summary>
        /// 添加对象
        /// </summary>
        /// <param name="obj">对象</param>
        public void AddObject(QuarkObject obj)
        {
            if (obj != null && !ObjectList.Contains(obj))
            {
                ObjectList.Add(obj);
            }
        }
        
        /// <summary>
        /// 移除对象
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns>是否成功</returns>
        public bool RemoveObject(QuarkObject obj)
        {
            if (obj != null)
            {
                return ObjectList.Remove(obj);
            }
            return false;
        }
        
        /// <summary>
        /// 添加依赖
        /// </summary>
        /// <param name="dependentInfo">依赖信息</param>
        public void AddDependent(QuarkBundleDependentInfo dependentInfo)
        {
            if (dependentInfo != null && !DependentBundleKeyList.Contains(dependentInfo))
            {
                DependentBundleKeyList.Add(dependentInfo);
            }
        }
        
        /// <summary>
        /// 移除依赖
        /// </summary>
        /// <param name="dependentInfo">依赖信息</param>
        /// <returns>是否成功</returns>
        public bool RemoveDependent(QuarkBundleDependentInfo dependentInfo)
        {
            if (dependentInfo != null)
            {
                return DependentBundleKeyList.Remove(dependentInfo);
            }
            return false;
        }
        
        /// <summary>
        /// 清空对象
        /// </summary>
        public void ClearObjects()
        {
            ObjectList.Clear();
        }
        
        /// <summary>
        /// 清空依赖
        /// </summary>
        public void ClearDependents()
        {
            DependentBundleKeyList.Clear();
        }
    }
    
    /// <summary>
    /// Quark对象
    /// </summary>
    [Serializable]
    public class QuarkObject
    {
        /// <summary>
        /// 对象名称
        /// </summary>
        public string ObjectName;
        
        /// <summary>
        /// 对象路径
        /// </summary>
        public string ObjectPath;
        
        /// <summary>
        /// 所属资源包名称
        /// </summary>
        public string BundleName;
        
        /// <summary>
        /// 对象类型
        /// </summary>
        public string ObjectType;
        
        /// <summary>
        /// 对象扩展名
        /// </summary>
        public string ObjectExtension;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public QuarkObject()
        {
        }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">对象名称</param>
        /// <param name="path">对象路径</param>
        /// <param name="bundleName">所属资源包名称</param>
        /// <param name="type">对象类型</param>
        /// <param name="extension">对象扩展名</param>
        public QuarkObject(string name, string path, string bundleName, string type, string extension)
        {
            ObjectName = name;
            ObjectPath = path;
            BundleName = bundleName;
            ObjectType = type;
            ObjectExtension = extension;
        }
    }
    
    /// <summary>
    /// 资源包依赖信息
    /// </summary>
    [Serializable]
    public class QuarkBundleDependentInfo
    {
        /// <summary>
        /// 资源包键名
        /// </summary>
        public string BundleKey;
        
        /// <summary>
        /// 资源包名称
        /// </summary>
        public string BundleName;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public QuarkBundleDependentInfo()
        {
        }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="bundleKey">资源包键名</param>
        /// <param name="bundleName">资源包名称</param>
        public QuarkBundleDependentInfo(string bundleKey, string bundleName)
        {
            BundleKey = bundleKey;
            BundleName = bundleName;
        }
        
        /// <summary>
        /// 相等比较
        /// </summary>
        /// <param name="obj">其他对象</param>
        /// <returns>是否相等</returns>
        public override bool Equals(object obj)
        {
            if (obj is QuarkBundleDependentInfo other)
            {
                return BundleKey == other.BundleKey && BundleName == other.BundleName;
            }
            return false;
        }
        
        /// <summary>
        /// 获取哈希码
        /// </summary>
        /// <returns>哈希码</returns>
        public override int GetHashCode()
        {
            return (BundleKey?.GetHashCode() ?? 0) ^ (BundleName?.GetHashCode() ?? 0);
        }
    }
}
