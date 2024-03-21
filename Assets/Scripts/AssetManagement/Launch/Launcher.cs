using UnityEngine;

public partial class Launcher : MonoBehaviour
{
    //包模式
    public static bool assetBundleMode { get; private set; }
    //包(本地代码)模式
    public static bool assetBundleModeLocalCode { get; private set; }
    //资源录制模式
    public static bool assetRecordMode { get; private set; }

    public bool checkUpdate = true;

    void Start()
    {

#if UNITY_EDITOR

//如果是配置模式，打开包模式
#if XConfigMode
        assetBundleMode = true;
#else
        UnityEditor.EditorPrefs.GetBool("QuickMenuKey_LaunchGameAssetBundle", false);
        checkUpdate = UnityEditor.EditorPrefs.GetBool("QuickMenuKey_LaunchGameUpdate", true);
#endif
        assetBundleModeLocalCode = UnityEditor.EditorPrefs.GetBool("QuickMenuKey_LaunchGameAssetBundleLocalCode", false);
        assetRecordMode = UnityEditor.EditorPrefs.GetBool("QuickMenuKey_LaunchGameRecordAssets", false);

        //LuaLoader.assetBundleModeLocalCode = assetBundleModeLocalCode;
#else
        assetBundleMode = true;
#endif

#if DEVELOPMENT_BUILD
        XLogger.INFO("DEVELOPMENT_BUILD");
#else
        XLogger.INFO("RELEASE_BUILD");
#endif

        XLogger.INFO("当前平台是：" + Application.platform.ToString());

        XLogger.INFO_Format("Launcher 游戏启动！！！");

        XLogger.INFO($"checkUpdate:{checkUpdate}");

#if UNITY_EDITOR
        Resources.UnloadUnusedAssets();
#endif
        gameObject.AddComponent<SelectedObjectHelper>();

        XLogger.s_MainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;

        DontDestroyOnLoad(gameObject);

        QualitySettings.masterTextureLimit = 0;
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 30;
        LaunchUpdate.LogEnabled = true;
        AssetManagement.AssetManager.LogEnabled = false;
        AssetManagement.AssetDownloadManager.LogEnabled = true;

        XConfig.ReadConfigAtFile(ContinueStart);
    }

    void ContinueStart()
    {

        LauncherJugglery.Destroy();

        InitTempCamera();

        DefaultLoaderGUI.Open();
        XLogger.INFO_Format("DefaultLoaderGUI.Open end");

        //if (XConfig.defaultConfig.isGetUrlByPHP)
        //    GetUrlByPHP(); //从后台拿资源地址
        //else
        StartCheckUpdate(); //直接使用default 配置地址
    }

    void StartCheckUpdate()
    {

        //if (Application.isEditor && !SystemInfo.graphicsDeviceVersion.StartsWith("OpenGL"))
        //{
        //    //编辑器模式下非opengl则用pc资源
        //    string def = XConfig.defaultConfig.testDownloadUrls[0];
        //    def = def.Replace("Android", "StandaloneWindows");
        //    for (int i = 0; i < XConfig.defaultConfig.testDownloadUrls.Length; i++)
        //        XConfig.defaultConfig.testDownloadUrls[i] = def;
        //}

        XVideoManager.Initialize();
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
