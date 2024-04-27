using UnityEngine;
using System.Collections;
using AssetManagement;
using DG.Tweening;
using XAudio;

public class LauncherEnter : MonoBehaviour
{
    public static bool progressTextVisible = false;
    public static float launcherInitStime;

    private void Start()
    {
        StartCoroutine(OnUpdateCompleteInitGame());
    }

    IEnumerator OnUpdateCompleteInitGame()
    {
        XVideoManager.Initialize();

        //DefaultLoaderGUI.SetProgressText(UpdateConst.GetLanguage(11007));
        DefaultLoaderGUI.SetContenText(UpdateConst.GetLanguage(11008));
        launcherInitStime = Time.realtimeSinceStartup;
        SetProgress(0, "InitAudioManager");

        //初始化清单资源
        //if(checkUpdate)
        {
            GameLoaderOptions mainifestLoader = ((GameLoaderOptions)AssetManagement.AssetManager.Instance.AssetLoaderOptions);
            StartCoroutine(mainifestLoader.InitLoaderOptions());

            while (mainifestLoader.initProgress < 1f)
            {
                SetProgress(mainifestLoader.initProgress * 0.5f, "Init GameLoaderOptions", 0.2f);
                yield return null;
            }
        }
        
        XLogger.INFO_Format("InitShaders");
        SetProgress(0.5f, "InitShaders");
        //初始化着色器
        //gameObject.AddComponent<XShader>();

        XAudioManager.instance.Init();

        Debug.Log("XAudioManager.instance.Init finish");

        //float progress = 0.5f;
        float stime = Time.time;

        while (!XAudioManager.instance.isInitSuccessful)
        {
            //初始化时间过长
            if (Time.time - stime > 15)
            {
                if (!XAudioManager.instance.isInitSuccessful)
                {
                    DefaultAlertGUI.Open("", UpdateConst.GetLanguage(12000));
                    yield break;
                }

                //if (!XShader.isInitSuccessful)
                //{
                //    DefaultAlertGUI.Open("", UpdateConst.GetLanguage(12001));
                //    yield break;
                //}

                DefaultAlertGUI.Open("", "Error 10000");
                yield break;
            }

            //progress += 0.08f;
            //SetProgress(progress, "");
            yield return null;
        }

        SetProgress(1, "", 0.1f);

        Debug.Log(" SetProgress(1,, 0.1f);");


        //初始化剧情接口
        // XSInterface.gameInterface = new XSInterfaceImpl();

        //AssetManager.Instance.gcAfterAction += () => { LuaEnvironment.Instance.GC(); };

        //天气系统
        //gameObject.AddComponent<WeatherManager>();

        DefaultLoaderGUI.isEnterGame = true;

        DefaultLoaderGUI.Close();

        //初始化摄像头
        Debug.Log("开始加载场景 Demo.unity");

        XScene.LoadScene("Demo.unity", UnityEngine.SceneManagement.LoadSceneMode.Single);

        XScene.onComplete = () =>
        {
            //GameObject cameraGo = GameObject.Find("Main Camera");
            //Camera cam = cameraGo.GetComponent<Camera>();
            //XGUI.XCamera.guiCamera = cam;
            XGUI.XGUIManager.Instance.Init();

            XGUI.XGUIManager.Instance.OpenView("LoginSelectView");

            XScene.onComplete = null;
        };

        Debug.Log("加载场景完成 Demo.unity");

    }

    void SetProgress(float v, string desc = "", float time = 0f)
    {
        time = time > 0 ? time : (v < 0.01f ? 0f : 0.5f);
        DefaultLoaderGUI.SetProgress(v, time);
        if (!string.IsNullOrEmpty(desc) && progressTextVisible)
            DefaultLoaderGUI.SetContenText(desc);
    }


    void OnDestroy()
    {
        DOTween.KillAll();
        XScene.onProgress = null;
        XScene.onComplete = null;
        XScene.onProgressLoader = null;
        //QualityManager.onChange = null;
    }
}
