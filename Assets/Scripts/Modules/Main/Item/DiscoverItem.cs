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

        public void Refresh(StoryData storyData)
        {
            storyName = storyData.title;
            storyId = storyData.id;
            storyNameLabel.text = storyName;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}

