using Quark.Asset;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Quark.Editor
{
    public class QuarkManifestDecryptTab
    {
        internal const string ManifestDecryptTabDataFileName = "QuarkVersion_ManifestDecryptTabData.json";
        QuarkManifestDecryptTabData tabData;
        public void OnEnable()
        {
            GetWindowData();
        }
        public void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            {
                tabData.ManifestPath = EditorGUILayout.TextField("ManifestPath", tabData.ManifestPath);
                if (GUILayout.Button("Browse", GUILayout.MaxWidth(128f)))
                {
                    var newPath = EditorUtility.OpenFilePanel("ManifestPath", tabData.ManifestPath, string.Empty);
                    if (!string.IsNullOrEmpty(newPath))
                    {
                        tabData.ManifestPath = newPath.Replace("\\", "/");
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            tabData.ManifestAesKey = EditorGUILayout.TextField("ManifestAesKey", tabData.ManifestAesKey);

            GUILayout.Space(16);

            EditorGUILayout.BeginHorizontal();
            {
                tabData.DecryptedManifestOutputPath = EditorGUILayout.TextField("CompareResultPath", tabData.DecryptedManifestOutputPath);
                if (GUILayout.Button("Browse", GUILayout.MaxWidth(128f)))
                {
                    var newPath = EditorUtility.OpenFolderPanel("CompareResultPath", tabData.DecryptedManifestOutputPath, string.Empty);
                    if (!string.IsNullOrEmpty(newPath))
                    {
                        tabData.DecryptedManifestOutputPath = newPath.Replace("\\", "/");
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            tabData.OpenDecryptPathWhenCompareDone = EditorGUILayout.ToggleLeft("Open decrypted path when output", tabData.OpenDecryptPathWhenCompareDone);
            GUILayout.Space(16);
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Decrypd"))
                {
                    var manifest = LoadManifest(tabData.ManifestPath, tabData.ManifestAesKey);
                    if (manifest != null)
                    {
                        OverwriteManifest(tabData.DecryptedManifestOutputPath, manifest);
                        QuarkUtility.LogInfo("Decrypted manifest overwrite done ! ");
                        if (tabData.OpenDecryptPathWhenCompareDone)
                        {
                            EditorUtility.RevealInFinder(tabData.DecryptedManifestOutputPath);
                        }
                    }
                }
                if (GUILayout.Button("Clear"))
                {

                }
            }
            GUILayout.EndHorizontal();
        }
        public void OnDisable()
        {
            SaveWindowData();
        }
        QuarkManifest LoadManifest(string path, string key)
        {
            if (File.Exists(path))
            {
                var context = QuarkUtility.ReadTextFileContent(path);
                return Quark.QuarkUtility.Manifest.DeserializeManifest(context, key);
            }
            else
            {
                return null;
            }
        }
        void OverwriteManifest(string path, QuarkManifest manifest)
        {
            var contect = Quark.QuarkUtility.Manifest.SerializeManifest(manifest, string.Empty);
            var folderPath = Path.GetDirectoryName(path);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            var manifestPath = Path.Combine(folderPath, $"Decrypt_{QuarkConstant.MANIFEST_NAME}");
            QuarkUtility.OverwriteTextFile(manifestPath, contect);
        }
        void GetWindowData()
        {
            try
            {
                tabData = QuarkEditorUtility.GetData<QuarkManifestDecryptTabData>(ManifestDecryptTabDataFileName);
            }
            catch
            {
                tabData = new QuarkManifestDecryptTabData();
                QuarkEditorUtility.SaveData(ManifestDecryptTabDataFileName, tabData);
            }
        }
        void SaveWindowData()
        {
            QuarkEditorUtility.SaveData(ManifestDecryptTabDataFileName, tabData);
        }
    }
}
