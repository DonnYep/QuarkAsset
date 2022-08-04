using System;
using UnityEngine;
namespace Quark.Asset
{
    /// <summary>
    /// Quark资源寻址对象；
    /// </summary>
    [Serializable]
    public class QuarkObject : IEquatable<QuarkObject>
    {
        [SerializeField]
        string assetName;
        [SerializeField]
        string assetExtension;
        [SerializeField]
        string assetPath;
        [SerializeField]
        string assetType;
        [SerializeField]
        string assetBundleName;
        /// <summary>
        ///  资源的名称；
        /// </summary>
        public string AssetName
        {
            get { return assetName; }
            set { assetName = value; }
        }
        /// <summary>
        /// 资源的后缀名；
        /// </summary>
        public string AssetExtension
        {
            get { return assetExtension; }
            set { assetExtension = value; }
        }
        /// <summary>
        /// 资源在Assets目录下的相对路径；
        /// </summary>
        public string AssetPath
        {
            get { return assetPath; }
            set { assetPath = value; }
        }
        /// <summary>
        /// 资源在unity中的类型；
        /// </summary>
        public string AssetType
        {
            get { return assetType; }
            set { assetType = value; }
        }
        /// <summary>
        /// 资源所在的AB包的名称；
        /// </summary>
        public string AssetBundleName
        {
            get { return assetBundleName; }
            set { assetBundleName = value; }
        }
        public bool Equals(QuarkObject other)
        {
            return other.AssetName == this.AssetName &&
                other.AssetPath == this.AssetPath &&
                other.AssetBundleName == this.AssetBundleName &&
                other.AssetExtension == this.AssetExtension;
        }
    }
}