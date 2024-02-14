using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XGUI;

namespace XModules.Main
{
    public class ProfileView : XBaseView
    {
        [SerializeField]
        XButton teamBtn;

        [SerializeField]
        XButton feedBackBtn;

        //[SerializeField]
        //XButton deleteBtn;

        [SerializeField]
        XButton editBtn;

        //[SerializeField]
        //XButton logoutBtn;

        //[SerializeField]
        //XImage icon;

        //[SerializeField]
        //XText nameLabel;

        //[SerializeField]
        //XText idLabel;

        // Start is called before the first frame update
        void Start()
        {
            string teamTitle = "Terms of Service";
            string teamContent = "Lret updstod. apr 01, 2021\nIhisfrcyPolcy de.cres our po cicsand pronodires co thn oo act ica, aie anddeaceura o Your intonnalion when youle tne Sony ce ed tels fou nbout Yocrincy rghis ardhow fhelhw crotccs Yo!Wn uae Your fe ronnl dta to prey co andimprovo tho sarwinn. By rng tho sar ino.You cgmo to the cdllcction and iza o!intomation in ascondenen y ith thisPryncy Palicy.";

            string feedBackTitle = "feed Back";
            string feedBackContent = "feed Back";

            teamBtn.onClick.AddListener(() =>
            {
                XGUIManager.Instance.OpenView("InfoView", UILayer.BaseLayer, null, teamTitle, teamContent);
            });

            feedBackBtn.onClick.AddListener(() =>
            {
                XGUIManager.Instance.OpenView("InfoView",UILayer.BaseLayer,null, feedBackTitle, feedBackContent);

            });

            editBtn.onClick.AddListener(() =>
            {
                XGUIManager.Instance.OpenView("ChooseImageView");
            });
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}


