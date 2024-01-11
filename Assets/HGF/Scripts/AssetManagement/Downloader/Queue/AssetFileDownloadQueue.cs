using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FileStruct = XAssetsFiles.FileStruct;
using System.Diagnostics;

namespace AssetManagement
{
    public class AssetFileDownloadQueue : MonoBehaviour
    {
        class FileComparer : IComparer<FileStruct> { public int Compare(FileStruct x, FileStruct y) { return y.priority.CompareTo(x.priority); } }
        //已经下载的字节
        private int m_BytesReceived;
        public int bytesReceived { get { return currentDownloader == null ? m_BytesReceived : m_BytesReceived + currentDownloader.bytesReceived; } set { m_BytesReceived = value; } }
        //此队列中总字节
        public int bytesTotal { get; protected set; }
        //失败文件的字节
        public int bytesReceivedError { get; protected set; }
        //每秒速度/字节
        public int downloadSpeed { get { return m_Stopwatch == null ? 0 : (int)(bytesReceived / m_Stopwatch.Elapsed.TotalSeconds); } }
        //本队列中处理的文件
        public List<FileStruct> totalFiles { get; protected set; }
        //当前下载的文件列表
        public List<FileStruct> currentFiles { get; protected set; }
        public string error { get; protected set; }
        public bool isDone { get; protected set; }
        public bool isPause { get; protected set; }
        public bool isRuning { get; protected set; }
        public Dictionary<string, string> errors { get; private set; }
        public int currentFileIdx { get; private set; }
        public int totalFileCount { get { return this.currentFiles == null ? 0 : this.currentFiles.Count; } }
        public FileStruct currentFS { get; private set; }
        public AssetFileThreadDownloader currentDownloader { get; protected set; }
        public int errorCount { get { return errors == null ? 0 : errors.Count; } }
        //出现异常是否暂停
        public bool errorPause { get; set; }
        public float progress { get { return (float)(bytesReceived + bytesReceivedError) / (float)bytesTotal; } }
        public System.Action onProgress { get; set; }
        public System.Action onComplete { get; set; }
        public System.Action onBeginStep { get; set; }
        public System.Action onEndStep { get; set; }
        public Stopwatch m_Stopwatch;

        public virtual AssetFileDownloadQueue SetFiles(List<FileStruct> list)
        {
            this.totalFiles = list;
            this.currentFiles = list;
            RecalcInit();
            return this;
        }

        protected virtual void RecalcInit()
        {
            this.bytesReceived = 0;
            this.bytesReceivedError = 0;
            this.currentFileIdx = -1;
            this.bytesTotal = 0;
            if (this.errors != null) this.errors.Clear();

            //计算总大小
            foreach (var file in this.currentFiles)
                this.bytesTotal += file.size;

            //优先级排序
            this.currentFiles.Sort(new FileComparer());
        }

        protected virtual void RecalcInitAdd(List<FileStruct> addList)
        {
            //计算大小
            foreach (var file in addList)
                this.bytesTotal += file.size;
            
            if (this.currentFileIdx != -1)
                this.currentFiles.RemoveRange(0, this.currentFileIdx);
            this.currentFileIdx = -1;
            this.currentFiles.InsertRange(this.currentFiles.Count, addList);
            //优先级排序
            this.currentFiles.Sort(new FileComparer());

            if (this.currentFS != null)
            {
                this.currentFiles.Insert(0, this.currentFS);
                this.currentFileIdx = 0;
            }
        }

        public virtual void StartDownload(sbyte tag = -1)
        {
            if (this.isRuning) return;
            this.isDone = false;
            this.isPause = false;
            this.isRuning = true;
            this.error = string.Empty;

            m_Stopwatch = Stopwatch.StartNew();
            //DownLoadNext();
        }

        public void Pause()
        {
            if (isPause)
                return;
            isRuning = false;
            isPause = true;
            this.currentFileIdx = Mathf.Clamp(this.currentFileIdx - 1, -1, currentFiles.Count);
            if (this.currentDownloader != null && this.currentDownloader.IsLoading)
            {
                this.currentDownloader.Abort();
                this.currentDownloader.onProgress -= OnLoaderProgress;
                this.currentDownloader.onComplete -= OnLoaderComplete;
                this.currentDownloader = null;
            }
        }


        void DownLoadNext()
        {
            if (isPause) return;

            //恢复暂停
            if (this.currentFS != null)
            {
                this.currentDownloader = AssetDownloadManager.Instance.RecoverAbort(this.currentFS.path) as AssetFileThreadDownloader;
                if (this.currentDownloader != null)
                {
                    this.currentDownloader.onProgress += OnLoaderProgress;
                    this.currentDownloader.onComplete += OnLoaderComplete;
                    return;
                }
            }

            int tidx = this.currentFileIdx + 1;
            if (tidx >= this.currentFiles.Count)
            {
                OnAllComplete();
                return;
            }

            this.currentFileIdx = tidx;

            this.currentFS = this.currentFiles[this.currentFileIdx];

            this.currentDownloader = AssetBundleDownloader.GetAssetBundle(currentFS.path, null, currentFS.md5, currentFS.md5, 0, -2);
            if (onBeginStep != null) onBeginStep.Invoke();
            this.currentDownloader.onProgress += OnLoaderProgress;
            this.currentDownloader.onComplete += OnLoaderComplete;
        }

        protected virtual void AddError(string fname, string errStr)
        {
            if (errors == null)
                errors = new Dictionary<string, string>();
            if (!errors.ContainsKey(fname))
                errors.Add(fname, errStr);
        }

        protected virtual void OnLoaderProgress()
        {
            if (onProgress != null) onProgress.Invoke();
        }

        protected virtual void OnLoaderComplete()
        {
            bool errPause = false;
            if (string.IsNullOrEmpty(this.currentDownloader.Error))
            {
                this.m_BytesReceived += this.currentFS.size;
            }
            else
            {
                this.error += this.currentDownloader.Error + "\n";
                if (errorPause)
                    errPause = true;
                else
                {
                    AddError(this.currentFS.path, this.currentDownloader.Error);
                    this.bytesReceivedError += this.currentFS.size;
                }

            }

            if (onEndStep != null) onEndStep.Invoke();
            if (errPause) Pause();

            //if (this.currentDownloader != null)
            //{
            //    this.currentDownloader.onProgress -= OnLoaderProgress;
            //    this.currentDownloader.onComplete -= OnLoaderComplete;
            //    this.currentDownloader = null;
            //}
            //DownLoadNext();
        }

        protected virtual void Update()
        {
            if (!(Time.frameCount % 10 == 0)) return;

            if (isRuning)
            {
                if (this.currentDownloader == null || this.currentDownloader.IsDone())
                    DownLoadNext();
            }
        }



        void OnAllComplete()
        {
            this.isDone = true;
            this.isRuning = false;
            this.isPause = false;
            if (onComplete != null) onComplete.Invoke();
        }
    }
}
