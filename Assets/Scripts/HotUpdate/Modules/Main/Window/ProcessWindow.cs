using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XGUI;

namespace XModules.Main.Window
{

    public class ProcessWindow : XBaseView
    {
        [SerializeField]
        XButton returnBtn;

        [SerializeField]
        XButton notifyBtn;

        [SerializeField]
        XButton canelBtn;

        // Start is called before the first frame update
        void Start()
        {
            returnBtn.onClick.AddListener(() => 
            {
                XGUIManager.Instance.CloseView("ProcessWindow");
            });

            notifyBtn.onClick.AddListener(() =>
            {
                XGUIManager.Instance.CloseView("ProcessWindow");
            });

            canelBtn.onClick.AddListener(() =>
            {
                XGUIManager.Instance.CloseView("ProcessWindow");
            });
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
