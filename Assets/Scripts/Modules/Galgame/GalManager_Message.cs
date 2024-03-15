using NativeWebSocket;
using System.Collections.Generic;
using UnityEngine;
using XGUI;
using XModules.Data;
using XModules.Proxy;
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

        WebSocket websocket = null;
        bool isConnecting = false;

        private void Awake ()
        {
            galComponent_ChoiceDic = new Dictionary<int, GalComponent_Choice>();

            xListView.onCreateRenderer.AddListener(onListCreateRenderer);
            xListView.onUpdateRenderer.AddListener(onListUpdateRenderer);
            //GameObject_Choice = Resources.Load<GameObject>("HGF/Button-Choice");
            sendBtn.onClick.AddListener(() => 
            {
                if (ConversationData.TempNpcCharacterInfo != null)
                {
                    ConversationData.tempInputMessage = inputField.text;
                    ConversationData.isRequestChating = true;

                    XEvent.EventDispatcher.DispatchEvent("ONESHOTCHAT");

                    inputField.text = "";
                }

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
            inputField.text = "";
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