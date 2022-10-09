using System;
using UnityEngine;
using System.Collections.Generic;
namespace Quark.Asset
{
    /// <summary>
    /// QuarkAssetDataset用于在Editor Runtime快速开发时使用；
    /// build之后需配合AB资源使用；
    /// </summary>
    [Serializable]
    public sealed class QuarkAssetDataset : ScriptableObject, IDisposable, IQuarkLoaderData
    {
        [SerializeField]
        List<QuarkObject> quarkObjectList;
        [SerializeField]
        List<QuarkAssetBundle> quarkAssetBundleList;
        [SerializeField]
        List<string> quarkAssetExts;
        [SerializeField]
        List<QuarkObject> quarkSceneList;
        public List<QuarkObject> QuarkObjectList
        {
            get
            {
                if (quarkObjectList == null)
                    quarkObjectList = new List<QuarkObject>();
                return quarkObjectList;
            }
        }
        /// <summary>
        /// 可识别的文件后缀名
        /// </summary>
        public List<string> QuarkAssetExts
        {
            get
            {
                if (quarkAssetExts == null)
                    quarkAssetExts = new List<string>();
                return quarkAssetExts;
            }
        }
        public List<QuarkAssetBundle> QuarkAssetBundleList
        {
            get
            {
                if (quarkAssetBundleList == null)
                    quarkAssetBundleList = new List<QuarkAssetBundle>();
                return quarkAssetBundleList;
            }
            set { quarkAssetBundleList = value; }
        }
        /// <summary>
        /// 场景资源文件；
        /// </summary>
        public List<QuarkObject> QuarkSceneList
        {
            get
            {
                if (quarkSceneList == null)
                    quarkSceneList = new List<QuarkObject>();
                return quarkSceneList;
            }
        }
        public void Dispose()
        {
            quarkObjectList?.Clear();
            quarkAssetBundleList?.Clear();
        }
    }
}