using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using AssetManagement;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif


/// <summary>
/// 外接配置可扩展
/// 将存放到 StreamingAssets
/// </summary>
[Serializable]
public class XConfig
{
    static string c_Path = Application.streamingAssetsPath + "/default.xcfg";
    private static XConfig s_DefaultConfig;
    public static XConfig defaultConfig 
    {
        get => s_DefaultConfig;
        set
        {
            s_DefaultConfig = value;
        }
    }

    //后台
    [SerializeField]
    private string m_CenterUrl;
    //测试后台
    [SerializeField]
    private string m_CenterUrlTest;
    //版本
    [SerializeField]
    private string m_CodeVer;
    //测试下载路径
    [SerializeField]
    private string[] m_TestDownloadUrls;

    //启动闪屏图片
    [SerializeField]
    private string[] m_startScreenImgs;

    //启动加载页图片
    [SerializeField]
    private string[] m_startLoadImgs;

    //启动下载资源标记（安装包内附带少量资源启动时需要检查完整性并下载）
    [SerializeField]
    private int m_InitDownloadTag = -1;

    //下载大小系数
    [SerializeField]
    private float m_DownloadSizeFactor = 1;

    //是否启动后台静默下载
    [SerializeField]
    private bool m_BackgroundDownload;

    //SDK模式
    [SerializeField]
    private bool m_SDKPattern;

    //是否从后台拿更新地址
    [SerializeField]
    private bool m_GetUrlByPHP;

    //buglyd
    [SerializeField]
    private string[] m_BuglyAppIDs;

    public string centerUrl
    {
        get
        {
            return m_CenterUrl;
        }
    }
    public string centerUrlTest { get { return m_CenterUrlTest; } }
    public string codeVer { get { return m_CodeVer; } }

    public string testDownloadUrls 
    { 
        get 
        {

#if UNITY_EDITOR
            return m_TestDownloadUrls[0] + "Android/";
#else
            return m_TestDownloadUrls[0] + Application.platform.ToString() + "/"; 
#endif
        }
    }

    public string[] startScreenImgs { get { return m_startScreenImgs; } }
    public string[] startLoadImgs { get { return m_startLoadImgs; } }

    public string[] buglyAppIDs { get { return m_BuglyAppIDs; } }

    public int initDownloadTag { get { return m_InitDownloadTag; } }
    
    public bool backgroundDownload { get { return m_BackgroundDownload; } }

    public bool isSDKPattern { get { return m_SDKPattern; } }
    public bool isGetUrlByPHP { get { return m_GetUrlByPHP; } }

    public float downloadSizeFactor { get { return m_DownloadSizeFactor; } }

    public static void CreateConfig(XConfig cfg)
    {
        System.IO.File.WriteAllText(c_Path, JsonUtility.ToJson(cfg, true));
    }

    public static void ReadConfigAtFile(Action action)
    {
        XConfig config = new XConfig();

        if (Application.platform == RuntimePlatform.Android)
        {
            WWW www = new WWW(c_Path);
            while (!www.isDone)
                System.Threading.Thread.Sleep(10);
            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.LogWarningFormat("XConfig::ReadConfig()  www error={0}", www.error);
            }

            config = JsonUtility.FromJson<XConfig>(www.text);
            www.Dispose();

            AssetManagement.AssetDefine.RemoteDownloadUrl = config.testDownloadUrls;

            s_DefaultConfig = config;

            action?.Invoke();
        }
        //else if (Application.platform == RuntimePlatform.WebGLPlayer)
        //{
        //    TimerManager.AddCoroutine(LoadDefaultConfig(action));
        //}
        else
        {
            if (!System.IO.File.Exists(c_Path))
            {
                Debug.LogWarningFormat("XConfig::ReadConfig() file not exists path={0}", c_Path);
            }
            config = JsonUtility.FromJson<XConfig>(System.IO.File.ReadAllText(c_Path));

            AssetManagement.AssetDefine.RemoteDownloadUrl = config.testDownloadUrls;

            s_DefaultConfig = config;

            action?.Invoke();
        }
    }

    static IEnumerator LoadDefaultConfig(Action action)
    {

#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        c_Path = "file://" + c_Path;
#endif
        UnityWebRequest www = UnityWebRequest.Get(c_Path);
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.Success)
        {
            s_DefaultConfig = JsonUtility.FromJson<XConfig>(www.downloadHandler.text);

            AssetManagement.AssetDefine.RemoteDownloadUrl = s_DefaultConfig.testDownloadUrls;
        }
        else
        {
            Debug.Log("Error: " + www.error);
        }

        action?.Invoke();
    }


    public static XConfig ReadConfig(string data)
    {
        return JsonUtility.FromJson<XConfig>(System.IO.File.ReadAllText(data));
    }
}
