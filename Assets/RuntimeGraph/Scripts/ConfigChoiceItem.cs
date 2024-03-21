using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XAudio;
using XGUI;

public class ConfigChoiceItem : MonoBehaviour
{
    public XImage ximage;
    public Image image;
    public Text label;
    public XButton button;
    public XButton musicBtn;
    public XButton videoBtn;

    UnityAction<string> callBack;
    ConfigChoice configChoice;

    string m_audio = string.Empty;
    string m_video = string.Empty;

    // Start is called before the first frame update
    void Start()
    {
        button.onClick.AddListener(() => 
        {
            XAudioManager.instance.StopBgmMusic();
            configChoice.SetActive(false);
            callBack?.Invoke(label.text);
        });

        musicBtn.onClick.AddListener(() => 
        {
            XAudioManager.instance.PlayBgmMusic(m_audio);
        });

        videoBtn.onClick.AddListener(() => 
        { 
            
        });
    }

    public void RefreshImage(string imageName,UnityAction<string> action,ConfigChoice _configChoice)
    {
        ximage.SetActive(true);
        image.SetActive(false);
        musicBtn.SetActive(false);
        videoBtn.SetActive(false);

        callBack = action;
        ximage.spriteAssetName = imageName;
        label.text = imageName;

        configChoice = _configChoice;
    }

    public void RefreshCharacter(string id,string imageName, UnityAction<string> action, ConfigChoice _configChoice)
    {
        musicBtn.SetActive(false);
        videoBtn.SetActive(false);

        callBack = action;

        if (!string.IsNullOrEmpty(imageName))
        {
            ximage.SetActive(true);
            image.SetActive(false);
            ximage.spriteAssetName = imageName;
        }
        else
        {
            ximage.SetActive(false);
            image.SetActive(true);
        }

        label.text = id;

        configChoice = _configChoice;
    }

    public void RefreshAudio(string audioName, UnityAction<string> action, ConfigChoice _configChoice)
    {
        ximage.SetActive(false);
        image.SetActive(false);
        musicBtn.SetActive(true);
        videoBtn.SetActive(false);

        callBack = action;
        label.text = audioName;

        m_audio = audioName;

        configChoice = _configChoice;
    }

    public void RefreshBgm(string audioName, UnityAction<string> action, ConfigChoice _configChoice)
    {
        ximage.SetActive(false);
        image.SetActive(false);
        musicBtn.SetActive(true);
        videoBtn.SetActive(false);

        callBack = action;
        label.text = audioName;
        m_audio = audioName;

        configChoice = _configChoice;
    }

    public void RefreshVideo(string videoName, UnityAction<string> action, ConfigChoice _configChoice)
    {
        ximage.SetActive(false);
        image.SetActive(false);
        musicBtn.SetActive(false);
        videoBtn.SetActive(true);

        callBack = action;
        label.text = videoName;

        m_video = videoName;

        configChoice = _configChoice;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
