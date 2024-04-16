using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace AssetManagement
{
    /// <summary>
    /// 资源下载器
    /// </summary>
    abstract public class AssetDownloader : AssetLoader
    {

        public enum State
        {
            None,
            Processing,
            Error,
            ErrorMd5,
            Pause,
            Abort,
            Done,
            ReStart,
        }

        protected string m_WebUrl;                   //下载文件地址
        protected string m_WebUrl2;                  //备用下载地址
        protected string m_Error;                    //异常内容
        protected float m_Timeout;                  //超时时间
        protected int m_Priority;                  //下载优先级
        protected State m_State = State.None;       //状态
        public AssetManagement.AssetDownloader.State state { get { return m_State; } }
        protected UnityWebRequest m_UnityWebRequest;          //当前下载请求
        public UnityAction onComplete { get; set; }
        public UnityAction onProgress { get; set; }
        //每秒下载的字节
        public int secondByte { get; protected set; }
        //已经下载的字节
        public int bytesReceived { get; protected set; }
        public virtual int totalByteSize { get { return 0; } }

        public UnityEngine.Networking.UnityWebRequest UnityWebRequest { get { return m_UnityWebRequest; } }
        protected AsyncOperation m_AsyncOperation;
        public int Priority { get { return m_Priority; } set { m_Priority = value; } }
        public float Timeout { get { return m_Timeout; } }
        public string WebUrl { get { return m_WebUrl; } }
        public bool IsAbort { get { return m_State == State.Abort; } }
        public bool IsLoading { get { return m_State == State.Processing; } }
        public bool IsPause { get { return m_State == State.Pause; } }


        override public string Error { get { return m_Error; } }

        abstract protected void InitResetWebRequest();

        /// <summary>
        /// 启动下载
        /// </summary>
        public virtual void Start()
        {
            if (IsLoading)
            {
                if (AssetDownloadManager.LogEnabled)
                {
                    Debug.LogWarningFormat("AssetDownloader::Start  Loading == true {0}", m_WebUrl);
                }
                return;
            }

            this.m_State = State.Processing;

            this.InitResetWebRequest();

            //if (m_UnityWebRequest == null)
            //{
            //    if (AssetDownloadManager.LogEnabled)
            //    {
            //        Debug.LogWarningFormat("AssetDownloader::Start  m_UnityWebRequest == null {0}", m_WebUrl);
            //    }
            //    return;
            //}


            
            if (this.m_UnityWebRequest != null)
            {
#if UNITY_5
             this.m_AsyncOperation = m_UnityWebRequest.Send();
#elif UNITY_2017
                this.m_AsyncOperation = m_UnityWebRequest.SendWebRequest();
#endif
            }

        }


        public virtual void Pause()
        {
            if (IsLoading)
            {
                this.m_State = State.Pause;
                if (this.m_UnityWebRequest != null)
                {
                    this.m_AsyncOperation = null;
                    this.m_UnityWebRequest.Abort();
                    this.m_UnityWebRequest.Dispose();
                    this.m_UnityWebRequest = null;
                }
            }
            else
            {
                if (AssetDownloadManager.LogEnabled)
                {
                    XLogger.WARNING_Format("AssetDownloader::Abort  Loading == false {0}", m_WebUrl);
                }
            }
        }


        /// <summary>
        /// 中止当前下载。一旦中中止必须手动调用 Start 才能再次启动
        /// </summary>
        public virtual void Abort()
        {
            if (IsLoading)
            {
                this.m_State = State.Abort;
                if (this.m_UnityWebRequest != null)
                {
                    this.m_AsyncOperation = null;
                    this.m_UnityWebRequest.Abort();
                    this.m_UnityWebRequest.Dispose();
                    this.m_UnityWebRequest = null;
                }
            }
            else
            {
                if (AssetDownloadManager.LogEnabled)
                {
                    Debug.LogWarningFormat("AssetDownloader::Abort  Loading == false {0}", m_WebUrl);
                }
            }
        }

        public override void Update()
        {
            if (IsAbort)
            {
                return;
            }

            if (IsPause)
            {
                return;
            }

            if (IsLoading)
            {
                this.DoLoadingUpdate();
                return;
            }

            this.Start();
        }

        public virtual void DoLoadingUpdate()
        {

        }


        public override bool IsDone()
        {
            return false;
        }

        override public void Dispose()
        {
            this.onProgress = null;
            this.onComplete = null;
            if (this.m_UnityWebRequest != null)
            {
                this.m_UnityWebRequest.Dispose();
                this.m_UnityWebRequest = null;
            }
        }
    }
}
