using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FileStruct = XAssetsFiles.FileStruct;

namespace AssetManagement
{
    //后台下载
    public class BackgroundDownloadQueue : AssetFileDownloadQueue
    {

        public class DownloadGroup
        {
            public sbyte donwloadtag;
            public int totalFileCount;          //总文件数
            public int donwloadedFileCount;     //已经下载文件数
            public long totalBytes;             //总需要下载的字节
            public long donwloadedBtyes;        //已经下载的字节
            public bool downloadable;           //当前组可下载
        }



        private static BackgroundDownloadQueue m_Instance;
        public static BackgroundDownloadQueue Instance { get { if (m_Instance == null) m_Instance = AssetManager.Instance.gameObject.AddComponent<BackgroundDownloadQueue>(); return m_Instance; } }

        //当前下载的分包标记
        public sbyte currentDownloadTag { get; private set; }
        public string currentDownloadStr;
        public List<FileStruct> totalNeedDownload { get; private set; }
        public List<DownloadGroup> allGroups { get; private set; }
        public override AssetFileDownloadQueue SetFiles(List<XAssetsFiles.FileStruct> list)
        {
            currentDownloadTag = -1;
            totalFiles = list;
            totalNeedDownload = new List<FileStruct>(list);
            currentFiles = new List<FileStruct>();
            InitDownloadGroup();
            return this;
        }

        protected virtual void InitDownloadGroup()
        {
            //分组
            allGroups = new List<DownloadGroup>();
            Dictionary<int, DownloadGroup> tempDic = new Dictionary<int, DownloadGroup>();
            foreach (XAssetsFiles.FileStruct item in totalNeedDownload)
            {
                DownloadGroup group;
                if (!tempDic.TryGetValue(item.tag, out group))
                {
                    group = new DownloadGroup();
                    group.donwloadedBtyes = 0;
                    group.donwloadedFileCount = 0;
                    group.donwloadtag = item.tag;
                    group.downloadable = item.tag <= 100;
                    allGroups.Add(group);
                    tempDic.Add(item.tag, group);
                }
                group.totalBytes += item.size;
                group.totalFileCount++;
            }

            allGroups.Sort((DownloadGroup v1, DownloadGroup v2) => { return v1.donwloadtag.CompareTo(v2.donwloadtag); });
        }


        public void ExternalDownload(string downStr, sbyte tag = -1)
        {
            if (allGroups == null || currentDownloadStr == downStr) return;
            currentDownloadStr = downStr;
            string[] downArray = downStr.Split(',');
            if (downArray.Length <= 0) return;
            List<string> list = new List<string>();            
            for (int i = 0; i < downArray.Length; i++)
            {                
                string assetBundleName = AssetManager.Instance.GetAssetBundleName(downArray[i]);
                if (!string.IsNullOrEmpty(assetBundleName))
                {
                    list.Add(assetBundleName);
                    string[] deps = AssetManager.Instance.GetAssetBundleDependence(assetBundleName);
                    if (deps.Length > 0)
                        list.AddRange(deps);
                }
            }
            int outValue = 0;
            Dictionary<string, int> tempDic = new Dictionary<string, int>();
            for (int j = 0; j < list.Count; j++)
            {
                if (!string.IsNullOrEmpty(list[j]) && !tempDic.TryGetValue(list[j], out outValue))
                {
                    tempDic.Add(list[j], 1);
                }
            }
            sbyte curTag = tag != -1 ? tag : currentDownloadTag;
            DownloadGroup curGroup = null;
            if (allGroups.Count > 0)
            {
                foreach (var group in allGroups)
                    if (group.donwloadtag == curTag)
                        curGroup = group;
            }
            if (curGroup == null)
            {
                curGroup = new DownloadGroup();
                curGroup.donwloadedBtyes = 0;
                curGroup.donwloadedFileCount = 0;
                curGroup.donwloadtag = curTag;
                curGroup.downloadable = true;
                allGroups.Add(curGroup);
            }

            List<FileStruct> addList = new List<FileStruct>();
            foreach (FileStruct item in totalNeedDownload)
            {                
                foreach (var temp in tempDic)
                {                    
                    if (item.path == temp.Key)
                    {
                        item.tag = curTag;
                        item.priority = short.MaxValue;
                        curGroup.totalBytes += item.size;
                        curGroup.totalFileCount++;
                        addList.Add(item);
                        break;
                    }
                }
            }            
            if (addList.Count > 0)
            {
                if (!this.isDone && (this.isRuning || this.isPause) && currentDownloadTag == curTag)
                {
                    RecalcInitAdd(addList);
                }
                else
                {
                    StartDownload(curTag);
                }
            }
        }               

        public override void StartDownload(sbyte tag = -1)
        {            
            currentDownloadTag = tag;
            RecalcInit();
            base.StartDownload();
        }

        //将会自动下载当前编号最小的分组
        public void StartAutoDownload()
        {
            DownloadGroup defaultGroup = null;
            foreach (var group in allGroups)
            {
                if (group.donwloadtag == -1)
                {
                    defaultGroup = group;
                }
                else if (group.downloadable && group.donwloadedFileCount < group.totalFileCount)
                {
                    StartDownload(group.donwloadtag);
                    return;
                }                                
            }

            //所有分组都下载完了，就开始下载-1未设置标记的组
            if (defaultGroup != null && defaultGroup.donwloadedFileCount < defaultGroup.totalFileCount)
                StartDownload(defaultGroup.donwloadtag);
        }


        protected override void RecalcInit()
        {
            currentFiles.Clear();
            //if (currentDownloadTag == 0)
            //{
            //    //0静默下载所有需要下载的资源
            //    currentFiles.AddRange(totalNeedDownload);
            //}
            //else
            //{
            //只下载标记的
            foreach (var item in totalNeedDownload)
                if (item.tag == currentDownloadTag)
                    currentFiles.Add(item);
            //}
            base.RecalcInit();
        }


        protected override void OnLoaderComplete()
        {
            if (this.currentDownloader == null)
                return;

            bool groupIsFinish = false;
            if (string.IsNullOrEmpty(this.currentDownloader.Error))
            {
                //下载完成移出
                totalNeedDownload.Remove(this.currentFS);

                DownloadGroup group;
                for (int i = 0; i < allGroups.Count; i++)
                {
                    group = allGroups[i];
                    if (group.donwloadtag == this.currentFS.tag)
                    {
                        group.donwloadedBtyes += this.currentFS.size;
                        group.donwloadedFileCount++;
                        if (group.donwloadedFileCount >= group.totalFileCount)
                            groupIsFinish = true;
                        break;
                    }
                }
            }
            base.OnLoaderComplete();

            if (groupIsFinish)
            {
                if (currentDownloadTag == sbyte.MaxValue)
                {
                    //LocalCacheUtility.SaveString("DownloadTag127", "");
                    currentDownloadStr = "";
                }
                StartAutoDownload();                
            }                
        }
       
        public void StartLimitDownload(int tag)
        {
            DownloadGroup group = GetDownloadGroup(tag);
            if (group != null)
            {
                group.downloadable = true;
                StartDownload(group.donwloadtag);
            }
        }

        public DownloadGroup GetDownloadGroup(int tag)
        {
            if (allGroups != null && allGroups.Count > 0)
            {
                foreach (var item in allGroups)
                    if (item.donwloadtag == tag) return item;
            }                
            return null;
        }

        //获取单个文件下载的进度
        public float GetSignalProgress()
        {
            if (currentDownloader != null)
                return currentDownloader.GetProgress();
            return 0;
        }

        //获取单个文件大小
        public float GetSignalSize()
        {
            if (currentFS != null)
                return currentFS.size;
            return 0;
        }
    }
}
