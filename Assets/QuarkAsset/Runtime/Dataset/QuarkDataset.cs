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
        List<QuarkObjectInfo> quarkSceneList;
        List<IQuarkBundleInfo> allCachedBundleInfos;
        /// <summary>
        /// 包含subbundle的所有bundle
        /// </summary>
        public List<IQuarkBundleInfo> AllCachedBundleInfos
        {
            get
            {
                if (allCachedBundleInfos == null)
                    allCachedBundleInfos = new List<IQuarkBundleInfo>();
                return allCachedBundleInfos;
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
        public IList<IQuarkBundleInfo> GetCacheAllBundleInfos()
        {
            CacheAllBundleInfos();
            return AllCachedBundleInfos;
        }
        public void CacheAllBundleInfos()
        {
            var infoList = AllCachedBundleInfos;
            infoList.Clear();
            var length = QuarkBundleInfoList.Count;
            for (int i = 0; i < length; i++)
            {
                var bundleInfo = quarkBundleInfoList[i];
                if (bundleInfo.Split)
                {
                    GetSubBundleInfo(bundleInfo, ref infoList);
                }
                else if (bundleInfo.Extract)
                {
                    GetIndividualBundleInfo(bundleInfo, ref infoList);
                }
                else
                {
                    infoList.Add(bundleInfo);
                }
            }
        }
        public void Dispose()
        {
            quarkBundleInfoList?.Clear();
        }
        void GetSubBundleInfo(QuarkBundleInfo bundleInfo, ref List<IQuarkBundleInfo> infoList)
        {
            //多次拆包不在此版本考虑范围内
            var subBundleInfos = bundleInfo.SubBundleInfoList;
            var length = subBundleInfos.Count;
            for (int i = 0; i < length; i++)
            {
                var subBundleInfo = subBundleInfos[i];
                infoList.Add(subBundleInfo);
            }
        }
        /// <summary>
        /// 获取独立成为bundle的资源
        /// </summary>
        /// <param name="bundleInfo"></param>
        /// <param name="infoList"></param>
        void GetIndividualBundleInfo(IQuarkBundleInfo bundleInfo, ref List<IQuarkBundleInfo> infoList)
        {
            var extract = bundleInfo.Extract;
            if (!extract)
                return;
            var objectInfoList = bundleInfo.ObjectInfoList;
            var length = objectInfoList.Count;
            for (int i = 0; i < length; i++)
            {
                var objectInfo = objectInfoList[i];
                var individualBundleInfo = new QuarkIndividualBundleInfo()
                {
                    BundlePath = objectInfo.ObjectPath,
                    BundleName = objectInfo.ObjectPath,
                    Extract = true
                };
                individualBundleInfo.ObjectInfoList.Add(objectInfo);
                individualBundleInfo.BundleKey = individualBundleInfo.BundleName;
                infoList.Add(individualBundleInfo);
            }
        }
    }
}