#define EDITOR

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using AssetManagement;
using Object = UnityEngine.Object;



[Serializable]
public class AssetProfilerDetail
{
    [Serializable]
    public class XAssetBundleInfo
    {
        public string bundleName;
        public int referenceCount;
        public int newReferenceCount; //新的引用计数方法
        public int rawReferenceCount;
        public int destoryTime;
        public int beginDestoryTime;
        public int doneFrame;
        public bool rootLoad;
        public int parentNum;
        public List<XRawObjectInfo> rawObjects;
    }

    [Serializable]
    public class XRawObjectInfo
    {
        public int referenceCount = 0;
        [NonSerialized]
        public Object rawObject;
        public string assetName;
        [NonSerialized]
        public List<Object> instanceObjects;
        public List<string> instanceObjectNames;
    }

    public List<XAssetBundleInfo> p_XAssetBundleInfos;
    public List<XRawObjectInfo> p_XRawObjectInfos;

    public XAssetBundleInfo FindXAssetBundleInfoByRawInfo(XRawObjectInfo rawInfo)
    {
        return p_XAssetBundleInfos.Find((XAssetBundleInfo binfo) =>
        {
            return binfo.rawObjects.Find((XRawObjectInfo xrawInfo) => { return xrawInfo == rawInfo; }) != null;
        });
    }


    public static AssetProfilerDetail CreateAssetProfilerDetail()
    {
        if (!Application.isPlaying) AssetCache.ClearCache();

        List<AssetProfilerDetail.XRawObjectInfo> rawObjectList = new List<AssetProfilerDetail.XRawObjectInfo>();
        List<AssetProfilerDetail.XAssetBundleInfo> bundleInfoList = new List<AssetProfilerDetail.XAssetBundleInfo>();

#if EDITOR
        Dictionary<AssetCache.RawObjectInfo, List<Object>> dic = new Dictionary<AssetCache.RawObjectInfo, List<Object>>();
#else
        Dictionary<AssetCache.RawObjectInfo, List<string>> dic = new Dictionary<AssetCache.RawObjectInfo, List<string>>();
#endif

        Dictionary<XAssetBundle, List<AssetProfilerDetail.XRawObjectInfo>> bunldDic = new Dictionary<XAssetBundle, List<AssetProfilerDetail.XRawObjectInfo>>();
        foreach (var item in AssetCache.AllInstanceObjectMap)
        {

#if EDITOR
            List<Object> gos;
            if (!dic.TryGetValue(item.Value.m_RawInfo, out gos))
            {
                gos = new List<Object>();
                dic.Add(item.Value.m_RawInfo, gos);
            }
            gos.Add(item.Value.m_InstanceObject);
#else
            List<string> gos;
            if (!dic.TryGetValue(item.Value.m_RawInfo, out gos))
            {
                gos = new List<string>();
                dic.Add(item.Value.m_RawInfo, gos);
            }

            if (!item.Value.m_InstanceObject || item.Value.m_InstanceObject.Equals(null))
            {
                gos.Add("null");
            }
            else
            {
                gos.Add(item.Value.m_InstanceObject.name);
            }
#endif



        }

        foreach (var item in AssetCache.AllRawObjectMap)
        {
            AssetProfilerDetail.XRawObjectInfo info = new AssetProfilerDetail.XRawObjectInfo();
            info.assetName = item.Value.m_AssetName;
            info.rawObject = item.Value.m_Object;
            info.referenceCount = item.Value.m_ReferenceCount;
#if EDITOR
            List<Object> gos;
            if (dic.TryGetValue(item.Value, out gos))
            {
                info.instanceObjects = gos;
            }
#else
            List<string> gos;
            if (dic.TryGetValue(item.Value, out gos))
            {
                info.instanceObjectNames = gos;
            }
#endif
            rawObjectList.Add(info);

            if (item.Value.m_Owner != null)
            {
                List<AssetProfilerDetail.XRawObjectInfo> rawList;
                if (!bunldDic.TryGetValue(item.Value.m_Owner, out rawList))
                {
                    rawList = new List<AssetProfilerDetail.XRawObjectInfo>();
                    bunldDic.Add(item.Value.m_Owner, rawList);
                }
                rawList.Add(info);
            }
        }


        if (AssetBundleManager.Instance != null)
        {
            foreach (var item in AssetBundleManager.Instance.XAssetBundleMap)
            {
                AssetProfilerDetail.XAssetBundleInfo info = new AssetProfilerDetail.XAssetBundleInfo();
                info.beginDestoryTime = item.Value.BeginDestoryTime;
                info.destoryTime = item.Value.DestoryTime;
                info.rawReferenceCount = item.Value.RawReferenceCount;
                info.bundleName = item.Value.BundleName;
                info.referenceCount = item.Value.ReferenceCount;
                info.doneFrame = item.Value.LoadDoneFrame;
                info.newReferenceCount = AssetBundleManager.Instance.GetReferenceCount(item.Value.BundleName);
                info.rootLoad = AssetBundleManager.Instance.loadRootBundleNames.Contains(info.bundleName);
                info.parentNum = AssetBundleManager.Instance.loadReverseRefMap.ContainsKey(info.bundleName) ? AssetBundleManager.Instance.loadReverseRefMap[info.bundleName].Count : 0;

                List<AssetProfilerDetail.XRawObjectInfo> rawList;
                if (bunldDic.TryGetValue(item.Value, out rawList))
                {
                    info.rawObjects = rawList;
                }

                bundleInfoList.Add(info);
            }
        }

        AssetProfilerDetail apd = new AssetProfilerDetail();
        apd.p_XAssetBundleInfos = bundleInfoList;
        apd.p_XRawObjectInfos = rawObjectList;
        return apd;
    }



    public void CSerialize(string path)
    {
        using (FileStream fs = File.OpenWrite(path))
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fs, this);
            fs.Flush();
            fs.Close();
        }
    }

    public static void Serialize(string path)
    {
        AssetProfilerDetail apd = CreateAssetProfilerDetail();
        apd.CSerialize(path);
    }

    public static AssetProfilerDetail Deserialize(string path)
    {
        AssetProfilerDetail apd;
        using (FileStream fs = File.OpenRead(path))
        {
            BinaryFormatter bf = new BinaryFormatter();
            apd = bf.Deserialize(fs) as AssetProfilerDetail;
            fs.Flush();
            fs.Close();
        }

        return apd;
    }
}
