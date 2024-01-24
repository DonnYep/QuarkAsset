using System.IO;
using UnityEditor;
using UnityEngine;

namespace Quark.Editor
{
    public class QuarkAssetBundleTabProfileLabel
    {
        public const string LabelDataName = "QuarkAsset_AssetBundleTabProfileLabelData.json";
        QuarkBuildProfile buildProfile;
        QuarkAssetBundleTab parent;
        Texture2D createAddNewIcon;
        Texture2D saveActiveIcon;
        public bool HasProfile
        {
            get { return buildProfile != null; }
        }
        string[] buildHandlers;

        bool isAesKeyInvalid = false;
        public bool IsAesKeyInvalid
        {
            get { return isAesKeyInvalid; }
        }
        public void OnEnable(QuarkAssetBundleTab parent, string[] buildHandlers)
        {
            this.buildHandlers = buildHandlers;
            this.parent = parent;
            GetLabelData();
            createAddNewIcon = QuarkEditorUtility.GetCreateAddNewIcon();
            saveActiveIcon = QuarkEditorUtility.GetSaveActiveIcon();
        }
        public void OnGUI()
        {
            GUILayout.Space(16);
            EditorGUILayout.BeginHorizontal();
            {
                buildProfile = (QuarkBuildProfile)EditorGUILayout.ObjectField("Build profile", buildProfile, typeof(QuarkBuildProfile), false);
                if (GUILayout.Button(createAddNewIcon, GUILayout.MaxWidth(QuarkEditorConstant.ICON_WIDTH)))
                {
                    var previousePreset = AssetDatabase.LoadAssetAtPath<QuarkBuildProfile>(QuarkEditorConstant.NEW_BUILD_PROFILE_PATH);
                    if (previousePreset != null)
                    {
                        var canCreate = UnityEditor.EditorUtility.DisplayDialog("QuarkBuildProfile already exist", $"Path {QuarkEditorConstant.NEW_BUILD_PROFILE_PATH} exists.Whether to continue to create and overwrite this file ?", "Create", "Cancel");
                        if (canCreate)
                        {
                            buildProfile = QuarkEditorUtility.CreateScriptableObject<QuarkBuildProfile>(QuarkEditorConstant.NEW_BUILD_PROFILE_PATH, HideFlags.NotEditable);
                        }
                    }
                    else
                    {
                        buildProfile = QuarkEditorUtility.CreateScriptableObject<QuarkBuildProfile>(QuarkEditorConstant.NEW_BUILD_PROFILE_PATH, HideFlags.NotEditable);
                    }
                }
                if (GUILayout.Button(saveActiveIcon, GUILayout.MaxWidth(QuarkEditorConstant.ICON_WIDTH)))
                {
                    QuarkEditorUtility.SaveScriptableObject(buildProfile);
                }
            }
            EditorGUILayout.EndHorizontal();
            if (buildProfile == null)
            {
                return;
            }
            GUILayout.Space(16);
            buildProfile.ProfileDescription = EditorGUILayout.TextField("Profile description", buildProfile.ProfileDescription);

            GUILayout.Space(16);
            DrawBuildSetting();
            DrawBuildType();
            DrawAESEncryption();
            DrawBuildPathLabel();
            DrawCopyToStreamingLabel();
        }
        public void OnDisable()
        {
            SaveLabelData();
        }
        public QuarkBuildParams GetBuildParams()
        {
            string abOutputPath = parent.TabData.ProfileTabAbsAssetBundleBuildPath;
            if (buildProfile == null)
            {
                var defaultBuildParams = new QuarkBuildParams();
                defaultBuildParams.AssetBundleOutputPath = abOutputPath;
                return defaultBuildParams;
            }
            else
            {
                var buildParams = buildProfile.GetBuildParams();
                buildParams.AssetBundleOutputPath = abOutputPath;
                return buildParams;
            }
        }
        public void Reset()
        {
            buildProfile?.Reset();
        }
        void GetLabelData()
        {
            var profilePath = parent.TabData.ProfilePath;
            if (!string.IsNullOrEmpty(profilePath))
            {
                buildProfile = AssetDatabase.LoadAssetAtPath<QuarkBuildProfile>(profilePath);
                if (buildProfile != null)
                {
                    var buildHandlerMaxIndex = buildHandlers.Length - 1;
                    if (buildProfile.AssetBundleBuildProfileData.QuarBuildHandlerIndex > buildHandlerMaxIndex)
                    {
                        buildProfile.AssetBundleBuildProfileData.QuarBuildHandlerIndex = buildHandlerMaxIndex;
                    }
                }
            }
        }
        void SaveLabelData()
        {
            parent.TabData.ProfilePath = AssetDatabase.GetAssetPath(buildProfile);
            QuarkEditorUtility.SaveScriptableObject(buildProfile);
        }
        void DrawAESEncryption()
        {
            buildProfile.AssetBundleBuildProfileData.UseAesEncryptionForManifest = EditorGUILayout.ToggleLeft("Aes encryption for buildInfo and manifest", buildProfile.AssetBundleBuildProfileData.UseAesEncryptionForManifest);
            if (buildProfile.AssetBundleBuildProfileData.UseAesEncryptionForManifest)
            {
                EditorGUILayout.LabelField("BuildInfo AES encryption key, key should be 16,24 or 32 bytes long");
                EditorGUILayout.BeginHorizontal();
                {
                    buildProfile.AssetBundleBuildProfileData.AesEncryptionKeyForManifest = EditorGUILayout.TextField("AESKey", buildProfile.AssetBundleBuildProfileData.AesEncryptionKeyForManifest);
                    if (GUILayout.Button("Generate Key", GUILayout.MaxWidth(128f)))
                    {
                        buildProfile.AssetBundleBuildProfileData.AesEncryptionKeyForManifest = QuarkUtility.GenerateRandomString(16);
                    }
                }
                EditorGUILayout.EndHorizontal();
                var aesKeyStr = buildProfile.AssetBundleBuildProfileData.AesEncryptionKeyForManifest;
                var aesKeyLength = System.Text.Encoding.UTF8.GetBytes(aesKeyStr).Length;
                EditorGUILayout.LabelField($"Current key length is:{aesKeyLength}");
                if (aesKeyLength != 16 && aesKeyLength != 24 && aesKeyLength != 32 && aesKeyLength != 0)
                {
                    EditorGUILayout.HelpBox("Key should be 16,24 or 32 bytes long", MessageType.Error);
                }
                GUILayout.Space(16);
            }
            buildProfile.AssetBundleBuildProfileData.UseOffsetEncryptionForAssetBundle = EditorGUILayout.ToggleLeft("Offset encryption for asserBundle", buildProfile.AssetBundleBuildProfileData.UseOffsetEncryptionForAssetBundle);
            if (buildProfile.AssetBundleBuildProfileData.UseOffsetEncryptionForAssetBundle)
            {
                EditorGUILayout.LabelField("AssetBundle encryption offset");
                buildProfile.AssetBundleBuildProfileData.EncryptionOffsetForAssetBundle = EditorGUILayout.IntField("Encryption offset", buildProfile.AssetBundleBuildProfileData.EncryptionOffsetForAssetBundle);
                if (buildProfile.AssetBundleBuildProfileData.EncryptionOffsetForAssetBundle < 0)
                    buildProfile.AssetBundleBuildProfileData.EncryptionOffsetForAssetBundle = 0;
            }
        }
        void DrawCopyToStreamingLabel()
        {
            GUILayout.BeginVertical();
            {
                buildProfile.AssetBundleBuildProfileData.CopyToStreamingAssets = EditorGUILayout.ToggleLeft("CopyToStreamingAssets", buildProfile.AssetBundleBuildProfileData.CopyToStreamingAssets);
                if (buildProfile.AssetBundleBuildProfileData.CopyToStreamingAssets)
                {
                    buildProfile.AssetBundleBuildProfileData.ClearStreamingAssetsDestinationPath = EditorGUILayout.ToggleLeft("Clear streaming assets destination path", buildProfile.AssetBundleBuildProfileData.ClearStreamingAssetsDestinationPath);

                    var streamingRelativePath = buildProfile.AssetBundleBuildProfileData.StreamingRelativePath.Trim();
                    if (string.IsNullOrEmpty(streamingRelativePath))
                    {
                        GUILayout.Label("Assets/StreamingAssets/[ Nullable ]");
                    }
                    else
                    {
                        GUILayout.Label($"Assets/StreamingAssets/{streamingRelativePath}");
                    }
                    buildProfile.AssetBundleBuildProfileData.StreamingRelativePath = EditorGUILayout.TextField("StreamingRelativePath", streamingRelativePath);
                }
            }
            GUILayout.EndVertical();
        }
        void DrawBuildType()
        {
            buildProfile.AssetBundleBuildProfileData.BuildType = (QuarkBuildType)EditorGUILayout.EnumPopup("Build type", buildProfile.AssetBundleBuildProfileData.BuildType);
            buildProfile.AssetBundleBuildProfileData.AssetBundleNameType = (AssetBundleNameType)EditorGUILayout.EnumPopup("Bundle name type", buildProfile.AssetBundleBuildProfileData.AssetBundleNameType);
            buildProfile.AssetBundleBuildProfileData.BuildVersion = EditorGUILayout.TextField("Build version", buildProfile.AssetBundleBuildProfileData.BuildVersion?.Trim());
            var profileData = buildProfile.AssetBundleBuildProfileData;
            switch (buildProfile.AssetBundleBuildProfileData.BuildType)
            {
                case QuarkBuildType.Full:
                    {
                        buildProfile.AssetBundleBuildProfileData.InternalBuildVersion = EditorGUILayout.IntField("Internal build version", buildProfile.AssetBundleBuildProfileData.InternalBuildVersion);
                        if (buildProfile.AssetBundleBuildProfileData.InternalBuildVersion < 0)
                            buildProfile.AssetBundleBuildProfileData.InternalBuildVersion = 0;
                        var absPath = Path.Combine(QuarkEditorUtility.ApplicationPath, profileData.ProjectRelativeBuildPath, profileData.BuildVersion, profileData.BuildTarget.ToString(), $"{profileData.BuildVersion}_{profileData.InternalBuildVersion}").Replace("\\", "/");
                        parent.TabData.ProfileTabAbsAssetBundleBuildPath = absPath;
                    }
                    break;
                case QuarkBuildType.Incremental:
                    {
                        var absPath = Path.Combine(QuarkEditorUtility.ApplicationPath, profileData.ProjectRelativeBuildPath, profileData.BuildVersion, profileData.BuildTarget.ToString(), profileData.BuildVersion).Replace("\\", "/");
                        parent.TabData.ProfileTabAbsAssetBundleBuildPath = absPath;
                    }
                    break;
            }
        }
        void DrawBuildPathLabel()
        {
            GUILayout.Space(16);

            buildProfile.AssetBundleBuildProfileData.UseProjectRelativeBuildPath = EditorGUILayout.ToggleLeft("Use project relative path", buildProfile.AssetBundleBuildProfileData.UseProjectRelativeBuildPath);

            if (buildProfile.AssetBundleBuildProfileData.UseProjectRelativeBuildPath)
            {
                GUILayout.BeginHorizontal();
                {
                    buildProfile.AssetBundleBuildProfileData.ProjectRelativeBuildPath = EditorGUILayout.TextField("Project relative path", buildProfile.AssetBundleBuildProfileData.ProjectRelativeBuildPath.Trim());
                    if (GUILayout.Button("Browse", GUILayout.MaxWidth(128f)))
                    {
                        BrowseProjectRelativeFolder();
                    }
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.BeginHorizontal();
                {
                    parent.TabData.ProfileTabAbsPath = EditorGUILayout.TextField("Build path", parent.TabData.ProfileTabAbsPath.Trim());
                    if (GUILayout.Button("Browse", GUILayout.MaxWidth(128f)))
                    {
                        BrowseFolder();
                    }
                }
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.LabelField("Build absolute path", parent.TabData.ProfileTabAbsAssetBundleBuildPath);
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Open build Path", GUILayout.MaxWidth(128f)))
                {
                    var path = parent.TabData.ProfileTabAbsAssetBundleBuildPath;
                    if (!Directory.Exists(path))
                    {
                        EditorUtility.RevealInFinder(Application.dataPath);
                    }
                    else
                    {
                        EditorUtility.RevealInFinder(path);
                    }
                }
            }
            GUILayout.EndHorizontal();
        }
        void DrawBuildSetting()
        {
            buildProfile.AssetBundleBuildProfileData.ForceRemoveAllAssetBundleNames = EditorGUILayout.ToggleLeft("Force remove all assetbundle names before build", buildProfile.AssetBundleBuildProfileData.ForceRemoveAllAssetBundleNames);
            GUILayout.Space(16);

            buildProfile.AssetBundleBuildProfileData.BuildTarget = (BuildTarget)EditorGUILayout.EnumPopup("Build target", buildProfile.AssetBundleBuildProfileData.BuildTarget);
            buildProfile.AssetBundleBuildProfileData.AssetBundleCompressType = (AssetBundleCompressType)EditorGUILayout.EnumPopup("Build compression type", buildProfile.AssetBundleBuildProfileData.AssetBundleCompressType);

            buildProfile.AssetBundleBuildProfileData.QuarBuildHandlerIndex = EditorGUILayout.Popup("Build handler", buildProfile.AssetBundleBuildProfileData.QuarBuildHandlerIndex, buildHandlers);
            var index = buildProfile.AssetBundleBuildProfileData.QuarBuildHandlerIndex;
            if (buildHandlers.Length > 0 && index < buildHandlers.Length)
            {
                buildProfile.AssetBundleBuildProfileData.BuildHandlerName = buildHandlers[index];
            }

            buildProfile.AssetBundleBuildProfileData.ForceRebuildAssetBundle = EditorGUILayout.ToggleLeft("Force rebuild assetBundle", buildProfile.AssetBundleBuildProfileData.ForceRebuildAssetBundle);
            buildProfile.AssetBundleBuildProfileData.DisableWriteTypeTree = EditorGUILayout.ToggleLeft("Disable write type tree", buildProfile.AssetBundleBuildProfileData.DisableWriteTypeTree);
            if (buildProfile.AssetBundleBuildProfileData.DisableWriteTypeTree)
                buildProfile.AssetBundleBuildProfileData.IgnoreTypeTreeChanges = false;

            buildProfile.AssetBundleBuildProfileData.DeterministicAssetBundle = EditorGUILayout.ToggleLeft("Deterministic assetBundle", buildProfile.AssetBundleBuildProfileData.DeterministicAssetBundle);
            buildProfile.AssetBundleBuildProfileData.IgnoreTypeTreeChanges = EditorGUILayout.ToggleLeft("Ignore type tree changes", buildProfile.AssetBundleBuildProfileData.IgnoreTypeTreeChanges);
            if (buildProfile.AssetBundleBuildProfileData.IgnoreTypeTreeChanges)
                buildProfile.AssetBundleBuildProfileData.DisableWriteTypeTree = false;
            GUILayout.Space(16);

        }
        void BrowseFolder()
        {
            var newPath = EditorUtility.OpenFolderPanel("Bundle Folder", buildProfile.AssetBundleBuildProfileData.BuildPath, string.Empty);
            if (!string.IsNullOrEmpty(newPath))
            {
                buildProfile.AssetBundleBuildProfileData.BuildPath = newPath.Replace("\\", "/");
            }
        }
        void BrowseProjectRelativeFolder()
        {
            var absPath = Path.Combine(QuarkEditorUtility.ApplicationPath, buildProfile.AssetBundleBuildProfileData.ProjectRelativeBuildPath);

            var newPath = EditorUtility.OpenFolderPanel("Bundle Folder", absPath, string.Empty);
            if (!string.IsNullOrEmpty(newPath))
            {
                if (newPath.Contains(QuarkEditorUtility.ApplicationPath))
                {
                    var charLength = QuarkEditorUtility.ApplicationPath.Length;
                    buildProfile.AssetBundleBuildProfileData.ProjectRelativeBuildPath = newPath.Remove(0, charLength + 1);
                }
                else
                {
                    buildProfile.AssetBundleBuildProfileData.ProjectRelativeBuildPath = QuarkEditorConstant.DEFAULT_ASSETBUNDLE_RELATIVE_PATH;
                }
            }
        }
        QuarkBuildProfile CreateBuildProfile()
        {
            var so = QuarkEditorUtility.CreateScriptableObject<QuarkBuildProfile>("Assets/Editor/NewQuarkBuildProfile.asset");
            return so;
        }
    }
}
