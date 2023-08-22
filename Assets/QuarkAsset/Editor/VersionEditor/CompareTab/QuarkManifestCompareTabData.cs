namespace Quark.Editor
{
    [System.Serializable]
    public class QuarkManifestCompareTabData
    {
        public string SrcManifestPath;
        public string SrcManifestAesKey;

        public string DiffManifestPath;
        public string DiffManifestAesKey;

        public bool Changed = true;
        public bool NewlyAdded = true;
        public bool Deleted = true;
        public bool Unchanged = true;
    }
}
