using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XGUI;
using UnityEngine.UI;
using UnityEngine.Events;
using XAudio;

public class ConfigChoice : MonoBehaviour
{
    UnityAction<string> callBack;

    Dictionary<int, ConfigChoiceItem> imageChoiceList = new Dictionary<int, ConfigChoiceItem>();

    List<string> configList = new List<string>();
    List<string> originConfigList = new List<string>();
    List<CharacterImage> characterImageList = new List<CharacterImage>();

    public XListView listView;
    public Button closeBtn;
    public UGUIVideoPlayer videoPlayer;
    public InputField sreachInput;
    public Button searchBtn;
    public enum ConfigType
    {
        Image,
        Character,
        Audio,
        Bgm,
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
        listView.onCreateRenderer.AddListener(onImageListCreateRenderer);
        listView.onUpdateRenderer.AddListener(onImageListUpdateRenderer);


        closeBtn.onClick.AddListener(() => 
        {
            XAudioManager.instance.StopBgmMusic();
            gameObject.SetActive(false);
        });

        searchBtn.onClick.AddListener(() => 
        {
            this.configList = filter(this.originConfigList);
            listView.dataCount = configList.Count;
            listView.ForceRefresh();
        });

    }

    public List<string> filter(List<string> configList)
    {
        List<string> returnList = new List<string>();

        foreach(var config in configList)
        {
            if (config.Contains(sreachInput.text))
            {
                returnList.Add(config);
            }
        }

        return returnList;
    }

    public void OnShowImage(List<string> configList,UnityAction<string> action)
    {
        sreachInput.SetActive(true);
        sreachInput.text = "";

        configType = ConfigType.Image;
        gameObject.SetActive(true);
        callBack = action;

        this.configList = configList;
        this.originConfigList = this.configList;

        listView.dataCount = this.configList.Count;
        listView.ForceRefresh();
    }

    public void OnShowCharacter(List<CharacterImage> _characterImageList, UnityAction<string> action)
    {
        sreachInput.SetActive(false);

        configType = ConfigType.Character;
        gameObject.SetActive(true);
        callBack = action;

        characterImageList = _characterImageList;
        listView.dataCount = characterImageList.Count;
        listView.ForceRefresh();
    }

    public void OnShowAudio(List<string> configList, UnityAction<string> action)
    {
        sreachInput.SetActive(true);
        sreachInput.text = "";

        configType = ConfigType.Audio;
        gameObject.SetActive(true);
        callBack = action;

        this.configList = configList;
        this.originConfigList = this.configList;

        listView.dataCount = this.configList.Count;
        listView.ForceRefresh();
    }

    public void OnShowBgm(List<string> configList, UnityAction<string> action)
    {
        sreachInput.SetActive(true);
        sreachInput.text = "";

        configType = ConfigType.Bgm;
        gameObject.SetActive(true);
        callBack = action;

        this.configList = configList;
        this.originConfigList = this.configList;

        listView.dataCount = this.configList.Count;
        listView.ForceRefresh();
    }

    public void OnShowVideo(List<string> configList, UnityAction<string> action)
    {
        sreachInput.SetActive(true);
        sreachInput.text = "";

        configType = ConfigType.Video;
        gameObject.SetActive(true);
        callBack = action;

        this.configList = configList;
        this.originConfigList = this.configList;

        listView.dataCount = this.configList.Count;
        listView.ForceRefresh();
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
            string name = configList[listItem.index];
            configItem.RefreshImage(name, callBack, this);
        }
        else if (configType == ConfigType.Character)
        {
            CharacterImage characterImage = characterImageList[listItem.index];
            configItem.RefreshCharacter(characterImage.ID, characterImage.imageName, callBack, this);
        }
        else if (configType == ConfigType.Bgm)
        {
            string name = configList[listItem.index];
            configItem.RefreshBgm(name, callBack, this);
        }
        else if (configType == ConfigType.Audio)
        {
            string name = configList[listItem.index];
            configItem.RefreshAudio(name, callBack, this);
        }
        else if (configType == ConfigType.Video)
        {
            string name = configList[listItem.index];
            string tmpName = name.Replace(".bytes", ".mp4");
            configItem.RefreshVideo(tmpName, callBack, this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
