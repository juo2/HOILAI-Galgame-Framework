using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DG.Tweening;
using AssetManagement;
using UnityEngine.Events;
using Object = UnityEngine.Object;

public partial class DefaultLoaderGUI : MonoBehaviour
{
    public static string s_AssetName = "GUI_Loading";
    public static bool isShowSplash = true;
    public static DefaultLoaderGUI s_DefaultLoaderGUI;
    public static bool isEnterGame = false;//是否已经进入游戏逻辑层
    static DefaultLoaderGUI()
    {
        GameObject go = new GameObject("DefaultLoaderGUI");
        s_DefaultLoaderGUI = go.AddComponent<DefaultLoaderGUI>();
        Object.DontDestroyOnLoad(go);

        GameObject xgame = GameObject.Find("xgame");
        if (xgame != null) go.transform.SetParent(xgame.transform);
        go.transform.localPosition = new Vector3(-5000, -5000, -5000);
    }
    public Text verText { get; private set; }
    public GameObject instanceObject { get; private set; }
    public Transform instanceTransform { get; private set; }
    public Slider progressSlider { get; private set; }
    public Text progressText { get; private set; }
    public Text contentText { get; private set; }
    public Text descText { get; private set; }

    public Camera loaderCamera { get; private set; }

    private string spriteAssetName = null;
    private string LoginName = null;
    private Sprite curSprite1 = null;
    private Sprite curSprite2 = null;
    private Sprite curSprite3 = null;
    private UnityAction onComplete = null;
    private Image m_image;

    private bool isInit = false;
    private Sprite curSprite = null;
    private Sprite curTitleSprite = null;
    private Sprite curMaskSprite = null;
    private Sprite curNamedesSprite = null;
    private Sprite curCenterSprite = null;
    void Load()
    {
        GameObject rawGO = Resources.Load<GameObject>(s_AssetName);
        if (rawGO == null)
        {
            XLogger.WARNING("DefaultLoaderGUI::Load . 包内资源格式异常 GameObject ");
            return;
        }

        instanceObject = GameObject.Instantiate<GameObject>(rawGO, transform);
        instanceTransform = instanceObject.transform;

        InitUI();
       

        if (!isEnterGame)
        {
            //进入游戏时，热更新页面的加载页，提供代理替换的功能
            string[] images = XConfig.defaultConfig.startLoadImgs;
            if (images != null && images.Length > 0)
                StartCoroutine(AgentLoadUI(images, true));
        }
        else
        {
            if (!string.IsNullOrEmpty(spriteAssetName))
            {
                if (curSprite1 != null && m_image != null)
                {
                    m_image.sprite = curSprite1;
                    m_image.gameObject.SetActive(true);
                }
            }
        }
        //暂时全部显示2D加载图
        if (curSprite != null && spriteAssetName != "default")
        {
            m_image.sprite = curSprite;
            m_image.gameObject.SetActive(true);
        }

        progressSlider.value = 0;
        progressSlider.gameObject.SetActive(true);

    }

    public static void SetVerText(string content)
    {
        if (s_DefaultLoaderGUI.verText != null)
        {
            s_DefaultLoaderGUI.verText.text = content;
        }
    }

    void _SetSliderState(bool active)
    {
        if (active)
        {
            progressSlider.value = 0;
        }
        else
        {
            Transform Background = instanceTransform.Find("Canvas/Background");
            if (Background != null)
                Background.gameObject.SetActive(false);
        }

        progressSlider.gameObject.SetActive(active);
    }
    void InitUI()
    {

        progressSlider = instanceObject.GetComponentInChildren<Slider>();
        m_image = instanceTransform.FindComponent("Image", "Canvas/Image_bg1") as Image;
        verText = instanceTransform.FindComponent("Text", "Canvas/Text_ver") as Text;
        contentText = instanceTransform.FindComponent("Text", "Canvas/Text") as Text;
        descText = instanceTransform.FindComponent("Text", "Canvas/descText") as Text;


        //禁止拖动进度条
        if (progressSlider)
        {
            progressSlider.interactable = false;
        }

        float titleScale = ((float)Screen.height / (float)Screen.width) / ((float)720 / (float)1280);
        
        loaderCamera = instanceObject.GetComponentInChildren<Camera>();
        if (loaderCamera != null)
            loaderCamera.depth = 99;

        isInit = true;

    }

    private void LoadImg(string name, string tran, UnityAction _onComplete)
    {
        spriteAssetName = name;
        onComplete = _onComplete;
        if (!string.IsNullOrEmpty(name) && name != "default")
        {

            StartCoroutine(LoadAsync(spriteAssetName));
        }
        else
        {
            spriteAssetName = "default";
            if (onComplete != null)
            {
                onComplete.Invoke();
                onComplete = null;
            }
        }

    }
    private void LoadLogin(string name, string tran, UnityAction _onComplete)
    {
        LoginName = name;
        onComplete = _onComplete;
        if (!string.IsNullOrEmpty(name) && name != "default")
        {

            StartCoroutine(LoadMask("Chuanghao_shipei.png"));
            StartCoroutine(LoadAsync(LoginName));
        }
        else
        {
            spriteAssetName = "default";
            if (onComplete != null)
            {
                onComplete.Invoke();
                onComplete = null;
            }
        }

    }

    IEnumerator LoadAsync(string _spriteAssetName)
    {
        string[] imgname = new string[] { _spriteAssetName + "_title.png", _spriteAssetName + "_careertitle.png", _spriteAssetName + ".png", _spriteAssetName + "_centertitle.png" };

        for (int i = 0; i < imgname.Length; i++)
        {
            if (AssetCache.ContainsRawObject(imgname[i]))
            {
                SetSprite(AssetCache.GetRawObject<Sprite>(imgname[i]), i);
                continue;
            }
            else
            {
                UnityEngine.Profiling.Profiler.BeginSample("Image.LoadAsync");
                AssetInternalLoader loader = AssetUtility.LoadAsset<Sprite>(imgname[i]);
                UnityEngine.Profiling.Profiler.EndSample();
                if (loader == null) continue;
                if (loader != null)
                {
                    if (loader.IsDone())
                    {
                        SetSprite(string.IsNullOrEmpty(loader.Error) ? loader.GetRawObject<Sprite>() : null, i);
                    }
                    else
                    {
                        yield return loader;
                        SetSprite(string.IsNullOrEmpty(loader.Error) ? loader.GetRawObject<Sprite>() : null, i);
                    }
                }
            }
        }

        //必须走，不然进不去游戏
        SetSpriteComplete();
    }
    IEnumerator LoadMask(string _spriteAssetName)
    {

        if (AssetCache.ContainsRawObject(_spriteAssetName))
        {
            SetMask(AssetCache.GetRawObject<Sprite>(_spriteAssetName));
        }
        else
        {
            AssetInternalLoader loader = AssetUtility.LoadAsset<Sprite>(_spriteAssetName);
            if (loader != null)
            {
                if (loader.IsDone())
                {
                    SetMask(string.IsNullOrEmpty(loader.Error) ? loader.GetRawObject<Sprite>() : null);
                }
                else
                {
                    yield return loader;
                    SetMask(string.IsNullOrEmpty(loader.Error) ? loader.GetRawObject<Sprite>() : null);
                }
            }
        }
    }
    void SetSpriteComplete()
    {
        if (onComplete != null)
        {
            onComplete.Invoke();
            onComplete = null;
        }
    }


    void SetSprite(Sprite sp, int i)
    {
        switch (i)
        {
            case 0:
                if (curTitleSprite != null)
                {
                    AssetUtility.DestroyAsset(curTitleSprite);
                    curTitleSprite = null;
                }
                curTitleSprite = sp;
                break;
            case 1:
                if (curNamedesSprite != null)
                {
                    AssetUtility.DestroyAsset(curNamedesSprite);
                    curNamedesSprite = null;
                }
                curNamedesSprite = sp;
                break;
            case 2:
                if (curSprite != null)
                {
                    AssetUtility.DestroyAsset(curSprite);
                    curSprite = null;
                }
                curSprite = sp;
                break;
            case 3:
                if (curCenterSprite != null)
                {
                    AssetUtility.DestroyAsset(curCenterSprite);
                    curCenterSprite = null;
                }
                curCenterSprite = sp;
                break;
            default:
                break;
        }
    }

    void SetMask(Sprite sp)
    {
        if (curMaskSprite != null)
        {
            AssetUtility.DestroyAsset(curMaskSprite);
            curMaskSprite = null;
        }
        curMaskSprite = sp;
    }

    void _Open()
    {
        if (instanceObject == null && !isInit)
        {
            Load();
        }
    }

    void _Close()
    {
        Debug.Log("Close start");

        if (instanceObject != null)
        {
            if (m_imagePre != null) m_imagePre.DOKill();
            if (m_image != null) m_image.DOKill();
            if (progressSlider != null) progressSlider.DOKill();
            StopAllCoroutines();

            Debug.Log("Close start111111111111");


            if (m_image != null && AssetCache.ContainsRawObject(m_image.sprite))
                AssetCache.DestroyAsset(m_image.sprite, 0);

            Object.DestroyImmediate(instanceObject);
            m_imagePre = null;
            m_image = null;
            instanceObject = null;
            instanceTransform = null;
            progressSlider = null;
            progressText = null;
            contentText = null;
            descText = null;
            spriteAssetName = null;
            if (curSprite1 != null)
            {
                AssetUtility.DestroyAsset(curSprite1);
                curSprite1 = null;
            }
            if (curSprite2 != null)
            {
                AssetUtility.DestroyAsset(curSprite2);
                curSprite2 = null;
            }
            if (curSprite3 != null)
            {
                AssetUtility.DestroyAsset(curSprite3);
                curSprite3 = null;
            }

            Debug.Log("Close start22222222222222");


            onComplete = null;


            Debug.Log($"onComplete:{onComplete}");

            isInit = false;

            Debug.Log($"isInit:{isInit}");


            if (m_StreamingImgs != null)
            {
                Debug.Log($"m_StreamingImgs:{m_StreamingImgs}");

                foreach (var item in m_StreamingImgs)
                {
                    Debug.Log($"item:{item}");

                    if (item != null && item.texture != null)
                    {
                        Debug.Log($"item.texture:{item.texture.name}");
                        Object.Destroy(item.texture);
                    }
                }

                Debug.Log($"m_StreamingImgs1111111111:{m_StreamingImgs}");

                m_StreamingImgs.Clear();

                Debug.Log($"m_StreamingImgs222222222222:{m_StreamingImgs}");

                m_StreamingImgs = null;
            }

            if (curSprite)
            {
                Debug.Log($"curSprite:{curSprite}");

                if (AssetCache.ContainsRawObject(curSprite))
                    AssetUtility.DestroyAsset(curSprite);
                curSprite = null;
            }

            Debug.Log("Close start333333333333333");


            if (curTitleSprite)
            {
                if (AssetCache.ContainsRawObject(curTitleSprite))
                    AssetUtility.DestroyAsset(curTitleSprite);
                curTitleSprite = null;
            }
            if (curNamedesSprite)
            {
                if (AssetCache.ContainsRawObject(curNamedesSprite))
                    AssetUtility.DestroyAsset(curNamedesSprite);
                curNamedesSprite = null;
            }
        }

        Debug.Log("Close end");

    }

    void _SetProgress(float v, float time)
    {
        if (instanceObject == null || progressSlider == null)
            return;
        progressSlider.DOKill();
        if (time <= 0)
            progressSlider.value = v;
        else
            progressSlider.DOValue(v, time);

        if (v >= 1)
            v = 1f;
    }


    public static void Open(Action pInitAction = null)
    {
        s_DefaultLoaderGUI._Open();
    }

    public static void Close()
    {
        s_DefaultLoaderGUI._Close();
    }

    public static void SetProgress(float v, float time = 0.1f)
    {
        s_DefaultLoaderGUI._SetProgress(v, time);
    }

    public static void SetDescText(string content)
    {
        if (s_DefaultLoaderGUI.descText != null)
        {
            s_DefaultLoaderGUI.descText.text = content;
        }

    }

    public static void SetContenText(string content)
    {
        if (s_DefaultLoaderGUI.contentText != null)
        {
            s_DefaultLoaderGUI.contentText.text = content;
        }

    }

    public static void SetText(string textPath, string content)
    {
        if (s_DefaultLoaderGUI.instanceTransform != null)
        {
            Text text = s_DefaultLoaderGUI.instanceTransform.FindComponent("Text", textPath) as Text;
            if (text != null)
                text.text = content;
        }
    }

    public static void SetImgBg(string name, string tran, UnityAction onComplete)
    {
        s_DefaultLoaderGUI.LoadImg(name, tran, onComplete);
    }

    public static void SetLogin(string name, string tran, UnityAction onComplete)
    {
        s_DefaultLoaderGUI.LoadLogin(name, tran, onComplete);
    }
    public static void SetSliderState(bool active)
    {
        s_DefaultLoaderGUI._SetSliderState(active);
    }
}
