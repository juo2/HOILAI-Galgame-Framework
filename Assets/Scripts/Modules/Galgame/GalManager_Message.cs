using System.Collections.Generic;
using UnityEngine;
using XGUI;
using XModules.Data;
using static XGUI.XListView;
using static XModules.Data.ConversationData.Struct_PlotData;

namespace XModules.GalManager
{
    public class GalManager_Message : MonoBehaviour
    {
        [SerializeField]
        XListView xListView;

        [SerializeField]
        XInputField inputField;

        [SerializeField]
        XButton sendBtn;

        List<Struct_Choice> struct_Choices;

        Dictionary<int, GalComponent_Choice> galComponent_ChoiceDic;

        private void Awake ()
        {
            galComponent_ChoiceDic = new Dictionary<int, GalComponent_Choice>();

            xListView.onCreateRenderer.AddListener(onListCreateRenderer);
            xListView.onUpdateRenderer.AddListener(onListUpdateRenderer);
            //GameObject_Choice = Resources.Load<GameObject>("HGF/Button-Choice");
            sendBtn.onClick.AddListener(() => 
            {
                foreach(var history in ConversationData.GetHistoryContentList())
                {
                    Debug.Log($"history.id:{history.id}");
                    Debug.Log($"history.speaker:{history.speaker}");
                    Debug.Log($"history.content:{history.content}");
                    Debug.Log($"history.optContent:{history.optContent}");
                    Debug.Log("------------------------------------------");
                }

                foreach (var choice in struct_Choices)
                {
                    Debug.Log($"choice.Title:{choice.Title}");
                }

                Debug.Log("inputField.text:" + inputField.text);

            });
        }

        void onListCreateRenderer(ListItemRenderer listItem)
        {
            //Debug.Log("GalManager_Choice onListCreateRenderer");

            GalComponent_Choice gl_choice = listItem.gameObject.GetComponent<GalComponent_Choice>();
            galComponent_ChoiceDic[listItem.instanceID] = gl_choice;
        }

        void onListUpdateRenderer(ListItemRenderer listItem)
        {
            //Debug.Log("GalManager_Choice onListUpdateRenderer");

            GalComponent_Choice gl_choice = galComponent_ChoiceDic[listItem.instanceID];
            Struct_Choice choices_data = struct_Choices[listItem.index];

            gl_choice.Init(choices_data.JumpID, choices_data.Title,true);
        }

        [SerializeField]
        public void CreatNewChoice (List<Struct_Choice> choiceList)
        {
            struct_Choices = choiceList;
            xListView.dataCount = choiceList.Count;
            xListView.ForceRefresh();
            //var _ = GameObject_Choice;
            //_.GetComponent<GalComponent_Choice>().Init(JumpID, Title);
            //Instantiate(_, this.transform);
            //return;
        }

    }
}