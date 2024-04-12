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
    public class HomeView : XBaseView
    {
        [SerializeField]
        XListView xListView;

        [SerializeField]
        XListView xListViewNoPlay;

        Dictionary<int, DiscoverItem> storyItemDic;
        Dictionary<int, DiscoverItem> storyNoPlayItemDic;

        // Start is called before the first frame update
        void Start()
        {
            storyItemDic = new Dictionary<int, DiscoverItem>();
            storyNoPlayItemDic = new Dictionary<int, DiscoverItem>();

            xListView.onCreateRenderer.AddListener(onListCreateRenderer);
            xListView.onUpdateRenderer.AddListener(onListUpdateRenderer);

            xListViewNoPlay.onCreateRenderer.AddListener(onNoPlayListCreateRenderer);
            xListViewNoPlay.onUpdateRenderer.AddListener(onNoPlayListUpdateRenderer);

            ProxyManager.GetStoryList(1, () => 
            {
                xListView.dataCount = DataManager.getStoryList().Count;
                xListView.ForceRefresh();

                ProxyManager.GetStoryList(0, () => {

                    xListViewNoPlay.dataCount = DataManager.getStoryNoPlayList().Count;
                    xListViewNoPlay.ForceRefresh();
                });

            });
        }


        void onListCreateRenderer(ListItemRenderer listItem)
        {
            //Debug.Log("GalManager_Choice onListCreateRenderer");

            DiscoverItem discoverItem = listItem.gameObject.GetComponent<DiscoverItem>();
            storyItemDic[listItem.instanceID] = discoverItem;

        }

        void onListUpdateRenderer(ListItemRenderer listItem)
        {
            DiscoverItem discoverItem = storyItemDic[listItem.instanceID];
            discoverItem.Refresh(DataManager.getStoryList()[listItem.index]);
            //gl_choice.Init(choices_data.JumpID, choices_data.Title);
        }

        void onNoPlayListCreateRenderer(ListItemRenderer listItem)
        {
            DiscoverItem discoverItem = listItem.gameObject.GetComponent<DiscoverItem>();
            storyNoPlayItemDic[listItem.instanceID] = discoverItem;
        }

        void onNoPlayListUpdateRenderer(ListItemRenderer listItem)
        {
            DiscoverItem discoverItem = storyNoPlayItemDic[listItem.instanceID];
            discoverItem.Refresh(DataManager.getStoryNoPlayList()[listItem.index]);
        }


        // Update is called once per frame
        void Update()
        {

        }
    }
}


