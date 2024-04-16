using UnityEngine;
using System.Collections.Generic;

namespace AssetManagement
{
    public class XAssetBundle
    {
        private AssetBundle m_Bundle;
        private string m_BundleName;
        private int m_LoadDoneFrame = -1;     //加载完成的帧数
        private int m_ReferenceCount = 1;     //依赖引用计数      
        private int m_RawReferenceCount = 0;  //源对象引用计数
        private int m_DestoryTime = 20;       //无引用将在20后清除  -1 将为永不销毁
        private int m_BeginDestoryTime = -1;  //销毁时间戳
        private bool m_IsAssetLoading = false; //此ab包是否正在加载资源中


        public string BundleName { get { return m_BundleName; } internal set { m_BundleName = value; } }
        public int LoadDoneFrame { get { return m_LoadDoneFrame; } internal set { m_LoadDoneFrame = value; } }
        public int DestoryTime { get { return m_DestoryTime; } set { m_DestoryTime = value; } }
        public int BeginDestoryTime { get { return m_BeginDestoryTime; } internal set { m_BeginDestoryTime = value; } }
        public AssetBundle Bundle { get { return m_Bundle; } internal set { m_Bundle = value; } }
        public int ReferenceCount { get { return m_ReferenceCount; } internal set { m_ReferenceCount = value; } }
        public int RawReferenceCount { get { return m_RawReferenceCount; } internal set { m_RawReferenceCount = value; } }
        public bool IsAssetLoading { get { return m_IsAssetLoading; } set { m_IsAssetLoading = value; } }

        internal void UnLoad(bool unloadAllLoadedObjects = false)
        {
            m_Bundle.Unload(unloadAllLoadedObjects);
            m_Bundle = null;
            m_BundleName = null;
            m_ReferenceCount = 1;
        }
    }
}
