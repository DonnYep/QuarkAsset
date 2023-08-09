using System;

namespace Quark.Editor
{
    [Serializable]
    public class QuarkManifestDecryptTabData
    {
        public string ManifestPath;
        public string ManifestAesKey;

        public string DecryptedManifestOutputPath;
        public bool OpenDecryptPathWhenCompareDone;
    }
}
