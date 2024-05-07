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

        string storyName;
        string storyId;

        // Start is called before the first frame update
        void Start()
        {
            beginBtn.onClick.AddListener(() => {

                XGUIManager.Instance.CloseView("MainView");

                XGUIManager.Instance.OpenView("ConversationView",UILayer.BaseLayer,null, storyId);

            });
        }

        public void Refresh(StoryData storyData,bool isNew)
        {
            storyName = storyData.title;
            storyId = storyData.id;
            storyNameLabel.text = storyName;
            newImage.SetActive(isNew);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}

