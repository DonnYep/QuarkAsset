namespace Quark.Manifest
{
    public enum QuarkBundleChangeType : int
    {
        Changed = 1 << 0,
        NewlyAdded = 1 << 1,
        Deleted = 1 << 2,
        Unchanged = 1 << 3,
    }
}
