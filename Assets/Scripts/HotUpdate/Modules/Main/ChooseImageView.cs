using System.Collections;
using System.Collections.Generic;
using System.IO;
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
        XLayoutView xLayoutView;

        [SerializeField]
        XScrollRect xScrollRect;

        [SerializeField]
        XButton sureBtn;

        List<string> s_data = new List<string>()
        {
            string.Empty,
        };

        // Start is called before the first frame update
        void Start()
        {
            xLayoutView.onCreateRenderer.AddListener(onListCreateRenderer);
            xLayoutView.onUpdateRenderer.AddListener(onListUpdateRenderer);

            xLayoutView.dataCount = s_data.Count ;
            xLayoutView.CreateItem();

            closeBtn.onClick.AddListener(() =>
            {
                XGUIManager.Instance.CloseView("ChooseImageView");
            });

            sureBtn.onClick.AddListener(() => 
            {
                XGUIManager.Instance.OpenView("ProcessWindow");
            });
        }

        public override void OnEnableView()
        {
            base.OnEnableView();
            XEvent.EventDispatcher.AddEventListener("LOAD_IMAGE", AddImage, this);
        }

        public override void OnDisableView()
        {
            base.OnDisableView();
            XEvent.EventDispatcher.RemoveEventListener("LOAD_IMAGE", AddImage, this);
        }

        void AddImage(string json)
        {
            SDK.PhotoData photoData = JsonUtility.FromJson<SDK.PhotoData>(json);

            if(photoData.exData == "0")
            {
                s_data.Add($"file://{photoData.path}");
                xLayoutView.AddItem();
                xScrollRect.ScrollToBottom();
            }
            else
            {
                int index = int.Parse(photoData.exData);
                s_data[index] = $"file://{photoData.path}";
                XLayoutItem layoutItem = xLayoutView.GetItem(index);

                ChooseImageItem chooseImageItem = layoutItem.gameObject.GetComponent<ChooseImageItem>();
                chooseImageItem.Refresh(layoutItem.index, s_data[layoutItem.index]);
            }
        }

        void onListCreateRenderer(XLayoutItem layoutItem)
        {
            //Debug.Log("GalManager_Choice onListCreateRenderer");
            ChooseImageItem chooseImageItem = layoutItem.gameObject.GetComponent<ChooseImageItem>();
            chooseImageItem.Refresh(layoutItem.index, s_data[layoutItem.index]);
        }

        void onListUpdateRenderer(XLayoutItem layoutItem)
        {
            //ChooseImageItem chooseImageItem = chooseImageItemDic[listItem.instanceID];
            //chooseImageItem.Refresh(listItem.index, s_data[listItem.index]);
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