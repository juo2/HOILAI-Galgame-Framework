using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XGUI;
using XModules.Data;
using XModules.Main.Item;
using static XGUI.XListView;

namespace XModules.Main
{

    public class DialogueView : XBaseView
    {
        [SerializeField]
        XListView xListView;

        Dictionary<int, DialogueItem> dialogueItemDic;

        // Start is called before the first frame update
        void Start()
        {
            dialogueItemDic = new Dictionary<int, DialogueItem>();
            xListView.onCreateRenderer.AddListener(onListCreateRenderer);
            xListView.onUpdateRenderer.AddListener(onListUpdateRenderer);

            xListView.dataCount = DataManager.getSessionList().Count;
            xListView.ForceRefresh();
        }

        void onListCreateRenderer(ListItemRenderer listItem)
        {
            DialogueItem dialogueItem = listItem.gameObject.GetComponent<DialogueItem>();
            dialogueItemDic[listItem.instanceID] = dialogueItem;

        }

        void onListUpdateRenderer(ListItemRenderer listItem)
        {
            DialogueItem dialogueItem = dialogueItemDic[listItem.instanceID];
            SessionData sessionData = DataManager.getSessionList()[listItem.index];

            NPCData npcData = DataManager.getNpcById(sessionData.npcId);

            if (npcData == null)
            {
                Debug.Log($"Ã»ÓÐnpcId:{sessionData.npcId}");
                return;
            }

            dialogueItem.Refresh(npcData.id,sessionData.id, npcData.content);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}


