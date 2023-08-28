using UnityEditor.IMGUI.Controls;

namespace Quark.Editor
{
    public class QuarkParseDependentTreeViewItem : TreeViewItem
    {
        public int ObjectCount { get; set; }
        public long BundleSize { get; set; }
        public string BundleFormatSize { get; set; }
        public string BundleHash{ get; set; }
        /// <summary>
        /// 是否是节点
        /// </summary>
        public bool IsLabelNode { get; set; }
        public QuarkParseDependentTreeViewItem(int id, int depth, string displayName) : base(id, depth, displayName)
        {
        }
    }
}
