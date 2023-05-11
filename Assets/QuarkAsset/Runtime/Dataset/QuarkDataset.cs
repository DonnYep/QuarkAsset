using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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
        Dictionary<string, QuarkBundleInfo> bundleInfoDict;
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
        public Dictionary<string, QuarkBundleInfo> BundleInfoDict
        {
            get
            {
                if (bundleInfoDict == null)
                {
                    bundleInfoDict = GetBundleInfos().ToDictionary((b) => b.BundleName);
                }
                return bundleInfoDict;
            }
        }
        public bool PeekBundleInfo(string displayName, out QuarkBundleInfo bundleInfo)
        {
            return BundleInfoDict.TryGetValue(displayName, out bundleInfo);
        }
        public void RegenerateBundleInfoDict()
        {
            bundleInfoDict?.Clear();
            bundleInfoDict = GetBundleInfos().ToDictionary((b) => b.BundleName);
        }
        public List<QuarkBundleInfo> GetBundleInfos()
        {
            List<QuarkBundleInfo> infoList = new List<QuarkBundleInfo>();
            var length = QuarkBundleInfoList.Count;
            for (int i = 0; i < length; i++)
            {
                var bundleInfo = quarkBundleInfoList[i];
                if (bundleInfo.Splittable)
                {
                    GetSubBundleInfo(bundleInfo, ref infoList);
                }
                else
                {
                    infoList.Add(bundleInfo);
                }
            }
            return infoList;
        }
        public void Dispose()
        {
            quarkBundleInfoList?.Clear();
        }
        void GetSubBundleInfo(QuarkBundleInfo bundleInfo, ref List<QuarkBundleInfo> infoList)
        {
            //多次拆包不在此版本考虑范围内
            var subBundleInfos = bundleInfo.SubBundleInfoList;
            var length = subBundleInfos.Count;
            for (int i = 0; i < length; i++)
            {
                var subBundleInfo = subBundleInfos[i];

                var newBundleInfo = new QuarkBundleInfo()
                {
                    BundleName = subBundleInfo.BundleName,
                    BundlePath = subBundleInfo.BundlePath,
                    BundleKey = subBundleInfo.BundleKey,
                    BundleSize = subBundleInfo.BundleSize,
                    BundleFormatBytes = subBundleInfo.BundleFormatBytes,
                };
                newBundleInfo.ObjectInfoList.AddRange(subBundleInfo.ObjectInfoList);
                newBundleInfo.DependentBundleKeyList.AddRange(subBundleInfo.DependentBundleKeyList);
                infoList.Add(newBundleInfo);
            }
        }
    }
}