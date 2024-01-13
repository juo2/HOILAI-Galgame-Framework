using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AssetManagement
{
    public partial class AssetManager
    {
        public class AssetLoadProfilerInfo
        {
            public string assetName;
            //加载时间
            public float loadTime;
            //ab加载时间
            public float abloadTime;
            //下载时间
            public float downloadTime;
            //创建时间
            public float createTime;
        }
        private Dictionary<string, AssetLoadProfilerInfo> m_AssetLoaderProfilerInfos = new Dictionary<string, AssetLoadProfilerInfo>(50);
        public Dictionary<string, AssetLoadProfilerInfo> assetLoaderProfilerInfos { get { return m_AssetLoaderProfilerInfos; } }

        public AssetLoadProfilerInfo GetProfilerInfo(string assetName)
        {
            AssetLoadProfilerInfo alpi;
            if (!m_AssetLoaderProfilerInfos.TryGetValue(assetName, out alpi))
            {
                if (m_AssetLoaderProfilerInfos.Count > 2000)
                    m_AssetLoaderProfilerInfos.Clear();
                alpi = new AssetLoadProfilerInfo();
                alpi.assetName = assetName;
                m_AssetLoaderProfilerInfos.Add(assetName, alpi);
            }
            return alpi;
        }
    }

}
