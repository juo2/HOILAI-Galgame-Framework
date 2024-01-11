using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DG.Tweening;
using AssetManagement;
using UnityEngine.Events;
using System;
using UnityEngine.Networking;

public partial class DefaultLoaderGUI : MonoBehaviour
{
    private Image m_imagePre;
    private Image m_imag;
    private Color color_1 = new Color(1, 1, 1, 0);
    private Color color_2 = new Color(1, 1, 1, 1);

    private List<Sprite> m_StreamingImgs;
    //游戏启动时加载也特殊显示逻辑
    //代理可替换图片
    IEnumerator AgentLoadUI(string[] images,bool isStream)
    {
        for (int i = 0; i < images.Length; i++)
        {
            string error = string.Empty;
            XLogger.INFO_Format("AgentLoadUI {0}", images[i]);

            Sprite sprite = null;
            if (!isStream)
            {
                sprite = XFileUtility.ReadStreamingImg(images[i], out error);
            }
            else
            {
                sprite = XFileUtility.ReadStreamingImgEx(images[i], out error);
            }

            if (sprite == null)
                yield return null;

            if (m_StreamingImgs == null) m_StreamingImgs = new List<Sprite>();
            m_StreamingImgs.Add(sprite);
            if (string.IsNullOrEmpty(error))
            {
                XLogger.INFO_Format("OnSwithUI {0}", images[i]);
                OnSwithUI();
                if (i == 0)
                {
                    m_imag.sprite = sprite;
                    m_imag.color = color_2;
                }
                else
                {
                    m_imag.sprite = sprite;
                }

                if (i < images.Length - 1)
                {
                    yield return new WaitForSeconds(4);
                }
            }
        }
    }

    private void OnSwithUI()
    {
        if (m_imag != null) return;
        //Transform trans = instanceTransform.Find("ui_jiazaiye_stand");
        //trans.SetActive(false);

        //Transform canvas = instanceTransform.Find("Canvas");
        m_imag = instanceTransform.FindComponent("Image", "Canvas/Image_bg1") as Image;
        m_imag.color = color_1;
        m_imag.SetActive(true);
        //m_imagePre = instanceTransform.FindComponent("Image", "Canvas/Image_bg2") as Image; 
        //m_imagePre.color = color_1;
        //m_imagePre.SetActive(true);
        //Image black = instanceTransform.FindComponent("Image", "Canvas/Image_black") as Image; 
        //black.color = Color.black;
        //black.SetActive(true);

        //if (XConfig.defaultConfig.isSDKPattern)
        //    HmlSdkProxy.instance.HideSplash();//关闭启动Android闪屏界面
    }
}
