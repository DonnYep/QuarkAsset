using System;
using UnityEngine;

namespace Quark.Asset
{
    [Serializable]
    public class QuarkObjectInfo : IEquatable<QuarkObjectInfo>
    {
        [SerializeField]
        string objectName;
        [SerializeField]
        string objectExtension;
        [SerializeField]
        string objectPath;
        [SerializeField]
        string objectType;
        [SerializeField]
        string bundleName;
        [SerializeField]
        long objectSize;
        [SerializeField]
        string objectFormatBytes;
        Texture2D objectIcon;
        /// <summary>
        ///  资源的名称；
        /// </summary>
        public string ObjectName
        {
            get { return objectName; }
            set { objectName = value; }
        }
        /// <summary>
        /// 资源的后缀名；
        /// </summary>
        public string ObjectExtension
        {
            get { return objectExtension; }
            set { objectExtension = value; }
        }
        /// <summary>
        /// 资源在Assets目录下的相对路径；
        /// </summary>
        public string ObjectPath
        {
            get { return objectPath; }
            set { objectPath = value; }
        }
        /// <summary>
        /// 资源在unity中的类型；
        /// </summary>
        public string ObjectType
        {
            get { return objectType; }
            set { objectType = value; }
        }
        /// <summary>
        /// 资源所在的AB包的名称；
        /// </summary>
        public string BundleName
        {
            get { return bundleName; }
            set { bundleName = value; }
        }
        public long ObjectSize
        {
            get { return objectSize; }
            set { objectSize = value; }
        }
        public string ObjectFormatBytes
        {
            get { return objectFormatBytes; }
            set { objectFormatBytes = value; }
        }
        public Texture2D ObjectIcon
        {
            get { return objectIcon; }
            set { objectIcon = value; }
        }
        public bool Equals(QuarkObjectInfo other)
        {
            return other.ObjectName == this.ObjectName &&
                other.ObjectPath == this.ObjectPath &&
                other.BundleName == this.BundleName &&
                other.ObjectExtension == this.ObjectExtension;
        }
    }
}
