using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using AssetManagement;

public partial class LaunchUpdate
{
    //首包路径
    public static string FIRST_PACK = "FIRST_PACK";

    //是否第一次安装
    private bool isFirstInstall = false;
    //检查本地版本

    //是否覆盖安装
    public bool isOverwriteInstall = false;
    void CheckLocalVersion()
    {
        UpdateUtility.ShowTextTip(UpdateConst.GetLanguage(1000));
        if (LogEnabled)
            XLogger.INFO("LaunchUpdate::CheckVersion. begin");

        string firstPath = PlayerPrefs.GetString(FIRST_PACK);

        XLogger.INFO("firstPath:" + firstPath);

        if (string.IsNullOrEmpty(firstPath))
        {
            XLogger.INFO("首次装包");
            PlayerPrefs.SetString(FIRST_PACK, AssetManagement.AssetDefine.BuildinAssetPath);
        }
        else
        {
            if (firstPath.Equals(AssetManagement.AssetDefine.BuildinAssetPath))
            {
                XLogger.INFO("正常打开");
            }
            else
            {
                XLogger.INFO("覆盖安装");
                XLogger.INFO("AssetManagement.AssetDefine.BuildinAssetPath:" + AssetManagement.AssetDefine.BuildinAssetPath);

                PlayerPrefs.SetString(FIRST_PACK, AssetManagement.AssetDefine.BuildinAssetPath);

                XLogger.INFO("PlayerPrefs.GetString(FIRST_PACK):" + PlayerPrefs.GetString(FIRST_PACK));

                isOverwriteInstall = true;
            }
        }


        string sdcardVerPath = Path.Combine(AssetDefine.ExternalSDCardsPath, c_VersionFileName);

        bool sdcardValidate = false;

        if (File.Exists(sdcardVerPath))
        {
            //扩展卡版本文件载入
            string versionJson = File.ReadAllText(sdcardVerPath);
            if (string.IsNullOrEmpty(versionJson))
            {
                //内容为空
                XLogger.INFO(string.Format("LaunchUpdate::CheckVersion() sdcard version is Empty ! path={0}", sdcardVerPath));
                UpdateUtility.ShowTextTip(UpdateConst.GetLanguage(1010));

            }
            else
            {
                try
                {
                    m_LocalVersion = UpdateUtility.DeVersion(versionJson, sdcardVerPath);
                    sdcardValidate = true;
                }
                catch (System.Exception e)
                {
                    //解析异常
                    XLogger.INFO(string.Format("LaunchUpdate::CheckVersion() sdcard version decode error ! path={0} error={1} data={2}", sdcardVerPath, e.ToString(), versionJson));
                    UpdateUtility.ShowTextTip(UpdateConst.GetLanguage(1020));
                }
            }
        }
        else
        {
            //暂定为第一次安装解压,有好的方法在移动
            isFirstInstall = true;
        }

        //若扩展卡版本文件不存在或是出现异常则使用内置版本文件
        if (!sdcardValidate)
        {
            //为了正确识别覆盖安装，扩展卡没有version但有代码则删掉代码
            string codeDir = Path.Combine(AssetDefine.ExternalSDCardsPath, "00");
            if (Directory.Exists(codeDir))
                Directory.Delete(codeDir, true);

            //内置版本文件载入
            if (LogEnabled)
                XLogger.INFO(string.Format("LaunchUpdate::CheckVersion() load buildin version ! path={0}", sdcardVerPath));
            string buildinVerPath = Path.Combine(AssetDefine.BuildinAssetPath, c_VersionFileName);

            string versionJson = string.Empty;
            string error = string.Empty;

            versionJson = XFileUtility.ReadStreamingFile(buildinVerPath, out error);

            if (string.IsNullOrEmpty(error))
            {
                //内容为空
                if (string.IsNullOrEmpty(versionJson))
                {

                    UpdateUtility.ShowTextTip(UpdateConst.GetLanguage(1100));
                    XLogger.INFO(string.Format("LaunchUpdate::CheckVersion() buildin version error ! path={0} error={1}", buildinVerPath, "content is Empty"));
                }
                else
                {
                    try
                    {
                        m_LocalVersion = UpdateUtility.DeVersion(versionJson, buildinVerPath);
                    }
                    catch (System.Exception e)
                    {
                        UpdateUtility.ShowTextTip(UpdateConst.GetLanguage(1101));
                        //解析异常
                        XLogger.INFO(string.Format("LaunchUpdate::CheckVersion() buildin version decode error! path={0} error={1} data={2}", buildinVerPath, e.ToString(), versionJson));
                    }
                }
            }
        }


        if (LogEnabled)
            XLogger.INFO("LaunchUpdate::CheckVersion. end");

        XAssetsFiles.s_CurrentVersion = m_LocalVersion;
        //显示版本号
        UpdateUtility.SetUIVersionInfo(m_LocalVersion, null);
    }

    //检查远程版本
    IEnumerator CheckRemoteVersionCoroutine()
    {
        string random = DateTime.Now.ToString("yyyymmddhhmmss");
        string remoteVerUrl = AssetDefine.RemoteDownloadUrl + c_VersionFileName + "?v=" + random;
        RecordLogger(string.Format("CheckRemoteVersionCoroutine. remoteVerUrl: {0}", remoteVerUrl));
        using (UnityWebRequest uwr = UnityWebRequest.Get(remoteVerUrl))
        {
            UnityWebRequestAsyncOperation async = uwr.SendWebRequest();
            while (!async.isDone)
            {
                if (onDownloadVerFileProgress != null) onDownloadVerFileProgress.Invoke(async.progress);
                SetProgress(ProgressType.Checkup, 0.1f + async.progress * 0.1f, UpdateConst.GetLanguage(11002));
                yield return 0;
            }

            if (!string.IsNullOrEmpty(uwr.error) || uwr.result == UnityWebRequest.Result.ProtocolError || uwr.result == UnityWebRequest.Result.ConnectionError)
            {
                m_ErrorCode = HotUpdateErrorCode.RemoteVerDownloaderError;
                XLogger.ERROR(string.Format("LaunchUpdate::CheckRemoteVersionCoroutine() remote version download error ! remoteVerUrl={0} error={1} url={2}", remoteVerUrl, uwr.error, remoteVerUrl));
                yield break;
            }

            string jsonData = uwr.downloadHandler.text;
            if (string.IsNullOrEmpty(jsonData))
            {
                m_ErrorCode = HotUpdateErrorCode.RemoteVerDecodeError;
                XLogger.ERROR(string.Format("LaunchUpdate::CheckRemoteVersionCoroutine() remote version content is Empty ! path={0} error={1} url={2}", remoteVerUrl, "content is Empty", remoteVerUrl));
            }
            else
            {
                try
                {

                    m_RemoteVersion = UpdateUtility.DeVersion(jsonData, remoteVerUrl);
                }
                catch (System.Exception e)
                {
                    m_ErrorCode = HotUpdateErrorCode.RemoteVerDecodeError;
                    XLogger.INFO(string.Format("LaunchUpdate::CheckRemoteVersionCoroutine() remote version decode error! path={0} error={1} data={2}", remoteVerUrl, e.ToString(), jsonData));
                }
            }
        }

        XAssetsFiles.s_CurrentVersion = m_RemoteVersion;
        //显示版本号
        UpdateUtility.SetUIVersionInfo(m_LocalVersion, m_RemoteVersion);
    }

    //版本是否发生改变
    bool isVerChange()
    {
        //没有文件列表
        if (GetValidFiles() == null)
            return true;

        //如果本地文件列表存在表示更新过，但是清单文件意外丢失
        string filePath = AssetDefine.ExternalSDCardsPath + c_FileListFileName;
        string manifestFilePath = AssetDefine.ExternalSDCardsPath + c_ManifestFileName;
        if (File.Exists(filePath) && !File.Exists(manifestFilePath))
            return true;

        if (m_RemoteVersion != null)
        {

            //文件列表md5不对需要更新
            if (File.Exists(filePath))
            {
                string fmd5 = XFileUtility.FileMd5(filePath);
                if (fmd5 != m_RemoteVersion.p_files_md5)
                {
                    RecordLogger(string.Format("p_files_md5: {0} -> {1}", fmd5, m_RemoteVersion.p_files_md5));
                    return true;
                }
            }


            //清单文件列表不对需要更新
            if (File.Exists(manifestFilePath))
            {
                string fmd5 = XFileUtility.FileMd5(manifestFilePath);
                if (fmd5 != m_RemoteVersion.p_manifest_md5)
                {
                    RecordLogger(string.Format("p_manifest_md5: {0} -> {1}", fmd5, m_RemoteVersion.p_manifest_md5));
                    return true;
                }

            }
        }


        //版本改表
        if (m_LocalVersion != null && m_RemoteVersion != null)
        {
            if (m_LocalVersion.p_DevVersion.gitVer != m_RemoteVersion.p_DevVersion.gitVer ||
                //本地文件列表或清单md5对应不上也需要下载新的文件列表与清单
                m_LocalVersion.p_files_md5 != m_RemoteVersion.p_files_md5 ||
                m_LocalVersion.p_manifest_md5 != m_RemoteVersion.p_manifest_md5)
            {
                return true;
            }
            return false;
        }
        return true;
    }

    //检查本地文件列表
    IEnumerator CheckLocalAssetFiles()
    {
        if (LogEnabled)
            XLogger.DEBUG("LaunchUpdate::CheckLocalFileList. start");
        string sdcardPath = Path.Combine(AssetDefine.ExternalSDCardsPath, c_FileListFileName);
        if (File.Exists(sdcardPath))
        {
            AssetBundleCreateRequest abcr = AssetBundle.LoadFromFileAsync(sdcardPath);
            yield return abcr;
            //扩展卡版本文件载入
            string jsonData = string.Empty;
            yield return UpdateUtility.ReadAssetListAsync(abcr.assetBundle, (string data) => { jsonData = data; });
            if (string.IsNullOrEmpty(jsonData))
            {
                //内容为空
                XLogger.DEBUG(string.Format("LaunchUpdate::CheckLocalFileList() sdcard assetFiles is Empty ! path={0}", sdcardPath));
            }
            else
            {
                try
                {
                    m_LocalFiles = UpdateUtility.DeFileList(jsonData);
                    XAssetsFiles.s_CurrentAssets = m_LocalFiles;
                }
                catch (System.Exception e)
                {
                    //解析异常
                    XLogger.DEBUG(string.Format("LaunchUpdate::CheckLocalFileList() sdcard assetFiles decode error ! path={0} error={1} data={2}", sdcardPath, e.ToString(), jsonData));
                }
            }
        }


        //内置文件列表
        //if (!sdcardValidate)
        {
            //内置版本文件载入
            if (LogEnabled)
                XLogger.DEBUG(string.Format("LaunchUpdate::CheckLocalFileList() load buildin assetFiles ! path={0}", sdcardPath));
            string buildinPath = Path.Combine(AssetDefine.BuildinAssetPath, c_FileListFileName);
            string jsonData = string.Empty;
            bool isExist = true;

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS
            isExist = File.Exists(buildinPath);
#endif
            if (isExist)
            {
                AssetBundleCreateRequest abcr = AssetBundle.LoadFromFileAsync(buildinPath);
                yield return abcr;
                //扩展卡版本文件载入
                yield return UpdateUtility.ReadAssetListAsync(abcr.assetBundle, (string data) => { jsonData = data; });
            }


            if (string.IsNullOrEmpty(jsonData))
            {
                //内容为空
                XLogger.DEBUG(string.Format("LaunchUpdate::CheckLocalFileList() buildin assetFiles error ! path={0} error={1}", buildinPath, "content is Empty"));
            }
            else
            {
                try
                {
                    m_BuildinFiles = UpdateUtility.DeFileList(jsonData);
                    XAssetsFiles.s_BuildingtAssets = m_BuildinFiles;
                    if (XAssetsFiles.s_CurrentAssets == null)
                        XAssetsFiles.s_CurrentAssets = m_BuildinFiles;
                }
                catch (System.Exception e)
                {
                    //解析异常
                    XLogger.DEBUG(string.Format("LaunchUpdate::CheckLocalFileList() buildin version decode error! path={0} error={1} data={2}", buildinPath, e.ToString(), jsonData));
                }
            }
        }
        if (LogEnabled)
            XLogger.DEBUG("LaunchUpdate::CheckLocalFileList. end");
    }


    //检查服务器文件列表
    IEnumerator CheckRemoteAssetFilesCoroutine()
    {
        string tempPath = AssetManager.Instance.AssetLoaderOptions.GetAssetDownloadSavePath(c_FileListFileName + ".dtemp");
        AssetFileThreadDownloader downloader = AssetFileThreadDownloader.Get(c_FileListFileName, tempPath, m_RemoteVersion.p_files_md5, m_RemoteVersion.p_files_md5);

        while (!downloader.IsDone())
        {
            SetProgress(ProgressType.Checkup, 0.3f + downloader.progress * 0.1f, UpdateConst.GetLanguage(11004));
            yield return 0;
        }

        //下载出现异常
        if (!string.IsNullOrEmpty(downloader.Error))
        {
            m_ErrorCode = HotUpdateErrorCode.RemoteFileListDownloaderError;

            m_ErrorStr = string.Format("LaunchUpdate::CheckRemoteAssetFilesCoroutine() downloader err ! path={0} error={1}", downloader.WebUrl, downloader.Error);
            XLogger.ERROR(m_ErrorStr);
            yield break;
        }


        try
        {
            if (File.Exists(downloader.DownloadPath))
            {
                m_RemoteFilesBytes = File.ReadAllBytes(downloader.DownloadPath);
                File.Delete(downloader.DownloadPath);
            }

        }
        catch (System.Exception ex)
        {
            XLogger.ERROR(string.Format("LaunchUpdate::CheckRemoteAssetFilesCoroutine() downloader err ! path={0} error={1}", downloader.WebUrl, ex.ToString()));
            yield break;
        }


        string jsonData = string.Empty;
        try
        {
            //解析
            jsonData = UpdateUtility.ReadAssetList(AssetBundle.LoadFromMemory(m_RemoteFilesBytes));
        }
        catch (System.Exception ex)
        {
            XLogger.ERROR(string.Format("LaunchUpdate::CheckRemoteAssetFilesCoroutine() path={0} error={1}", downloader.WebUrl, ex.ToString()));
        }


        if (string.IsNullOrEmpty(jsonData))
        {
            //解析异常
            m_ErrorCode = HotUpdateErrorCode.RemoteFileListDecodeError;
            m_ErrorStr = string.Format("LaunchUpdate::CheckRemoteAssetFilesCoroutine() remote assetFiles content is Empty ! path={0} error={1}", downloader.WebUrl, "content is Empty");
            XLogger.ERROR(m_ErrorStr);
        }
        else
        {
            try
            {

                m_RemoteFiles = UpdateUtility.DeFileList(jsonData);
                XAssetsFiles.s_CurrentAssets = m_RemoteFiles;
            }
            catch (System.Exception e)
            {
                m_ErrorCode = HotUpdateErrorCode.RemoteFileListDecodeError;
                m_ErrorStr = string.Format("LaunchUpdate::CheckRemoteAssetFilesCoroutine() remote assetFiles decode error! path={0} error={1} data={2}", downloader.WebUrl, e.ToString(), jsonData);
                XLogger.ERROR(m_ErrorStr);
            }
        }
    }

    //下载清单文件
    IEnumerator CheckDownloadMainfest()
    {
        AssetFileThreadDownloader downloader = AssetFileThreadDownloader.Get(c_ManifestFileName, null, m_RemoteVersion.p_manifest_md5);
        while (!downloader.IsDone())
        {
            SetProgress(ProgressType.Checkup, 0.4f + downloader.GetProgress() * 0.1f, UpdateConst.GetLanguage(11004));
            yield return 0;
        }
        yield return downloader;

        if (!string.IsNullOrEmpty(downloader.Error))
        {
            if (downloader.state == AssetDownloader.State.ErrorMd5)
                m_ErrorCode = HotUpdateErrorCode.ManifestMD5Error;
            else
                m_ErrorCode = HotUpdateErrorCode.ManifestDownloaderError;

            m_ErrorStr = string.Format("LaunchUpdate::CheckDownloadMainfest() downloader err ! path={0} error={1}", downloader.WebUrl, downloader.Error);
            XLogger.ERROR(m_ErrorStr);
            yield break;
        }
    }

    //收集需要下载的文件
    IEnumerator CollectNeedDownloadFiles()
    {
        int downloadTag = XConfig.defaultConfig.initDownloadTag;
        RecordLogger(string.Format("HotUpdateStep -> {0}  downloadTag={1}.", "CollectNeedDownloadFiles", downloadTag));
        m_UpdateNeedDownload = new List<XAssetsFiles.FileStruct>();
        m_BackgroundNeedDownload = new List<XAssetsFiles.FileStruct>();

        XAssetsFiles files = GetValidFiles();//服务器、本地、包内
        XAssetsFiles localFiles = m_LocalFiles != null ? m_LocalFiles : m_BuildinFiles;

        int count = 0;
        int totalCount = files.p_AllFiles.Count;
        bool isdone = false;

#if !UNITY_WEBGL
        System.Threading.ThreadPool.QueueUserWorkItem((object state) =>
        {
#endif
            if (LogEnabled)
                XLogger.DEBUG("LaunchUpdate::CollectNeedDownloadFiles. start");
            foreach (var asset in files.p_AllFiles)
            {
                count++;

                string sdcardPath = AssetDefine.ExternalSDCardsPath + asset.path;
                //string fileMd5 = string.Empty;
                bool exists = File.Exists(sdcardPath);
                //if (exists)
                //fileMd5 = XFileUtility.FileMd5(sdcardPath);

                XAssetsFiles.FileStruct add = null;

                //本地或首包
                XAssetsFiles.FileStruct file = null;

                //是否为最新    //sd卡有并且md5也对应得上 表示为最新的文件
                bool isNewest = exists && localFiles != null &&
                                localFiles.allFilesMap.TryGetValue(asset.path, out file) && file.md5 == asset.md5;

                bool isBuilding = (asset.options & XAssetsFiles.FileOptions.BUILDING) == XAssetsFiles.FileOptions.BUILDING;
                if (isBuilding) //是否为首包
                {
                    //此资源为内置/首包资源 
                    //但内置/首包中不存在 或者 已经发生变化！需要下载
                    if (m_BuildinFiles == null || !m_BuildinFiles.allFilesMap.TryGetValue(asset.path, out file) || file.md5 != asset.md5)
                    {
                        //并且本地没有最新的
                        if (!isNewest)
                            add = asset;
                    }
                }
                else
                {
                    //不是内置/首包资源

                    // 本地不存在  或者 文件不是最新的！需要下载   md5超慢放弃
                    //if (string.IsNullOrEmpty(fileMd5) || fileMd5 != asset.md5)
                    //{
                    //    add = asset;
                    //}

                    // 本地不存在  或者 文件不是最新的！需要下载
                    if (!exists || localFiles == null || !localFiles.allFilesMap.TryGetValue(asset.path, out file) || file.md5 != asset.md5)
                    {
                        add = asset;
                    }
                }

                if (add == null)
                    continue;

                //本地的文件不是最新的文件了需要删掉
                if (exists)
                {
                    if (((add.options & XAssetsFiles.FileOptions.LUA) == XAssetsFiles.FileOptions.LUA))
                    { }
                    else
                        File.Delete(sdcardPath);
                }


                if (downloadTag != -1 && downloadTag == add.tag)
                {
                    //启动需要下载的扩展包
                    if ((add.options & XAssetsFiles.FileOptions.DLL) == XAssetsFiles.FileOptions.DLL)
                    {
                        if (DeleteDllUpdate(add))
                        {
                            m_LaunchDllUpdate = true;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    m_UpdateNeedDownload.Add(add);

                    RecordLogger(string.Format("tagDownload                   -> name={0}  size={1} tag={2} exist={3} isBuilding={4} smd5={5} lmd5={6}",
                        add.path, XUtility.FormatBytes(add.size), add.tag, exists, isBuilding, add.md5, file != null ? file.md5 : "null"));

                }
                else if ((add.options & XAssetsFiles.FileOptions.LAUNCHDOWNLOAD) == XAssetsFiles.FileOptions.LAUNCHDOWNLOAD)
                {
                    //启动需要下载
                    if ((add.options & XAssetsFiles.FileOptions.DLL) == XAssetsFiles.FileOptions.DLL)
                    {
                        if (DeleteDllUpdate(add))
                        {
                            m_LaunchDllUpdate = true;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    m_UpdateNeedDownload.Add(add);

                    RecordLogger(string.Format("LaunchDownload                 -> name={0}  size={1} tag={2} exist={3} isBuilding={4} smd5={5} lmd5={6}",
                        add.path, XUtility.FormatBytes(add.size), add.tag, exists, isBuilding, add.md5, file != null ? file.md5 : "null"));
                }
                else
                {
                    //后台需要下载
                    m_BackgroundNeedDownload.Add(add);

                }
            }
            isdone = true;
            if (LogEnabled)
                XLogger.DEBUG("LaunchUpdate::CollectNeedDownloadFiles. end");

#if !UNITY_WEBGL
        });

        while (!isdone)
        {
            float p = (float)count / (float)totalCount;
            SetProgress(ProgressType.Checkup, 0.5f + p * 0.5f, string.Format(UpdateConst.GetLanguage(11005), p * 100));
            yield return 0;
        }
#else
        yield return 0;
#endif

        SetProgress(ProgressType.Checkup, 1f, string.Format(UpdateConst.GetLanguage(11005), 100f));
    }

    bool DeleteDllUpdate(XAssetsFiles.FileStruct add)
    {
        bool isCanUpdate = false;
#if ENABLE_IL2CPP && UNITY_ANDROID

//#if DEVELOPMENT_BUILD
//        if (add.path.Contains("00000000000000000000000000000002") && add.path.Contains(Bootstrap.get_arch_abi()) && add.path.Contains("debug"))
//        {
//            //通过
//            isCanUpdate = true;
//        }
//#else
//        if (add.path.Contains("00000000000000000000000000000002") && add.path.Contains(Bootstrap.get_arch_abi()) && !add.path.Contains("debug"))
//        {
//            //通过
//            isCanUpdate = true;
//        }
//#endif
        
#else
        if (add.path.Contains("00000000000000000000000000000001"))
        {

            //通过
            isCanUpdate = true;
        }
#endif

        return isCanUpdate;
    }
     
    //下载启动更新文件
    IEnumerator DownloadUpdateAssets()
    {
        if (m_UpdateNeedDownload.Count < 1)
        {
            RecordLogger(string.Format("HotUpdateStep ->  {0}  count={1} ", "DownloadUpdateAssets", 0));
            SetProgress(ProgressType.Downloading, 1);
            yield break;
        }

        bool isFastUpdate = true;
        AssetFileDownloadQueue download = gameObject.AddComponent<AssetFileDownloadQueue>();
        download.onBeginStep = () => { RecordLogger(string.Format("HotUpdateStep::onBeginDownload                                ->  {0} ", download.currentDownloader.WebUrl)); };
        download.onEndStep = () =>
        {
            string err = download.currentDownloader.Error;
            err = string.IsNullOrEmpty(err) ? "" : "Error=" + err;
            RecordLogger(string.Format("HotUpdateStep::onEndStep                                ->  {0}    {1}", download.currentDownloader.WebUrl, err));
        };

        download.errorPause = true;
        download.SetFiles(m_UpdateNeedDownload);

        RecordLogger(string.Format("HotUpdateStep ->  {0}  count={1} totalSize={2}.", "DownloadUpdateAssets", download.totalFileCount, XUtility.FormatBytes(download.bytesTotal)));

//#if !UNITY_EDITOR
        //提示下载大小
        string tipContent = string.Format(UpdateConst.GetLanguage(XUtility.IsNetworkWifi() ? 10010 : 10011), XUtility.FormatBytes(download.bytesTotal * XConfig.defaultConfig.downloadSizeFactor));
        DefaultAlertGUI alertDownloadTip = DefaultAlertGUI.Open("", tipContent, UpdateConst.GetLanguage(11206), "", DefaultAlertGUI.ButtonOpt.Sure);
        yield return alertDownloadTip.Wait();
//#endif

        download.StartDownload();
        float progress = 0;
        while (!download.isDone)
        {
            if (download.isPause)
            {
                //下载出现异常

                if (!XUtility.IsNetworkValid())
                {
                    //下载过程中把网络关掉了
                    DefaultAlertGUI.Open(UpdateConst.GetLanguage(11301), UpdateConst.GetLanguage(11302), "", "", DefaultAlertGUI.ButtonOpt.None);
                    while (!XUtility.IsNetworkValid()) yield return 0;
                    DefaultAlertGUI.Close();
                    download.StartDownload();
                }
                else
                {

                    //string content = string.Format(UpdateConst.GetLanguage(11303), download.currentFS.path + " \n" + download.error);
                    string content = UpdateConst.GetLanguage(11303);
                    string title = UpdateConst.GetLanguage(11301);
                    string sureStr = UpdateConst.GetLanguage(11304);
                    DefaultAlertGUI alert = DefaultAlertGUI.Open(title, content, sureStr, "", DefaultAlertGUI.ButtonOpt.Sure);

                    XLogger.ReportException("更新下载异常", download.currentFS.path, download.error);

                    //等待玩家点击重试
                    //yield return alert.Wait();
                    m_ErrorCode = HotUpdateErrorCode.DownloaderHotUpdateError;
                    m_ErrorStr = content;
                    break;
                }
            }
            else
            {
                string tip = string.Empty;
                int showBytesReceived = download.bytesReceived;
                if (showBytesReceived > download.bytesTotal)
                    showBytesReceived = download.bytesTotal;
                if (isFastUpdate || Time.frameCount % 20 == 0)
                {
                    tip = string.Format(UpdateConst.GetLanguage(11006),
                                                XUtility.FormatBytes(showBytesReceived * XConfig.defaultConfig.downloadSizeFactor),
                                                XUtility.FormatBytes(download.bytesTotal * XConfig.defaultConfig.downloadSizeFactor),
                                                Mathf.Clamp(download.currentFileIdx + 1, 0, download.totalFileCount),
                                                Mathf.Clamp(download.totalFileCount, 0, download.totalFileCount),
                                                XUtility.FormatBytes(download.downloadSpeed));
                }


                float cprogress = (float)showBytesReceived / (float)download.bytesTotal;
                if (cprogress > progress)
                    progress = cprogress;
                SetProgress(ProgressType.Downloading, progress, tip);
            }
            yield return 0;
        }
    }

    IEnumerator CollectBackgroundDownloaderFiles()
    {
        //版本没有发生变化，将不会走到 CollectNeedDownloadFiles 不会收集后台需要下载的资源
        if (m_BackgroundNeedDownload == null)
        {
            RecordLogger(string.Format("HotUpdateStep -> {0}", "CollectBackgroundDownloaderFiles"));
            XAssetsFiles localFiles = m_LocalFiles != null ? m_LocalFiles : m_BuildinFiles;
            XAssetsFiles files = GetValidFiles();

            int count = 0;
            int totalCount = files.p_AllFiles.Count;
            bool isdone = false;

#if !UNITY_WEBGL
            System.Threading.ThreadPool.QueueUserWorkItem((object state) =>
            {
#endif
                if (LogEnabled)
                    XLogger.DEBUG("LaunchUpdate::CollectBackgroundDownloaderFiles. start");

                m_BackgroundNeedDownload = new List<XAssetsFiles.FileStruct>();
                foreach (var asset in files.p_AllFiles)
                {
                    count++;
                    string sdcardPath = AssetDefine.ExternalSDCardsPath + asset.path;

                    bool exists = File.Exists(sdcardPath);

                    XAssetsFiles.FileStruct add = null;

                    XAssetsFiles.FileStruct file;
                    if (m_BuildinFiles == null || !m_BuildinFiles.allFilesMap.TryGetValue(asset.path, out file) || file.md5 != asset.md5)
                    {
                        //首包不存在
                        add = asset;

                    }
                    if (!exists || localFiles == null || !localFiles.allFilesMap.TryGetValue(asset.path, out file) || file.md5 != asset.md5)
                    {
                        //本地不存在此资源
                        add = asset;
                    }
                    else
                    {
                        //本地存在的文件，究竟需不需要md5验证下是不是发生改变，md5验证将会延长文件校验时间
                        //如果不用文件本身的md5,可能会有一种情况。玩家在后台下载的过程中，关掉游戏，此时本地文件列表是服务器最新文件列表，
                        //再次启动游戏文件列表对比将筛选不出来发生改变的同名文件，这个时候文件的md5跟列表中的md5对比就可以解决.
                        //try
                        //{
                        //    string fmd5 = XFileUtility.FileMd5(sdcardPath);
                        //    if (localFiles == null || !localFiles.allFilesMap.TryGetValue(asset.path, out file) || fmd5 != asset.md5)
                        //    {
                        //        add = asset;
                        //    }
                        //}
                        //catch (System.Exception ex)
                        //{
                        //    XLogger.ERROR(string.Format("LaunchUpdate::CollectBackgroundDownloaderFiles. path = {0} error = {1}", asset.path, ex.ToString()));
                        //}
                    }

                    if (asset == null)
                        continue;

                    if (exists)
                    {
                        if (((add.options & XAssetsFiles.FileOptions.LUA) == XAssetsFiles.FileOptions.LUA)) { }
                        else
                            File.Delete(sdcardPath);
                    }
                    m_BackgroundNeedDownload.Add(asset);
                }

                isdone = true;
                if (LogEnabled)
                    XLogger.DEBUG("LaunchUpdate::CollectBackgroundDownloaderFiles. end");

#if !UNITY_WEBGL
            });
#endif

            while (!isdone)
                yield return 0;
        }
    }

    //启动后台文件下载
    IEnumerator LaunchBackgroundDownloader()
    {
        yield return CollectBackgroundDownloaderFiles();
        bool backgroundDownload = XConfig.defaultConfig.backgroundDownload;

        UpdateUtility.ShowTextTip(string.Format(UpdateConst.GetLanguage(5001), m_BackgroundNeedDownload.Count + " " + backgroundDownload));
        BackgroundDownloadQueue.Instance.SetFiles(m_BackgroundNeedDownload);

        int totalSize = 0;
        //tag各类型的数量
        Dictionary<int, int> tagType = new Dictionary<int, int>();
        foreach (var asset in m_BackgroundNeedDownload)
        {
            if (!tagType.ContainsKey(asset.tag))
                tagType.Add(asset.tag, 0);
            ++tagType[asset.tag];
            totalSize += asset.size;
        }



        RecordLogger(string.Format("HotUpdateStep -> {0} count={1} totalSize={2}. backgroundDownload={3}", "LaunchBackgroundDownloader",
            m_BackgroundNeedDownload.Count, XUtility.FormatBytes(totalSize), backgroundDownload));

        foreach (var item in tagType)
            RecordLogger(string.Format("                 -> tag={0} count={1}", item.Key, item.Value));

        if (backgroundDownload)
        {
            BackgroundDownloadQueue.Instance.StartAutoDownload();
        }
    }
}
