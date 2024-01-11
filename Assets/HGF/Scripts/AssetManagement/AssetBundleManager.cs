using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AssetManagement
{
    public class AssetBundleManager : MonoBehaviour
    {
        public static System.Action<XAssetBundle> onLoadComplete;
        public static System.Action<XAssetBundle> onUnLoadComplete;

        public class AssetBundleLoadInfo
        {
            //加载耗时
            public float loadTime;
            //下载耗时
            public float downloadTime;
            //包名字
            public string assetBundleName;
        }
        private Dictionary<string, AssetBundleLoadInfo> m_XAssetBundleLoadInfos = new Dictionary<string, AssetBundleLoadInfo>();
        public Dictionary<string, AssetBundleLoadInfo> XAssetBundleLoadInfos { get { return m_XAssetBundleLoadInfos; } }

        private AssetBundleComparer m_AssetBundleComparer = new AssetBundleComparer();


        //加载完的AB包
        private Dictionary<string, XAssetBundle> m_XAssetBundleMap = new Dictionary<string, XAssetBundle>();
        public Dictionary<string, XAssetBundle> XAssetBundleMap { get { return m_XAssetBundleMap; } }
        private Dictionary<string, int> m_XAssetBundleRefCountMap = new Dictionary<string, int>();
        public Dictionary<string, int> XAssetBundleRefCountMap { get { return m_XAssetBundleRefCountMap; } }

        //AB依赖缓存
        private Dictionary<string, string[]> m_Dependence = new Dictionary<string, string[]>();

        //正在加载中的AB包
        private DictionaryExt<string, AssetBundleLoader> m_AssetLoading = new DictionaryExt<string, AssetBundleLoader>();
        public DictionaryExt<string, AssetBundleLoader> AssetLoading { get { return m_AssetLoading; } }

        //错误列表
        private Dictionary<string, string> m_Error = new Dictionary<string, string>();
        public Dictionary<string, string> Error { get { return m_Error; } }

        //加载反引用,每个ab包加进来被谁引用进来的
        private Dictionary<string, List<string>> m_LoadReverseRefMap = new Dictionary<string, List<string>>(300);
        public Dictionary<string, List<string>> loadReverseRefMap { get { return m_LoadReverseRefMap; } }

        //所有加载的主ab包名(一般此包是主动加载不是被动依赖的)
        private HashSet<string> m_LoadRootBundleNames = new HashSet<string>();
        public HashSet<string> loadRootBundleNames { get { return m_LoadRootBundleNames; } }


        private List<string> m_TempList = new List<string>();
        private bool m_SrotTag = false;

        public AssetBundleLoadInfo GetAssetBundleLoadInfo(string assetBundleName)
        {
            AssetBundleLoadInfo info;
            if (!m_XAssetBundleLoadInfos.TryGetValue(assetBundleName, out info))
            {
                info = new AssetBundleLoadInfo();
                info.assetBundleName = assetBundleName;
                m_XAssetBundleLoadInfos.Add(assetBundleName, info);
            }
            return info;
        }

        public XAssetBundle GetTryXAssetBundle(string assetBundleName, out string error)
        {
            error = string.Empty;
            //加载中的主ab包是不能够被自动卸载的
            if (XAssetBundleMap.ContainsKey(assetBundleName))
                XAssetBundleMap[assetBundleName].BeginDestoryTime = -1;
            if (ExamineAssetBundle(assetBundleName, out error) && XAssetBundleMap.ContainsKey(assetBundleName))
            {
                return XAssetBundleMap[assetBundleName];
            }
            return null;
        }
        public int GetReferenceCount(string assetBundleName) { return m_XAssetBundleRefCountMap.ContainsKey(assetBundleName) ? m_XAssetBundleRefCountMap[assetBundleName] : 0; }

        public void LoadAssetBundle(string assetBundleName)
        {

            CalcReverseRefMap(assetBundleName);

            if (DependenceRectify(assetBundleName))
                return;

            if (this.LoadAssetBundleInternal(assetBundleName, assetBundleName))
            {
                this.LoadDependence(assetBundleName);
            }
        }

        void CalcAssetBundleRefCount(string assetBundleName, int v)
        {
            if (!m_XAssetBundleRefCountMap.ContainsKey(assetBundleName))
                m_XAssetBundleRefCountMap.Add(assetBundleName, v);
            else
                m_XAssetBundleRefCountMap[assetBundleName] += v;
        }

        void CalcReverseRefMap(string assetBundleName, bool isRemove = false)
        {
            if (!isRemove)
            {
                if (!m_LoadRootBundleNames.Contains(assetBundleName)) m_LoadRootBundleNames.Add(assetBundleName);


                string[] dependence = GetDependence(assetBundleName);
                foreach (var item in dependence)
                {
                    //反记录引用
                    List<string> revRefList;
                    if (!m_LoadReverseRefMap.TryGetValue(item, out revRefList))
                    {
                        revRefList = ListPool<string>.Get();
                        revRefList.Add(assetBundleName);
                        m_LoadReverseRefMap.Add(item, revRefList);
                    }
                    else
                    {
                        if (!revRefList.Contains(assetBundleName)) revRefList.Add(assetBundleName);
                    }
                }
            }
            else
            {

                if (m_LoadRootBundleNames.Contains(assetBundleName)) m_LoadRootBundleNames.Remove(assetBundleName);
                string[] dependence = GetDependence(assetBundleName);
                foreach (var item in dependence)
                {
                    //移除反记录
                    List<string> revRefList;
                    if (m_LoadReverseRefMap.TryGetValue(item, out revRefList) && revRefList.Contains(assetBundleName))
                    {
                        revRefList.Remove(assetBundleName);
                        if (revRefList.Count <= 0)
                        {
                            ListPool<string>.Release(m_LoadReverseRefMap[item]);
                            m_LoadReverseRefMap.Remove(item);
                        }
                    }
                }
            }
        }


        //是否有异常的依赖被删除
        private bool DependenceRectify(string assetBundleName)
        {
            bool result = true;
            if (!m_XAssetBundleMap.ContainsKey(assetBundleName))
                return false;
            string[] dependence = GetDependence(assetBundleName);

            if (dependence.Length == 0) return true;

            foreach (var depName in dependence)
            {
                if (m_AssetLoading.ContainsKey(depName)) continue;
                if (m_Error.ContainsKey(depName)) continue;
                //若依赖的ab被异常原因卸载
                if (!m_XAssetBundleMap.ContainsKey(depName))
                {
                    this.LoadAssetBundleInternal(depName, assetBundleName);
                }
            }
            return result;
        }


        private void LoadDependence(string assetBundleName)
        {
            string[] dependence = GetDependence(assetBundleName);
            if (dependence != null)
            {
                foreach (var item in dependence)
                    this.LoadAssetBundleInternal(item, assetBundleName);
            }

        }

        private bool LoadAssetBundleInternal(string assetBundleName, string parent)
        {
            if (string.IsNullOrEmpty(assetBundleName))
                return false;


            CalcAssetBundleRefCount(assetBundleName, assetBundleName == parent ? 0 : 1);


            if (m_XAssetBundleMap.ContainsKey(assetBundleName))
            {
                if (m_XAssetBundleMap[assetBundleName].Bundle == null)
                    Debug.LogWarningFormat("AssetBundleManager::LoadAssetBundleInternal m_XAssetBundleMap Bundle is null assetBundleName:{0}", assetBundleName);

                //如果正在准备卸载则直接重置
                m_XAssetBundleMap[assetBundleName].BeginDestoryTime = -1;
                return false;
            }


            if (assetBundleName == parent)
                WriteLog("Load:{0} {1}", parent, m_XAssetBundleRefCountMap[assetBundleName]);
            else
                WriteLog("              Load:{0} {1}", assetBundleName, m_XAssetBundleRefCountMap[assetBundleName]);


            if (m_AssetLoading.ContainsKey(assetBundleName))
                return false;

            if (m_Error.ContainsKey(assetBundleName))
                return false;

            AssetBundleLoader assetBundleLoader = new AssetBundleLoader(assetBundleName);
            m_AssetLoading.Add(assetBundleName, assetBundleLoader);
            m_SrotTag = true;


            return true;
        }


        public void UnloadAllAssetBundle()
        {
            m_TempList.Clear();
            m_Error.Clear();
            //foreach (var item in m_XAssetBundleMap)
            //{
            //    if (item.Value.DestoryTime != -1)
            //    {
            //        item.Value.UnLoad();
            //        m_TempList.Add(item.Key);
            //    }
            //}

            //foreach (var item in m_TempList)
            //{
            //    m_XAssetBundleMap.Remove(item);
            //}

            m_TempList.Clear();
        }


        public void UnloadAssetBundle(string assetBundleName, bool immediately = false)
        {
            CalcReverseRefMap(assetBundleName, true);
            if (this.UnloadAssetBundleInternal(assetBundleName, assetBundleName, immediately))
            {
                this.UnloadDependence(assetBundleName, immediately);
            }
        }

        private void UnloadDependence(string assetBundleName, bool immediately = false)
        {
            string[] dependence = GetDependence(assetBundleName);
            if (dependence != null)
            {
                foreach (var item in dependence)
                {
                    this.UnloadAssetBundleInternal(item, assetBundleName, immediately);
                }

            }

        }

        private bool UnloadAssetBundleInternal(string assetBundleName, string parent, bool immediately = false)
        {
            CalcAssetBundleRefCount(assetBundleName, -1);

            XAssetBundle asset;
            if (!m_XAssetBundleMap.TryGetValue(assetBundleName, out asset))
            {
                return false;
            }

            if (immediately && GetReferenceCount(assetBundleName) <= 0)
                asset.BeginDestoryTime = (int)Time.time;

            WriteLog("          UnloadAssetBundleInternal: {0} {1}", assetBundleName, GetReferenceCount(assetBundleName));

            return true;
        }


        public void UnloadScene(string assetBundleName)
        {
            if (string.IsNullOrEmpty(assetBundleName)) return;
            UnloadAssetBundle(assetBundleName, true);
            string[] deps = GetDependence(assetBundleName);
            XAssetBundle xAssetBundle;


            if (XAssetBundleMap.TryGetValue(assetBundleName, out xAssetBundle))
            {
                AssetCache.UnloadCacheByAssetBudnle(xAssetBundle);
                m_XAssetBundleMap.Remove(assetBundleName);
                xAssetBundle.UnLoad(true);
            }



            foreach (var item in deps)
            {
                if (XAssetBundleMap.TryGetValue(item, out xAssetBundle))
                {

                    if (xAssetBundle.IsAssetLoading) continue;
                    if (GetReferenceCount(item) <= 0 &&
                        xAssetBundle.DestoryTime != -1 &&
                        xAssetBundle.RawReferenceCount <= 0)
                    {

                        if (m_LoadReverseRefMap.ContainsKey(item))
                            continue;


                        AssetCache.UnloadCacheByAssetBudnle(xAssetBundle);
                        m_XAssetBundleMap.Remove(item);
                        xAssetBundle.UnLoad(true);
                    }
                }

            }
        }




        public void UnloadUnusedObject()
        {
            //m_TempList.Clear();
            //List<string> dontUnload = dontUnloadList;
            //foreach (var item in m_XAssetBundleMap)
            //{
            //    XAssetBundle asset = item.Value;
            //    if (asset.ReferenceCount <= 0 && asset.DestoryTime != -1 && asset.RawReferenceCount <= 0)
            //    {
            //        if (!string.IsNullOrEmpty(asset.BundleName))
            //        {

            //            //先临时只卸载场景
            //            if (asset.BundleName.StartsWithEx("02/art/env/scenes"))
            //            {
            //                m_TempList.Add(item.Key);
            //            }


            //            //if (dontUnload != null && !dontUnload.Contains(asset.BundleName))
            //            //    m_TempList.Add(item.Key);
            //        }
            //        else
            //        {
            //            m_TempList.Add(item.Key);
            //        }
            //    }
            //}

            //foreach (var key in m_TempList)
            //{
            //    XAssetBundle asset = m_XAssetBundleMap[key];
            //    if (onUnLoadComplete != null)
            //        onUnLoadComplete.Invoke(asset);
            //    if (m_XAssetBundleRefCountMap.ContainsKey(key))
            //        m_XAssetBundleRefCountMap.Remove(key);
            //    asset.UnLoad();
            //    m_XAssetBundleMap.Remove(key);
            //}
            //m_TempList.Clear();
        }


        //private int unloadCount = 0;
        void CheckUnloadUnusedAssetBundle()
        {
            foreach (var item in m_XAssetBundleMap)
            {
                if (item.Value.IsAssetLoading) continue;
                if (GetReferenceCount(item.Key) <= 0 &&
                    item.Value.DestoryTime != -1 &&
                    item.Value.RawReferenceCount <= 0)
                {


                    if (m_LoadReverseRefMap.ContainsKey(item.Key))
                        continue;


                    if (item.Value.BeginDestoryTime == -1)
                    {
                        AssetCache.CheckAssetBundleRawObjectRef(item.Value);
                        return;
                    }


                    if (Time.time < item.Value.BeginDestoryTime)
                        continue;


                    WriteLog("UnLoad: {0}", item.Key);


                    if (m_LoadRootBundleNames.Contains(item.Key))
                    {
                        UnloadAssetBundle(item.Key, true);

                        //if (unloadCount++ >= 10)
                        //{
                        //    unloadCount = 0;
                        //    Resources.UnloadUnusedAssets();
                        //    WriteLog("Resources.UnloadUnusedAssets: {0}", "");
                        //}
                    }
                    //XLogger.WARNING_Format("Unload:{0}", item.Key);

                    //if (item.Value.BeginDestoryTime > -1)


                    //UnloadAssetBundle(item.Key);
                    AssetCache.UnloadCacheByAssetBudnle(item.Value);
                    m_XAssetBundleMap.Remove(item.Key);
                    item.Value.UnLoad(true);


                    break;
                }
            }
        }



        public float GetAssetBundleAllDependenceProgress(string assetBundleName)
        {
            if (string.IsNullOrEmpty(assetBundleName))
                return 0.0f;

            float count = GetAssetBundleProgress(assetBundleName);
            float total = 1f;
            string[] dependence = GetDependence(assetBundleName);
            if (dependence != null)
            {
                total += dependence.Length;
                foreach (var depAssetBundleName in dependence)
                    count += GetAssetBundleProgress(depAssetBundleName);
            }

            count = Mathf.Clamp01((float)count / (float)total);

            return count;
        }

        public float GetAssetBundleProgress(string assetBundleName)
        {
            float count = 0;
            if (m_XAssetBundleMap.ContainsKey(assetBundleName) || m_Error.ContainsKey(assetBundleName))
                count++;
            else if (m_AssetLoading.ContainsKey(assetBundleName))
                count += m_AssetLoading[assetBundleName].GetProgress();
            return count;
        }

        public int GetAssetBundleReceivedBytes(string assetBundleName, bool dep = false)
        {
            int size = 0;
            if ((m_XAssetBundleMap.ContainsKey(assetBundleName) || m_Error.ContainsKey(assetBundleName)))
                size = GetAssetBundleSize(assetBundleName);
            else if (m_AssetLoading.ContainsKey(assetBundleName))
            {
                if (m_AssetLoading[assetBundleName].isDownloaderType)
                {
                    size = m_AssetLoading[assetBundleName].GetDownloadReceivedBytes();
                }
            }

            if (dep)
            {
                string[] dependence = GetDependence(assetBundleName);
                if (dependence != null)
                {
                    foreach (var depAssetBundleName in dependence)
                        size += GetAssetBundleReceivedBytes(depAssetBundleName, false);
                }
            }

            return size;
        }


        public int GetAssetBundleSize(string assetBundleName, bool dep = false)
        {
            int size = AssetManager.Instance.GetAssetBundleSize(assetBundleName);
            if (dep)
            {
                string[] dependence = GetDependence(assetBundleName);
                if (dependence != null)
                {
                    foreach (var depAssetBundleName in dependence)
                        size += GetAssetBundleSize(depAssetBundleName, false);
                }
            }
            return size;
        }


        //包是否在下载中
        //dep 是否检查依赖
        public bool IsDownloading(string assetBundleName, bool dep = false)
        {
            bool downloading = false;
            if (!dep)
            {
                AssetBundleLoader abl;
                if (m_AssetLoading.TryGetValue(assetBundleName, out abl))
                {
                    downloading = abl.isDownloaderType;
                }
                return downloading;
            }

            string[] dependence = GetDependence(assetBundleName);
            if (dependence != null)
            {
                AssetBundleLoader abl;
                foreach (var depName in dependence)
                {
                    if (m_AssetLoading.TryGetValue(depName, out abl))
                    {
                        downloading = abl.isDownloaderType;
                        if (downloading) break;
                    }
                }
            }

            return downloading;
        }

        private static StringBuilder s_StringBuilder = new StringBuilder();
        public string GetDownloadInfoToString(string assetBundleName)
        {
            s_StringBuilder.Length = 0;

            if (IsDownloading(assetBundleName))
                s_StringBuilder.AppendFormat("{0}  <color=#00ff00>{1}</color>\n", GetAssetBundleProgress(assetBundleName), assetBundleName);
            else
                s_StringBuilder.AppendFormat("{0}  {1}\n", GetAssetBundleProgress(assetBundleName), assetBundleName);

            string[] dependence = GetDependence(assetBundleName);
            if (dependence != null)
            {
                foreach (var depName in dependence)
                {
                    if (IsDownloading(depName))
                        s_StringBuilder.AppendFormat("{0}  <color=#00ff00>{1}</color>\n", GetAssetBundleProgress(depName), depName);
                    else
                        s_StringBuilder.AppendFormat("{0}  {1}\n", GetAssetBundleProgress(depName), depName);
                }
            }

            return s_StringBuilder.ToString();
        }

        public string GetDownloadSizeInfoToString(string assetBundleName)
        {
            s_StringBuilder.Length = 0;
            int rectSize = GetAssetBundleReceivedBytes(assetBundleName);
            int totalSize = GetAssetBundleSize(assetBundleName);
            if (IsDownloading(assetBundleName))
                s_StringBuilder.AppendFormat("{0} / {1}  <color=#00ff00>{2}</color>\n", XUtility.FormatBytes(rectSize), XUtility.FormatBytes(totalSize), assetBundleName);
            else
                s_StringBuilder.AppendFormat("{0} / {1}  {2}\n", XUtility.FormatBytes(totalSize), XUtility.FormatBytes(totalSize), assetBundleName);

            string[] dependence = GetDependence(assetBundleName);
            if (dependence != null)
            {
                foreach (var depName in dependence)
                {
                    rectSize = GetAssetBundleReceivedBytes(depName);
                    totalSize = GetAssetBundleSize(depName);
                    if (IsDownloading(depName))
                        s_StringBuilder.AppendFormat("{0} / {1}  <color=#00ff00>{2}</color>\n", XUtility.FormatBytes(rectSize), XUtility.FormatBytes(totalSize), depName);
                    else
                        s_StringBuilder.AppendFormat("{0} / {1}  {2}\n", XUtility.FormatBytes(totalSize), XUtility.FormatBytes(totalSize), depName);
                }
            }

            return s_StringBuilder.ToString();
        }


        public string[] GetDependence(string assetBundleName)
        {
            string[] dependence = null;
            if (!m_Dependence.TryGetValue(assetBundleName, out dependence))
            {
                dependence = AssetManager.Instance.GetAssetBundleDependence(assetBundleName);
                m_Dependence.Add(assetBundleName, dependence);
            }
            return dependence;
        }


        public bool ExamineAssetBundle(string assetBundleName, out string error)
        {
            error = string.Empty;

            if (string.IsNullOrEmpty(assetBundleName))
                return false;

            if (m_AssetLoading.ContainsKey(assetBundleName))
                return false;

            if (m_Error.ContainsKey(assetBundleName))
            {
                m_Error.TryGetValue(assetBundleName, out error);
                return false;
            }

            string[] dependence = GetDependence(assetBundleName);
            foreach (var depBundleName in dependence)
            {
                if (string.IsNullOrEmpty(depBundleName))
                    continue;

                if (m_AssetLoading.ContainsKey(depBundleName))
                    return false;

                if (m_Error.ContainsKey(depBundleName))
                {
                    error = string.Format("dependence load error: {0}  des={1}", depBundleName, m_Error[depBundleName]);
                    return false;
                }

                //if (!m_XAssetBundleMap.ContainsKey(depBundleName))
                //    return false;
            }
            return true;
        }


        void Update()
        {

            if (m_AssetLoading.Count <= 0)
                return;

            int count = 0;
            if (m_SrotTag)
            {
                m_AssetLoading.Sort();
                m_SrotTag = false;
            }
            foreach (var item in m_AssetLoading.mList)
            {
                string assetBundleName = item;
                AssetBundleLoader loader = m_AssetLoading[assetBundleName];
                loader.Update();
                if (loader.IsDone())
                {
                    AssetBundle assetBundle = loader.assetBundle;
                    if (assetBundle == null)
                    {
                        if (!m_Error.ContainsKey(assetBundleName))
                            m_Error.Add(assetBundleName, loader.Error);
                    }
                    else
                    {
                        XAssetBundle asset = new XAssetBundle();
                        asset.BundleName = assetBundleName;
                        asset.Bundle = assetBundle;
                        asset.LoadDoneFrame = Time.frameCount;
                        //asset.ReferenceCount = GetReferenceCount(assetBundleName); //加载完成赋予加载过程中被其它ab计数的数量
                        if (assetBundle.isStreamedSceneAssetBundle)
                            CalcAssetBundleRefCount(assetBundleName, 1);//场景默认加1引用不然被会自动检查时销毁掉
                        if (!m_XAssetBundleMap.ContainsKey(assetBundleName))
                            m_XAssetBundleMap.Add(assetBundleName, asset);

                        if (onLoadComplete != null)
                            onLoadComplete.Invoke(asset);
                    }

                    m_TempList.Add(assetBundleName);
                }

                if (MaxLoadCount <= 0)
                    continue;

                if (++count >= MaxLoadCount)
                    break;
            }

            if (m_TempList.Count > 0)
            {
                foreach (var item in m_TempList)
                    m_AssetLoading.Remove(item);

                m_TempList.Clear();
            }
        }

        void LateUpdate()
        {
            CheckUnloadUnusedAssetBundle();
        }




        public int MaxLoadCount
        {
            get { return AssetManager.Instance.AssetLoaderOptions.GetAssetBundleLoadMaxNum(); }
        }

        public List<string> dontUnloadList
        {
            get { return AssetManager.Instance.AssetLoaderOptions.GetDontUnloadList(); }
        }

        class AssetBundleComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                AssetBundleLoader a = AssetBundleManager.Instance.AssetLoading[x];
                AssetBundleLoader b = AssetBundleManager.Instance.AssetLoading[y];

                return b.priority.CompareTo(a.priority);
            }
        }


        const string logsPath = "logs/assetload.log";
        public static void WriteLog(string format, params object[] s)
        {
            //#if !(!UNITY_EDITOR && UNITY_STANDALONE)
            if (testlog) XLogger.RecordLog(Path.Combine(AssetDefine.ExternalSDCardsPath, logsPath), string.Format(format, s));
            //#endif
        }


        public static bool testlog = false;
        private static AssetBundleManager m_Instance;
        public static AssetManagement.AssetBundleManager Instance { get { return m_Instance; } }
        public void Awake()
        {
            m_Instance = this;
            //if (Debug.isDebugBuild)
            if (testlog) XLogger.ClearRecordLog(Path.Combine(AssetDefine.ExternalSDCardsPath, logsPath));
        }

    }
}
