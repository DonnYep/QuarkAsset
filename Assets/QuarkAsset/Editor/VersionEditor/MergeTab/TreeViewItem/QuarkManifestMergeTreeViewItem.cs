using UnityEditor.IMGUI.Controls;
namespace Quark.Editor
{
    public class QuarkManifestMergeTreeViewItem : TreeViewItem
    {
        public int ObjectCount { get; set; }
        public long BundleSize { get; set; }
        public string BundleKey{ get; set; }
        public string BundleHash{ get; set; }
        public string BundleFormatSize { get; set; }
        public bool IsIncremental { get; set; }
        public QuarkManifestMergeTreeViewItem(int id, int depth, string displayName) : base(id, depth, displayName)
        {
        }
    }
}
