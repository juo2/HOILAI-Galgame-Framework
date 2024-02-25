using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NativeWebSocket;
using UnityEngine.UI;

public class WebSocketTest : MonoBehaviour
{
    WebSocket websocket;

    public Button send;

    public InputField inputField;

    async void Start()
    {
        websocket = new WebSocket("ws://23.94.26.242:8080/chat/websocket/1/1761765043705843714");

        send.onClick.AddListener(() => 
        {
            SendMessageWebSocket(inputField.text);
        });

        websocket.OnOpen += () =>
        {
            Debug.Log("Connection open!");
        };

        websocket.OnError += (e) =>
        {
            Debug.Log("Error! " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed!");
        };

        websocket.OnMessage += (bytes) =>
        {
            var message = System.Text.Encoding.UTF8.GetString(bytes);
            Debug.Log("Received OnMessage! " + message);
        };

        await websocket.Connect();
    }

    // 调用这个方法来发送消息
    public async void SendMessageWebSocket(string message)
    {
        if (websocket.State == WebSocketState.Open)
        {
            Debug.Log($"SendMessageWebSocket:{message}");
            // 发送文本消息
            await websocket.SendText(message);
        }
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket.DispatchMessageQueue();
#endif
    }

    private async void OnApplicationQuit()
    {
        await websocket.Close();
    }

    private void OnDestroy()
    {
        send.onClick.RemoveAllListeners();
    }
}
