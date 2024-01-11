using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using System.IO;
using UnityEngine.Networking;

public partial class Launcher : MonoBehaviour
{
    //包模式
    public static bool assetBundleMode { get; private set; }
    //包(本地代码)模式
    public static bool assetBundleModeLocalCode { get; private set; }
    //资源录制模式
    public static bool assetRecordMode { get; private set; }

    //是否il2cpp
    public static bool isIl2cpp = false;

    public bool checkUpdate = true;
    void Start()
    {

#if !UNITY_EDITOR && UNITY_WEBGL
        WebGLInput.captureAllKeyboardInput = false;
#endif

#if UNITY_EDITOR
        assetBundleMode = UnityEditor.EditorPrefs.GetBool("QuickMenuKey_LaunchGameAssetBundle", false);
        assetBundleModeLocalCode = UnityEditor.EditorPrefs.GetBool("QuickMenuKey_LaunchGameAssetBundleLocalCode", false);
        assetRecordMode = UnityEditor.EditorPrefs.GetBool("QuickMenuKey_LaunchGameRecordAssets", false);
        checkUpdate = UnityEditor.EditorPrefs.GetBool("QuickMenuKey_LaunchGameNoUpdate", true);
        //LuaLoader.assetBundleModeLocalCode = assetBundleModeLocalCode;
#else
        assetBundleMode = true;
#endif
        //if (Debug.isDebugBuild)
        //    gameObject.AddComponent<XProfiler>();

        //XProfiler.ActivedProfiler(true);
        //LuaEnvironment.CreateLuaEnv();
        //XGUIManager.Instance.Initialize();
#if ENABLE_MONO
        XLogger.INFO("USING_MONO");
#elif ENABLE_IL2CPP
        isIl2cpp = true;
        XLogger.INFO("USING_IL2CPP");
#endif

#if DEVELOPMENT_BUILD
        XLogger.INFO("DEVELOPMENT_BUILD");
#else
        XLogger.INFO("RELEASE_BUILD");
#endif

        XLogger.INFO_Format("Launcher 游戏启动！！！");

#if UNITY_EDITOR
        Resources.UnloadUnusedAssets();
#endif
        XLogger.s_MainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;

        DontDestroyOnLoad(gameObject);

        QualitySettings.masterTextureLimit = 0;
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 30;
        LaunchUpdate.LogEnabled = true;
        AssetManagement.AssetManager.LogEnabled = false;
        AssetManagement.AssetDownloadManager.LogEnabled = true;

//#if !UNITY_WEBGL
//        XConfig.ReadConfigAtFile();
//#endif

//#if UNITY_ANDROID
//        if (XConfig.defaultConfig.isSDKPattern)
//        {
//            //AndroidPermission.RequestPermissions();
//            LauncherJugglery.ContinueFun = ContinueStart;
//            LauncherJugglery.Open();
//        }
//        else
//        {
//            StartCoroutine(ContinueStart());
//        }
//#else
//        //GameSdkData.sdCardPermission = true;

//#endif
        StartCoroutine(ContinueStart());

    }

    IEnumerator ContinueStart()
    {

#if UNITY_WEBGL
        string filePath = Path.Combine(Application.streamingAssetsPath, "default.xcfg");

        UnityWebRequest webRequest = UnityWebRequest.Get(filePath);

        Debug.Log($"开始加载:{filePath}");

        yield return webRequest.SendWebRequest();

        Debug.Log($"加载完成:{filePath}");


        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            string configFileContent = webRequest.downloadHandler.text;
            // 在此处处理文件内容

            XConfig config = new XConfig();
                
            config = JsonUtility.FromJson<XConfig>(configFileContent);

            AssetManagement.AssetDefine.RemoteDownloadUrl = config.testDownloadUrls[0];

            XConfig.defaultConfig = config;
        }
        else
        {
            Debug.LogError("Failed to load config file: " + webRequest.error);
        }
#else
         XConfig.ReadConfigAtFile();
#endif

        yield return 0;


        LauncherJugglery.Destroy();

        //闪屏结束，IOS数据埋点
#if UNITY_IOS && !UNITY_EDITOR
            GameSdkProxy.instance.ReqUploadUserData(@"{""payId"":"""",""updateTime"":0,""createTime"":0,""orderId"":"""",""feePoint"":"""",""isPrintLog"":true,""action"":0,""serverCreateTime"":""0"",""roleLevel"":0,""power"":0,""productId"":"""",""partyName"":"""",""vipLevel"":0,""roleId"":0,""roleName"":"""",""serverName"":"""",""serverId"":0,""balance"":0}");
            Debug.Log("IOS SDK 完成闪屏上报");
#endif
#if UNITY_STANDALONE
        //gameObject.AddComponent<XProfiler>();
#else
        //if (Debug.isDebugBuild)
        //    gameObject.AddComponent<XProfiler>();
#endif

        //gameObject.AddComponent<DevicesComponent>();

        //gameObject.AddComponent<ScreenOrientationComponent>();
        
        //int astc = XUtility.IsSupportsASTC();
        //if (astc != -1) XLogger.ERROR_Format("IsSupportsASTC : {0}", astc); else XLogger.INFO_Format("IsSupportsASTC : {0}", astc);

        InitTempCamera();

        DefaultLoaderGUI.Open();
        XLogger.INFO_Format("DefaultLoaderGUI.Open end");

        if (XConfig.defaultConfig.isGetUrlByPHP)
            GetUrlByPHP(); //从后台拿资源地址
        else
            StartCheckUpdate(); //直接使用default 配置地址

    }



    void StartCheckUpdate()
    {
#if UNITY_IOS && !UNITY_EDITOR
        //XConfig.defaultConfig.backgroundDownload = GameSdkData.IsIosVerify() ? false : XConfig.defaultConfig.backgroundDownload;
        //检测审核模式下的进度条状态
        DefaultLoaderGUI.SetSliderState(!GameSdkData.IsIosVerify());
#endif
        if (Application.isEditor && !SystemInfo.graphicsDeviceVersion.StartsWith("OpenGL"))
        {
            //编辑器模式下非opengl则用pc资源
            string def = XConfig.defaultConfig.testDownloadUrls[0];
            def = def.Replace("Android", "StandaloneWindows");
            for (int i = 0; i < XConfig.defaultConfig.testDownloadUrls.Length; i++)
                XConfig.defaultConfig.testDownloadUrls[i] = def;
        }

        AssetManagement.AssetManager.Instance.Initialize(new GameLoaderOptions());

        if(checkUpdate)
        {
            LaunchUpdate update = gameObject.AddComponent<LaunchUpdate>();
            update.p_IsCheckUpdate = checkUpdate;
            update.onUpdateComplete = OnUpdateComplete;
        }
        else
        {
            OnUpdateComplete();
        }
    }

    void InitTempCamera()
    {
        //临时相机
        new GameObject("TempCamera", typeof(Camera));
    }

    private void OnUpdateComplete()
    {

        DefaultLoaderGUI.SetProgress(1);
        StartCoroutine(OnUpdateCompleteInitGame());
    }
}
