
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 启动时的闪屏，健康公告等
/// 包括CG动画
/// </summary>
public class LauncherJugglery : MonoBehaviour
{

    private static LauncherJugglery m_Jugglery;
    public static UnityAction ContinueFun;
    public static void Open()
    {
        GameObject go = new GameObject("ScreenImage");
        m_Jugglery = go.AddComponent<LauncherJugglery>();
        go.transform.localPosition = new Vector3(-5000, -5000, -5000);

        m_Jugglery.StartJugglery();

    }

    public static void Destroy()
    {
        if(m_Jugglery != null)
            GameObject.Destroy(m_Jugglery.gameObject);

        ContinueFun = null;
    }



    /// ///////////////////////////////////////////// //////////////////////////////////////////
    /// /// //////////////////////////////////////////
    /// /// //////////////////////////////////////////
    /// /// //////////////////////////////////////////



    private Image m_image;
    private Image m_imagePre;
    private Transform m_transform;
    private Camera m_camera;
    public void StartJugglery()
    {
        StartCoroutine(StartLogin());
    }

    private IEnumerator StartLogin()
    {
        ///闪屏图片
        string[] imgs = XConfig.defaultConfig.startScreenImgs;
        if (imgs != null && imgs.Length > 0)
        {
            yield return StartCoroutine(StartSplashscreen());
        }

        //CG动画
        if (ContinueFun != null) ContinueFun.Invoke();
    }


    //图片///////////////////
    private IEnumerator StartSplashscreen()
    {
        gameObject.AddCanvas(0);
        Canvas canvas = gameObject.GetComponent<Canvas>();
        m_transform = gameObject.GetComponent<Transform>();
        //需要一个摄像机，否则无法调整alpha通过

        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        GameObject cobj = new GameObject("Camera");
        m_camera = cobj.AddComponent<Camera>();
        m_camera.cullingMask = LayerMask.GetMask("UI");
        m_camera.clearFlags = CameraClearFlags.Color;
        m_camera.backgroundColor = Color.black;
        canvas.worldCamera = m_camera;

        m_image = CreateImage("Image1");
        m_imagePre = CreateImage("Image2");

        m_transform.SetLayer(LayerMask.NameToLayer("UI"), true);//设置层级

        yield return ShowSceenImg();
    }
    private Image CreateImage(string ObjName)
    {
        GameObject obj = new GameObject(ObjName);
        obj.transform.SetParentOEx(m_transform);
        Image imge  = obj.AddComponent<Image>();
        imge.color = color_1;
        RectTransform imgRect = imge.GetComponent<RectTransform>();
        imgRect.anchorMin = Vector2.zero;
        imgRect.anchorMax = Vector2.one;
        imgRect.anchoredPosition = Vector2.zero;

        return imge;
    }

    private bool bhideSplash = false;
    //隐藏第一张闪屏的图片
    private void HideSplash()
    {
        if (bhideSplash) return;
        bhideSplash = true;
        //if (XConfig.defaultConfig.isSDKPattern)
        //    GameSdkProxy.instance.HideSplash();
    }


    private Color color_1 = new Color(1, 1, 1, 0);
    private IEnumerator ShowSceenImg()
    {
        string[] imgs = XConfig.defaultConfig.startScreenImgs;

        for(int i = 0;i < imgs.Length;i++)
        {
            string errer = string.Empty;
            Sprite sprite = XFileUtility.ReadStreamingImgEx(imgs[i], out errer);

            HideSplash();

            if (string.IsNullOrEmpty(errer))
            {
                if(i == 0)
                {
                    m_image.sprite = sprite;
                    m_image.color = Color.white;
                }
                else
                {
                    m_imagePre.DOFade(0, 0.1f);

                    m_image.sprite = sprite;
                    m_image.color = color_1;
                    m_image.DOFade(1, 0.5f);   
                }

                yield return new WaitForSeconds(2);
                if(m_imagePre != null)
                {
                    m_imagePre.sprite = sprite;
                    m_imagePre.color = Color.white;
                }
               
            }
        }
    }


    //CG动画///////////////////




    private void OnDestroy()
    {
        if (m_image != null) m_image.DOKill();
        if (m_imagePre != null) m_imagePre.DOKill();
        StopAllCoroutines();
        m_image = null;
        m_imagePre = null;
        if (m_camera != null) GameObject.Destroy(m_camera);

        m_camera = null;
        
    }
}
