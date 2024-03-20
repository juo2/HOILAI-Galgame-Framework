using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XGUI;
using UnityEngine.UI;
using UnityEngine.Events;

public class ConfigChoice : MonoBehaviour
{
    UnityAction<string> callBack;

    Dictionary<int, ConfigChoiceItem> imageChoiceList = new Dictionary<int, ConfigChoiceItem>();
    List<string> configImageList = new List<string>();
    public XListView imageListView;
    
    public Button closeBtn;

    // Start is called before the first frame update
    void Start()
    {
        imageListView.onCreateRenderer.AddListener(onImageListCreateRenderer);
        imageListView.onUpdateRenderer.AddListener(onImageListUpdateRenderer);


        closeBtn.onClick.AddListener(() => 
        {
            gameObject.SetActive(false);
        });
    }

    public void OnShow(List<string> configList,UnityAction<string> action)
    {
        gameObject.SetActive(true);

        callBack = action;

        configImageList = configList;

        imageListView.dataCount = configImageList.Count;
        imageListView.ForceRefresh();
    }

    void onImageListCreateRenderer(XListView.ListItemRenderer listItem)
    {
        ConfigChoiceItem configItem = listItem.gameObject.GetComponent<ConfigChoiceItem>();
        imageChoiceList[listItem.instanceID] = configItem;
    }

    void onImageListUpdateRenderer(XListView.ListItemRenderer listItem)
    {
        ConfigChoiceItem configItem = imageChoiceList[listItem.instanceID];
        string name = configImageList[listItem.index];
        configItem.Refresh(name, callBack,this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
