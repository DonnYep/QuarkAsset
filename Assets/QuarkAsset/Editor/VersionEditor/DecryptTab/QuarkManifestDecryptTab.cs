using Quark.Asset;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Quark.Editor
{
    public class QuarkManifestDecryptTab
    {
        internal const string ManifestDecryptTabDataFileName = "QuarkVersion_ManifestDecryptTabData.json";
        QuarkManifestDecryptTabData wndData;
        public void OnEnable()
        {
            GetWindowData();

        }
        public void OnGUI()
        {

            EditorGUILayout.BeginHorizontal();
            {
                wndData.ManifestPath = EditorGUILayout.TextField("ManifestPath", wndData.ManifestPath);
                if (GUILayout.Button("Browse", GUILayout.MaxWidth(128f)))
                {
                    var newPath = EditorUtility.OpenFilePanel("ManifestPath", wndData.ManifestPath, string.Empty);
                    if (!string.IsNullOrEmpty(newPath))
                    {
                        wndData.ManifestPath = newPath.Replace("\\", "/");
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            wndData.ManifestAesKey = EditorGUILayout.TextField("ManifestAesKey", wndData.ManifestAesKey);

            GUILayout.Space(16);

            EditorGUILayout.BeginHorizontal();
            {
                wndData.DecryptedManifestOutputPath = EditorGUILayout.TextField("CompareResultPath", wndData.DecryptedManifestOutputPath);
                if (GUILayout.Button("Browse", GUILayout.MaxWidth(128f)))
                {
                    var newPath = EditorUtility.OpenFolderPanel("CompareResultPath", wndData.DecryptedManifestOutputPath, string.Empty);
                    if (!string.IsNullOrEmpty(newPath))
                    {
                        wndData.DecryptedManifestOutputPath = newPath.Replace("\\", "/");
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            wndData.OpenDecryptPathWhenCompareDone = EditorGUILayout.ToggleLeft("Open decrypted path when output", wndData.OpenDecryptPathWhenCompareDone);
            GUILayout.Space(16);

            if (GUILayout.Button("Decrypd"))
            {

                var manifest = LoadManifest(wndData.ManifestPath, wndData.ManifestAesKey);
                if (manifest != null)
                {
                    OverwriteManifest(wndData.DecryptedManifestOutputPath, manifest);
                    QuarkUtility.LogInfo("Decrypted manifest overwrite done ! ");
                    if (wndData.OpenDecryptPathWhenCompareDone)
                    {
                        EditorUtility.RevealInFinder(wndData.DecryptedManifestOutputPath);
                    }
                }
            }
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
                wndData = QuarkEditorUtility.GetData<QuarkManifestDecryptTabData>(ManifestDecryptTabDataFileName);
            }
            catch
            {
                wndData = new QuarkManifestDecryptTabData();
                QuarkEditorUtility.SaveData(ManifestDecryptTabDataFileName, wndData);
            }
        }
        void SaveWindowData()
        {
            QuarkEditorUtility.SaveData(ManifestDecryptTabDataFileName, wndData);
        }
    }
}
