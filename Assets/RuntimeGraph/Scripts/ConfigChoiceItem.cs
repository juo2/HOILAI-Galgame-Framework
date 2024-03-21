using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
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

    // Start is called before the first frame update
    void Start()
    {
        button.onClick.AddListener(() => 
        {
            configChoice.SetActive(false);
            callBack?.Invoke(label.text);
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
