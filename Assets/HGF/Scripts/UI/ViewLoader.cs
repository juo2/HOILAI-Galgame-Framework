using UnityEngine;
using System.Collections;
using AssetManagement;
using UnityEngine.Events;
//using XLua;

public class ViewLoader : MonoBehaviour
{
    //[CSharpCallLua]
    //public delegate LuaTable LuaFuncAction(string arg0, GameObject arg1);

    [HideInInspector]
    public AssetInternalLoader loader;
    [HideInInspector]
    public string loadprefab = "GUI_CommonLoad_View.prefab";
    [HideInInspector]
    public string luaViewName = "game.modules.common.views.CommonLoadView";
    //[HideInInspector]
    public GameObject rawLoadGameObject;
    //[HideInInspector]
    public GameObject instanceLoadGameObject;

    public bool checkDownload = true;

    public string targetViewName;

    //private LuaFuncAction G_FUNC_CREATE_VIEW;

    //private LuaTable m_LuaViewObject;



    //private void Start()
    //{
    //    ReStart();
    //}

    public void ReStart()
    {
        if (loader == null)
        {
            XLogger.ERROR("ViewLoader::Start loader is null");
            return;
        }

        StopAllCoroutines();

        if (instanceLoadGameObject)
        {
            InitLoadView();
            //return;
        }

        StartCoroutine(CheckAsync());
    }

    IEnumerator CheckAsync()
    {
        bool isLoadCommonui = false;
        while (!loader.IsDone())
        {
            if (instanceLoadGameObject)
            {
                //已经加载过界面
            }
            else if (!isLoadCommonui)
            {
                //checkDownload 为 true 只有为下载的任务才显示，为 false 则一切加载都显示
                if (!checkDownload || (checkDownload && loader.IsDownloading()))
                {
                    isLoadCommonui = true;
                    yield return LoadCommonGUI();
                }
            }
            yield return 0;
        }

        if (instanceLoadGameObject)
        {
            DestoryObject();
        }
    }


    IEnumerator LoadCommonGUI()
    {
        if (instanceLoadGameObject)
        {
            InitLoadView();
            yield break;
        }



        if (AssetCache.ContainsRawObject(loadprefab))
        {
            rawLoadGameObject = AssetCache.GetRawObject<GameObject>(loadprefab);
            InitLoadView();
            yield break;
        }

        AssetInternalLoader loader = AssetUtility.LoadAsset<GameObject>(loadprefab);
        if (loader == null)
        {
            yield break;
        }

        loader.Update();
        yield return loader;

        if (string.IsNullOrEmpty(loader.Error))
        {

            rawLoadGameObject = loader.GetRawObject<GameObject>();

            if (loader.IsDone())
                yield break;

            InitLoadView();
        }
    }

    void InitLoadView()
    {
        if (rawLoadGameObject == null)
        {
            return;
        }

        if (!instanceLoadGameObject)
            instanceLoadGameObject = Object.Instantiate(rawLoadGameObject, transform);

        //if (G_FUNC_CREATE_VIEW == null)
        //{
        //    if (LuaEnvironment.Instance.LuaEnv != null)
        //    {
        //        G_FUNC_CREATE_VIEW = LuaEnvironment.Instance.LuaEnv.Global.Get<LuaFuncAction>("G_FUNC_CREATE_VIEW");
        //        if (G_FUNC_CREATE_VIEW == null)
        //        {
        //            XLogger.ERROR("ViewLoader::InitLoadView G_FUNC_CREATE_VIEW is null");
        //        }
        //    }
        //}

        //if (G_FUNC_CREATE_VIEW != null)
        //{
        //    if (m_LuaViewObject == null)
        //        m_LuaViewObject = G_FUNC_CREATE_VIEW.Invoke(luaViewName, instanceLoadGameObject);
        //    m_LuaViewObject.Set("asyncLoader", loader);
        //    m_LuaViewObject.Set("targetViewName", targetViewName);
        //    UnityAction<object> on_open_refresh = m_LuaViewObject.Get<UnityAction<object>>("on_open_refresh");
        //    on_open_refresh.Invoke(m_LuaViewObject);
        //}

    }


    void DestoryObject()
    {
        //if (m_LuaViewObject != null)
        //{
        //    UnityAction<object> dispose = m_LuaViewObject.Get<UnityAction<object>>("dispose");
        //    if (dispose == null)
        //        XLogger.ERROR("ViewLoader::OnDestroy dispose is null");
        //    else
        //        dispose.Invoke(m_LuaViewObject);

        //    m_LuaViewObject.Dispose();
        //    m_LuaViewObject = null;
        //}

        if (instanceLoadGameObject)
        {
            Object.DestroyImmediate(instanceLoadGameObject);
            instanceLoadGameObject = null;
        }

    }

    private void OnDisable()
    {
        //G_FUNC_CREATE_VIEW = null;
    }


    void OnDestroy()
    {
        DestoryObject();
        if (rawLoadGameObject != null)
            AssetUtility.DestroyAsset(rawLoadGameObject);
    }
}
