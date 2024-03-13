using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XGUI;
using XModules.Data;
using XModules.Main.Item;
using XModules.Proxy;
using static XGUI.XListView;

namespace XModules.Main
{

    public class DialogueView : MonoBehaviour
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
        }

        void OnEnable()
        {
            xListView.SetActive(false);

            ProxyManager.GetUserSessionList(() => {

                xListView.SetActive(true);
                xListView.dataCount = DataManager.getSessionList().Count;
                xListView.ForceRefresh();
            });
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

            dialogueItem.Refresh(npcData.id,sessionData.id, npcData.description);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}


