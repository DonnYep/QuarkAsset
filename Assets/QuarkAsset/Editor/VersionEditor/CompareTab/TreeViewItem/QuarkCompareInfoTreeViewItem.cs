using Quark.Manifest;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Quark.Editor
{
    public class QuarkCompareInfoTreeViewItem : TreeViewItem
    {
        public string BundleName { get; set; }
        public string BundleKey { get; set; }
        public string BundleHash { get; set; }
        public long BundleSize { get; set; }
        public string BundleFormatSize { get; set; }
        public QuarkBundleChangeType BundleChangeType { get; set; }
        public QuarkCompareInfoTreeViewItem(int id, int depth, string displayName, Texture2D icon) : base(id, depth, displayName)
        {
            this.icon= icon;
        }
    }
}
