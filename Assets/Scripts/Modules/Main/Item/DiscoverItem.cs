using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XGUI;

namespace XModules.Main.Item
{
    public class DiscoverItem : MonoBehaviour
    {
        [SerializeField]
        XButton beginBtn;

        // Start is called before the first frame update
        void Start()
        {
            beginBtn.onClick.AddListener(() => {

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

