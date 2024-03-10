using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XGUI;
using XModules.Main.Item;
using static XGUI.XListView;

namespace XModules.Main
{
    public class HomeView : XBaseView
    {

        List<string> storyList = new List<string> { "buqun1", "junjing1", };

        [SerializeField]
        XListView xListView;

        Dictionary<int, DiscoverItem> discoverItemDic;

        // Start is called before the first frame update
        void Start()
        {

            discoverItemDic = new Dictionary<int, DiscoverItem>();
            xListView.onCreateRenderer.AddListener(onListCreateRenderer);
            xListView.onUpdateRenderer.AddListener(onListUpdateRenderer);

            xListView.dataCount = storyList.Count;
            xListView.ForceRefresh();
        }

        void onListCreateRenderer(ListItemRenderer listItem)
        {
            //Debug.Log("GalManager_Choice onListCreateRenderer");

            DiscoverItem discoverItem = listItem.gameObject.GetComponent<DiscoverItem>();
            discoverItemDic[listItem.instanceID] = discoverItem;

        }

        void onListUpdateRenderer(ListItemRenderer listItem)
        {
            DiscoverItem discoverItem = discoverItemDic[listItem.instanceID];
            discoverItem.Refresh(storyList[listItem.index]);
            //gl_choice.Init(choices_data.JumpID, choices_data.Title);
        }


        // Update is called once per frame
        void Update()
        {

        }
    }
}


