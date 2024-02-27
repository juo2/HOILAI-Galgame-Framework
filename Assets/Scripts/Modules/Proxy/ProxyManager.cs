using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking; // 用于网络请求
using XModules.Data;

namespace XModules.Proxy
{
    public class ProxyManager
    {
        static string url = "http://23.94.26.242:8080";

        public static void SendCodeRequest(string email, Action callBack = null, Action errorBack = null)
        {
            TimerManager.AddCoroutine(SendCodeRequest($"{url}/chat/user/sendCode", email, callBack, errorBack));
        }

        public static void LoginRequest(string email, string code, Action callBack = null, Action errorBack = null)
        {
            TimerManager.AddCoroutine(LoginRequest($"{url}/chat/user/login", email, code, callBack, errorBack));
        }

        public static void GetNPCAllList(Action callBack = null, Action errorBack = null)
        {
            TimerManager.AddCoroutine(GetNPCAllList($"{url}/chat/npc/npcAllList", callBack, errorBack));
        }

        public static void GetChatRecord(Action callBack = null, Action errorBack = null)
        {
            TimerManager.AddCoroutine(GetChatRecord($"{url}/chat/chatRecord/getChatRecord", callBack, errorBack));
        }

        static IEnumerator SendCodeRequest(string url, string email, Action callBack, Action errorBack)
        {
            WWWForm form = new WWWForm();
            form.AddField("email", email);

            UnityWebRequest webRequest = UnityWebRequest.Post(url, form);

            // 设置User-Agent，虽然在Unity中这不是必需的，但为了保持一致性，我们仍然包含这个步骤
            webRequest.SetRequestHeader("User-Agent", "Apifox/1.0.0 (https://apifox.com)");

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(webRequest.error);
                errorBack?.Invoke();
            }
            else
            {
                Debug.Log(webRequest.downloadHandler.text);

                callBack?.Invoke();
            }
        }

        static IEnumerator LoginRequest(string url, string email, string code, Action callBack, Action errorBack)
        {

            Debug.Log($"code:{code}");

            // 使用WWWForm来构建表单数据
            WWWForm form = new WWWForm();
            form.AddField("loginType", "1");
            form.AddField("email", email);
            form.AddField("code", code);
            form.AddField("accessToken", "");

            // 创建UnityWebRequest，设置URL和方法
            UnityWebRequest webRequest = UnityWebRequest.Post(url, form);

            // 设置User-Agent（可选）
            webRequest.SetRequestHeader("User-Agent", "Apifox/1.0.0 (https://apifox.com)");

            // 发送请求并等待响应
            yield return webRequest.SendWebRequest();

            // 检查是否有错误发生
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                // 打印错误信息
                Debug.LogError(webRequest.error);
                errorBack?.Invoke();
            }
            else
            {
                // 请求成功，使用webRequest.downloadHandler.text获取响应内容

                DataManager.playerResponse = JsonUtility.FromJson<PlayerResponse>(webRequest.downloadHandler.text);

                Debug.Log(webRequest.downloadHandler.text);
                callBack?.Invoke();
            }
        }

        static IEnumerator GetNPCAllList(string url, Action callBack, Action errorBack)
        {
            Debug.Log($"token:{DataManager.getToken()}");

            // 这个请求没有表单数据，但我们依然创建一个空的WWWForm对象，以符合UnityWebRequest.Post的参数要求
            WWWForm form = new WWWForm();
            //form.AddField("token", token);
            UnityWebRequest webRequest = UnityWebRequest.Post(url, form);

            // 设置User-Agent，虽然在Unity中这不是必需的，但为了保持一致性，我们仍然包含这个步骤
            webRequest.SetRequestHeader("User-Agent", "Apifox/1.0.0 (https://apifox.com)");
            webRequest.SetRequestHeader("token", DataManager.getToken());

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(webRequest.error);
                errorBack?.Invoke();
            }
            else
            {
                DataManager.npcResponse = JsonUtility.FromJson<NPCResponse>(webRequest.downloadHandler.text);
                Debug.Log(webRequest.downloadHandler.text);

                callBack?.Invoke();
            }
        }

        static IEnumerator GetChatRecord(string url, Action callBack, Action errorBack)
        {

            Debug.Log($"playerResponse.data.id:{DataManager.getPlayerId()}");

            WWWForm form = new WWWForm();
            form.AddField("userId", DataManager.getPlayerId());
            form.AddField("npcId", "1");

            UnityWebRequest webRequest = UnityWebRequest.Post(url, form);

            // 设置User-Agent
            webRequest.SetRequestHeader("User-Agent", "Apifox/1.0.0 (https://apifox.com)");
            webRequest.SetRequestHeader("token", DataManager.getToken());

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(webRequest.error);
                errorBack?.Invoke();
            }
            else
            {
                DataManager.chatResponse = JsonUtility.FromJson<ChatResponse>(webRequest.downloadHandler.text);
                Debug.Log(webRequest.downloadHandler.text);

                callBack?.Invoke();
            }
        }

    }

}

