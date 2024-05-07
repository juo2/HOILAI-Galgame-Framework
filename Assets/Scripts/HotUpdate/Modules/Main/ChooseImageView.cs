using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XGUI;
using XModules.Main.Item;
using static XGUI.XListView;

namespace XModules.Main
{
    public class ChooseImageView : XBaseView
    {
        [SerializeField]
        XButton closeBtn;
        [SerializeField]
        XListView xListView;
        [SerializeField]
        XButton sureBtn;

        Dictionary<int, ChooseImageItem> chooseImageItemDic;


        // Start is called before the first frame update
        void Start()
        {
            chooseImageItemDic = new Dictionary<int, ChooseImageItem>();
            xListView.onCreateRenderer.AddListener(onListCreateRenderer);
            xListView.onUpdateRenderer.AddListener(onListUpdateRenderer);

            xListView.dataCount = 1;
            xListView.ForceRefresh();

            closeBtn.onClick.AddListener(() =>
            {
                XGUIManager.Instance.CloseView("ChooseImageView");
            });

            sureBtn.onClick.AddListener(() => 
            {
                XGUIManager.Instance.OpenView("ProcessWindow");
            });
        }


        void onListCreateRenderer(ListItemRenderer listItem)
        {
            //Debug.Log("GalManager_Choice onListCreateRenderer");

            ChooseImageItem chooseImageItem = listItem.gameObject.GetComponent<ChooseImageItem>();
            chooseImageItemDic[listItem.instanceID] = chooseImageItem;

        }

        void onListUpdateRenderer(ListItemRenderer listItem)
        {
            ChooseImageItem chooseImageItem = chooseImageItemDic[listItem.instanceID];
            //dialogueItem.Refresh(listItem.index);
            //dialogueItem.Refresh("Elena");
            //gl_choice.Init(choices_data.JumpID, choices_data.Title);
        }

        // Update is called once per frame
        void Update()
        {

        }

        

    }
}