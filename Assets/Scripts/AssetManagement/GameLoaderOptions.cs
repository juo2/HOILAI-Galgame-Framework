using UnityEngine;
using System.IO;
using System.Collections;
using AssetManagement;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameLoaderOptions : DefaultAssetLoaderOptions
{
    public static List<string> dontUnloadList = new List<string> { "01/gui/fonts.asset", "01/gui/fonts2.asset", /*"01/gui/modules/mainui/prefabs/baseprefabs.asset"*/ };

    public static int MAX_DOWNLAOD_NUM = 2;  //最大下载并发数
    public static int MAX_ABLOADER_NUM = -1; //最大ab加载并发数


    private AssetManifest m_AssetManifest;
    //服务器最新的清单文件
    private XAssetManifest m_XAssetManifest;
    public XAssetManifest xAssetManifest { get { return m_XAssetManifest; } }

    private bool m_AssetBundleMode;
    private bool m_AssetRecordMode;
    public float initProgress = 0;
    public GameLoaderOptions()
    {
        m_AssetBundleMode = Launcher.assetBundleMode || Launcher.assetBundleModeLocalCode;
        m_AssetRecordMode = Launcher.assetRecordMode;
#if UNITY_EDITOR
        if (!m_AssetBundleMode)
            m_AssetManifest = AssetManifest.EditorLoadAssetManifest();
#endif
        XLogger.INFO(string.Format("GameLoaderOptions::ctor() m_AssetBundleMode={0}", m_AssetBundleMode));

        if (XConfig.defaultConfig != null && !XConfig.defaultConfig.isGetUrlByPHP)
        {
            AssetDefine.RemoteDownloadUrl = XConfig.defaultConfig.testDownloadUrls;
            AssetDefine.RemoteSpareUrls.Clear();
        }

        XLogger.INFO("GameLoaderOptions::() AssetDefine.RemoteUrl: " + AssetDefine.RemoteDownloadUrl);
    }


    public IEnumerator InitLoaderOptions()
    {
        string manifestPath = AssetDefine.ExternalSDCardsPath + AssetDefine.AssetManifestName;
        if (!File.Exists(manifestPath))
            manifestPath = AssetDefine.BuildinAssetPath + AssetDefine.AssetManifestName;

        AssetBundleCreateRequest abcr = AssetBundle.LoadFromFileAsync(manifestPath);
        while (!abcr.isDone)
        {
            initProgress = abcr.progress * 0.5f;
            yield return null;
        }

        initProgress = 0.5f;
        AssetBundle manifestBundle = abcr.assetBundle;
        if (manifestBundle != null)
        {


            AssetBundleRequest abr = manifestBundle.LoadAssetAsync<XAssetManifest>(manifestBundle.GetAllAssetNames()[0]);
            while (!abr.isDone)
            {
                initProgress = abr.progress * 0.45f + 0.5f;
                yield return null;
            }

            m_XAssetManifest = abr.asset as XAssetManifest;

            manifestBundle.Unload(false);
        }
        else
        {
            XLogger.INFO(string.Format("GameLoaderOptions::ctor() manifestBundle is null path={0} ", manifestPath));
        }

        initProgress = 1f;
    }

    public override string GetAssetBundleName(string assetName)
    {
        if (m_AssetBundleMode || m_XAssetManifest != null)
            return m_XAssetManifest.GetAssetBundleNameAtAssetName(assetName);
        return null;
    }

    public override string[] GetAssetBundleDependence(string assetBundlename)
    {
        if (m_AssetBundleMode || m_XAssetManifest != null)
            return m_XAssetManifest.GetAllDependencies(assetBundlename);
        return null;
    }

    public override string GetAssetPathAtName(string asssetName)
    {
        if (m_XAssetManifest != null && !IsEditorLoad(asssetName))
        {
            return asssetName;
        }

        return m_AssetManifest.GetAssetPath(asssetName);
    }

    public override uint GetAssetBundleCrc(string assetBundlename)
    {
        if (m_AssetBundleMode || m_XAssetManifest != null)
            return m_XAssetManifest.GetAssetBundleCrc(assetBundlename);
        return 0;
    }

    public override Hash128 GetAssetBundleHash(string assetBundlename)
    {
        return default(Hash128);
    }

    public override string GetAssetBundleMd5(string assetBundlename)
    {
        if (XAssetsFiles.s_CurrentAssets != null && XAssetsFiles.s_CurrentAssets.allFilesMap.ContainsKey(assetBundlename))
            return XAssetsFiles.s_CurrentAssets.allFilesMap[assetBundlename].md5;
        return string.Empty;
    }

    public override bool GetAssetBundleIsBuildin(string assetBundlename)
    {
        //服务器或本地最新
        XAssetsFiles.FileStruct srvFile = null;
        //跟随机安装包的
        XAssetsFiles.FileStruct buildinFile = null;

        if (XAssetsFiles.s_CurrentAssets != null &&
            XAssetsFiles.s_CurrentAssets.allFilesMap.TryGetValue(assetBundlename, out srvFile))
        { }

        if (XAssetsFiles.s_BuildingtAssets != null &&
            XAssetsFiles.s_BuildingtAssets.allFilesMap.TryGetValue(assetBundlename, out buildinFile))
        { }

        bool result = false;
        if (srvFile != null)
        {
            if ((srvFile.options & XAssetsFiles.FileOptions.BUILDING) == XAssetsFiles.FileOptions.BUILDING)
            {
                if (buildinFile != null)
                {
                    if ((buildinFile.options & XAssetsFiles.FileOptions.BUILDING) == XAssetsFiles.FileOptions.BUILDING)
                        result = srvFile.md5 == buildinFile.md5;
                }
            }
        }


        return result;
    }

    public override bool GetAssetBundleIsNeeedDownload(string assetBundlename)
    {
        return true;
    }

    public override int GetAssetBundleLoadMaxNum()
    {
        return MAX_ABLOADER_NUM;
    }

    public override int GetDownLoaderMaxNum()
    {
        return MAX_DOWNLAOD_NUM;
    }

    public override int GetAssetBundleByteSize(string assetBundlename)
    {
        if (XAssetsFiles.s_CurrentAssets != null)
        {
            XAssetsFiles.FileStruct fs;
            if (XAssetsFiles.s_CurrentAssets.allFilesMap.TryGetValue(assetBundlename, out fs))
                return fs.size;
        }
        return 0;
    }

    public override string GetAssetDownloadUrl(string assetPath, int index = -1)
    {
        if (index == -1)
            return AssetDefine.RemoteDownloadUrl + "/" + assetPath;

        if (AssetDefine.RemoteSpareUrls.Count > index)
            return AssetDefine.RemoteSpareUrls[index] + "/" + assetPath;

        //XLogger.ERROR_Format("GameLoaderOptions::GetAssetDownloadUrl. AssetDefine.RemoteSpareUrls.Count:{0} index:{1}", AssetDefine.RemoteSpareUrls.Count, index);

        return string.Empty;
    }

    public override string GetAssetDownloadSavePath(string assetPath)
    {
        return AssetDefine.ExternalSDCardsPath + "/" + assetPath;
    }

    //内置资源
    public override string GetBuildinAssetPath(string assetPath)
    {
        return AssetDefine.BuildinAssetPath + assetPath;
    }

    public override bool IsEditorLoad(string assetName)
    {
        if (m_AssetBundleMode)
            return false;
        return this.m_AssetManifest.ContainsAsset(assetName);
    }

    public override bool ContainsAsset(string assetName)
    {
        if (!m_AssetBundleMode && m_AssetManifest != null && m_AssetManifest.ContainsAsset(assetName))
            return true;
        return m_XAssetManifest != null && m_XAssetManifest.ContainsAsset(assetName);
    }


    public override void RecordAsset(string assetName)
    {
        if (!m_AssetRecordMode) return;
        if (AssetsRecord.s_CurrentRecord == null)
            AssetsRecord.s_CurrentRecord = new AssetsRecord();
        AssetsRecord.s_CurrentRecord.Record(assetName);
    }

    public override List<string> GetDontUnloadList()
    {
        return dontUnloadList;
    }
}
