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
    List<CharacterImage> characterImageList = new List<CharacterImage>();

    public XListView imageListView;
    public Button closeBtn;

    public enum ConfigType
    {
        Image,
        Character,
        Audio,
        Video
    }

    public struct CharacterImage
    {
        public string ID;
        public string imageName;
    }

    public ConfigType configType = ConfigType.Image;

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

    public void OnShowImage(List<string> configList,UnityAction<string> action)
    {
        configType = ConfigType.Image;
        gameObject.SetActive(true);
        callBack = action;

        configImageList = configList;
        imageListView.dataCount = configImageList.Count;
        imageListView.ForceRefresh();
    }

    public void OnShowCharacter(List<CharacterImage> _characterImageList, UnityAction<string> action)
    {
        configType = ConfigType.Character;
        gameObject.SetActive(true);
        callBack = action;

        characterImageList = _characterImageList;
        imageListView.dataCount = characterImageList.Count;
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

        if(configType == ConfigType.Image)
        {
            string name = configImageList[listItem.index];
            configItem.RefreshImage(name, callBack, this);
        }
        else if (configType == ConfigType.Character)
        {
            CharacterImage characterImage = characterImageList[listItem.index];
            configItem.RefreshCharacter(characterImage.ID, characterImage.imageName, callBack, this);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
