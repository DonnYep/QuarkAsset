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
        SerializedProperty sp_LoadMode;
        SerializedProperty sp_QuarkAssetDataset;
        SerializedProperty sp_Url;
        SerializedProperty sp_QuarkBuildPath;
        SerializedProperty sp_EnableStreamingRelativeBundlePath;
        SerializedProperty sp_StreamingRelativeBundlePath;

        SerializedProperty sp_EnablePersistentRelativeBundlePath;
        SerializedProperty sp_PersistentRelativeBundlePath;

        SerializedProperty sp_EnableDownloadRelativePath;
        SerializedProperty sp_DownloadRelativePath;

        SerializedProperty sp_CustomeAbsolutePath;
        SerializedProperty sp_DownloadedPath;
        SerializedProperty sp_EncryptionOffset;
        SerializedProperty sp_ManifestAesEncryptKey;
        public override void OnInspectorGUI()
        {
            targetObject.Update();
            sp_AutoStart.boolValue = EditorGUILayout.ToggleLeft("AutoStart", sp_AutoStart.boolValue);
            sp_LoadMode.enumValueIndex = (byte)(QuarkLoadMode)EditorGUILayout.EnumPopup("LoadMode", (QuarkLoadMode)sp_LoadMode.enumValueIndex);
            switch ((QuarkLoadMode)sp_LoadMode.enumValueIndex)
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
            sp_LoadMode = targetObject.FindProperty("loadMode");
            sp_QuarkAssetDataset = targetObject.FindProperty("quarkAssetDataset");
            sp_Url = targetObject.FindProperty("url");
            sp_DownloadedPath = targetObject.FindProperty("downloadedPath");
            sp_EncryptionOffset = targetObject.FindProperty("encryptionOffset");

            sp_EnableStreamingRelativeBundlePath = targetObject.FindProperty("enableStreamingRelativeBuildPath");
            sp_StreamingRelativeBundlePath = targetObject.FindProperty("streamingRelativeBuildPath");

            sp_EnablePersistentRelativeBundlePath = targetObject.FindProperty("enablePersistentRelativeBundlePath");
            sp_PersistentRelativeBundlePath = targetObject.FindProperty("persistentRelativeBundlePath");

            sp_EnableDownloadRelativePath=targetObject.FindProperty("enableDownloadRelativePath");
            sp_DownloadRelativePath=targetObject.FindProperty("downloadRelativePath");

            sp_CustomeAbsolutePath = targetObject.FindProperty("customeAbsolutePath");
            sp_ManifestAesEncryptKey = targetObject.FindProperty("manifestAesEncryptKey");
            sp_QuarkBuildPath = targetObject.FindProperty("quarkBuildPath");
        }
        void DrawBuildAssetBundleTab()
        {
            EditorGUILayout.HelpBox("Asset bundle build path", MessageType.Info);

            EditorGUILayout.BeginVertical();
            sp_QuarkBuildPath.enumValueIndex = (byte)(QuarkBuildPath)EditorGUILayout.EnumPopup("BundlePath", (QuarkBuildPath)sp_QuarkBuildPath.enumValueIndex);
            var buildType = (QuarkBuildPath)sp_QuarkBuildPath.enumValueIndex;
            switch (buildType)
            {
                case QuarkBuildPath.StreamingAssets:
                    {
                        sp_EnableStreamingRelativeBundlePath.boolValue = EditorGUILayout.ToggleLeft("EnableRelativeBundlePath", sp_EnableStreamingRelativeBundlePath.boolValue);
                        var useRelativePath = sp_EnableStreamingRelativeBundlePath.boolValue;
                        if (useRelativePath)
                        {
                            sp_StreamingRelativeBundlePath.stringValue = EditorGUILayout.TextField("RelativeBundlePath", sp_StreamingRelativeBundlePath.stringValue.Trim());
                        }
                    }
                    break;
                case QuarkBuildPath.PersistentDataPath:
                    {
                        sp_EnablePersistentRelativeBundlePath.boolValue = EditorGUILayout.ToggleLeft("EnableRelativeBundlePath", sp_EnablePersistentRelativeBundlePath.boolValue);
                        var useRelativePath = sp_EnablePersistentRelativeBundlePath.boolValue;
                        if (useRelativePath)
                        {
                            sp_PersistentRelativeBundlePath.stringValue = EditorGUILayout.TextField("RelativeBundlePath", sp_PersistentRelativeBundlePath.stringValue.Trim());
                        }
                    }
                    break;
                case QuarkBuildPath.URL:
                    {
                        sp_Url.stringValue = EditorGUILayout.TextField("Url", sp_Url.stringValue.Trim());
                        EditorGUILayout.BeginVertical();
                        sp_DownloadedPath.enumValueIndex = (byte)(QuarkDownloadedPath)EditorGUILayout.EnumPopup("DownloadPath", (QuarkDownloadedPath)sp_DownloadedPath.enumValueIndex);
                        var pathType = (QuarkDownloadedPath)sp_DownloadedPath.enumValueIndex;
                        if (pathType != QuarkDownloadedPath.CustomePath)
                        {
                            sp_EnableDownloadRelativePath.boolValue = EditorGUILayout.ToggleLeft("EnableDownloadRelativePath", sp_EnableDownloadRelativePath.boolValue);
                            var useRelativePath = sp_EnableDownloadRelativePath.boolValue;
                            if (useRelativePath)
                            {
                                sp_DownloadRelativePath.stringValue = EditorGUILayout.TextField("DownloadRelativePath", sp_DownloadRelativePath.stringValue.Trim());
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
            sp_EncryptionOffset.longValue = EditorGUILayout.LongField("EncryptOffset", sp_EncryptionOffset.longValue);
            var offsetVar = sp_EncryptionOffset.longValue;
            if (offsetVar < 0)
                sp_EncryptionOffset.longValue = 0;
        }
        void DrawAESEncryption()
        {
            EditorGUILayout.Space(8);
            sp_ManifestAesEncryptKey.stringValue = EditorGUILayout.TextField("ManifestAesKey", sp_ManifestAesEncryptKey.stringValue);
            var keyStr = sp_ManifestAesEncryptKey.stringValue;
            var keyLength = System.Text.Encoding.UTF8.GetBytes(keyStr).Length;
            EditorGUILayout.LabelField($"Current key length is:{keyLength }");
            if (keyLength != 16 && keyLength != 24 && keyLength != 32 && keyLength != 0)
            {
                EditorGUILayout.HelpBox("Key should be 16,24 or 32 bytes long", MessageType.Error);
            }
        }
    }
}