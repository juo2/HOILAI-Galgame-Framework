using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XGUI;

public class ConfigChoiceItem : MonoBehaviour
{
    public XImage image;
    public Text label;
    public XButton button;
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

    public void Refresh(string imageName,UnityAction<string> action,ConfigChoice _configChoice)
    {
        callBack = action;
        image.spriteAssetName = imageName;
        label.text = imageName;

        configChoice = _configChoice;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
