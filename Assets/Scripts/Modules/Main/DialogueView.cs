using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XGUI;
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

            xListView.dataCount = 1;
            xListView.ForceRefresh();
        }

        void onListCreateRenderer(ListItemRenderer listItem)
        {
            //Debug.Log("GalManager_Choice onListCreateRenderer");

            DialogueItem dialogueItem = listItem.gameObject.GetComponent<DialogueItem>();
            dialogueItemDic[listItem.instanceID] = dialogueItem;



        }

        void onListUpdateRenderer(ListItemRenderer listItem)
        {
            DialogueItem dialogueItem = dialogueItemDic[listItem.instanceID];
            dialogueItem.Refresh(listItem.index);
            //gl_choice.Init(choices_data.JumpID, choices_data.Title);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}


