using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace AssetManagement
{
    public class AssetInternalLoader : AssetLoader
    {
        private const string NonExist = "Non-existe";

        private bool m_ABLoading = false;
        private bool m_IsEditor = false;
        protected bool m_IsDone = false;
        protected string m_Error;
        protected string m_AssetName;
        public string assetName { get { return m_AssetName; } }
        protected string m_AssetBundleName;
        protected System.Type m_AssetType;
        private XAssetBundle m_XAssetBundle;
        protected AsyncOperation m_AsyncOperation;
        public System.Action<float> onProgress { get; set; }
        public System.Action<AssetInternalLoader> onComplete { get; set; }
        public XAssetBundle XAssetBundle { get { return m_XAssetBundle; } }

        private bool m_IsDownload = false;
        private int m_TotalByteSize = -1;
        private int m_TotalDownloadByteSize = -1;
        private int m_LastDownloadSize = 0;
        private List<string> m_DownloadFiles;

        public bool IsEditor { get { return m_IsEditor; } set { m_IsEditor = value; } }
        public bool isSceneLoad { get; set; }
        public LoadSceneMode loadSceneMode { get; set; }
        public bool isPreLoad { get; set; } //是否是预加载
        private Stopwatch m_Stopwatch;
        public float startloadTime;
        public AssetInternalLoader(string assetName)
            : this(assetName, typeof(Object))
        {

        }


        public AssetInternalLoader(string assetName, System.Type assetType)
        {
            this.m_AssetName = assetName;
            this.m_AssetType = assetType;

            if (assetName == NonExist) this.m_Error = NonExist;

            m_AssetBundleName = GetAssetBundleName();
            m_Stopwatch = Stopwatch.StartNew();
        }


        public override void Update()
        {
            if (IsDone() && m_DownloadFiles != null)
            {
                ListPool<string>.Release(m_DownloadFiles);
                m_DownloadFiles = null;
            }


            if (asyncOperation != null)
            {
                m_IsDone = asyncOperation.isDone;

                //预加载缓存资源但不增加引用计数
                if (m_IsDone && isPreLoad)
                {
                    GetRawObjectNotRef<Object>();
                    ////预加载资源30秒没用便卸载
                    //if (this.m_XAssetBundle != null)
                    //    this.m_XAssetBundle.BeginDestoryTime = (int)Time.time + 30;
                }
                return;
            }


            //if (AssetCache.ContainsRawObject(this.m_AssetName))
            //{
            //    m_AssetBundleName = GetAssetBundleName();
            //    m_XAssetBundle = AssetBundleManager.Instance.GetTryXAssetBundle(m_AssetBundleName, out m_Error);
            //    m_IsDone = true;
            //}


            if (m_IsDone)
                return;

            if (!m_ABLoading)
            {
                m_LastDownloadSize = 0;
                m_ABLoading = true;
                m_AssetBundleName = GetAssetBundleName();
                AssetBundleManager.WriteLog("LoadAsset:{0}", assetName);
                if (!string.IsNullOrEmpty(m_AssetBundleName))
                    AssetBundleManager.Instance.LoadAssetBundle(m_AssetBundleName);
                else
                {
                    m_Error = string.Format("AssetInternalLoader::Update  m_AssetBundleName IsNullOrEmpty {0}", this.m_AssetName);
                    if (Debug.isDebugBuild)
                        XLogger.WARNING_Format("<color=red>AssetInternalLoader::Update  m_AssetBundleName IsNullOrEmpty <color=yellow>{0}</color>  资源没有! </color>", this.m_AssetName);
                }
            }


            if (!string.IsNullOrEmpty(m_Error))
            {
                m_IsDone = true;
                return;
            }


            if (m_ABLoading)
            {
                m_XAssetBundle = AssetBundleManager.Instance.GetTryXAssetBundle(m_AssetBundleName, out m_Error);
                if (m_XAssetBundle != null)
                {
                    if (m_XAssetBundle.Bundle.isStreamedSceneAssetBundle)
                    {
                        if (isPreLoad)
                        {
                            m_IsDone = true;
                            return;

                        }
                        m_XAssetBundle.IsAssetLoading = true;

                        Debug.Log("场景加载 m_AsyncOperation");

                        //场景加载
                        m_AsyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(m_XAssetBundle.Bundle.GetAllScenePaths()[0], loadSceneMode);
                    }
                    else if (m_XAssetBundle.Bundle.Contains(this.m_AssetName))
                    {
                        m_XAssetBundle.IsAssetLoading = true;
                        m_AsyncOperation = m_XAssetBundle.Bundle.LoadAssetAsync(this.m_AssetName, this.m_AssetType);
                    }
                    else
                    {
                        m_Error = string.Format("XAssetBundle Contains false name={0}", this.m_AssetName);
                    }

                    startloadTime = Time.realtimeSinceStartup;
                }
                return;
            }
        }

        private string GetAssetBundleName()
        {
            return AssetManager.Instance.GetAssetBundleName(this.m_AssetName);
        }


        public bool IsDownloading()
        {
            if (!m_ABLoading || string.IsNullOrEmpty(m_AssetBundleName)) return false;
            if (!m_IsDownload)
                m_IsDownload = AssetBundleManager.Instance.IsDownloading(m_AssetBundleName, true);
            return m_IsDownload;
        }

        public override bool IsDone()
        {
            if (!string.IsNullOrEmpty(Error))
                return true;

            if (asyncOperation != null)
            {
                if (asyncOperation.isDone)
                {
                    m_XAssetBundle.IsAssetLoading = false;
                    AssetManager.AssetLoadProfilerInfo alpi = AssetManager.Instance.GetProfilerInfo(assetName);
                    if (alpi.loadTime <= 0)
                    {
                        alpi.loadTime = Time.realtimeSinceStartup - startloadTime;
                        AssetBundleManager.AssetBundleLoadInfo abli = AssetBundleManager.Instance.GetAssetBundleLoadInfo(m_AssetBundleName);
                        alpi.abloadTime = abli.loadTime;
                        alpi.downloadTime = abli.downloadTime;
                    }
                }
                return asyncOperation.isDone;
            }

            if (m_XAssetBundle != null && m_XAssetBundle.Bundle != null && m_XAssetBundle.Bundle.isStreamedSceneAssetBundle)
                return true;

            return this.m_IsDone;
        }

        override public float GetProgress()
        {
            if (this.IsDone())
                return 1f;

            float progress = AssetBundleManager.Instance.GetAssetBundleAllDependenceProgress(m_AssetBundleName);

            if (asyncOperation != null)
                progress = (progress + asyncOperation.progress);

            //if (!isSceneLoad)
            progress *= 0.5f;

            return Mathf.Clamp01(progress);
        }


        public int GetTotalBytes()
        {
            if (m_TotalByteSize == -1)
                m_TotalByteSize = AssetBundleManager.Instance.GetAssetBundleSize(m_AssetBundleName, true);
            return m_TotalByteSize;
        }


        //总共需要下载的字节
        public int GetDownloadTotalBytesSize()
        {
            if (m_TotalDownloadByteSize == -1)
            {
                m_TotalDownloadByteSize = 0;
                foreach (var donwloadName in downloadFiles)
                    m_TotalDownloadByteSize += AssetBundleManager.Instance.GetAssetBundleSize(donwloadName);
            }
            return m_TotalDownloadByteSize;
        }


        //当前下载的字节
        public int GetDownloadReceivedBytesSize()
        {
            int tsize = 0;
            foreach (var donwloadName in downloadFiles)
                tsize += AssetBundleManager.Instance.GetAssetBundleReceivedBytes(donwloadName, false);
            if (tsize < m_LastDownloadSize)
                tsize = m_LastDownloadSize;
            else
                m_LastDownloadSize = tsize;
            return tsize;
        }

        //需要下载的文件数
        public int GetNeedDownloadFileCount()
        {
            return downloadFiles.Count;
        }

        //已经下载的文件数
        public int GetDownloadReceivedFileCount()
        {
            int count = 0;
            foreach (var donwloadName in downloadFiles)
                if (AssetManager.Instance.IsNeedDownloadAssetBundle(donwloadName))
                    count++;
            return downloadFiles.Count - count;
        }

        //当前正在下载的文件
        public string GetCurDownloadFile()
        {
            string result = string.Empty;
            foreach (var donwloadName in downloadFiles)
                if (AssetBundleManager.Instance.IsDownloading(donwloadName))
                {
                    result = donwloadName;
                    break;
                }
            return result;
        }

        //每秒下载的字节
        public int GetSecondByte()
        {
            return (int)(GetDownloadReceivedBytesSize() / m_Stopwatch.Elapsed.TotalSeconds);
        }


        protected virtual Object GetRawObject()
        {
            Object cacheObject = AssetCache.GetCacheRawObject(this.m_AssetName);
            if (cacheObject != null)
                return cacheObject;

            AssetBundleRequest abr = this.asyncOperation != null ? this.asyncOperation as AssetBundleRequest : null;

            Object asset = abr != null ? abr.asset : null;

            if (this.m_XAssetBundle == null || asset == null)
                return null;

            return asset;
        }


        public virtual T Instantiate<T>(Transform parent = null) where T : Object
        {
            Object asset = GetRawObject();
            if (asset == null)
                return default(T);

            float stime = Time.realtimeSinceStartup;
            T obj = AssetCache.InstantiateObject(this.m_XAssetBundle, asset, this.m_AssetName, parent) as T;
            AssetManager.AssetLoadProfilerInfo alpi = AssetManager.Instance.GetProfilerInfo(assetName);
            if (alpi.createTime <= 0) alpi.createTime = Time.realtimeSinceStartup - stime;

            return obj;
        }

        public virtual Object Instantiate(Transform parent)
        {
            Object asset = GetRawObject();
            if (asset == null)
                return null;

            float stime = Time.realtimeSinceStartup;
            Object obj = AssetCache.InstantiateObject(this.m_XAssetBundle, asset, this.m_AssetName, parent);
            AssetManager.AssetLoadProfilerInfo alpi = AssetManager.Instance.GetProfilerInfo(assetName);
            if (alpi.createTime <= 0) alpi.createTime = Time.realtimeSinceStartup - stime;
            return obj;

        }

        public virtual T GetRawObject<T>() where T : Object
        {
            Object asset = GetRawObject();
            if (asset == null)
                return default(T);
            return AssetCache.AddRawObject(this.m_XAssetBundle, asset, this.m_AssetName) as T;
        }

        public virtual T GetRawObjectNotRef<T>() where T : Object
        {
            Object asset = GetRawObject();
            if (asset == null)
                return default(T);
            return AssetCache.AddRawObjectNotRef(this.m_XAssetBundle, asset, this.m_AssetName) as T;
        }


        public string GetDownLoaderInfo()
        {
            if (!string.IsNullOrEmpty(m_AssetBundleName))
            {
                return AssetBundleManager.Instance.GetDownloadInfoToString(m_AssetBundleName);
            }
            return string.Empty;
        }


        public string GetDownLoaderSizeInfo()
        {
            if (!string.IsNullOrEmpty(m_AssetBundleName))
            {
                return AssetBundleManager.Instance.GetDownloadSizeInfoToString(m_AssetBundleName);
            }
            return string.Empty;
        }

        public override string Error { get { return this.m_Error; } }

        public List<string> downloadFiles
        {
            get
            {
                if (m_DownloadFiles == null)
                {
                    m_DownloadFiles = ListPool<string>.Get();
                    AssetManager.Instance.GetNeedDownloadAssetBundle(m_AssetBundleName, ref m_DownloadFiles);
                }
                return m_DownloadFiles;
            }
        }

        public AsyncOperation asyncOperation
        {
            get { return m_AsyncOperation; }
        }

        public static AssetInternalLoader Get(string assetName, System.Type type)
        {
            return AssetManager.Instance.LoadBundleAsset(assetName, type);
        }

        public override string ToString()
        {
            return m_AssetBundleName;
        }
    }
}
