using System;
using UnityEngine;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using Debug = UnityEngine.Debug;
using UnityEngine.Networking;

public class XWebFileClient
{
    public static int TIME_OUT_VALUE = 10;

#if !UNITY_WEBGL
    private WebClient m_WebClient;
#else 
    Coroutine webGLDownCoroutine;
#endif
    private Stopwatch m_Stopwatch;
    public int bytesReceived { get; private set; }
    public int secondByte { get; private set; }
    public float progress { get; private set; }
    public bool isClientDispose { get; private set; }
    public bool isClientCancelled { get; private set; }

    public string weburl { get; private set; }
    public string weburlver { get; private set; }
    public string weburl2 { get; private set; }
    public string weburlver2 { get; private set; }
    public string donwloadUrl { get; private set; }
    public string version { get; private set; }
    public string localSavePath { get; private set; }
    public string localTempSavePath { get; private set; }
    public string md5Test { get; private set; }
    public int md5ErrRepairCount { get; private set; }  //md5异常修复
    public int defErrRepairCount { get; private set; }  //普通异常修复
    public int timeoutErrRepairCount { get; private set; }  //下载超时异常修复 次数
    public string error { get; private set; }
    public bool isDone { get; private set; }
    public bool isMD5Error { get; private set; }
    public float timeoutTag { get; private set; }  //超时标记

    private void InitWebClient()
    {
#if !UNITY_WEBGL
        m_WebClient = new WebClient();
        m_WebClient.DownloadProgressChanged += OnDownloadProgressChanged;
        m_WebClient.DownloadFileCompleted += OnDownloadFileCompleted;
        m_WebClient.Disposed += OnDownloadDisposed;
        m_WebClient.Proxy = null;
#endif
    }

    public void Reset()
    {
        md5ErrRepairCount = 0;
        defErrRepairCount = 0;
        timeoutErrRepairCount = 0;
        timeoutTag = 0;
    }


    //再次下载此文件
    public void ReDownload()
    {
        //没有备用地址随机版本号回源
        if (string.IsNullOrEmpty(weburl2))
        {
            string random = DateTime.Now.ToString("yyyymmddhhmmss");
            this.version = random;
        }
        DownloadFileAsync(weburl, weburl2, localTempSavePath, localSavePath, md5Test, version);
    }

    public void DownloadFileAsync(string url, string url2, string saveTempPath, string savePath, string md5, string version = "")
    {
#if !UNITY_WEBGL
        if (m_WebClient == null)
            InitWebClient();
#endif
        this.version = version;
        isMD5Error = false;
        isDone = false;
        error = string.Empty;
        weburl = url;
        weburl2 = url2;
        localTempSavePath = saveTempPath;
        localSavePath = savePath;
        md5Test = md5;
        bytesReceived = 0;
        secondByte = 0;
        progress = 0.0f;
        timeoutTag = 0;
        isClientCancelled = false;
        isClientDispose = false;
        m_Stopwatch = Stopwatch.StartNew();

        weburlver = weburl;
        weburlver2 = weburl2;
        if (!string.IsNullOrEmpty(version))
        {
            weburlver = string.Format("{0}?v={1}", weburl, version);
            if (!string.IsNullOrEmpty(weburlver2))
                weburlver2 = string.Format("{0}?v={1}", weburlver2, version);
        }

        donwloadUrl = donwloadUrl == weburlver && !string.IsNullOrEmpty(weburlver2) ? weburlver2 : weburlver;

#if !UNITY_WEBGL
        m_WebClient.DownloadFileAsync(new Uri(donwloadUrl), localTempSavePath);
#else

        Debug.Log($"开始下载: donwloadUrl:{donwloadUrl}");
        webGLDownCoroutine = TimerManager.AddCoroutine(WebGLDownCoroutine());
#endif
    }

    IEnumerator WebGLDownCoroutine()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(donwloadUrl))
        {
            webRequest.downloadHandler = new DownloadHandlerFile(localTempSavePath);
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"File downloaded successfully , donwloadUrl:{donwloadUrl},localTempSavePath:{localTempSavePath}");
                CompleteHandle();
            }
            else
            {
                if (defErrRepairCount > 0)
                {
                    Debug.LogError("Failed to download file: " + webRequest.error);
                    //XLogger.ReportException("下载文件操作异常", error, "");
                    isDone = true;
                }
                else
                {
                    defErrRepairCount++;
                    XLogger.WARNING_Format("下载文件操作异常修复 91 url:{0} md5Test:{1} defErrRepairCount:{2} localTempSavePath:{3} ", donwloadUrl, md5Test, defErrRepairCount, localTempSavePath);
                    ReDownload();
                }
            }
        }
    }


    public void CancelDownload()
    {
#if !UNITY_WEBGL
        if (m_WebClient == null)
            return;
#endif
        Dispose();
    }

    //子线程
    private void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
    {
        Dispose();
        if (e.Error != null)
        {
            if (defErrRepairCount > 0)
            {
                error = string.Format("XWebFileClient::OnDownloadFileCompleted 85 url:{0} error:{1}", donwloadUrl, e.Error.ToString());
                //XLogger.ReportException("下载异常", error, "");
                isDone = true;
            }
            else
            {
                defErrRepairCount++;
                XLogger.WARNING_Format("下载异常修复 91 url:{0} md5Test:{1} defErrRepairCount:{2} localTempSavePath:{3}  error:{4}", donwloadUrl, md5Test, defErrRepairCount, localTempSavePath, e.Error.ToString());
                ReDownload();
                return;
            }
        }
        else if (e.Cancelled)
        {
            isClientCancelled = true;
        }
        else
        {
            try
            {
                CompleteHandle();
            }
            catch (Exception exception)
            {
                if (defErrRepairCount > 0)
                {
                    error = string.Format("XWebFileClient::OnDownloadFileCompleted 109 url:{0} error:{1}", donwloadUrl, exception.ToString());
                    //XLogger.ReportException("下载文件操作异常", error, "");
                    isDone = true;
                }
                else
                {
                    defErrRepairCount++;
                    XLogger.WARNING_Format("下载文件操作异常修复 91 url:{0} md5Test:{1} defErrRepairCount:{2} localTempSavePath:{3} ", donwloadUrl, md5Test, defErrRepairCount, localTempSavePath);
                    ReDownload();
                    return;
                }
            }
        }
    }


    private void CompleteHandle()
    {
        //不做md5校验
        if (string.IsNullOrEmpty(md5Test))
        {
            if (File.Exists(localSavePath))
                File.Delete(localSavePath);
            File.Move(localTempSavePath, localSavePath);

        }
        else if (!string.IsNullOrEmpty(md5Test))//md5校验
        {
            //md5异常修复测试代码
            //if (md5ErrRepairCount == 0)
            //    File.WriteAllText(localTempSavePath, "alsdkfjaklsdfjkalsdf");

            string fmd5 = XFileUtility.FileMd5(localTempSavePath);
            if (fmd5 == md5Test)
            {
                if (Directory.Exists(localSavePath))
                    Directory.Delete(localSavePath, true);


                if (File.Exists(localSavePath))
                {
                    //删除旧文件
                    if (XFileUtility.FileMd5(localSavePath) != md5Test)
                    {
                        File.Delete(localSavePath);
                        File.Move(localTempSavePath, localSavePath);
                    }
                    else
                        File.Delete(localTempSavePath);
                }
                else
                {
                    File.Move(localTempSavePath, localSavePath);
                }


                if (defErrRepairCount > 0 || md5ErrRepairCount > 0 || timeoutErrRepairCount > 0)
                {
                    XLogger.WARNING_Format("成功修复下载文件 url:{0} md5Test:{1} file:{2}  md5ErrRepairCount:{3} localTempSavePath:{4} ", donwloadUrl, md5Test, fmd5, md5ErrRepairCount, localTempSavePath);
                }
            }
            else
            {
                if (md5ErrRepairCount > 0)
                {
                    //校验失败删除临时文件
                    File.Delete(localTempSavePath);
                    isMD5Error = true;
                    error = string.Format("md5 error url:{0} md5Test:{1} file:{2}  md5ErrRepairCount:{3} localTempSavePath:{4} ", donwloadUrl, md5Test, fmd5, md5ErrRepairCount, localTempSavePath);
                    //XLogger.ReportException("md5异常", error, "");
                }
                else
                {
                    md5ErrRepairCount++;
                    //string random = DateTime.Now.ToString("yyyymmddhhmmss");
                    //this.version = random;
                    XLogger.WARNING_Format("md5异常修复 url:{0} md5Test:{1} file:{2}  md5ErrRepairCount:{3} localTempSavePath:{4} ", donwloadUrl, md5Test, fmd5, md5ErrRepairCount, localTempSavePath);
                    ReDownload();
                    return;
                }
            }
        }

        isDone = true;
    }


    //子线程
    private void OnDownloadDisposed(object sender, EventArgs e)
    {
        isClientDispose = true;

#if !UNITY_WEBGL
        if (m_WebClient != null)
        {
            m_WebClient.DownloadProgressChanged -= OnDownloadProgressChanged;
            m_WebClient.DownloadFileCompleted -= OnDownloadFileCompleted;
            m_WebClient.Disposed -= OnDownloadDisposed;
        }
#endif

        //XLogger.WARNING_Format("OnDownloadDisposed {0}", weburl);
    }

    //子线程
    private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        timeoutTag = 0;
        bytesReceived = (int)e.BytesReceived;
        secondByte = (int)(e.BytesReceived / m_Stopwatch.Elapsed.TotalSeconds);
        progress = e.ProgressPercentage * 0.01f;
    }

    public void Update()
    {
        timeoutTag += Time.unscaledDeltaTime;
        //Debug.Log(donwloadUrl + "    " + timeoutTag);
        if (timeoutTag >= TIME_OUT_VALUE)
        {
            if (timeoutErrRepairCount > 3)
            {
                error = string.Format("XWebFileClient::Update timeout url:{0} timeoutErrRepairCount:{1}", donwloadUrl, timeoutErrRepairCount);
                isDone = true;
                XLogger.ReportException("下载超时", error, "");
                return;
            }

            CancelDownload();

            timeoutErrRepairCount++;
            //超时重下
            XLogger.WARNING_Format("下载超时重试修复 url:{0} md5Test:{1} timeoutErrRepairCount:{2} localTempSavePath:{3} ", donwloadUrl, md5Test, timeoutErrRepairCount, localTempSavePath);
            ReDownload();
        }
    }

    public void Dispose()
    {
#if !UNITY_WEBGL
        if (m_WebClient != null)
        {
            m_WebClient.CancelAsync();
            m_WebClient.Dispose();
            m_WebClient = null;
        }
#else
        if (webGLDownCoroutine != null)
        {
            TimerManager.DelCoroutine(webGLDownCoroutine);
        }
#endif


    }
}
