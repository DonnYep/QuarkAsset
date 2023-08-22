using UnityEditor.IMGUI.Controls;

namespace Quark.Editor
{
    public class QuarkBundleDetailTreeViewItem : TreeViewItem
    {
        string bundleSize;
        public int ObjectCount { get; set; }
        public string BundleSize
        {
            get
            {
                if (string.IsNullOrEmpty(bundleSize))
                    return "<UNKONW>";
                return bundleSize;
            }
            set { bundleSize = value; }
        }
        public QuarkBundleDetailType BundleDetailType { get; set; }
        public string BundlePath { get; set; }
        public QuarkBundleDetailTreeViewItem(int id, int depth, string displayName) : base(id, depth, displayName)
        {
        }
    }
}
