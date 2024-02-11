using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XGUI;

namespace XModules.Main.Item
{
    public class ChatItem : MonoBehaviour
    {
        [SerializeField]
        XText label;

        public void SetContent(string content)
        {
            label.text = content;
        }

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}