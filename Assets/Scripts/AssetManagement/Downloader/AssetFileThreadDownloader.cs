using System;
using UnityEngine;
using System.Collections;
using System.Net;
using System.Diagnostics;
using System.IO;
using Debug = UnityEngine.Debug;

namespace AssetManagement
{
    public class AssetFileThreadDownloader : AssetDownloader
    {
        protected string m_Md5;
        protected string m_Version;
        protected string m_DownloadPath;
        private string m_TempDownloadPath;


        protected XWebFileClient m_XWebClient;
#pragma warning disable 0414
        private bool m_IsTickPause = false;

        //进度
        public float progress { get; private set; }

        public string TempDownloadPath { get { return m_TempDownloadPath; } }
        public string DownloadPath { get { return m_DownloadPath; } }

        public static AssetFileThreadDownloader Get(string assetPath, string downloadPath = null, string md5 = null, string version = null, int timeout = 0, int priority = 0)
        {
            AssetFileThreadDownloader loader = AssetDownloadManager.Instance.GetDownloadInstance<AssetFileThreadDownloader>(assetPath, timeout, priority);
            loader.m_WebUrl = AssetManager.Instance.AssetLoaderOptions.GetAssetDownloadUrl(assetPath);
            loader.m_WebUrl2 = AssetManager.Instance.AssetLoaderOptions.GetAssetDownloadUrl(assetPath, 0);
            loader.m_Md5 = md5;
            loader.m_Timeout = timeout;
            loader.m_Priority = priority;
            loader.m_DownloadPath = string.IsNullOrEmpty(downloadPath) ? AssetManager.Instance.AssetLoaderOptions.GetAssetDownloadSavePath(assetPath) : downloadPath;
            loader.m_Version = version;
            return loader;
        }

        protected override void InitResetWebRequest()
        {

            string url = this.m_WebUrl;

            //if (!string.IsNullOrEmpty(this.m_Version))
            //    url += "?v=" + this.m_Version;

            this.m_TempDownloadPath = this.m_DownloadPath + ".temp";
            string parent = Path.GetDirectoryName(this.m_DownloadPath);

            //XLogger.WARNING_Format("InitResetWebRequest {0}", WebUrl);

            try
            {
                if (!Directory.Exists(parent))
                {
                    if (File.Exists(parent)) File.Delete(parent);
                    Directory.CreateDirectory(parent);
                }
                else if (File.Exists(this.m_TempDownloadPath))
                    File.Delete(this.m_TempDownloadPath);
            }
            catch (Exception e)
            {
                XLogger.ERROR_Format("AssetFileThreadDownloader::InitResetWebRequest. {0}", e.ToString());
            }

            m_XWebClient = Pool<XWebFileClient>.Get();
            m_XWebClient.Reset();
            m_XWebClient.DownloadFileAsync(url, m_WebUrl2, this.m_TempDownloadPath, this.m_DownloadPath, m_Md5, this.m_Version);
        }

        private void DownloadFileCompleted()
        {
            m_State = State.Done;
        }

        public override bool IsDone()
        {
            return this.m_State == State.Done || this.m_State == State.Error || this.m_State == State.ErrorMd5;
        }

        public override void Pause()
        {
            if (IsLoading)
            {
                m_IsTickPause = true;
                //XLogger.WARNING_Format("Pause {0}",WebUrl);
                this.m_State = State.Pause;
                try
                {
                    m_XWebClient.CancelDownload();
                    Pool<XWebFileClient>.Release(m_XWebClient);
                    m_XWebClient = null;
                }
                catch (Exception e)
                {
                    XLogger.ERROR_Format("AssetFileThreadDownloader::Pause " + e.ToString());
                }
            }
            else
            {
                if (AssetDownloadManager.LogEnabled)
                {
                    UnityEngine.Debug.LogWarningFormat("AssetDownloader::Abort  Loading == false {0}", m_WebUrl);
                }
            }
        }

        public override void Abort()
        {
            if (IsLoading)
            {
                this.m_State = State.Abort;
                try
                {
                    m_XWebClient.CancelDownload();
                    Pool<XWebFileClient>.Release(m_XWebClient);
                    m_XWebClient = null;
                }
                catch (Exception e)
                {
                    XLogger.ERROR_Format("AssetFileThreadDownloader::Abort " + e.ToString());
                }
            }
            else
            {
                if (AssetDownloadManager.LogEnabled)
                {
                    Debug.LogWarningFormat("AssetFileThreadDownloader::Abort  Loading == false {0}", m_WebUrl);
                }
            }
        }


        public override void DoLoadingUpdate()
        {
            if (m_XWebClient != null)
            {
                m_XWebClient.Update();
                progress = m_XWebClient.progress;
                bytesReceived = m_XWebClient.bytesReceived;
                secondByte = m_XWebClient.secondByte;
                if (!string.IsNullOrEmpty(m_XWebClient.error))
                {
                    m_Error = m_XWebClient.error;
                    m_State = m_XWebClient.isMD5Error ? State.ErrorMd5 : State.Error;

                }
                else if (m_XWebClient.isDone)
                {
                    m_XWebClient.CancelDownload();
                    DownloadFileCompleted();
                }
            }
        }

        override public float GetProgress()
        {
            return progress;
        }

        public override void Dispose()
        {
            if (m_XWebClient != null)
            {
                m_XWebClient.CancelDownload();
                Pool<XWebFileClient>.Release(m_XWebClient);
                m_XWebClient = null;
            }
            base.Dispose();
        }
    }
}
