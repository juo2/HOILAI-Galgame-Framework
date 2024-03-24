using NativeWebSocket;
using UnityEngine;
using XModules.Data;
using static XModules.Data.ConversationData;

namespace XModules.GalManager
{
    public partial class ConversationView : XBaseView
    {

        public int currentLoop = 0;
        //public int currentScore = 0;

        enum MessageStatus
        {
            None,
            SendingMessage,
            ReceiveMessage,
            CompleteMessage
        }

        MessageStatus messageStatus = MessageStatus.None;

        public (int, int) ExtractForgivenessValue(string input)
        {
            // 查找"原谅值："文本的位置
            int startIndex = input.IndexOf("原谅值：") + "原谅值：".Length;
            if (startIndex < "原谅值：".Length)
            {
                // 如果没有找到"原谅值："，可能需要处理这种情况
                Debug.LogError("输入字符串不包含'原谅值：'");
                return (10, 100);
            }

            // 从"原谅值："之后开始查找直到遇到换行符或字符串结束
            int endIndex = input.IndexOf('\n', startIndex);
            if (endIndex == -1) // 如果没有找到换行符，则假设原谅值后面没有其他文本
            {
                endIndex = input.Length;
            }

            // 提取原谅值字符串
            string forgivenessStr = input.Substring(startIndex, endIndex - startIndex).Trim();

            // 检查并移除"[DONE]"（如果存在）
            const string doneTag = "[DONE]";
            int doneIndex = forgivenessStr.IndexOf(doneTag);
            if (doneIndex != -1)
            {
                forgivenessStr = forgivenessStr.Remove(doneIndex).Trim();
            }

            // 使用'/'分割原谅值字符串来获取分子和分母
            string[] parts = forgivenessStr.Split('/');
            if (parts.Length != 2)
            {
                // 如果分割结果不是两部分，可能需要处理这种情况
                Debug.LogError("原谅值格式不正确");
                return (10, 100);
            }

            // 尝试转换分子和分母为整数
            if (int.TryParse(parts[0], out int numerator) && int.TryParse(parts[1], out int denominator))
            {
                return (numerator, denominator);
            }
            else
            {
                // 如果转换失败，可能需要处理这种情况
                Debug.LogError("原谅值中的数字不是有效的整数");
                return (10, 100);
            }
        }
        public void OneShotChat()
        {
            messageStatus = MessageStatus.SendingMessage;

            Debug.Log("Enter OneShotChat------------------------------");

            string json = "";

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

                json = DataManager.getWebStreamSocketRequest(textContent, tempInputMessage, options);
            }
            else
            {
                json = DataManager.getWebStreamSocketLoopRequest(tempInputMessage);
            }

            ChoiceComplete();
            SendMessageWebSocket(json);
            character_img.SetActive(true);
            var content = tempInputMessage;
            character_img.SetImage(SelfCharacterInfo.image);
            Gal_SelfText.SetActive(true);

            Gal_SelfText.StartTextContent(content, SelfCharacterInfo.name);

            //下一步
            MessageTouchBack.SetActive(true);
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


            if (Gal_Message.inputType == GalManager_Message.InputType.Choice)
            {
                Gal_OtherText.StreamTextContent(ConversationData.TempNpcCharacterInfo.name);
            }
            else if(Gal_Message.inputType == GalManager_Message.InputType.Loop)
            {
                Gal_OtherText.StreamTextContent(ConversationData.TempNpcCharacterInfo.name,false);
            }

            SendCharMessage(ConversationData.TempNpcCharacterInfo.characterID, "", ConversationData.TempNpcCharacterInfo.isSelf);

            //AddHistoryContent(ConversationData.TempNpcCharacterInfo.characterID, ConversationData.TempNpcCharacterInfo.name, "");
            //MessageTouchBack.SetActive(false);
        }

        void Button_Click_Finish()
        {
            DisableAllText();

            (int value,int maxValue) = ExtractForgivenessValue(currentWebSocketSteamContent);

            Debug.Log($"Button_Click_Finish value:{value}");
            Debug.Log($"Button_Click_Finish maxValue:{maxValue}");

            currentLoop++;
            if (value <= 0 || value>= maxValue ||   ( currentLoop >= Gal_Message.loop && Gal_Message.loop != -1) )
            {
                ConversationData.IsCanJump = true;
                MessageTouchBack.SetActive(false);
                DisableWebSocket();

                //回到主线
                Button_Click_NextPlot();
            }
            else
            {
                character_img.SetActive(true);
                character_img.SetImage(SelfCharacterInfo.image);
                Gal_Message.SetActive(true);
                SendCharMessage("", "", true);
            }
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
                messageStatus = MessageStatus.CompleteMessage;
            }
        }


        async void EnableWebSocket()
        {
            string url = string.Empty;

            if (Gal_Message.inputType == GalManager_Message.InputType.Choice)
            {
                url = $"ws://119.91.133.26/chat/webStreamSocket/{ConversationData.TempNpcCharacterInfo.characterID}/{DataManager.getPlayerId()}";
            }
            else if(Gal_Message.inputType == GalManager_Message.InputType.Loop)
            {
                url = $"ws://119.91.133.26/chat/webSocketStream/{ConversationData.TempNpcCharacterInfo.characterID}/{DataManager.getPlayerId()}";
            }

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

                if(messageStatus == MessageStatus.SendingMessage)
                {
                                //瞬发
            ConversationData.IsSpeak = false;
                    Button_Click_Message();
                }

                messageStatus = MessageStatus.ReceiveMessage;

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