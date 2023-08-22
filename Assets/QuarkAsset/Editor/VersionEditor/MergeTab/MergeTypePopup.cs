using System;
using UnityEditor;
using UnityEngine;

namespace Quark.Editor
{
    public class MergeTypePopup : PopupWindowContent
    {
        public Action onClose;
        public override Vector2 GetWindowSize()
        {
            return new Vector2(192, 64);
        }
        public override void OnGUI(Rect rect)
        {
            EditorGUILayout.Space(4);
            QuarkManifestMergeLabelDataProxy.ShowBuiltIn = EditorGUILayout.ToggleLeft("Built-In", QuarkManifestMergeLabelDataProxy.ShowBuiltIn);
            QuarkManifestMergeLabelDataProxy.ShowIncremental = EditorGUILayout.ToggleLeft("Incremental", QuarkManifestMergeLabelDataProxy.ShowIncremental);
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
