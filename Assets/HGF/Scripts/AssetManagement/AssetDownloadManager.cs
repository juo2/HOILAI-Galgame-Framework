using UnityEngine;
using System;
using System.Collections.Generic;

namespace AssetManagement
{
    public class AssetDownloadManager : MonoBehaviour
    {
        public static bool LogEnabled = false;
        public static int DefBufferSize = 1024;



        private List<string> m_TempList = new List<string>();

        //当前下载任务
        private List<string> m_CurDownloadingKeys = new List<string>();

        //所有下载任务
        private List<string> m_DownloadingKeys = new List<string>();
        //所有下载任务
        private Dictionary<string, AssetDownloader> m_Downloading = new Dictionary<string, AssetDownloader>();

        //已经中止的任务
        private Dictionary<string, AssetDownloader> m_Aborting = new Dictionary<string, AssetDownloader>();

        //下载异常的任务
        private Dictionary<string, string> m_AllError = new Dictionary<string, string>();

        //已经下载的任务
        private List<string> m_AlreadyDownlaod = new List<string>();

        //每秒下载的字节
        private int m_SecondByte;
        //已经下载的总大小
        private long m_TotalByteSize;
        //已经下载的文件总数
        private int m_DownloadTotalCount;




        private AssetDownloaderComparer m_LoaderComparer = new AssetDownloaderComparer();
        //排序标识
        private bool m_SortFlag = false;
        //暂停
        private bool m_IsPause = false;
        public bool IsPause { get { return m_IsPause; } }
        public List<string> CurDownloadingKeys { get { return m_CurDownloadingKeys; } }
        public List<string> DownloadingKeys { get { return m_DownloadingKeys; } }
        public Dictionary<string, AssetDownloader> Downloading { get { return m_Downloading; } }
        public Dictionary<string, AssetDownloader> Aborting { get { return m_Aborting; } }
        public Dictionary<string, string> AllError { get { return m_AllError; } }
        public int secondByte { get { return m_SecondByte; } }
        public long totalByteSize { get { return m_TotalByteSize; } }
        public int downloadTotalCount { get { return m_DownloadTotalCount; } }
        public List<string> alreadyDownlaod { get { return m_AlreadyDownlaod; } }

        void Update()
        {
            m_SecondByte = 0;
            if (this.m_IsPause)
                return;

            //bool isSort = false;
            if (this.m_SortFlag)
            {
                //优先级发生改变 将UI优先级调到最高
                SortQueueHandle();

                this.m_DownloadingKeys.Sort(this.m_LoaderComparer);
                this.m_SortFlag = false;
                //isSort = true;
            }

            if (this.m_DownloadingKeys.Count < 1 && this.m_CurDownloadingKeys.Count < 1)
            {
                return;
            }

            if (MaxDownLoaderCount == -1 || this.m_CurDownloadingKeys.Count < MaxDownLoaderCount)
            {
                foreach (var item in this.m_DownloadingKeys)
                {
                    AssetDownloader loader = this.m_Downloading[item];
                    if (!loader.IsLoading)
                    {
                        if (!this.m_CurDownloadingKeys.Contains(item))
                            this.m_CurDownloadingKeys.Add(item);

                        //暂停等待的任务再次启动
                        if (loader.IsPause) loader.Start();
                        break;
                    }
                }
            }


            foreach (var item in this.m_CurDownloadingKeys)
            {
                AssetDownloader loader;
                if (!this.m_Downloading.TryGetValue(item, out loader))
                {
                    Debug.LogErrorFormat("m_Downloading no exist {0}", item);
                    this.m_TempList.Add(item);
                    continue;
                }

                loader.Update();
                if (loader.onProgress != null) loader.onProgress();
                if (loader.IsAbort)
                {
                    if (!this.m_Aborting.ContainsKey(item))
                        this.m_Aborting.Add(item, loader);
                    this.m_TempList.Add(item);
                }
                else if (!string.IsNullOrEmpty(loader.Error))
                {
                    if (!this.m_AllError.ContainsKey(item))
                        this.m_AllError.Add(item, loader.Error);
                    this.m_TempList.Add(item);
                }
                else if (loader.IsDone())
                {
                    this.m_TotalByteSize += loader.totalByteSize;
                    this.m_DownloadTotalCount++;
                    this.m_TempList.Add(item);
                    this.m_AlreadyDownlaod.Add(loader.WebUrl);
                }

                m_SecondByte += loader.secondByte;
            }


            if (this.m_TempList.Count > 0)
            {
                AssetDownloader value;
                foreach (var item in this.m_TempList)
                {
                    if (!this.Downloading.TryGetValue(item, out value))
                        break;
                    AssetDownloader dloader = this.m_Downloading[item];
                    if (dloader.onComplete != null)
                        dloader.onComplete.Invoke();
                    dloader.Dispose();
                    this.m_Downloading.Remove(item);
                    this.m_DownloadingKeys.Remove(item);
                    this.m_CurDownloadingKeys.Remove(item);
                    if (dloader.IsDone() && this.m_Aborting.ContainsKey(item))
                        this.m_Aborting.Remove(item);
                }
                this.m_TempList.Clear();
            }
        }


        //插队处理
        void SortQueueHandle()
        {
            int timeTag = (int)Time.time;
            foreach (var loader in this.m_Downloading)
            {
                //ui优先级设为最高
                //不中止当前下载的任务
                if (loader.Value.IsLoading || loader.Key.StartsWithEx("01/"))
                {
                    loader.Value.Priority = timeTag + 9999;
                }
            }
        }


        internal T GetDownloadInstance<T>(string assetPath, int timeout = 0, int priority = 0) where T : AssetDownloader, new()
        {
            AssetDownloader loader;
            bool create = true;
            if (this.m_Downloading.TryGetValue(assetPath, out loader))
            {
                create = false;
            }
            else if (this.m_Aborting.TryGetValue(assetPath, out loader))
            {
                if (create)
                    this.m_Aborting.Remove(assetPath);
            }
            else if (this.m_AllError.ContainsKey(assetPath))
            {
                if (create)
                    this.m_AllError.Remove(assetPath);
            }

            if (loader == null)
                loader = new T();

            if (create)
            {
                this.m_Downloading.Add(assetPath, loader);
                this.m_DownloadingKeys.Add(assetPath);
            }

            if (priority > 0)
                this.m_SortFlag = true;

            return loader as T;
        }

        //public void PauseAll()
        //{
        //    this.m_IsPause = true;
        //    if (this.m_CurDownloadingKeys.Count > 0)
        //    {
        //        foreach (var item in this.m_CurDownloadingKeys)
        //        {
        //            AssetDownloader downloader;
        //            if (m_Downloading.TryGetValue(item, out downloader))
        //            {
        //                downloader.Abort();
        //            }
        //        }
        //    }
        //}

        //public void PauseRecover()
        //{
        //    foreach (var item in m_Aborting)
        //        item.Value.Start();
        //    m_Aborting.Clear();
        //}

        //中止恢复
        public AssetDownloader RecoverAbort(string assetPath)
        {
            AssetDownloader loader;
            if (this.m_Aborting.TryGetValue(assetPath, out loader))
            {
                this.m_Aborting.Remove(assetPath);
                this.m_Downloading.Add(assetPath, loader);
                this.m_DownloadingKeys.Add(assetPath);
                this.m_CurDownloadingKeys.Add(assetPath);
                loader.Start();
                return loader;
            }
            return null;
        }



        public int MaxDownLoaderCount { get { return AssetManager.Instance.AssetLoaderOptions.GetDownLoaderMaxNum(); } }


        private static AssetDownloadManager m_Instance;
        public static AssetDownloadManager Instance { get { return m_Instance; } }



        public void Awake() { m_Instance = this; }
    }



    class AssetDownloaderComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            AssetDownloader v1, v2;
            AssetDownloadManager.Instance.Downloading.TryGetValue(x, out v1);
            AssetDownloadManager.Instance.Downloading.TryGetValue(y, out v2);
            if (v1 != null && v2 != null)
            {
                return v1.Priority > v2.Priority ? -1 : 1;
            }
            return 0;
        }
    }
}

