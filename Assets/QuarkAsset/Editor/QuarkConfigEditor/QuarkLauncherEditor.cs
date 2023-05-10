using UnityEditor;
using Quark.Asset;
namespace Quark.Editor
{
    [CustomEditor(typeof(QuarkLauncher), false)]
    public class QuarkLauncherEditor : UnityEditor.Editor
    {
        SerializedObject targetObject;
        QuarkLauncher quarkConfig;
        bool encryptionToggle;
        SerializedProperty sp_AutoStartBasedOnConfig;
        SerializedProperty sp_LoadMode;
        SerializedProperty sp_QuarkDataset;
        SerializedProperty sp_QuarkBuildPath;
        SerializedProperty sp_EnableStreamingRelativeBundlePath;
        SerializedProperty sp_StreamingRelativeBundlePath;

        SerializedProperty sp_EnablePersistentRelativeBundlePath;
        SerializedProperty sp_PersistentRelativeBundlePath;

        SerializedProperty sp_EncryptionOffset;
        SerializedProperty sp_ManifestAesEncryptKey;
        public override void OnInspectorGUI()
        {
            targetObject.Update();
            sp_AutoStartBasedOnConfig.boolValue = EditorGUILayout.ToggleLeft("Auto-start based on launcher configuration", sp_AutoStartBasedOnConfig.boolValue);
            if (sp_AutoStartBasedOnConfig.boolValue)
            {
                EditorGUILayout.Space(8);
                sp_LoadMode.enumValueIndex = (byte)(QuarkLoadMode)EditorGUILayout.EnumPopup("Load mode", (QuarkLoadMode)sp_LoadMode.enumValueIndex);
                switch ((QuarkLoadMode)sp_LoadMode.enumValueIndex)
                {
                    case QuarkLoadMode.AssetDatabase:
                        {
                            sp_QuarkDataset.objectReferenceValue = EditorGUILayout.ObjectField("QuarkDataset", (QuarkDataset)sp_QuarkDataset.objectReferenceValue, typeof(QuarkDataset), false);
                        }
                        break;
                    case QuarkLoadMode.AssetBundle:
                        {
                            DrawBuildAssetBundleTab();
                            EditorGUILayout.Space(8);
                            EditorGUILayout.LabelField("Encryption",EditorStyles.boldLabel);
                            encryptionToggle = EditorGUILayout.Foldout(encryptionToggle,"Params");
                            if (encryptionToggle)
                            {
                                DrawOffstEncryption();
                                DrawAESEncryption();
                            }
                        }
                        break;
                }
            }
            targetObject.ApplyModifiedProperties();
        }
        private void OnEnable()
        {
            quarkConfig = target as QuarkLauncher;
            targetObject = new SerializedObject(quarkConfig);
            sp_AutoStartBasedOnConfig = targetObject.FindProperty("autoStartBasedOnConfig");
            sp_LoadMode = targetObject.FindProperty("loadMode");
            sp_QuarkDataset = targetObject.FindProperty("quarkDataset");
            sp_EncryptionOffset = targetObject.FindProperty("encryptionOffset");

            sp_EnableStreamingRelativeBundlePath = targetObject.FindProperty("enableStreamingRelativeBuildPath");
            sp_StreamingRelativeBundlePath = targetObject.FindProperty("streamingRelativeBuildPath");

            sp_EnablePersistentRelativeBundlePath = targetObject.FindProperty("enablePersistentRelativeBundlePath");
            sp_PersistentRelativeBundlePath = targetObject.FindProperty("persistentRelativeBundlePath");

            sp_ManifestAesEncryptKey = targetObject.FindProperty("manifestAesKey");
            sp_QuarkBuildPath = targetObject.FindProperty("quarkBuildPath");
        }
        void DrawBuildAssetBundleTab()
        {
            EditorGUILayout.Space(8);

            EditorGUILayout.LabelField("AssetBundle path",EditorStyles.boldLabel);

            sp_QuarkBuildPath.enumValueIndex = (byte)(QuarkBuildPath)EditorGUILayout.EnumPopup("Path type", (QuarkBuildPath)sp_QuarkBuildPath.enumValueIndex);
            var buildType = (QuarkBuildPath)sp_QuarkBuildPath.enumValueIndex;
            switch (buildType)
            {
                case QuarkBuildPath.StreamingAssets:
                    {
                        sp_EnableStreamingRelativeBundlePath.boolValue = EditorGUILayout.ToggleLeft("Enable streamingAsset relative path <Nullable>", sp_EnableStreamingRelativeBundlePath.boolValue);
                        var useRelativePath = sp_EnableStreamingRelativeBundlePath.boolValue;
                        if (useRelativePath)
                        {
                            EditorGUILayout.LabelField("StreamingAsset relative path");
                            sp_StreamingRelativeBundlePath.stringValue = EditorGUILayout.TextField(sp_StreamingRelativeBundlePath.stringValue.Trim());
                        }
                    }
                    break;
                case QuarkBuildPath.PersistentDataPath:
                    {
                        sp_EnablePersistentRelativeBundlePath.boolValue = EditorGUILayout.ToggleLeft("Enable persistentDataPath relative path <Nullable>", sp_EnablePersistentRelativeBundlePath.boolValue);
                        var useRelativePath = sp_EnablePersistentRelativeBundlePath.boolValue;
                        if (useRelativePath)
                        {
                            EditorGUILayout.LabelField("PersistentDataPath relative path");
                            sp_PersistentRelativeBundlePath.stringValue = EditorGUILayout.TextField(sp_PersistentRelativeBundlePath.stringValue.Trim());
                        }
                    }
                    break;
            }
        }
        void DrawOffstEncryption()
        {
            sp_EncryptionOffset.longValue = EditorGUILayout.LongField("Encryption offset", sp_EncryptionOffset.longValue);
            var offsetVar = sp_EncryptionOffset.longValue;
            if (offsetVar < 0)
                sp_EncryptionOffset.longValue = 0;
        }
        void DrawAESEncryption()
        {
            EditorGUILayout.Space(8);
            sp_ManifestAesEncryptKey.stringValue = EditorGUILayout.TextField("Manifest aes key <Nullable>", sp_ManifestAesEncryptKey.stringValue);
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