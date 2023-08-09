using UnityEditor;

namespace Quark.Editor
{
    public class QuarkCommand
    {
        [MenuItem("Window/QuarkAsset/Command/ForceRemoveAllAssetBundleNames")]
        public static void ForceRemoveAllAssetBundleNames()
        {
            var run = EditorUtility.DisplayDialog("AssetBundleCommand", "This operation will force remove all assetBundle names , whether to continue ?", "Ok", "Cancel");
            if (run)
            {
                var allBundleNames = AssetDatabase.GetAllAssetBundleNames();
                foreach (var bundleName in allBundleNames)
                {
                    AssetDatabase.RemoveAssetBundleName(bundleName, true);
                }
                QuarkUtility.LogInfo("Force remove all assetBundle names done");
            }
        }
    }
}
