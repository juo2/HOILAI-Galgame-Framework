using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XGUI;
using XModules.Data;
using XModules.Proxy;

namespace XModules.Main.Item
{
    public class DialogueItem : MonoBehaviour
    {
        [SerializeField]
        XImage icon;

        [SerializeField]
        XText label;

        [SerializeField]
        XImage sexy;

        [SerializeField]
        XButton btn;

        string npcId = null;
        string sessionId = null;
        string npcName = null;
        // Start is called before the first frame update
        void Start()
        {
            btn.onClick.AddListener(() => {

                if (DataManager.IsHasChatResponse(npcId))
                {
                    XGUIManager.Instance.OpenView("ChatWindow",UILayer.BaseLayer,null, npcId, sessionId, npcName);
                }
                else
                {
                    ProxyManager.GetChatRecord(npcId, () =>
                    {
                        XGUIManager.Instance.OpenView("ChatWindow", UILayer.BaseLayer, null, npcId, sessionId, npcName);
                    });
                }
               
            });
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Refresh(string _npcId ,string _sessionId, string name)
        {
            npcId = _npcId;
            sessionId = _sessionId;
            label.text = name;
            npcName = name;
        }
    }
}



