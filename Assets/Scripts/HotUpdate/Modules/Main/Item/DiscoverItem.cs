using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XGUI;

namespace XModules.Main.Item
{
    public class DiscoverItem : MonoBehaviour
    {
        [SerializeField]
        XButton beginBtn;

        [SerializeField]
        XText storyNameLabel;

        [SerializeField]
        GameObject newImage;

        [SerializeField]
        GameObject process;

        [SerializeField]
        XText processLabel;

        [SerializeField]
        GameObject enter;

        [SerializeField]
        GameObject history;

        string storyName;
        string storyId;

#if UNITY_EDITOR
        bool isEditor = false;
#endif
        // Start is called before the first frame update
        void Start()
        {
            enter.SetActive(false);
            newImage.SetActive(false);
            process.SetActive(false);
            history.SetActive(false);

            beginBtn.onClick.AddListener(() => 
            {
                XGUIManager.Instance.CloseView("MainView");

#if UNITY_EDITOR
                XGUIManager.Instance.OpenView("ConversationView", UILayer.BaseLayer, null, storyId,isEditor);
#else
                XGUIManager.Instance.OpenView("ConversationView",UILayer.BaseLayer,null, storyId);
#endif
            });
        }

        public void Refresh(StoryData storyData,bool isNew)
        {
            storyName = storyData.title;
            storyId = storyData.id;
            storyNameLabel.text = storyName;
            newImage.SetActive(isNew);

#if UNITY_EDITOR
            isEditor = storyData.isEditor;
#endif
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}

