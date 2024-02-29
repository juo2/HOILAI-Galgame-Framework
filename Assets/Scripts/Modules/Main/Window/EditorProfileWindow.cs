using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XGUI;

namespace XModules.Main.Window
{
    public class EditorProfileWindow : XBaseView
    {

        [SerializeField]
        XButton closeBtn;

        // Start is called before the first frame update
        void Awake()
        {
            closeBtn.onClick.AddListener(() => 
            {
                XGUIManager.Instance.CloseView("EditorProfileWindow");
            });
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
