using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XGUI;
using XModules.Data;
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

        [SerializeField]
        XScrollRect chatScrollRect;

        Stack<ChatItem> gptChatItemPool;
        Stack<ChatItem> meChatItemPool;

        string npcId = null;
        string sessionId = null;

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
            gptChatItemPool = new Stack<ChatItem>();
            meChatItemPool = new Stack<ChatItem>();

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

                //AddGptChatItem("你好，我是平行原住的gpt机器人");

                chatScrollRect.ScrollToBottom();
            });
        }


        public override void OnEnableView()
        {
            base.OnEnableView();

            npcId = viewArgs[0] as string;
            sessionId = viewArgs[1] as string;

            List<ChatData> chatDataList = DataManager.getChatDatabyNpcId(npcId);

            foreach(var data in chatDataList)
            {
                if (data.role == "user")
                {
                    AddMeChatItem(data.content);
                }
                else if(data.role == "assistant")
                {
                    AddGptChatItem(data.content);
                }
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}


