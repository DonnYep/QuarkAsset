using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Quark.Editor
{
    public class QuarkBundleTreeViewItem:TreeViewItem
    {
        string bundleSize;
        public int ObjectCount { get; set; }
        public bool Splittable{ get; set; }
        public int SplittedBundleCount { get; set; }
        public Texture2D SplittableIcon { get; set; }
        public Texture2D UnsplittableIcon { get; set; }
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
        public QuarkBundleTreeViewItem(int id, int depth, string displayName, Texture2D icon) : base(id, depth, displayName)
        {
            this.icon = icon;
        }
    }
}
