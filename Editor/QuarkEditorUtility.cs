﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Networking;

namespace Quark.Editor
{
    public static partial class QuarkEditorUtility
    {
        static string libraryPath;
        public static string LibraryPath
        {
            get
            {
                if (string.IsNullOrEmpty(libraryPath))
                {
                    var editorPath = new DirectoryInfo(Application.dataPath);
                    var rootPath = editorPath.Parent.FullName + "/Library/";
                    libraryPath = Path.Combine(rootPath, "QuarkAsset");
                }
                return libraryPath;
            }
        }
        static Dictionary<string, List<string>> dependenciesMap = new Dictionary<string, List<string>>();
        static string applicationPath;
        public static string ApplicationPath
        {
            get
            {
                if (string.IsNullOrEmpty(applicationPath))
                {
                    applicationPath = Directory.GetCurrentDirectory();
                }
                return applicationPath;
            }
        }
        public static void BuildAssetBundle(BuildTarget target, string outPath, BuildAssetBundleOptions options = BuildAssetBundleOptions.ChunkBasedCompression)
        {
            AssetDatabase.RemoveUnusedAssetBundleNames();
            if (!Directory.Exists(outPath))
            {
                Directory.CreateDirectory(outPath);
            }
            BuildPipeline.BuildAssetBundles(outPath, options | BuildAssetBundleOptions.DeterministicAssetBundle, target);
        }
        public static void BuildSceneBundle(string[] sceneList, string outPath)
        {
            if (!Directory.Exists(outPath))
            {
                Directory.CreateDirectory(outPath);
            }
            var buildOptions = new BuildPlayerOptions()
            {
                scenes = sceneList,
                locationPathName = outPath,
                target = BuildTarget.StandaloneWindows,
                options = BuildOptions.BuildAdditionalStreamedScenes
            };
            BuildPipeline.BuildPlayer(buildOptions);
            AssetDatabase.Refresh();
        }
        public static T GetData<T>(string dataName)
            where T : class, new()
        {
            var filePath = Path.Combine(LibraryPath, dataName);
            var json = QuarkUtility.ReadTextFileContent(filePath);
            var data = QuarkUtility.ToObject<T>(json);
            return data;
        }
        public static void SaveData<T>(string dataName, T data)
        {
            var json = QuarkUtility.ToJson(data);
            QuarkUtility.OverwriteTextFile(LibraryPath, dataName, json);
        }
        public static void ClearData(string fileName)
        {
            var filePath = Path.Combine(LibraryPath, fileName);
            QuarkUtility.DeleteFile(filePath);
        }
        /// <summary>
        /// 获取原生Folder资源icon
        /// </summary>
        /// <returns>icon</returns>
        public static Texture2D GetFolderIcon()
        {
            return EditorGUIUtility.FindTexture("Folder Icon");
        }
        public static Texture2D GetRefreshIcon()
        {
            return EditorGUIUtility.FindTexture("Refresh");
        }
        public static Texture2D ToTexture2D(Texture texture)
        {
            return Texture2D.CreateExternalTexture(
                texture.width,
                texture.height,
                TextureFormat.RGB24,
                false, false,
                texture.GetNativeTexturePtr());
        }
        /// <summary>
        /// 获取除自生以外的依赖资源的所有路径；
        /// </summary>
        /// <param name="path">目标资源地址</param>
        /// <returns>依赖的资源路径</returns>
        public static string[] GetDependencises(string path)
        {
            dependenciesMap.Clear();
            //全部小写
            List<string> list = null;
            if (!dependenciesMap.TryGetValue(path, out list))
            {
                list = AssetDatabase.GetDependencies(path).Select((s) => s.ToLower()).ToList();
                list.Remove(path.ToLower());
                //检测依赖路径
                CheckAssetsPath(list);
                dependenciesMap[path] = list;
            }
            return list.ToArray();
        }
        /// <summary>
        /// 获取文件夹的MD5；
        /// </summary>
        /// <param name="srcPath">文件夹路径</param>
        /// <returns>MD5</returns>
        public static string CreateDirectoryMd5(string srcPath)
        {
            var filePaths = Directory.GetFiles(srcPath, "*", SearchOption.AllDirectories).OrderBy(p => p).ToArray();
            using (var md5 = MD5.Create())
            {
                foreach (var filePath in filePaths)
                {
                    byte[] pathBytes = Encoding.UTF8.GetBytes(filePath);
                    md5.TransformBlock(pathBytes, 0, pathBytes.Length, pathBytes, 0);
                    byte[] contentBytes = File.ReadAllBytes(filePath);
                    md5.TransformBlock(contentBytes, 0, contentBytes.Length, contentBytes, 0);
                }
                md5.TransformFinalBlock(new byte[0], 0, 0);
                return BitConverter.ToString(md5.Hash).Replace("-", "").ToLower();
            }
        }
        /// <summary>
        /// 获取可以打包的资源
        /// </summary>
        static void CheckAssetsPath(List<string> list)
        {
            if (list.Count == 0)
                return;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var path = list[i];
                //文件不存在,或者是个文件夹移除
                if (!File.Exists(path) || Directory.Exists(path))
                {
                    list.RemoveAt(i);
                    continue;
                }
                //判断路径是否为editor依赖
                if (path.Contains("/editor/"))
                {
                    list.RemoveAt(i);
                    continue;
                }
                //特殊后缀
                var ext = Path.GetExtension(path).ToLower();
                if (ext == ".cs" || ext == ".js" || ext == ".dll")
                {
                    list.RemoveAt(i);
                    continue;
                }
            }
        }

        /// <summary>
        /// EditorCoroutine 嵌套协程无法识别 yield return IEnumerator；
        /// 嵌套协程尽量使用yield return EditorCoroutine；
        /// </summary>
        public static EditorCoroutine StartCoroutine(IEnumerator coroutine)
        {
            return EditorCoroutineUtility.StartCoroutineOwnerless(coroutine);
        }
        public static void StopCoroutine(EditorCoroutine coroutine)
        {
            EditorCoroutineUtility.StopCoroutine(coroutine);
        }
        public static void StopCoroutine(IEnumerator coroutine)
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(coroutine);
        }
        public static MultiColumnHeaderState CreateBundleMultiColumnHeader()
        {
            var columns = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Index"),
                    headerTextAlignment = TextAlignment.Left,
                    sortingArrowAlignment = TextAlignment.Left,
                    sortedAscending = false,
                    minWidth=24,
                    width=40,
                    maxWidth=92,
                    autoResize = true,
                    canSort=false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Size"),
                    headerTextAlignment = TextAlignment.Left,
                    sortingArrowAlignment = TextAlignment.Left,
                    sortedAscending = false,
                    minWidth=24,
                    width=64,
                    maxWidth=92,
                    autoResize = true,
                    canSort=true
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Count"),
                    headerTextAlignment = TextAlignment.Left,
                    sortingArrowAlignment = TextAlignment.Left,
                    sortedAscending = false,
                    minWidth=36,
                    width = 48,
                    maxWidth=92,
                    autoResize = false,
                    canSort=true
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Bundle"),
                    headerTextAlignment = TextAlignment.Left,
                    sortingArrowAlignment = TextAlignment.Left,
                    sortedAscending = false,
                    minWidth=192,
                    width = 768,
                    maxWidth=1024,
                    autoResize = false,
                    canSort=true
                }
            };
            var state = new MultiColumnHeaderState(columns);
            return state;
        }
        public static MultiColumnHeaderState CreateObjectMultiColumnHeader()
        {
            var columns = new[]
            {
            new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Index"),
                    headerTextAlignment = TextAlignment.Left,
                    sortingArrowAlignment = TextAlignment.Left,
                    sortedAscending = false,
                    minWidth=24,
                    width=40,
                    maxWidth=92,
                    autoResize = true,
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("ObjectName"),
                    headerTextAlignment = TextAlignment.Left,
                    sortingArrowAlignment = TextAlignment.Left,
                    sortedAscending = false,
                    minWidth=128,
                    width=168,
                    maxWidth=320,
                    autoResize = true,
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Extension"),
                    headerTextAlignment = TextAlignment.Left,
                    sortingArrowAlignment = TextAlignment.Left,
                    sortedAscending = false,
                    minWidth=56,
                    width=92,
                    maxWidth=192,
                    autoResize = true,
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("BundleName"),
                    headerTextAlignment = TextAlignment.Left,
                    sortingArrowAlignment = TextAlignment.Left,
                    sortedAscending = false,
                    minWidth=92,
                    width=320,
                    maxWidth=384,
                    autoResize = true,
                },
                 new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("AssetPath"),
                    headerTextAlignment = TextAlignment.Left,
                    sortingArrowAlignment = TextAlignment.Left,
                    sortedAscending = false,
                    minWidth=192,
                    width=768,
                    maxWidth=1024,
                    autoResize = true,
                }
            };
            var state = new MultiColumnHeaderState(columns);
            return state;
        }
        /// <summary>
        /// 获取文件夹中文件的总体大小；
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="availableExtes">可识别的后缀名</param>
        /// <returns>size</returns>
        public static long GetUnityDirectorySize(string path, List<string> availableExtes)
        {
            if (!path.StartsWith("Assets"))
                return 0;
            if (!AssetDatabase.IsValidFolder(path))
                return 0;
            var fullPath = Path.Combine(ApplicationPath, path);
            if (!Directory.Exists(fullPath))
                return 0;
            DirectoryInfo directory = new DirectoryInfo(fullPath);
            var allFiles = directory.GetFiles("*.*", SearchOption.AllDirectories);
            long totalSize = 0;
            foreach (var file in allFiles)
            {
                if (availableExtes.Contains(file.Extension))
                    totalSize += file.Length;
            }
            return totalSize;
        }
    }
}