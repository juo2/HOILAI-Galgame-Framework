using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using AssetManagement;
using System;

/// <summary>
/// 游戏启动更新检查
/// </summary>
public partial class LaunchUpdate : MonoBehaviour
{
    private static string s_LogFilePath = Path.Combine(AssetDefine.ExternalSDCardsPath, "launch.log");
    private static int s_LogFileSize = 1024 * 1024 * 5; //5m


    public static bool LogEnabled = true;
    public enum HotUpdateStepConst
    {
        LocalVer,
        RemoteVer,
        AssetsFiles,
        RemoteAssetsFiles,
        DownloadManifest,
        DownloaderHotUpdateAssets,
    }


    public enum HotUpdateErrorCode : int
    {
        None = 0,
        RemoteVerDownloaderError = 1000,    //服务器版本文件下载异常
        RemoteVerDecodeError = 2000,    //服务器版本文件解析异常

        RemoteFileListDownloaderError = 3000,    //服务器文件列表下载异常
        RemoteFileListDecodeError = 3002,    //服务器文件列表解析异常
        RemoteFileListMD5Error = 3003,    //服务器文件列表md5校验失败

        ManifestMD5Error = 4001,    //清单文件md5校验失败
        ManifestDownloaderError = 4003,    //清单文件下载失败

        DownloaderHotUpdateError = 4004,    //启动更新资源异常
    }

    public enum ProgressType
    {
        Checkup,            //检查资源
        Downloading,     //下载资源
    }

    //注意如果不检查更新将直接用包内资源跑，也不会启动后台下载
    public bool p_IsCheckUpdate = true;

    public System.Action<float> onDownloadVerFileProgress;
    public System.Action onUpdateComplete;

    const string c_VersionFileName = "version.txt";
    const string c_FileListFileName = "files";
    const string c_ManifestFileName = "xassetmanifest";

    private XVersionFile m_BuildingVersion;
    //本地版本文件  sdcard->StreamingAssets
    private XVersionFile m_LocalVersion;
    //服务器版本文件 
    private XVersionFile m_RemoteVersion;

    //内置文件列表
    private XAssetsFiles m_BuildinFiles;
    //本地文件列表
    private XAssetsFiles m_LocalFiles;
    //服务器文件列表
    private XAssetsFiles m_RemoteFiles;
    //远程文件列表在完成更新后保存在本地
    private byte[] m_RemoteFilesBytes;

    //启动更新列表
    private List<XAssetsFiles.FileStruct> m_UpdateNeedDownload;
    //后台更新列表
    private List<XAssetsFiles.FileStruct> m_BackgroundNeedDownload;

    //启动列表中是否有dll需要生启游戏
    private bool m_LaunchDllUpdate = false;

    private HotUpdateErrorCode m_ErrorCode;
    private string m_ErrorStr;
    void Start()
    {
        CheckUpdateLoggerSize();
        RecordLogger(string.Empty);
        RecordLogger(string.Empty);
        RecordLogger("Start ..");
        StartUpdate();
    }

    void StartUpdate()
    {
        m_ErrorCode = HotUpdateErrorCode.None;
        m_ErrorStr = string.Empty;
        SetProgress(ProgressType.Checkup, 0f);
        StartCoroutine(StartUpHotUpdate());
    }

    IEnumerator StartUpHotUpdate()
    {
        yield return null;
        //没有连接网络提示连接网络

        Debug.Log($"Application.internetReachability:{Application.internetReachability}");
        Debug.Log($"Application.internetReachability:{Application.internetReachability}");
        Debug.Log($"Application.internetReachability:{Application.internetReachability}");
        Debug.Log($"Application.internetReachability:{Application.internetReachability}");
        Debug.Log($"Application.internetReachability:{Application.internetReachability}");
        Debug.Log($"Application.internetReachability:{Application.internetReachability}");

        if (!XUtility.IsNetworkValid())
        {
            RecordLogger(string.Format("StartUpHotUpdate -> NetworkValid .. Application.internetReachability = {0}", Application.internetReachability));
            DefaultAlertGUI.Open(UpdateConst.GetLanguage(11301), UpdateConst.GetLanguage(11302), "", "", DefaultAlertGUI.ButtonOpt.None);
            while (!XUtility.IsNetworkValid())
                yield return 0;
            DefaultAlertGUI.Close();
        }

        yield return HotUpdateStep(HotUpdateStepConst.LocalVer);

        if (this.m_ErrorCode == HotUpdateErrorCode.None)
        {
            RecordLogger(string.Format("StartUpHotUpdate -> finish.."));

            OnUpdateComplete();
        }
        else
        {
            string title = UpdateConst.GetLanguage(11301);
            string content = string.Format(UpdateConst.GetLanguage(11306), m_ErrorCode.ToString() + "\n" + m_ErrorStr);

            DefaultAlertGUI alert = DefaultAlertGUI.Open(title, content, UpdateConst.GetLanguage(11305), "", DefaultAlertGUI.ButtonOpt.Sure);
            UpdateUtility.ShowTextTip(string.Format(UpdateConst.GetLanguage(1201), this.m_ErrorCode));
            XLogger.ReportException("更新检查异常", m_ErrorCode.ToString(), m_ErrorStr);
            yield return alert.Wait();
            StartUpdate();
        }
    }

    //返回当前可信度最高的文件列表
    XAssetsFiles GetValidFiles()
    {
        return m_RemoteFiles != null ? m_RemoteFiles : (m_LocalFiles != null ? m_LocalFiles : m_BuildinFiles);
    }

    IEnumerator HotUpdateStep(HotUpdateStepConst step)
    {
        if (this.m_ErrorCode != HotUpdateErrorCode.None)
        {
            RecordLogger(string.Format("HotUpdateStep -> m_ErrorCode = {0}   m_ErrorStr = {1}", this.m_ErrorCode, this.m_ErrorStr));
            yield break;
        }
        else
        {
            this.m_ErrorCode = HotUpdateErrorCode.None;
        }

        bool isSDKPattern = XConfig.defaultConfig.isSDKPattern;//SDK模式
        switch (step)
        {
            case HotUpdateStepConst.LocalVer:  //检查本地版本
                {

                    Debug.Log($"Application.temporaryCachePath:{Application.temporaryCachePath}");
                    Debug.Log($"Application.persistentDataPath:{Application.persistentDataPath}");
                    Debug.Log($"Application.dataPath:{Application.dataPath}");

                    Debug.Log($"AssetDefine.ExternalSDCardsPath:{AssetDefine.ExternalSDCardsPath}");

                    CheckLocalVersion();
                    SetProgress(ProgressType.Checkup, 0.1f, UpdateConst.GetLanguage(11001));

                    if (m_LocalVersion == null)
                    {
                        RecordLogger(string.Format("HotUpdateStep -> {0}  p_IsCheckUpdate = {1} Ver == null ", step, p_IsCheckUpdate));
                    }
                    else
                    {
                        RecordLogger(string.Format("HotUpdateStep -> {0}  p_IsCheckUpdate = {1} Ver = {2} ", step, p_IsCheckUpdate, UpdateUtility.GetVersionStrInfo(m_LocalVersion)));
                    }

                    if (p_IsCheckUpdate)
                        yield return HotUpdateStep(HotUpdateStepConst.RemoteVer);
                    else //不检更新
                        yield return CheckLocalAssetFiles();

                    ////暂定为第一次安装解压完成
                    //if (isSDKPattern && isFirstInstall)
                    //    HmlSdkProxy.instance.UploadUnzip(2);

                    break;
                }
            case HotUpdateStepConst.RemoteVer: //检查服务器版本
                {
                    if (LogEnabled)
                    {
                        XLogger.DEBUG("LaunchUpdate::HotUpdateStep HotUpdateStepConst.RemoteVer. start");
                    }

                    yield return CheckRemoteVersionCoroutine();
                    if (m_ErrorCode != HotUpdateErrorCode.None)
                    {
                        m_ErrorCode = HotUpdateErrorCode.None;
                        if (AssetDefine.RemoteSpareUrls.Count > 0)
                        {
                            XLogger.ERROR_Format("LaunchUpdate::HotUpdateStep 切换cdn from:{0}  to:{1}", AssetDefine.RemoteDownloadUrl, AssetDefine.RemoteSpareUrls[0]);
                            AssetDefine.RemoteDownloadUrl = AssetDefine.RemoteSpareUrls[0];
                            yield return CheckRemoteVersionCoroutine();
                        }
                    }

                    SetProgress(ProgressType.Checkup, 0.2f);


                    if (m_RemoteVersion == null)
                    {
                        RecordLogger(string.Format("HotUpdateStep -> {0} p_IsCheckUpdate = {1} Ver == null ", step, p_IsCheckUpdate));
                    }
                    else
                    {
                        RecordLogger(string.Format("HotUpdateStep -> {0} p_IsCheckUpdate = {1} Ver = {2} ", step, p_IsCheckUpdate, UpdateUtility.GetVersionStrInfo(m_RemoteVersion)));
                    }

                    if (LogEnabled)
                    {
                        XLogger.DEBUG("LaunchUpdate::HotUpdateStep HotUpdateStepConst.RemoteVer. end");
                    }

                    yield return HotUpdateStep(HotUpdateStepConst.AssetsFiles);
                    break;
                }
            case HotUpdateStepConst.AssetsFiles: //检查本地文件列表
                {
                    if (LogEnabled)
                    {
                        XLogger.DEBUG("LaunchUpdate::HotUpdateStep HotUpdateStepConst.AssetsFiles. start");
                    }

                    yield return CheckLocalAssetFiles();
                    SetProgress(ProgressType.Checkup, 0.3f, UpdateConst.GetLanguage(11003));
                    if (LogEnabled)
                    {
                        XLogger.DEBUG("LaunchUpdate::HotUpdateStep HotUpdateStepConst.AssetsFiles. end");
                    }

                    bool change = isVerChange();
                    RecordLogger(string.Format("HotUpdateStep -> {0} isVerChange = {1}", step, change));
                    if (change) //版本发生变化
                        yield return HotUpdateStep(HotUpdateStepConst.RemoteAssetsFiles);
                    else //版本未发生变化校验本地文件
                        yield return HotUpdateStep(HotUpdateStepConst.DownloaderHotUpdateAssets);
                    break;
                }
            case HotUpdateStepConst.RemoteAssetsFiles: //检查远程文件列表
                {
                    RecordLogger(string.Format("HotUpdateStep -> {0}", step));
                    if (LogEnabled)
                    {
                        XLogger.DEBUG("LaunchUpdate::HotUpdateStep HotUpdateStepConst.RemoteAssetsFiles. start");
                    }
                    yield return CheckRemoteAssetFilesCoroutine();
                    SetProgress(ProgressType.Checkup, 0.4f);
                    if (LogEnabled)
                    {
                        XLogger.DEBUG("LaunchUpdate::HotUpdateStep HotUpdateStepConst.RemoteAssetsFiles. end");
                    }

                    yield return HotUpdateStep(HotUpdateStepConst.DownloadManifest);
                    break;
                }
            case HotUpdateStepConst.DownloadManifest: //下载清单文件
                {
                    RecordLogger(string.Format("HotUpdateStep -> {0}", step));
                    if (LogEnabled)
                    {
                        XLogger.DEBUG("LaunchUpdate::HotUpdateStep HotUpdateStepConst.DownloadManifest. start");
                    }

                    yield return CheckDownloadMainfest();

                    SetProgress(ProgressType.Checkup, 0.5f);

                    if (LogEnabled)
                    {
                        XLogger.DEBUG("LaunchUpdate::HotUpdateStep HotUpdateStepConst.DownloadManifest. end");
                    }
                    yield return HotUpdateStep(HotUpdateStepConst.DownloaderHotUpdateAssets);
                    break;
                }
            case HotUpdateStepConst.DownloaderHotUpdateAssets: //收集并下载启动更新资源
                {
                    RecordLogger(string.Format("HotUpdateStep -> {0}", step));
                    if (LogEnabled)
                    {
                        XLogger.DEBUG("LaunchUpdate::HotUpdateStep HotUpdateStepConst.DownloaderHotUpdateAssets. start");
                    }

                    //if (isSDKPattern)
                    //    HmlSdkProxy.instance.UploadUpdate(1);

                    yield return CollectNeedDownloadFiles();
                    SetProgress(ProgressType.Checkup, 1f);
                    if (LogEnabled)
                    {
                        XLogger.DEBUG("LaunchUpdate::HotUpdateStep HotUpdateStepConst.DownloaderHotUpdateAssets. end");
                    }

                    UpdateUtility.ShowTextTip(string.Format(UpdateConst.GetLanguage(5000), m_UpdateNeedDownload.Count, m_BackgroundNeedDownload.Count, m_LaunchDllUpdate, GetValidFiles().p_FileCount));
                    if (LogEnabled)
                        XLogger.INFO(string.Format("m_UpdateNeedDownload:{0} m_BackgroundNeedDownload:{1} m_LaunchDllUpdate:{2} m_RemoteFiles:{3}", m_UpdateNeedDownload.Count, m_BackgroundNeedDownload.Count, m_LaunchDllUpdate, GetValidFiles().p_FileCount));


                    yield return DownloadUpdateAssets();

                    SetProgress(ProgressType.Downloading, 1f);
                    break;
                }
            default:
                break;
        }
    }

    void RecordLogger(string content)
    {
        XLogger.RecordLog(s_LogFilePath, content, "Launch ");
    }

    void CheckUpdateLoggerSize()
    {
        FileInfo finfo = new FileInfo(s_LogFilePath);
        if (finfo.Exists && finfo.Length >= s_LogFileSize)
            finfo.Delete();
    }

    void SetProgress(ProgressType t, float v, string desc = "")
    {
        if (t == ProgressType.Checkup)
        {
            //DefaultLoaderGUI.SetProgressText(UpdateConst.GetLanguage(10002));
            DefaultLoaderGUI.SetProgress(v);
        }
        else
        {
            //DefaultLoaderGUI.SetProgressText(UpdateConst.GetLanguage(10003));
            DefaultLoaderGUI.SetProgress(v);
        }

        if (!string.IsNullOrEmpty(desc))
        {
            DefaultLoaderGUI.SetContenText(desc);
        }
    }


    void OnUpdateComplete()
    {
        UpdateUtility.ShowTextTip(UpdateConst.GetLanguage(10000));

        if (m_RemoteFiles != null)
        {
            string verpath = AssetDefine.ExternalSDCardsPath + c_VersionFileName;
            XFileUtility.WriteText(verpath, JsonUtility.ToJson(m_RemoteVersion, true));
            string flspath = AssetDefine.ExternalSDCardsPath + c_FileListFileName;
            XFileUtility.WriteBytes(flspath, m_RemoteFilesBytes);
            m_RemoteFilesBytes = null;
        }

        if (onUpdateComplete != null)
        {
            onUpdateComplete.Invoke();
        }

        if (p_IsCheckUpdate)
        {
            //启动后台下载
            StartCoroutine(LaunchBackgroundDownloader());
        }
    }

}
