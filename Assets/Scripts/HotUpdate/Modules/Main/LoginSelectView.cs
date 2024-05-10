using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XGUI;
using XModules.Main.Item;
using XModules.Proxy;
using static XGUI.XListView;

namespace XModules.Main
{
    public class LoginSelectView : XBaseView
    {
        [SerializeField]
        XButton btn1;

        [SerializeField]
        XButton btn2;

        [SerializeField]
        XButton btn3;

        // Start is called before the first frame update
        void Start()
        {
            //btn1.label = "热更新测试";
            btn1.onClick.AddListener(() => 
            {
                //临时操作直接登录
                string id = PlayerPrefs.GetString("TEMP_ID");
                if (string.IsNullOrEmpty(id))
                {
                    XGUI.XGUIManager.Instance.CloseView("LoginSelectView");
                    XGUI.XGUIManager.Instance.OpenView("MainView");
                }
                else
                {
                    ProxyManager.GetNPCAllList(() =>
                    {
                        ProxyManager.GetUserSessionList(() =>
                        {
                            XGUIManager.Instance.CloseView("LoginSelectView");
                            XGUIManager.Instance.OpenView("MainView");
                        });
                    });
                }

            });

            btn2.onClick.AddListener(() =>
            {
                XGUI.XGUIManager.Instance.CloseView("LoginSelectView");
                XGUI.XGUIManager.Instance.OpenView("LoginView");
            });

            btn3.onClick.AddListener(() => 
            {
                SDK.SDKManager.Instance.Login();
            
            });

        }


        // Update is called once per frame
        void Update()
        {

        }
    }
}


