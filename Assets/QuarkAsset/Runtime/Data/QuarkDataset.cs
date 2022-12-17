using System;
using UnityEngine;
using System.Collections.Generic;
namespace Quark.Asset
{
    /// <summary>
    /// build之后需配合AB资源使用；
    /// </summary>
    [Serializable]
    public sealed class QuarkDataset : ScriptableObject, IDisposable, IQuarkLoaderData
    {
        [SerializeField]
        List<QuarkBundleInfo> quarkBundleInfoList;
        [SerializeField]
        List<string> quarkAssetExts;
        [SerializeField]
        List<QuarkObjectInfo> quarkSceneList;
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
        public List<QuarkBundleInfo> QuarkBundleInfoList
        {
            get
            {
                if (quarkBundleInfoList == null)
                    quarkBundleInfoList = new List<QuarkBundleInfo>();
                return quarkBundleInfoList;
            }
            set { quarkBundleInfoList = value; }
        }
        /// <summary>
        /// 场景资源文件；
        /// </summary>
        public List<QuarkObjectInfo> QuarkSceneList
        {
            get
            {
                if (quarkSceneList == null)
                    quarkSceneList = new List<QuarkObjectInfo>();
                return quarkSceneList;
            }
        }
        public void Dispose()
        {
            quarkBundleInfoList?.Clear();
        }
    }
}