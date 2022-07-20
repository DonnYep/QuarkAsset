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
        public QuarkAssetObjectInfo GetQuarkAssetObjectInfo()
        {
            var assetBundleName = QuarkObject.AssetBundleName;
            var assetPath = QuarkObject.AssetPath;
            var referenceCount = ReferenceCount;
            var assetName = QuarkObject.AssetName;
            var assetExtension = QuarkObject.AssetExtension;
            var assetType = QuarkObject.AssetType;
            var info = QuarkAssetObjectInfo.Create(assetName, assetPath, assetBundleName, assetExtension, assetType, referenceCount);
            return info;
        }
    }
}
