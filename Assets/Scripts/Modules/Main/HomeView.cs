using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XGUI;

namespace XModules.Main
{
    public class HomeView : XBaseView
    {
        [SerializeField]
        XButton play;

        // Start is called before the first frame update
        void Start()
        {
            play.onClick.AddListener(() => {

                XGUIManager.Instance.CloseView("MainView");

                XGUIManager.Instance.OpenView("ConversationView");

            });
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}


