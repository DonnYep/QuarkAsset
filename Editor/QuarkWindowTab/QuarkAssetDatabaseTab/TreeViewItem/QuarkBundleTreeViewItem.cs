using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Quark.Editor
{
    public class QuarkBundleTreeViewItem:TreeViewItem
    {
        public int ObjectCount { get; set; }
        public QuarkBundleTreeViewItem(int id, int depth, string displayName, Texture2D icon) : base(id, depth, displayName)
        {
            this.icon = icon;
        }
    }
}
