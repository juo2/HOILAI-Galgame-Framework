using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XGUI;

namespace XModules.Main
{
    public class InfoView : XBaseView
    {
        [SerializeField]
        XText titleLabel;

        [SerializeField]
        XText contentLabel;

        [SerializeField]
        XButton closeBtn;

        // Start is called before the first frame update
        void Start()
        {
            closeBtn.onClick.AddListener(() => {

                XGUIManager.Instance.CloseView("InfoView");
            });
        }

        // Update is called once per frame
        void Update()
        {

        }

        public override void OnEnableView()
        {
            base.OnEnableView();

            if (viewArgs.Length >= 2)
            {
                titleLabel.text = viewArgs[0] as string;
                contentLabel.text = viewArgs[1] as string;
            }

            
        }

    }
}