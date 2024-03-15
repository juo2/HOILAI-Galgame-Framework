using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XGUI;
using XModules.Data;
using XModules.Main.Item;
using NativeWebSocket;
using XModules.Proxy;

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

        [SerializeField]
        XButton infoBtn;

        [SerializeField]
        XButton resetBtn;

        [SerializeField]
        XText nameLabel;

        Stack<ChatItem> gptChatItemPool;
        Stack<ChatItem> meChatItemPool;

        List<ChatItem> gptChatItemList;
        List<ChatItem> meChatItemList;

        string npcId = null;
        string sessionId = null;
        
        WebSocket websocket = null;
        bool isConnecting = false;
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
            ChatItem chatItem = null;
            if (gptChatItemPool.Count > 0)
            {
                chatItem = gptChatItemPool.Pop();
            }
            else
            {
                chatItem = Instantiate(chatRightItem);
            }
            gptChatItemList.Add(chatItem);
            AddChatItem(chatItem, content);
        }

        void AddMeChatItem(string content)
        {
            ChatItem chatItem = null;
            if (meChatItemPool.Count > 0)
            {
                chatItem = meChatItemPool.Pop();
            }
            else
            {
                chatItem = Instantiate(chatLeftItem);
            }
            meChatItemList.Add(chatItem);
            AddChatItem(chatItem, content);
        }

        // Start is called before the first frame update
        void Awake()
        {
            gptChatItemPool = new Stack<ChatItem>();
            meChatItemPool = new Stack<ChatItem>();

            gptChatItemList = new List<ChatItem>();
            meChatItemList = new List<ChatItem>();

            chatRightItem.SetActive(false);
            chatLeftItem.SetActive(false);

            closeBtn.onClick.AddListener(() =>
            {
                XGUIManager.Instance.CloseView("ChatWindow");
            });

            sureBtn.onClick.AddListener(() =>
            {
                SendMessageWebSocket(inputField.text);
                //AddMeChatItem(inputField.text);
                inputField.text = "";
                //AddGptChatItem("你好，我是平行原住的gpt机器人");
                //chatScrollRect.ScrollToBottom();
            });

            infoBtn.onClick.AddListener(() => 
            {
                resetBtn.SetActive(!resetBtn.gameObject.activeSelf);
            });

            resetBtn.onClick.AddListener(() => 
            {
                ProxyManager.DeleteUserSession(sessionId, () => 
                {
                    ClearAllChatItem();
                });
            });
        }


        public override void OnEnableView()
        {
            base.OnEnableView();

            npcId = viewArgs[0] as string;
            sessionId = viewArgs[1] as string;
            
            string npcName = viewArgs[2] as string;

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

            EnableWebSocket();

            LaterScroll();

            nameLabel.text = npcName;
        }

        public override void OnDisableView()
        {
            base.OnDisableView();

            ClearAllChatItem();

            DisableWebSocket();
        }

        void ClearAllChatItem()
        {
            foreach(var item in gptChatItemList)
            {
                item.SetActive(false);
                gptChatItemPool.Push(item);
            }

            foreach (var item in meChatItemList)
            {
                item.SetActive(false);
                meChatItemPool.Push(item);
            }

            gptChatItemList.Clear();
            meChatItemList.Clear();
        }

        async void EnableWebSocket()
        {
            string url = $"ws://ai.sorachat.site/chat/websocket/{npcId}/{DataManager.getPlayerId()}";

            Debug.Log($"url:{url}");

            websocket = new WebSocket(url);

            websocket.OnOpen += () =>
            {
                isConnecting = true;
                Debug.Log("Connection open!");
            };

            websocket.OnError += (e) =>
            {
                isConnecting = false;
                Debug.Log("Error! " + e);
            };

            websocket.OnClose += (e) =>
            {
                isConnecting = false;
                Debug.Log("Connection closed!");
            };

            websocket.OnMessage += (bytes) =>
            {
                var message = System.Text.Encoding.UTF8.GetString(bytes);
                Debug.Log("Received OnMessage! " + message);

                DataManager.createChatData(npcId, "assistant", message);

                AddGptChatItem(message);

                //chatScrollRect.ScrollToBottom();
                LaterScroll();
            };
            Debug.Log("调用了websocket.Connect");
            await websocket.Connect();
        }

        async void DisableWebSocket()
        {
            if (websocket == null)
                return;

            Debug.Log("调用了websocket.Close");
            websocket.CancelConnection();
            await websocket.Close();

            websocket = null;
        }

        async void SendMessageWebSocket(string message)
        {
            if (websocket.State == WebSocketState.Open && isConnecting)
            {
                DataManager.createChatData(npcId, "user", message);

                AddMeChatItem(message);

                Debug.Log($"SendMessageWebSocket:{message}");

                LaterScroll();
                // 发送文本消息
                await websocket.SendText(message);
            }
        }

        void Update()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            if (isConnecting && websocket != null)
                websocket.DispatchMessageQueue();
#endif
        }

        void LaterScroll()
        {
            StartCoroutine(LaterScrollExe());
        }

        IEnumerator LaterScrollExe()
        {
            yield return new WaitForEndOfFrame();
            chatScrollRect.ScrollToBottom();
        }

        private void OnDestroy()
        {
            DisableWebSocket();
        }
    }
}


