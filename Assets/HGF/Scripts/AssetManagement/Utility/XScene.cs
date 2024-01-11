using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AssetManagement;
using UnityEngine.Profiling;

public partial class XScene
{
    public static string GLOBAL_SCENE_MAT_QUALITY = "GLOBAL_SCENE_MAT_QUALITY";

    private static Dictionary<string, XScene> s_XScenes = new Dictionary<string, XScene>();
    public static Dictionary<string, XScene> xScenes { get { return s_XScenes; } }
    public static System.Action<float> onProgress { get; set; }
    public static System.Action<AssetInternalLoader> onProgressLoader { get; set; }
    public static System.Action onComplete { get; set; }
    public static System.Action<AssetInternalLoader> onError { get; set; }
    public static XScene activeXScene { get; private set; }
    public static XScene activeLastXScene { get; private set; }
    public static bool staticBatchingEnabled = false;
    public static bool thisLoadNoUnloadasset = false; //本次加载场景是否不卸载资源
    public static bool thisLoadNoUnloadassetBefore = false; //本次加载场景之前是否不卸载资源


    private GameObject m_XSceneRoot;
    private Transform m_XSceneRootTransform;
    private Scene m_UnityScene;
    public Transform sceneRootTransform { get { return m_XSceneRootTransform; } }
    public string assetName { get; private set; }
    public bool isLoading { get; private set; }
    public bool isUnload { get; private set; }
    public bool isPreload { get; private set; }
    public LoadSceneMode loadSceneMode { get; private set; }
    //public XSceneMatQuality matQuality { get; private set; }
    //public XEnvironmentSetting environmentSetting { get; private set; }
    public Light mainLight { get; private set; }
    public bool isAddDisabledRoot = true;
    public int lightmapDataIndexOffset = -1;

    public XScene(string assetName, LoadSceneMode mode)
    {
        this.assetName = assetName;
        this.loadSceneMode = mode;
    }

    public void Load(bool isPreload = false)
    {
        this.isPreload = isPreload;
        this.isLoading = true;
        SceneManager.sceneLoaded += OnUnitySceneLoaded;

        XLogger.INFO_Format("开始加载场景：{0}", assetName);

        AssetInternalLoader loader = AssetUtility.LoadScene(assetName, loadSceneMode);
        if (loader != null)
        {
            loader.onProgress += (float v) => { OnAssetProgress(v, loader); };
            loader.onComplete += OnAssetLoadComplete;
        }
    }


    private void OnAssetProgress(float v, AssetInternalLoader loader)
    {
        if (onProgress != null)
        {
            onProgress.Invoke(v);
        }

        if (onProgressLoader != null)
        {
            onProgressLoader.Invoke(loader);
        }
    }

    public static bool testErr = false;
    private void OnAssetLoadComplete(AssetInternalLoader loader)
    {
        if (testErr)
        {
            this.isLoading = false;
            //模拟异常
            if (onError != null)
                onError.Invoke(loader);
            return;
        }

        if (!string.IsNullOrEmpty(loader.Error))
        {
            this.isLoading = false;
            if (onError != null)
                onError.Invoke(loader);
            XLogger.ERROR_Format("XScene.OnAssetLoadComplete Error {0}", loader.Error);
            XLogger.ReportException("场景加载异常", string.Format("XScene.OnAssetLoadComplete Error {0}", loader.Error), "XScene::OnAssetLoadComplete");
            return;
        }

        //LuaEnvironment.Instance.GC();
        //AssetUtility.UnloadAllAsset();

        //AssetUtility.UnloadUnusedAssets(true);
        this.isLoading = false;
    }

    void OnUnitySceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (testErr) return;
        TimerManager.AddCoroutine(OnComplete(scene, mode));
    }

    IEnumerator OnComplete(Scene scene, LoadSceneMode mode)
    {

        XLogger.INFO_Format("加载场景完成：{0}", assetName);

        SceneManager.sceneLoaded -= OnUnitySceneLoaded;
        InitScene(scene);
        //if (!thisLoadNoUnloadasset && !isPreload)
        //    yield return AssetManager.Instance.UnloadUnusedAssets();

        yield return null;
        if (onComplete != null)
        {
            Profiler.BeginSample("XScene.OnComplete");
            onComplete.Invoke();
            Profiler.EndSample();
        }
        thisLoadNoUnloadasset = false;
        thisLoadNoUnloadassetBefore = false;
    }

    void InitScene(Scene scene)
    {
        m_UnityScene = scene;

        //预加载模式禁用掉场景，等待再次加载激活场景
        if (isPreload)
        {
            GameObject[] roots = m_UnityScene.GetRootGameObjects();
            foreach (var item in roots)
            {
                if (item.name == "XScene")
                {
                    item.SetActive(false);
                    break;
                }
            }
            return;
        }


        DisabledOtherScene();

        ActivedScene();

        //if (staticBatchingEnabled)
        //    InitStaticBatching();

        if (this.m_XSceneRoot == null)
        {
            XLogger.WARNING(string.Format("XScene::InitScene. m_XSceneRoot is null  rootCount = {0}", m_UnityScene.rootCount));
        }
    }


    void ActivedScene()
    {
        activeLastXScene = activeXScene;
        activeXScene = this;
        SceneManager.SetActiveScene(m_UnityScene);

        if (this.m_XSceneRoot == null)
        {
            GameObject[] roots = m_UnityScene.GetRootGameObjects();
            foreach (var item in roots)
            {
                if (item.name == "XScene")
                {
                    this.m_XSceneRoot = item;
                    //environmentSetting = this.m_XSceneRoot.GetComponentInChildren<XEnvironmentSetting>();
                    //matQuality = this.m_XSceneRoot.AddComponent<XSceneMatQuality>();
                    this.m_XSceneRootTransform = item.transform;
                    break;
                }
            }
        }

        if (this.m_XSceneRoot)
            this.m_XSceneRoot.SetActive(true);



        InitLight();

        InitActivedCamera();

        //InitCCRamp();

        InitLightmapDataIndexOffset();

    }

    public void SetEnvActive(bool active)
    {
        //if (!environmentSetting) return;

        //environmentSetting.enabled = active;
    }


    void DisabledOtherScene()
    {
        //当前场景为add模式则禁用其它场景
        if (loadSceneMode == LoadSceneMode.Additive)
        {
            foreach (var item in xScenes)
            {
                if (item.Value != this && item.Value.isAddDisabledRoot && item.Value.sceneRootTransform != null)
                    item.Value.sceneRootTransform.SetActive(false);
            }
        }
    }

    void InitLight()
    {
        if (RenderSettings.sun != null)
        {
            mainLight = RenderSettings.sun;
        }
    }


    //防止美术提交时把相机没关掉
    void InitActivedCamera()
    {
        Transform cameras = FindObject(null, "Cameras") as Transform;
        if (cameras)
        {
            Camera[] cams = cameras.GetComponentsInChildren<Camera>();
            foreach (var cam in cams)
            {
                if (cam.enabled)
                {

                    cam.SetActive(false);
                    XLogger.ERROR_Format("XScene::InitActivedCamera {0} 美术检查是否是异常相机", cam);
#if UNITY_EDITOR
                    UnityEditor.EditorGUIUtility.PingObject(cam);
#endif
                }
            }
        }
    }

    void InitLightmapDataIndexOffset()
    {
        if (loadSceneMode == LoadSceneMode.Additive && lightmapDataIndexOffset > 0)
        {
            if (this.m_XSceneRootTransform != null)
            {
                //RendererLightmapData[] lightmapDatas = this.m_XSceneRootTransform.GetComponentsInChildren<RendererLightmapData>();
                //foreach (var data in lightmapDatas)
                //{
                //    data.lightmapIndex = lightmapDataIndexOffset;
                //}
            }
        }
    }

    public void InitStaticBatching()
    {
        UnityEngine.Profiling.Profiler.BeginSample("XScene::InitStaticBatching");
        if (this.m_XSceneRootTransform != null)
        {
            foreach (Transform node in this.m_XSceneRootTransform)
            {
                if (node.childCount > 0)
                {
                    foreach (Transform child in node)
                    {
                        if (child.name == "static" || child.name == "Static")
                        {
                            child.name = "StaticBatching";
                            StaticBatchingUtility.Combine(child.gameObject);
                        }
                    }
                }
            }
        }
        UnityEngine.Profiling.Profiler.EndSample();
    }


    public Object FindObject(System.Type type = null, string relative = null)
    {
        return m_XSceneRootTransform != null ? m_XSceneRootTransform.FindComponent(type, relative) : null;
    }


    void WaitCreateSceneObject()
    {
        //进入场景后创建需要创建的物件
    }

    IEnumerator WaitCreateCoroutine()
    {
        yield return 0;
    }

    public IEnumerator UnloadScene(bool remove = true)
    {
        isUnload = true;
        this.m_XSceneRoot = null;
        this.m_XSceneRootTransform = null;


        if (SceneManager.sceneCount == 1)
            CreateEmptyScene();

        if (activeLastXScene == this)
            activeLastXScene = null;

        if (remove)
            xScenes.Remove(assetName);

        if (m_UnityScene != default(Scene))
            yield return SceneManager.UnloadSceneAsync(m_UnityScene);
        else
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene != default(Scene) && activeScene.name != "Empty")
                yield return SceneManager.UnloadSceneAsync(activeScene);
        }

        yield return AssetManager.Instance.UnloadScene(assetName);



    }

    public static bool ContainsScene(string assetName)
    {
        return xScenes.ContainsKey(assetName);
    }

    public static void LoadScene(string assetName, LoadSceneMode mode)
    {
        TimerManager.AddCoroutine(UnloadAllScene(assetName, mode));
    }

    public static void PreLoadScene(string assetName, LoadSceneMode mode)
    {
        TimerManager.AddCoroutine(UnloadAllScene(assetName, mode, true));
    }

    public static bool ReLoadErrScene(string assetName)
    {
        if (xScenes.ContainsKey(assetName))
        {
            LoadSceneMode mode = xScenes[assetName].loadSceneMode;
            xScenes[assetName].UnloadScene();
            LoadScene(assetName, mode);
            return true;
        }
        else
        {
            XLogger.ERROR_Format("XScene:ReLoadErrScene not exist  {0}", assetName);
        }

        return false;
    }

    //卸载所有场景
    public static void UnLoadAllScene()
    {
        TimerManager.AddCoroutine(UnloadAllScene(string.Empty, LoadSceneMode.Single));
    }

    public static void CreateEmptyScene()
    {
        if (SceneManager.GetSceneByName("Empty").IsValid())
            return;
#pragma warning disable 0219
        Scene empty = SceneManager.CreateScene("Empty");

        GameObject go = new GameObject("EmptyCamera");
        Camera cam = go.AddComponent<Camera>();
        cam.depth = -99;
    }

    static IEnumerator UnloadAllScene(string assetName, LoadSceneMode mode, bool isPreload = false)
    {


        Debug.Log("UnloadAllScene start");

        while (true)
        {
            bool isLoadingScene = false;
            foreach (var item in xScenes)
            {
                if (item.Value.isLoading)
                {
                    isLoadingScene = true;
                    break;
                }
            }

            if (isLoadingScene)
                yield return 0;
            else
                break;
        }

        if (mode == LoadSceneMode.Single && !xScenes.ContainsKey(assetName))
            CreateEmptyScene();


        if (xScenes.ContainsKey(assetName))
        {
            xScenes[assetName].ActivedScene();
            xScenes[assetName].DisabledOtherScene();
            yield return 0;
            if (onComplete != null)
            {
                onComplete.Invoke();
            }
        }

        bool isUnloadScene = false;

        if (mode == LoadSceneMode.Single && !isPreload)
        {
            XScene[] scenes = xScenes.Values.ToArray();
            foreach (var item in scenes)
            {
                isUnloadScene = true;
                if (item.assetName != assetName)
                {
                    yield return item.UnloadScene(true);
                }

            }
        }


        //if (isUnloadScene)
        //{
        //    if (!thisLoadNoUnloadassetBefore && !thisLoadNoUnloadasset && !isPreload)
        //    {
        //        AssetManager.Instance.UnloadUnusedAssets();
        //        //yield return AssetManager.Instance.UnloadUnusedAssets();
        //    }
        //}


        if (!string.IsNullOrEmpty(assetName))
        {
            LoadInternalScene(assetName, mode, isPreload);
        }

        Debug.Log("UnloadAllScene end");


    }

    static void LoadInternalScene(string assetName, LoadSceneMode mode, bool isPreload = false)
    {

        Debug.Log("LoadInternalScene start");

        if (xScenes.ContainsKey(assetName))
        {
            thisLoadNoUnloadassetBefore = false;
            thisLoadNoUnloadasset = false;
            //XLogger.ERROR(string.Format("XScene::LoadInternalScene. exist scene assetName = {0} mode = {1}", assetName, mode));
            return;
        }
        XScene scene = new XScene(assetName, mode);

        if (mode == LoadSceneMode.Additive)
        {
            int index = LightmapSettings.lightmaps != null ? LightmapSettings.lightmaps.Length : 0;
            scene.lightmapDataIndexOffset = index;
        }

        scene.Load(isPreload);
        xScenes.Add(assetName, scene);

        Debug.Log("LoadInternalScene end");

    }
}
