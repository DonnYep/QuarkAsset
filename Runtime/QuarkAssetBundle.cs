using System;
using System.Collections.Generic;
using UnityEngine;
namespace Quark.Asset
{
    [Serializable]
    public class QuarkAssetBundle : IEquatable<QuarkAssetBundle>
    {
        [SerializeField]
        string assetBundleName;
        [SerializeField]
        string assetBundlePath;
        [SerializeField]
        string assetBundleKey;
        [SerializeField]
        List<string> dependentBundlekKeyList;
        [SerializeField]
        long assetBundleSize;
        [SerializeField]
        List<QuarkObject> quarkObjects;
        /// <summary>
        /// AB包的名称；
        /// </summary>
        public string AssetBundleName
        {
            get { return assetBundleName; }
            set { assetBundleName = value; }
        }
        /// <summary>
        /// AB在Assets目录下的地址；
        /// </summary>
        public string AssetBundlePath
        {
            get { return assetBundlePath; }
            set { assetBundlePath = value; }
        }
        /// <summary>
        /// AB加载时候使用的名称；
        /// </summary>
        public string AssetBundleKey
        {
            get { return assetBundleKey; }
            set { assetBundleKey = value; }
        }
        /// <summary>
        /// 资源的依赖项；
        /// </summary>
        public List<string> DependentBundleKeyList
        {
            get
            {
                if (dependentBundlekKeyList == null)
                    dependentBundlekKeyList = new List<string>();
                return dependentBundlekKeyList;
            }
            set { dependentBundlekKeyList = value; }
        }
        public long AssetBundleSize
        {
            get { return assetBundleSize; }
            set { assetBundleSize = value; }
        }
        /// <summary>
        /// 包所含的资源列表；
        /// </summary>
        public List<QuarkObject> QuarkObjects
        {
            get
            {
                if (quarkObjects == null)
                    quarkObjects = new List<QuarkObject>();
                return quarkObjects;
            }
            set { quarkObjects = value; }
        }
        public bool Equals(QuarkAssetBundle other)
        {
            return other.AssetBundleName == this.AssetBundleName;
        }
    }
}
