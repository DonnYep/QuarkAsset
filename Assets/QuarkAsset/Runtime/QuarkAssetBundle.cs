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
        List<string> dependList;
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
        /// 资源的依赖项；
        /// </summary>
        public List<string> DependList
        {
            get
            {
                if (dependList == null)
                    dependList = new List<string>();
                return dependList;
            }
            set { dependList = value; }
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
