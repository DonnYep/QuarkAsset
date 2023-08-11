using UnityEditor;
using UnityEngine;

namespace Quark.Editor
{
    public class ChangeTypePopup : PopupWindowContent
    {
        bool changed = true;
        bool newlyAdded = true;
        bool deleted = true;
        bool unchanged = true;
        Texture2D addedIcon = EditorGUIUtility.FindTexture("CollabCreate Icon");
        Texture2D expiredIcon = EditorGUIUtility.FindTexture("CollabDeleted Icon");
        Texture2D changedIcon = EditorGUIUtility.FindTexture("CollabEdit Icon");
        public override Vector2 GetWindowSize()
        {
            return new Vector2(192, 92);
        }
        public override void OnGUI(Rect rect)
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.BeginHorizontal();
            {
                //绘制icon
                changed = EditorGUILayout.ToggleLeft("Changed", changed);
            }
            EditorGUILayout.EndHorizontal();
            newlyAdded = EditorGUILayout.ToggleLeft("NewlyAdded", newlyAdded);
            deleted = EditorGUILayout.ToggleLeft("Deleted", deleted);
            unchanged = EditorGUILayout.ToggleLeft("Unchanged", unchanged);
        }

        public override void OnOpen()
        {
            Debug.Log("Popup opened: " + this);
        }

        public override void OnClose()
        {
            Debug.Log("Popup closed: " + this);
        }
    }
}
