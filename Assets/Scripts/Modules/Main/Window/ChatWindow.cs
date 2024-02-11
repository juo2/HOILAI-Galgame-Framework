using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XGUI;
using XModules.Main.Item;

namespace XModules.Main.Window
{
    public class ChatWindow : XBaseView
    {
        [SerializeField]
        XInputField inputField;

        [SerializeField]
        XButton closeBtn;

        [SerializeField]
        XButton sureBtn;

        [SerializeField]
        ChatItem chatRightItem;

        [SerializeField]
        ChatItem chatLeftItem;

        [SerializeField]
        Transform chatRoot;

        void AddChatItem(ChatItem chatItem,string content)
        {
            chatItem.SetActive(true);
            chatItem.transform.SetParent(chatRoot);
            chatItem.transform.SetAsLastSibling();

            chatItem.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            chatItem.transform.localScale = Vector3.one;
            
            chatItem.SetContent(content);
        }


        void AddGptChatItem(string content)
        {
            ChatItem chatItem = Instantiate(chatRightItem);
            AddChatItem(chatItem, content);
        }

        void AddMeChatItem(string content)
        {
            ChatItem chatItem = Instantiate(chatLeftItem);
            AddChatItem(chatItem, content);
        }

        // Start is called before the first frame update
        void Awake()
        {
            chatRightItem.SetActive(false);
            chatLeftItem.SetActive(false);

            closeBtn.onClick.AddListener(() =>
            {
                XGUIManager.Instance.CloseView("ChatWindow");
            });

            sureBtn.onClick.AddListener(() =>
            {
                AddMeChatItem(inputField.text);
                inputField.text = "";

                AddGptChatItem("你好，我是平行原住的gpt机器人");
            });
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}


