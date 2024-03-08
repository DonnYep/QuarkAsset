using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Quark.Editor
{
    public class QuarkBundleTreeViewItem:TreeViewItem
    {
        string bundleSize;
        public int ObjectCount { get; set; }
        public bool Split{ get; set; }
        public bool Extract{ get; set; }
        public int SplitBundleCount { get; set; }
        public Texture2D SplitIcon { get; set; }
        public Texture2D NotSplitIcon { get; set; }
        public Texture2D ExtractIcon { get; set; }
        public Texture2D NotExtractIcon { get; set; }
        public Texture2D FolerOpenedIcon { get; set; }
        public string BundleSize
        {
            get
            {
                if (string.IsNullOrEmpty(bundleSize))
                    return QuarkEditorConstant.UNKONW;
                return bundleSize;
            }
            set { bundleSize = value; }
        }
        public QuarkBundleTreeViewItem(int id, int depth, string displayName, Texture2D icon) : base(id, depth, displayName)
        {
            this.icon = icon;
        }
    }
}
