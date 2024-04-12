using UnityEngine;
using System;
using System.Collections.Generic;

namespace AssetManagement
{
    public class AssetBundleLoader : AssetLoader
    {
        public static bool isWWW = false;

        private const string NullError = "AssetBundle is Null";
        public enum LoaderType
        {
            None,      //默认
            Buidling,  //首包
            LocalSave, //sd卡
            Download,  //下载
        }

        private string m_Error;
        private bool m_Loading = false;
        private string m_AssetBundleName;
        //加载类型
        private LoaderType m_LoaderType = LoaderType.None;
        //本地加载发生文件错误，将尝试其它途径加载,一般将走下载去下载服务器最新文件
        private bool m_CorrectionOfError = false;
        private AssetBundleCreateRequest m_AssetBundleCreateRequest;
        private AssetBundleDownloader m_AssetBundleDownloader;
        private WWW m_WWW;

        public float startTime = 0f;
        public float startDownloadTime = 0f;
        //延迟测试时间，为了更好的观察ab加载的状态
        public static float delayTest = 0f;
        private int m_Priority = -1;
        public int priority
        {
            get { return isLoading ? 65535 : m_Priority; }
            set { m_Priority = value; }
        }

        public AssetBundleLoader(string name)
        {
            this.m_AssetBundleName = name;
            //UI加载优先级最高
            if (name.StartsWithEx("01")) m_Priority = 512;

        }


        override public void Update()
        {

            if (checkDownloader()) return;


            startTime = Time.realtimeSinceStartup;
            isLoading = true;

            ExecuteLoader();
        }

        //检查是否加载中
        private bool checkDownloader()
        {
            if (isLoading)
            {
                if (this.m_AssetBundleDownloader != null)
                {
                    if (!string.IsNullOrEmpty(this.m_AssetBundleDownloader.Error))
                        this.m_Error = string.Format("AssetBundleDownloader error {0}", this.m_AssetBundleDownloader.Error);

                    if (!this.m_AssetBundleDownloader.IsDone())
                        return true;
                    AssetBundleManager.AssetBundleLoadInfo linfo = AssetBundleManager.Instance.GetAssetBundleLoadInfo(m_AssetBundleName);
                    linfo.downloadTime = Time.realtimeSinceStartup - startDownloadTime;
                    this.m_AssetBundleDownloader = null;
                }
                else
                {
                    return true;
                }
            }

            return false;
        }


        private void ExecuteLoader(bool correctionOfError = false)
        {
            //从扩展卡加载
            string fullPath = string.Empty;
            if (correctionOfError)
            {
                if (m_LoaderType == LoaderType.LocalSave)
                {
                    //首包异常修复，从下载目录加载，若下载目录不存在则下载
                    fullPath = AssetManager.Instance.AssetLoaderOptions.GetAssetDownloadSavePath(this.m_AssetBundleName);
                    if (isWWW) fullPath = "file://" + fullPath;
                    if (!System.IO.File.Exists(fullPath))
                    {
                        m_LoaderType = LoaderType.Download;
                    }
                }

            }
            else
            {
                //扩展卡WWW 都加 file://    非安卓 安装目录 也加 file://
                fullPath = AssetManager.Instance.AssetLoaderOptions.GetAssetDownloadSavePath(this.m_AssetBundleName);
                if (System.IO.File.Exists(fullPath))
                {
                    if (isWWW) fullPath = "file://" + fullPath;
                    m_LoaderType = LoaderType.LocalSave;
                }
                else if (AssetManager.Instance.AssetLoaderOptions.GetAssetBundleIsBuildin(this.m_AssetBundleName))
                {
                    //从首包加载
                    m_LoaderType = LoaderType.Buidling;
                    fullPath = AssetManager.Instance.AssetLoaderOptions.GetBuildinAssetPath(this.m_AssetBundleName);
                    if (isWWW && Application.platform != RuntimePlatform.Android) fullPath = "file://" + fullPath;
                }
                else if (AssetManager.Instance.AssetLoaderOptions.GetAssetBundleIsNeeedDownload(this.m_AssetBundleName))
                {
                    //从远程服务器下载||并加载
                    m_LoaderType = LoaderType.Download;
                }
            }


            if (m_LoaderType == LoaderType.Buidling || m_LoaderType == LoaderType.LocalSave)
            {
                uint crc = AssetManager.Instance.GetAssetBundleCrc(this.m_AssetBundleName);

                if (isWWW)
                    m_WWW = WWW.LoadFromCacheOrDownload(fullPath, (int)crc, crc);
                else
                    m_AssetBundleCreateRequest = AssetBundle.LoadFromFileAsync(fullPath, crc);
            }
            else if (m_LoaderType == LoaderType.Download)
            {
                startDownloadTime = Time.realtimeSinceStartup;
                string md5 = AssetManager.Instance.AssetLoaderOptions.GetAssetBundleMd5(this.m_AssetBundleName);
                int priority = (int)Time.time + 999;
                this.m_AssetBundleDownloader = AssetBundleDownloader.GetAssetBundle(this.m_AssetBundleName, null, md5, md5, 0, priority);
            }
            else
            {
                //没有符合条件的加载
                this.m_Error = string.Format("AssetBundleLoader::Update Unsigned condition loading {0}", this.m_AssetBundleName);
                Debug.LogWarning(this.m_Error);
            }
        }



        override public bool IsDone()
        {
            //延迟测试代码
            if (delayTest > 0)
            {
                if (Time.time - startTime < delayTest)
                {
                    return false;
                }
            }


            if (!string.IsNullOrEmpty(this.m_Error))
                return true;

            if (isWWW && m_WWW != null)
            {
                if (m_WWW.isDone)
                {

                    //本地文件异常进行修复
                    if (!string.IsNullOrEmpty(m_WWW.error) || m_WWW.assetBundle == null)
                    {
                        return CorrectionOfError();
                    }

                    if (m_CorrectionOfError)
                    {
                        XLogger.ReportException("本地文件修复成功", string.Format("m_LoaderType={0}  assetBundleName={1}", m_LoaderType, m_AssetBundleName), "");
                    }

                    return true;
                }
            }



            if (m_AssetBundleCreateRequest != null && m_AssetBundleCreateRequest.isDone)
            {
                //本地文件异常进行修复
                if (m_AssetBundleCreateRequest.assetBundle == null)
                {
                    return CorrectionOfError();
                }

                if (m_CorrectionOfError)
                {
                    XLogger.ReportException("本地文件修复成功", string.Format("m_LoaderType={0}  assetBundleName={1}", m_LoaderType, m_AssetBundleName), "");
                }

                AssetBundleManager.AssetBundleLoadInfo linfo = AssetBundleManager.Instance.GetAssetBundleLoadInfo(m_AssetBundleName);
                if (linfo.loadTime <= 0) linfo.loadTime = Time.realtimeSinceStartup - startTime;
                return true;
            }

            return false;
        }


        //异常修复本地文件
        private bool CorrectionOfError()
        {
            m_CorrectionOfError = true;
            XLogger.ERROR_Format("本地文件异常修复 m_LoaderType={0}  assetBundleName={1}", m_LoaderType, m_AssetBundleName);

            if (m_LoaderType == LoaderType.Buidling)
            {
                //首包文件异常
                m_WWW = null;
                m_AssetBundleCreateRequest = null;
                m_LoaderType = LoaderType.LocalSave;
                ExecuteLoader(true);
                return false;
            }
            else if (m_LoaderType == LoaderType.LocalSave)
            {
                //扩展卡文件异常
                m_WWW = null;
                m_AssetBundleCreateRequest = null;
                m_LoaderType = LoaderType.Download;
                ExecuteLoader(true);
                return false;
            }
            return true;
        }

        public override void Dispose()
        {
            m_AssetBundleName = null;
            if (m_AssetBundleCreateRequest != null)
                m_AssetBundleCreateRequest = null;
            this.m_AssetBundleDownloader = null;
        }

        override public float GetProgress()
        {
            float progress = 1f;
            if (this.IsDone())
                return progress;
            else
                progress = 0.0f;

            if (this.isDownloaderType)
            {
                if (this.m_AssetBundleDownloader != null)
                {
                    progress = this.m_AssetBundleDownloader.GetProgress();
                }
                else if (isWWW && m_WWW != null)
                {
                    progress = m_WWW.progress + 1.0f;
                }
                else if (this.m_AssetBundleCreateRequest != null)
                {
                    progress = m_AssetBundleCreateRequest.progress + 1.0f;
                }

                progress *= 0.5f;

            }
            else if (isWWW && m_WWW != null)
            {
                progress = m_WWW.progress;
            }
            else if (m_AssetBundleCreateRequest != null)
            {
                progress = m_AssetBundleCreateRequest.progress;
            }

            return progress;
        }

        public int GetDownloadReceivedBytes()
        {
            int size = 0;
            if (this.isDownloaderType)
            {
                if (this.m_AssetBundleDownloader != null)
                {
                    size = this.m_AssetBundleDownloader.bytesReceived;
                }
                else if (this.m_AssetBundleCreateRequest != null || m_WWW != null)
                {
                    size = AssetManager.Instance.GetAssetBundleSize(m_AssetBundleName);
                }
            }
            return size;
        }


        public AssetBundle assetBundle
        {
            get
            {
                if (isWWW && m_WWW != null) return m_WWW.assetBundle;
                return m_AssetBundleCreateRequest != null ? m_AssetBundleCreateRequest.assetBundle : null;
            }
        }

        public override string Error
        {
            get
            {
                if (!string.IsNullOrEmpty(this.m_Error))
                    return this.m_Error;

                if (isWWW && m_WWW != null)
                {
                    string errorStr = null;
                    if (!string.IsNullOrEmpty(m_WWW.error))
                        errorStr = m_WWW.error + m_AssetBundleName;
                    return errorStr;
                }
                else if (m_AssetBundleCreateRequest != null && m_AssetBundleCreateRequest.isDone && m_AssetBundleCreateRequest.assetBundle == null)
                {
                    //加载AssetBundle发生了错误 要么crc错误 要么文件损坏等等
                    return NullError;
                }
                return null;
            }
        }

        public bool isDownloaderType { get { return m_LoaderType == LoaderType.Download; } }
        public bool correctionOfError { get { return m_CorrectionOfError; } }
        public bool isLoading { get { return m_Loading; } private set { m_Loading = value; } }
        public string assetBundleName { get { return m_AssetBundleName; } }
    }
}
