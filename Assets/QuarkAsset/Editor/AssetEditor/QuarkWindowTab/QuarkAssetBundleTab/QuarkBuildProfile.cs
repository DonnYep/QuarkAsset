using UnityEngine;

namespace Quark.Editor
{
    public class QuarkBuildProfile : ScriptableObject
    {
        public string ProfileDescription;
        public string BuildVersion;
        public int InternalBuildVersion;
        public AssetBundleNameType AssetBundleNameType;
        public BuildType BuildType;
        public bool UseAesEncryptionForManifest;
        public string AesEncryptionKeyForManifest;
        public int EncryptionOffsetForAssetBundle;
        public bool UseOffsetEncryptionForAssetBundle;
    }
}
