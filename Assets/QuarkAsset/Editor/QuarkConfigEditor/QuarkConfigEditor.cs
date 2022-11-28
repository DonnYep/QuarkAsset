using UnityEditor;
using Quark.Asset;
namespace Quark.Editor
{
    [CustomEditor(typeof(QuarkConfig), true)]
    public class QuarkConfigEditor : UnityEditor.Editor
    {
        SerializedObject targetObject;
        QuarkConfig quarkConfig;
        bool encryptionToggle;
        SerializedProperty sp_AutoStart;
        SerializedProperty sp_QuarkAssetLoadMode;
        SerializedProperty sp_QuarkAssetDataset;
        SerializedProperty sp_Url;
        SerializedProperty sp_PingUrl;
        SerializedProperty sp_QuarkBuildPath;
        SerializedProperty sp_EnableStreamingRelativeLoadPath;
        SerializedProperty sp_StreamingRelativeLoadPath;

        SerializedProperty sp_CustomeAbsolutePath;
        SerializedProperty sp_QuarkDownloadedPath;
        SerializedProperty sp_EncryptionOffset;
        SerializedProperty sp_BuildInfoAESEncryptionKey;
        public override void OnInspectorGUI()
        {
            targetObject.Update();
            sp_AutoStart.boolValue = EditorGUILayout.ToggleLeft("AutoStart", sp_AutoStart.boolValue);
            sp_QuarkAssetLoadMode.enumValueIndex = (byte)(QuarkLoadMode)EditorGUILayout.EnumPopup("QuarkAssetLoadMode", (QuarkLoadMode)sp_QuarkAssetLoadMode.enumValueIndex);
            switch ((QuarkLoadMode)sp_QuarkAssetLoadMode.enumValueIndex)
            {
                case QuarkLoadMode.AssetDatabase:
                    {
                        sp_QuarkAssetDataset.objectReferenceValue = EditorGUILayout.ObjectField("QuarkAssetDataset", (QuarkAssetDataset)sp_QuarkAssetDataset.objectReferenceValue, typeof(QuarkAssetDataset), false);
                    }
                    break;
                case QuarkLoadMode.AssetBundle:
                    {
                        DrawBuildAssetBundleTab();
                        EditorGUILayout.Space(8);
                        encryptionToggle = EditorGUILayout.Foldout(encryptionToggle, "Encryption");
                        if (encryptionToggle)
                        {
                            DrawOffstEncryption();
                            DrawAESEncryption();
                        }
                    }
                    break;
            }
            targetObject.ApplyModifiedProperties();
        }
        private void OnEnable()
        {
            quarkConfig = target as QuarkConfig;
            targetObject = new SerializedObject(quarkConfig);
            sp_AutoStart = targetObject.FindProperty("autoStart");
            sp_QuarkAssetLoadMode = targetObject.FindProperty("quarkAssetLoadMode");
            sp_QuarkAssetDataset = targetObject.FindProperty("quarkAssetDataset");
            sp_Url = targetObject.FindProperty("url");
            sp_PingUrl = targetObject.FindProperty("pingUrl");
            sp_QuarkDownloadedPath = targetObject.FindProperty("quarkDownloadedPath");
            sp_EncryptionOffset = targetObject.FindProperty("encryptionOffset");

            sp_EnableStreamingRelativeLoadPath = targetObject.FindProperty("enableStreamingRelativeBuildPath");
            sp_StreamingRelativeLoadPath = targetObject.FindProperty("streamingRelativeBuildPath");

            sp_CustomeAbsolutePath = targetObject.FindProperty("customeAbsolutePath");
            sp_BuildInfoAESEncryptionKey = targetObject.FindProperty("buildInfoAESEncryptionKey");
            sp_QuarkBuildPath = targetObject.FindProperty("quarkBuildPath");
        }
        void DrawBuildAssetBundleTab()
        {
            EditorGUILayout.HelpBox("Asset bundle build path", MessageType.Info);

            EditorGUILayout.BeginVertical();
            sp_QuarkBuildPath.enumValueIndex = (byte)(QuarkBuildPath)EditorGUILayout.EnumPopup("QuarkBuildPath", (QuarkBuildPath)sp_QuarkBuildPath.enumValueIndex);
            var buildType = (QuarkBuildPath)sp_QuarkBuildPath.enumValueIndex;
            switch (buildType)
            {
                case QuarkBuildPath.StreamingAssets:
                    {
                        sp_EnableStreamingRelativeLoadPath.boolValue = EditorGUILayout.Toggle("EnableStreamingRelativeBuildPath", sp_EnableStreamingRelativeLoadPath.boolValue);
                        var useRelativePath = sp_EnableStreamingRelativeLoadPath.boolValue;
                        if (useRelativePath)
                        {
                            sp_StreamingRelativeLoadPath.stringValue = EditorGUILayout.TextField("StreamingRelativeBuildPath", sp_StreamingRelativeLoadPath.stringValue.Trim());
                        }
                    }
                    break;
                case QuarkBuildPath.Remote:
                    {
                        sp_PingUrl.boolValue = EditorGUILayout.Toggle("PingUrl", sp_PingUrl.boolValue);
                        sp_Url.stringValue = EditorGUILayout.TextField("Url", sp_Url.stringValue.Trim());
                        EditorGUILayout.BeginVertical();
                        sp_QuarkDownloadedPath.enumValueIndex = (byte)(QuarkDownloadedPath)EditorGUILayout.EnumPopup("QuarkDownloadPath", (QuarkDownloadedPath)sp_QuarkDownloadedPath.enumValueIndex);
                        var pathType = (QuarkDownloadedPath)sp_QuarkDownloadedPath.enumValueIndex;
                        if (pathType != QuarkDownloadedPath.Custome)
                        {
                            sp_EnableStreamingRelativeLoadPath.boolValue = EditorGUILayout.Toggle("UseStreamingRelativeLoadPath", sp_EnableStreamingRelativeLoadPath.boolValue);
                            var useRelativePath = sp_EnableStreamingRelativeLoadPath.boolValue;
                            if (useRelativePath)
                            {
                                sp_StreamingRelativeLoadPath.stringValue = EditorGUILayout.TextField("RelativeLoadPath", sp_StreamingRelativeLoadPath.stringValue.Trim());
                            }
                        }
                        else
                        {
                            sp_CustomeAbsolutePath.stringValue = EditorGUILayout.TextField("CustomeAbsolutePath", sp_CustomeAbsolutePath.stringValue.Trim());
                        }
                        EditorGUILayout.EndVertical();

                    }
                    break;
            }
            EditorGUILayout.EndVertical();
        }
        void DrawOffstEncryption()
        {
            sp_EncryptionOffset.longValue = EditorGUILayout.LongField("QuarkEncryptOffset", sp_EncryptionOffset.longValue);
            var offsetVar = sp_EncryptionOffset.longValue;
            if (offsetVar < 0)
                sp_EncryptionOffset.longValue = 0;
        }
        void DrawAESEncryption()
        {
            EditorGUILayout.Space(8);
            sp_BuildInfoAESEncryptionKey.stringValue = EditorGUILayout.TextField("QuarkAesKey", sp_BuildInfoAESEncryptionKey.stringValue);
            var keyStr = sp_BuildInfoAESEncryptionKey.stringValue;
            var keyLength = System.Text.Encoding.UTF8.GetBytes(keyStr).Length;
            EditorGUILayout.LabelField($"Current key length is:{keyLength }");
            if (keyLength != 16 && keyLength != 24 && keyLength != 32 && keyLength != 0)
            {
                EditorGUILayout.HelpBox("Key should be 16,24 or 32 bytes long", MessageType.Error);
            }
        }
    }
}