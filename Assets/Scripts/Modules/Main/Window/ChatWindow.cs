using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XGUI;
using XModules.Data;
using XModules.Main.Item;
using NativeWebSocket;
using XModules.Proxy;
using static XModules.Data.ConversationData;

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
        XButton closeResetBtn;

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

        bool isRequestingChat = false;

        ChatItem currentChatItem;
        
        public const float DefaultSpeed = 0.045f;

        void AddChatItem(ChatItem chatItem,string content)
        {
            chatItem.SetActive(true);
            chatItem.transform.SetParent(chatRoot);
            chatItem.transform.SetAsLastSibling();

            chatItem.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            chatItem.transform.localScale = Vector3.one;
            
            chatItem.SetContent(content);
        }

        void AddGptChatItem(string content,bool isSteam = false)
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
            

            if(isSteam)
            {
                currentChatItem = chatItem;
                ClearCacheOneChar();
                StartCoroutine(StreamTextContentInternal());
                AddChatItem(chatItem, "");
            }
            else
            {
                AddChatItem(chatItem, content);
            }

        }

        IEnumerator StreamTextContentInternal()
        {
            bool isDone = false;

            string targetChar = getCacheOneChar();

            if (currentWebSocketSteamContent.Contains("[DONE]"))
            {
                isDone = true;
                targetChar = "";
                Debug.Log("[DONE][DONE][DONE][DONE][DONE][DONE][DONE][DONE]");
            }

            currentChatItem.StreamContent(targetChar);

            if (isDone)
            {
                yield return new WaitForSeconds(DefaultSpeed);
                DataManager.createChatData(npcId, "assistant", currentWebSocketSteamContent.Replace("[DONE]", "")); 
                isRequestingChat = false;
            }
            else
            {
                yield return new WaitForSeconds(DefaultSpeed);
                yield return StreamTextContentInternal();
            }
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
                if(isRequestingChat)
                {
                    return;
                }

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
                resetBtn.SetActive(true);
                closeResetBtn.SetActive(true);
            });

            resetBtn.onClick.AddListener(() => 
            {
                ProxyManager.DeleteUserSession(sessionId,npcId, () => 
                {
                    ClearAllChatItem();
                });

                resetBtn.SetActive(false);
                closeResetBtn.SetActive(false);
            });

            closeResetBtn.onClick.AddListener(() => 
            {
                resetBtn.SetActive(false);
                closeResetBtn.SetActive(false);
            });
        }


        public override void OnEnableView()
        {
            base.OnEnableView();

            resetBtn.SetActive(false);
            closeResetBtn.SetActive(false);

            currentChatItem = null;
            isRequestingChat = false;

            npcId = viewArgs[0] as string;
            sessionId = viewArgs[1] as string;
            
            string npcName = viewArgs[2] as string;

            List<ChatData> chatDataList = DataManager.getChatDatabyNpcId(npcId);

            foreach (var data in chatDataList)
            {
                if (data.role == "user")
                {
                    AddMeChatItem(data.content);
                }
                else if (data.role == "assistant")
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
            string url = $"ws://119.91.133.26/chat/webSocketStreamTalk/{npcId}/{DataManager.getPlayerId()}";

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

                if(!isRequestingChat)
                {
                    AddGptChatItem(message,true);
                }

                cacheOutMessageList.Add(message);

                isRequestingChat = true;

                Debug.Log("Received OnMessage! " + message);

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


