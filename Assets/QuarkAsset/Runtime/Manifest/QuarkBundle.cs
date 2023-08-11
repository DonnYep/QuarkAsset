using System;
using System.Collections.Generic;
using UnityEngine;
namespace Quark.Asset
{
    /// <summary>
    /// Assetbundle 模式下，manifest中包含的包体寻址对象；
    /// </summary>
    [Serializable]
    public class QuarkBundle : IEquatable<QuarkBundle>
    {
        [SerializeField]
        string bundleName;
        [SerializeField]
        string bundlePath;
        [SerializeField]
        string bundleKey;
        [SerializeField]
        List<QuarkBundleDependentInfo> dependentBundlekKeyList;
        [SerializeField]
        List<QuarkObject> objectList;
        /// <summary>
        /// AB包的名称；
        /// </summary>
        public string BundleName
        {
            get { return bundleName; }
            set { bundleName = value; }
        }
        /// <summary>
        /// AB在Assets目录下的地址；
        /// </summary>
        public string BundlePath
        {
            get { return bundlePath; }
            set { bundlePath = value; }
        }
        /// <summary>
        /// AB加载时候使用的名称；
        /// </summary>
        public string BundleKey
        {
            get { return bundleKey; }
            set { bundleKey = value; }
        }
        /// <summary>
        /// 资源的依赖项；
        /// </summary>
        public List<QuarkBundleDependentInfo> DependentBundleKeyList
        {
            get
            {
                if (dependentBundlekKeyList == null)
                    dependentBundlekKeyList = new List<QuarkBundleDependentInfo>();
                return dependentBundlekKeyList;
            }
            set { dependentBundlekKeyList = value; }
        }
        /// <summary>
        /// 包所含的资源列表；
        /// </summary>
        public List<QuarkObject> ObjectList
        {
            get
            {
                if (objectList == null)
                    objectList = new List<QuarkObject>();
                return objectList;
            }
            set { objectList = value; }
        }
        public bool Equals(QuarkBundle other)
        {
            return other.BundleName == this.BundleName;
        }
    }
}
