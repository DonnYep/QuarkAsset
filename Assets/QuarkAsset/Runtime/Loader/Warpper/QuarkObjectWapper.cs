using System;
namespace Quark.Asset
{
    /// <summary>
    /// QuarkAssetObject引用计数类
    /// </summary>
    internal class QuarkObjectWapper : IEquatable<QuarkObjectWapper>
    {
        int referenceCount;
        public QuarkObject QuarkObject;
        /// <summary>
        /// 资源的引用计数；
        /// </summary>
        public int ReferenceCount
        {
            get { return referenceCount; }
            set
            {
                referenceCount = value;
                if (referenceCount < 0)
                    referenceCount = 0;
            }
        }
        public bool Equals(QuarkObjectWapper other)
        {
            return other.QuarkObject == this.QuarkObject &&
                other.ReferenceCount == this.ReferenceCount;
        }
        public QuarkObjectState GetQuarkAssetObjectInfo()
        {
            var assetBundleName = QuarkObject.BundleName;
            var assetPath = QuarkObject.ObjectPath;
            var referenceCount = ReferenceCount;
            var assetName = QuarkObject.ObjectName;
            var assetExtension = QuarkObject.ObjectExtension;
            var assetType = QuarkObject.ObjectType;
            var info = QuarkObjectState.Create(assetName, assetPath, assetBundleName, assetExtension, assetType, referenceCount);
            return info;
        }
    }
}
