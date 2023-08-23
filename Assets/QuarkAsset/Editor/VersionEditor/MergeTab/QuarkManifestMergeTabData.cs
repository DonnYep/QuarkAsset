namespace Quark.Editor
{
    [System.Serializable]
    public class QuarkManifestMergeTabData
    {
        public string SrcManifestPath;
        public string SrcManifestAesKey;

        public string DiffManifestPath;
        public string DiffManifestAesKey;

        public bool ShowIncremental = true;
        public bool ShowBuiltIn = true;

        public bool ShowMergedManifest;
    }
}
