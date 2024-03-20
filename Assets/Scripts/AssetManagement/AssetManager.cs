using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace AssetManagement
{
    public partial class AssetManager : MonoBehaviour
    {
        public static bool LogEnabled = true;

        public static string[] UnLogFilter;

        public static float unloadtimetag = 0.0f; //清理的时间点
        //当达到此数的一半便启用清理
        public static int unloadBundleCountGC = 60;
        public static int unloadResCountGC = 5;
        public int unloadBundleCount { get; private set; }
        //本次清理有多个bundle被卸载了
        public int unloadResCount { get; private set; }
        //是否启动了卸载协程
        public bool isStartUnloadUnusedCoroutine { get; private set; }
        //是否正在异步卸载资源
        public bool isUnloadUnusedAssetsing { get; private set; }
        public UnityAction gcBeforeAction;
        public UnityAction gcAfterAction;
        private AssetLoaderOptions m_AssetLoaderOptions;
        public AssetManagement.AssetLoaderOptions AssetLoaderOptions { get { return m_AssetLoaderOptions; } }
        private Dictionary<string, XAssetBundle> m_XAssetBundleMap = new Dictionary<string, XAssetBundle>(50);
        private DictionaryExt<string, AssetInternalLoader> m_AssetLoader = new DictionaryExt<string, AssetInternalLoader>(50);
        public DictionaryExt<string, AssetInternalLoader> assetLoader { get { return m_AssetLoader; } }
        private Dictionary<string, AssetInternalLoader> m_FrameAddLoader = new Dictionary<string, AssetInternalLoader>(50);
        private List<string> m_TempList = new List<string>();

        public void Initialize(AssetLoaderOptions options)
        {
            this.m_AssetLoaderOptions = options;
            AssetBundleManager.onLoadComplete += OnAssetBundleLoadComplete;
            AssetBundleManager.onUnLoadComplete += OnAssetBundleUnLoadComplete;
        }


        void OnAssetBundleLoadComplete(XAssetBundle asset)
        {
            if (LogEnabled)
            {
                if (UnLogFilter != null)
                {
                    if (Array.IndexOf<string>(UnLogFilter, asset.BundleName) != -1)
                        XLogger.DEBUG(string.Format("<color=#7CCEFFFF>AssetManager::OnAssetBundleLoadComplete : {0}</color>", asset.BundleName));
                }
                else
                {
                    XLogger.DEBUG(string.Format("<color=#7CCEFFFF>AssetManager::OnAssetBundleLoadComplete : {0}</color>", asset.BundleName));
                }

            }

        }

        void OnAssetBundleUnLoadComplete(XAssetBundle asset)
        {
            if (LogEnabled)
            {
                if (UnLogFilter != null)
                {
                    if (Array.IndexOf<string>(UnLogFilter, asset.BundleName) != -1)
                        XLogger.DEBUG(string.Format("<color=#FF7CEBFF>AssetManager::OnAssetBundleUnLoadComplete : {0}</color>", asset.BundleName));
                }
                else
                {
                    XLogger.DEBUG(string.Format("<color=#FF7CEBFF>AssetManager::OnAssetBundleUnLoadComplete : {0}</color>", asset.BundleName));
                }
            }

            unloadBundleCount++;
            //UnloadUnusedAssetsInternal();
        }

        public Object GetCacheObject(string assetsName)
        {
            Object cacheObject = AssetCache.GetCacheRawObject(assetsName);
            if (cacheObject != null)
                return cacheObject;

            return null;
        }

        public AssetInternalLoader LoadBundleAsset<T>(string assetName)
        {
            return LoadBundleAsset(assetName, typeof(T));
        }

        public AssetInternalLoader LoadBundleAsset(string assetName, System.Type type)
        {

#if UNITY_EDITOR
            try
            {
                string fileName = System.IO.Path.GetFileNameWithoutExtension(assetName);
                if (string.IsNullOrEmpty(fileName))
                {
                    Debug.LogErrorFormat("AssetManager::LoadBundleAsset fileName is IsNullOrEmpty assetName={0}", assetName);
                    return null;
                }
            }
            catch (Exception e)
            {
                XLogger.ERROR_Format("assetName: {0}  error:{1}  ", assetName, e.ToString());
            }
#endif


            if (string.IsNullOrEmpty(assetName))
            {
                Debug.LogErrorFormat("AssetManager::LoadBundleAsset is IsNullOrEmpty assetName={0}", assetName);
                return null;
            }

            RecordAsset(assetName);

            string assetPath = GetAssetPathAlName(assetName);

            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogErrorFormat("AssetManager::LoadBundleAsset is IsNullOrEmpty 资源不存在 assetName={0} type={1}", assetName, type);
                return null;
            }

            AssetInternalLoader loader;
            if (m_FrameAddLoader.TryGetValue(assetPath, out loader))
            {
                loader.isPreLoad = false;
                return loader;
            }

            if (!m_AssetLoader.TryGetValue(assetPath, out loader))
            {
                if (IsEditorLoad(assetName))
                {
#if UNITY_EDITOR
                    loader = new EditorAssetLoader(assetPath, type);

#else
                    Debug.LogErrorFormat("不在编辑器模式使用了编辑器加载  assetPath={0}",assetPath);
#endif
                }
                else
                {
                    loader = new AssetInternalLoader(assetPath, type);
                }
                m_FrameAddLoader.Add(assetPath, loader);
            }
            loader.isPreLoad = false;

            return loader;
        }


        /// <summary>
        /// 用来卸载场景没有Raw的资源,实例化的对象或是Raw对象不要使用此接口不然将会导致引用计数错乱
        /// 正常使用  DestroyAsset 来卸载资源
        /// </summary>
        /// <param name="assetName"></param>
        //public void UnloadAsset(string assetName)
        //{
        //    string assetBundleName = GetAssetBundleName(assetName);
        //    //if (m_XAssetBundleMap.ContainsKey(assetBundleName))
        //    AssetBundleManager.Instance.UnloadAssetBundle(assetBundleName);
        //    //else
        //    //    Debug.LogErrorFormat("AssetManager::UnloadAsset m_XAssetBundleMap not exist assetName={0} assetBundleName={1}", assetName, assetBundleName);
        //}


        public IEnumerator UnloadScene(string assetName)
        {
            string assetBundleName = GetAssetBundleName(assetName);
            AssetBundleManager.Instance.UnloadScene(assetBundleName);
            yield return UnloadUnusedAssetsCoroutine();
        }

        void Update()
        {
            UpdateUnloading();
            UpdateLoading();
        }


        void UpdateLoading()
        {
            //等待资源卸载完成再开始加载
            //The file 'none' is corrupted! Remove it and launch unity again! [Position out of bounds!]
            if (isUnloadUnusedAssetsing) return;

            if (m_FrameAddLoader.Count > 0)
            {
                foreach (var item in m_FrameAddLoader)
                    m_AssetLoader.Add(item.Key, item.Value);
                m_FrameAddLoader.Clear();
            }

            if (m_AssetLoader.Count < 1) return;

            foreach (var item in m_AssetLoader.mList)
            {
                string key = item;
                AssetInternalLoader loader = m_AssetLoader[key];

                loader.Update();
                if (loader.onProgress != null)
                    loader.onProgress.Invoke(loader.GetProgress());

                if (loader.IsDone())
                {
                    if (string.IsNullOrEmpty(loader.Error))
                    {
                        if (loader.XAssetBundle == null)
                        {
                            if (!loader.IsEditor)
                                Debug.LogWarning(string.Format("key={0} m_XAssetBundle is null loader={1}", key, loader));
                        }
                        else
                        {
                            //if (string.IsNullOrEmpty(loader.XAssetBundle.BundleName))
                            //    Debug.LogWarning(string.Format("AssetManager::UpdateLoading() key={0} loader.XAssetBundle.BundleName is null loader={1}", key, loader));
                            //else if (!m_XAssetBundleMap.ContainsKey(loader.XAssetBundle.BundleName))
                            //    m_XAssetBundleMap.Add(loader.XAssetBundle.BundleName, loader.XAssetBundle);
                        }

                    }
                    m_TempList.Add(key);
                }
            }

            if (m_TempList.Count > 0)
            {
                foreach (var item in m_TempList)
                {

                    //XLogger.DEBUG_Format("AssetLoadComplete: {0}", item);

                    AssetInternalLoader load = null;
                    if (m_AssetLoader.TryGetValue(item, out load))
                    {
                        if (load.onProgress != null)
                        {
                            load.onProgress(1);
                            load.onProgress = null;
                        }

                        if (load.onComplete != null)
                        {
                            load.onComplete(load);
                            load.onComplete = null;
                        };

                        m_AssetLoader.Remove(item);
                    }

                }
                m_TempList.Clear();
            }


        }


        void UpdateUnloading()
        {
            //if (this.m_XAssetBundleMap.Count < 1) return;
            //foreach (var item in this.m_XAssetBundleMap)
            //{
            //    XAssetBundle xAssetBundle = item.Value;
            //    if (xAssetBundle.RawReferenceCount < 1)
            //    {
            //        m_TempList.Add(item.Key);
            //    }
            //}


            //if (m_TempList.Count > 0)
            //{
            //    foreach (var item in m_TempList)
            //    {
            //        AssetBundleManager.Instance.UnloadAssetBundle(item);
            //        this.m_XAssetBundleMap.Remove(item);
            //    }
            //    m_TempList.Clear();
            //}
        }

        //根据资源包名返回需要下载的资源
        public void GetNeedDownloadAssetBundle(string assetBundleName, ref List<string> result)
        {
            result.Clear();
            GetNeedDownloadAssetBundleInternal(assetBundleName, ref result, true);
        }

        //根据资源名返回需要下载的资源
        public void GetNeedDownloadAssetBundleByAssetName(string assetName, ref List<string> result)
        {
            if (string.IsNullOrEmpty(assetName)) return;
            string assetBundleName = GetAssetBundleName(assetName);
            if (!string.IsNullOrEmpty(assetBundleName))
                GetNeedDownloadAssetBundle(assetBundleName, ref result);
        }

        private void GetNeedDownloadAssetBundleInternal(string assetBundleName, ref List<string> result, bool dep = false)
        {
            if (IsNeedDownloadAssetBundle(assetBundleName))
                result.Add(assetBundleName);
            if (dep)
            {
                string[] dependence = GetAssetBundleDependence(assetBundleName);
                if (dependence != null)
                {
                    foreach (var depAssetBundleName in dependence)
                        GetNeedDownloadAssetBundleInternal(depAssetBundleName, ref result, false);
                }
            }
        }

        public bool IsNeedDownloadAssetBundle(string assetBundleName)
        {
            string fullPath = m_AssetLoaderOptions.GetAssetDownloadSavePath(assetBundleName);
            return !System.IO.File.Exists(fullPath) && !m_AssetLoaderOptions.GetAssetBundleIsBuildin(assetBundleName);
        }

        public string GetAssetBundleName(string assetName)
        {
            return this.m_AssetLoaderOptions.GetAssetBundleName(assetName);
        }

        public string GetAssetPathAlName(string assetName)
        {
            return this.m_AssetLoaderOptions.GetAssetPathAtName(assetName);
        }

        public int GetAssetBundleSizeName(string assetName)
        {
            string str = GetAssetBundleName(assetName);
            if (!string.IsNullOrEmpty(str))
                return GetAssetBundleSize(str);
            return 0;
        }

        public void RecordAsset(string assetName)
        {
            this.m_AssetLoaderOptions.RecordAsset(assetName);
        }

        public string[] GetAssetBundleDependence(string assetBundlename)
        {
            return this.m_AssetLoaderOptions.GetAssetBundleDependence(assetBundlename);
        }

        public int GetAssetBundleSize(string assetBundlename)
        {
            return this.m_AssetLoaderOptions.GetAssetBundleByteSize(assetBundlename);
        }

        public string GetAssetBundleMd5(string assetBundlename)
        {
            return this.m_AssetLoaderOptions.GetAssetBundleMd5(assetBundlename);
        }

        public string GetAssetBundleHash(string assetBundlename)
        {
            return this.m_AssetLoaderOptions.GetAssetBundleHash(assetBundlename).ToString();
        }

        public string GetAssetDownloadSavePath(string assetBundlename)
        {
            return this.m_AssetLoaderOptions.GetAssetDownloadSavePath(assetBundlename);
        }

        public uint GetAssetBundleCrc(string assetBundlename)
        {
            return this.m_AssetLoaderOptions.GetAssetBundleCrc(assetBundlename);
        }

        public bool IsEditorLoad(string asssetName)
        {
            return this.m_AssetLoaderOptions.IsEditorLoad(asssetName);
        }


        //卸载引用计数为0的资源
        public void UnloadUnusedAssets()
        {
            //foreach (var item in this.m_XAssetBundleMap)
            //{
            //    XAssetBundle xAssetBundle = item.Value;
            //    if (xAssetBundle.RawReferenceCount < 1)
            //    {
            //        m_TempList.Add(item.Key);
            //    }
            //}

            //if (m_TempList.Count > 0)
            //{
            //    foreach (var item in m_TempList)
            //    {
            //        AssetBundleManager.Instance.UnloadAssetBundle(item);
            //        this.m_XAssetBundleMap.Remove(item);
            //    }
            //    m_TempList.Clear();
            //}
        }



        public void UnloadAllAssets()
        {


            UnloadUnusedAssets();

            //if (isStartUnloadUnusedCoroutine) return;
            //isStartUnloadUnusedCoroutine = true;
            //StartCoroutine(UnloadUnusedAssetsCoroutine());
        }

        //public IEnumerator UnloadUnusedAssets(bool force = false)
        //{
        //    yield return null;
        //    //yield return UnloadUnusedAssetsInternal(force);
        //}

        public void UnloadUnusedAssetsCount()
        {
            unloadResCount++;
        }

        IEnumerator UnloadUnusedAssetsInternal(bool force = false)
        {
            if (isStartUnloadUnusedCoroutine)
            {
                while (isStartUnloadUnusedCoroutine)
                {
                    yield return null;
                }
                yield break;
            }

            isStartUnloadUnusedCoroutine = true;
            yield return UnloadUnusedAssetsCoroutine();
        }

        public IEnumerator UnloadUnusedAssetsCoroutine()
        {
            yield return null;
//            //注意资源加载中是不能卸载资源的
//            while (m_AssetLoader.Count > 0) yield return null;
//            isUnloadUnusedAssetsing = true;
//            isStartUnloadUnusedCoroutine = true;
//            Debug.Log("<color=green>AssetManager.UnloadUnusedAssetsCoroutine</color>");

//            XLogger.DEBUG(string.Format("<color=#3EFF00FF>AssetManager::UnloadUnusedAssetsCoroutine() start. {0}</color>", m_AssetLoader.Count));


//            if (gcBeforeAction != null) gcBeforeAction.Invoke();

//            System.GC.Collect();

//#if UNITY_EDITOR
//            UnityEditor.EditorUtility.UnloadUnusedAssetsImmediate(true);
//#endif

//            yield return Resources.UnloadUnusedAssets();

//            System.GC.Collect();
//            //if (LogEnabled)

//            //if (unloadBundleCount >= unloadBundleCountGC)
//            {
//                if (LogEnabled)
//                    XLogger.DEBUG(string.Format("<color=#3EFF00FF>AssetManager::UnloadUnusedAssetsCoroutine() GC {0}</color>", unloadBundleCount));
//                //unloadBundleCount = 0;
//                //System.GC.Collect();
//            }

//            XLogger.DEBUG("<color=#3EFF00FF>AssetManager::UnloadUnusedAssetsCoroutine() end.</color>");
//            if (gcAfterAction != null) gcAfterAction.Invoke();
//            isUnloadUnusedAssetsing = false;
//            isStartUnloadUnusedCoroutine = false;
//            //unloadtimetag = Time.realtimeSinceStartup;
        }

#if XConfigMode
        public List<string> GetConfigImages()
        {
            return this.m_AssetLoaderOptions.GetConfigImages();
        }
#endif

        private static AssetManager m_Instance;
        public static AssetManager Instance
        {
            get
            {
                if (m_Instance != null)
                    return m_Instance;
                GameObject go = new GameObject("AssetManager");
                go.hideFlags = HideFlags.HideInHierarchy;
                m_Instance = go.AddComponent<AssetManager>();
                go.AddComponent<AssetBundleManager>();
                go.AddComponent<AssetDownloadManager>();
#if UNITY_EDITOR //不是编辑器模式运行不需要判断
                if (Application.isPlaying)
#endif
                    Object.DontDestroyOnLoad(go);
                return m_Instance;
            }
        }
    }
}

