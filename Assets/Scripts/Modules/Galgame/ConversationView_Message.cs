using NativeWebSocket;
using UnityEngine;
using XModules.Data;
using static XModules.Data.ConversationData;

namespace XModules.GalManager
{
    public partial class ConversationView : XBaseView
    {

        public int currentLoop = 0;

        public void OneShotChat()
        {
            Debug.Log("Enter OneShotChat------------------------------");

            if (Gal_Message.inputType == GalManager_Message.InputType.Choice)
            {
                string textContent = "";
                foreach (var history in ConversationData.GetHistoryContentList())
                {
                    textContent = textContent + $"{history.speaker}:{ history.content } { history.optContent}";
                }

                string options = "";
                for (int i = 0; i < PlotData.ChoiceTextList.Count; i++)
                {
                    var choice = PlotData.ChoiceTextList[i];
                    options = $"{options}{i}:{choice.Title}";
                }

                string json = DataManager.getWebStreamSocketRequest(textContent, ConversationData.tempInputMessage, options);

                SendMessageWebSocket(json);

                ChoiceComplete();

                character_img.SetActive(true);
                var content = ConversationData.tempInputMessage;
                character_img.SetImage(ConversationData.SelfCharacterInfo.image);
                Gal_SelfText.SetActive(true);

                Gal_SelfText.StartTextContent(content, ConversationData.SelfCharacterInfo.name);

                //下一步
                MessageTouchBack.SetActive(true);
            }
            else
            {

            }
        }
        
        void Button_Click_isRequestChating()
        {
            Debug.Log("Enter Button_Click_isRequestChating------------------------------");

            character_img.SetActive(true);
            character_img.SetImage(ConversationData.TempNpcCharacterInfo.image);

            Gal_OtherText.SetActive(true);
            Gal_OtherText.StartTextContent("............", ConversationData.TempNpcCharacterInfo.name);

            SendCharMessage(ConversationData.TempNpcCharacterInfo.characterID, "", ConversationData.TempNpcCharacterInfo.isSelf);
        }

        void Button_Click_Message()
        {
            Debug.Log("Enter Button_Click_Message------------------------------");
            //string content = DataManager.getNpcResponse();

            character_img.SetActive(true);
            character_img.SetImage(ConversationData.TempNpcCharacterInfo.image);

            Gal_OtherText.SetActive(true);
            Gal_OtherText.StreamTextContent(ConversationData.TempNpcCharacterInfo.name);

            SendCharMessage(ConversationData.TempNpcCharacterInfo.characterID, "", ConversationData.TempNpcCharacterInfo.isSelf);

            //AddHistoryContent(ConversationData.TempNpcCharacterInfo.characterID, ConversationData.TempNpcCharacterInfo.name, "");
            //MessageTouchBack.SetActive(false);
        }

        void StreamFinish()
        {
            if (Gal_Message.inputType == GalManager_Message.InputType.Choice)
            {
                int oneShotSelect = getOneShotChatSelect();

                Struct_PlotData.Struct_Choice choice = PlotData.ChoiceTextList[oneShotSelect];

                //回归主线
                PlotData.NextJumpID = choice.JumpID;

                ConversationData.IsCanJump = true;

                AddHistoryContent(ConversationData.TempNpcCharacterInfo.characterID, ConversationData.TempNpcCharacterInfo.name, webSocketSteamContent);

                MessageTouchBack.SetActive(false);
                DisableWebSocket();
            }
            else if(Gal_Message.inputType == GalManager_Message.InputType.Loop)
            {
                currentLoop++;
                if(currentLoop >= Gal_Message.loop && Gal_Message.loop != -1)
                {
                    ConversationData.IsCanJump = true;
                    MessageTouchBack.SetActive(false);
                    DisableWebSocket();
                }
                else
                {
                    character_img.SetActive(true);
                    character_img.SetImage(SelfCharacterInfo.image);
                    Gal_Message.SetActive(true);
                    SendCharMessage("", "", true);
                }
            }
        }

        async void EnableWebSocket()
        {
            webSocketSteamContent = "";
            currentWebSocketSteamContent = "";
            cacheOutMessageList.Clear();
            cacheIndex = 0;

            string url = $"ws://119.91.133.26/chat/webStreamSocket/{ConversationData.TempNpcCharacterInfo.characterID}/{DataManager.getPlayerId()}";

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
                ConversationData.isRequestChating = false;
                var message = System.Text.Encoding.UTF8.GetString(bytes);
                Debug.Log("Received OnMessage! " + message);

                cacheOutMessageList.Add(message);

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
                Debug.Log($"SendMessageWebSocket:{message}");
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

        private void OnDestroy()
        {
            DisableWebSocket();
        }

    }
}