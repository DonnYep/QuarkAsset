using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Quark.Editor
{
    public class QuarkObjectTreeViewItem : TreeViewItem
    {
        public string BundleName { get; set; }
        public string AssetExtension { get; set; }
        public string FormatBytes { get; set; }
        public string AssetPath { get; set; }
        public string AssetType { get; set; }
        public Texture2D ObjectTypeIcon { get; set; }

        public QuarkObjectTreeViewItem(int id, int depth, string displayName, Texture2D icon) : base(id, depth, displayName)
        {
            this.icon = icon;
        }
    }
}
