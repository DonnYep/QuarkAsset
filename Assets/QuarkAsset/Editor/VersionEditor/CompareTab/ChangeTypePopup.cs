using UnityEditor;
using UnityEngine;

namespace Quark.Editor
{
    public class ChangeTypePopup : PopupWindowContent
    {
        public System.Action onClose;
        public override Vector2 GetWindowSize()
        {
            return new Vector2(192, 92);
        }
        public override void OnGUI(Rect rect)
        {
            EditorGUILayout.Space(4);
            QuarkManifestCompareTabDataProxy.ShowChanged = EditorGUILayout.ToggleLeft("Changed", QuarkManifestCompareTabDataProxy.ShowChanged);
            QuarkManifestCompareTabDataProxy.ShowNewlyAdded = EditorGUILayout.ToggleLeft("NewlyAdded", QuarkManifestCompareTabDataProxy.ShowNewlyAdded);
            QuarkManifestCompareTabDataProxy.ShowDeleted = EditorGUILayout.ToggleLeft("Deleted", QuarkManifestCompareTabDataProxy.ShowDeleted);
            QuarkManifestCompareTabDataProxy.ShowUnchanged = EditorGUILayout.ToggleLeft("Unchanged", QuarkManifestCompareTabDataProxy.ShowUnchanged);
        }
        public override void OnOpen()
        {

        }
        public override void OnClose()
        {
            onClose?.Invoke();
        }
    }
}
