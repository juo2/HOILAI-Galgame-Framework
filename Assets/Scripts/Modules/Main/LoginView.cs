using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XGUI;
using XModules.Main.Item;
using XModules.Proxy;

namespace XModules.Main
{
    public class LoginView : XBaseView
    {
        [SerializeField]
        XButton btn1;

        [SerializeField]
        XButton btn2;

        [SerializeField]
        XInputField XInputField1;

        [SerializeField]
        XInputField XInputField2;

        // Start is called before the first frame update
        void Start()
        {
            XInputField1.text = "849616969@qq.com";

            btn1.onClick.AddListener(() => 
            {
                ProxyManager.SendCodeRequest(XInputField1.text);
            });

            btn2.onClick.AddListener(() =>
            {
                ProxyManager.LoginRequest(XInputField1.text, XInputField2.text, () => 
                {
                    ProxyManager.GetNPCAllList(() => {

                        ProxyManager.GetUserSessionList(() => {

                            XGUIManager.Instance.CloseView("LoginView");
                            XGUIManager.Instance.OpenView("MainView");

                        });

                    });
                });
            });
        }


        // Update is called once per frame
        void Update()
        {

        }
    }
}


