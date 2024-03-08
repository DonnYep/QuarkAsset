using System;
using System.Collections.Generic;
using UnityEngine;

namespace Quark.Asset
{
    /// <summary>
    /// Editor模式下在dataset中显示信息使用的数据结构
    /// </summary>
    [Serializable]
    public class QuarkBundleInfo : IEquatable<QuarkBundleInfo>, IQuarkBundleInfo
    {
        [SerializeField]
        string bundleName;
        [SerializeField]
        string bundlePath;
        [SerializeField]
        string bundleKey;
        [SerializeField]
        long bundleSize;
        [SerializeField]
        string bundleFormatBytes;
        [SerializeField]
        List<QuarkObjectInfo> objectInfoList;
        [SerializeField]
        List<QuarkBundleDependentInfo> dependentBundleKeyList;
        [SerializeField]
        bool split;
        /// <summary>
        /// 标记对象为独立的ab包
        /// </summary>
        [SerializeField]
        bool extract;
        [SerializeField]
        List<QuarkSubBundleInfo> subBundleInfoList;
        /// <summary>
        /// AB包的名称；
        /// </summary>
        public string BundleName
        {
            get { return bundleName; }
            set
            {
                var srcValue = value;
                if (!string.IsNullOrEmpty(srcValue))
                {
                    srcValue = QuarkUtility.FormatAssetBundleName(srcValue);
                }
                bundleName = srcValue;
            }
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
        public long BundleSize
        {
            get { return bundleSize; }
            set { bundleSize = value; }
        }
        public string BundleFormatBytes
        {
            get { return bundleFormatBytes; }
            set { bundleFormatBytes = value; }
        }
        /// <summary>
        /// 包所含的资源列表；
        /// </summary>
        public List<QuarkObjectInfo> ObjectInfoList
        {
            get
            {
                if (objectInfoList == null)
                    objectInfoList = new List<QuarkObjectInfo>();
                return objectInfoList;
            }
            set { objectInfoList = value; }
        }
        /// <summary>
        /// 资源的依赖项；
        /// </summary>
        public List<QuarkBundleDependentInfo> DependentBundleKeyList
        {
            get
            {
                if (dependentBundleKeyList == null)
                    dependentBundleKeyList = new List<QuarkBundleDependentInfo>();
                return dependentBundleKeyList;
            }
            set { dependentBundleKeyList = value; }
        }
        /// <summary>
        /// 是否分割包体下的子目录作为独立的一个bundle。
        /// <para>此参数与<see cref="Extract"/>互斥</para>
        /// </summary>
        public bool Split
        {
            get { return split; }
            set
            {
                split = value;
                if (split)
                    extract = false;
            }
        }
        /// <summary>
        /// 是否将包体内的所有资源作为单独的bundle。
        /// <para>此参数与<see cref="Split"/>互斥</para>
        /// </summary>
        public bool Extract
        {
            get { return extract; }
            set
            {
                extract = value;
                if (extract)
                    split = false;
            }
        }
        public List<QuarkSubBundleInfo> SubBundleInfoList
        {
            get
            {
                if (subBundleInfoList == null)
                    subBundleInfoList = new List<QuarkSubBundleInfo>();
                return subBundleInfoList;
            }
            set { subBundleInfoList = value; }
        }
        public bool Equals(QuarkBundleInfo other)
        {
            return other.bundleName == this.bundleName || other.bundlePath == bundlePath;
        }
    }
}
