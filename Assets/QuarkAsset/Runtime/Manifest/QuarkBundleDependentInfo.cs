using System;
using UnityEngine;
namespace Quark.Asset
{
    [Serializable]
    public class QuarkBundleDependentInfo : IEquatable<QuarkBundleDependentInfo>
    {
        [SerializeField]
        string bundleKey;
        [SerializeField]
        string bundleName;
        public string BundleKey
        {
            get { return bundleKey; }
            set { bundleKey = value; }
        }
        public string BundleName
        {
            get { return bundleName; }
            set { bundleName = value; }
        }
        public bool Equals(QuarkBundleDependentInfo other)
        {
            return other.bundleKey == bundleKey
                && other.bundleName == bundleName;
        }
    }
}
