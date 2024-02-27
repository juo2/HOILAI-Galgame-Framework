using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XGUI;
using XModules.Main.Item;
using static XGUI.XListView;

namespace XModules.Main
{
    public class LoginSelectView : XBaseView
    {
        [SerializeField]
        XButton btn1;

        [SerializeField]
        XButton btn2;

        // Start is called before the first frame update
        void Start()
        {
            btn1.onClick.AddListener(() => 
            {
                XGUI.XGUIManager.Instance.CloseView("LoginSelectView");
                XGUI.XGUIManager.Instance.OpenView("MainView");
            });

            btn2.onClick.AddListener(() =>
            {
                XGUI.XGUIManager.Instance.CloseView("LoginSelectView");
                XGUI.XGUIManager.Instance.OpenView("LoginView");
            });
        }


        // Update is called once per frame
        void Update()
        {

        }
    }
}


