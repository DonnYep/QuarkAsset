using UnityEditor.IMGUI.Controls;

namespace Quark.Editor
{
    public class QuarkParseBundleTreeViewItem : TreeViewItem
    {
        public int ObjectCount { get; set; }
        public long BundleSize { get; set; }
        public string BundleKey { get; set; }
        public string BundleHash { get; set; }
        public string BundleFormatSize { get; set; }
        public string BundlePath{ get; set; }
        public int BundleDependentCount{ get; set; }
        public QuarkParseBundleTreeViewItem(int id, int depth, string displayName) : base(id, depth, displayName)
        {
        }
    }
}
