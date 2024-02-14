using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XGUI;

namespace XModules.Main.Item
{
    public class DialogueItem : MonoBehaviour
    {
        [SerializeField]
        XImage icon;

        [SerializeField]
        XText label;

        [SerializeField]
        XImage sexy;

        [SerializeField]
        XButton btn;

        // Start is called before the first frame update
        void Start()
        {
            btn.onClick.AddListener(() => {

                XGUIManager.Instance.OpenView("ChatWindow");

            });
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Refresh(string name)
        {
            label.text = name;
        }
    }
}



