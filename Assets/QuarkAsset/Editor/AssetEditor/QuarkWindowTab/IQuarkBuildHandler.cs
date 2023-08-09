using UnityEditor;
namespace Quark.Editor
{
    public interface IQuarkBuildHandler
    {
        void OnBuildStart(BuildTarget buildTarget, string assetBundleBuildPath);
        void OnBuildComplete(BuildTarget buildTarget, string assetBundleBuildPath);
    }
}
