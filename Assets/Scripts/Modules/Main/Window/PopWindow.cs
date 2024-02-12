using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XGUI;
using XModules.Main.Item;

namespace XModules.Main.Window
{
    public class PopWindow : XBaseView
    {
        [SerializeField]
        XText title;

        [SerializeField]
        XText label;

        [SerializeField]
        XButton closeBtn;

        [SerializeField]
        XButton NoBtn;

        [SerializeField]
        XButton YesBtn;

        // Start is called before the first frame update
        void Awake()
        {
            closeBtn.onClick.AddListener(() =>
            {
                XGUIManager.Instance.CloseView("PopWindow");
            });

            closeBtn.onClick.AddListener(() =>
            {
            });
        }

        public void SetContent(string title)
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}


