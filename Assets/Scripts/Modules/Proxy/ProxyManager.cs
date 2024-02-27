using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking; // ������������
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

            // ����User-Agent����Ȼ��Unity���ⲻ�Ǳ���ģ���Ϊ�˱���һ���ԣ�������Ȼ�����������
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

            // ʹ��WWWForm������������
            WWWForm form = new WWWForm();
            form.AddField("loginType", "1");
            form.AddField("email", email);
            form.AddField("code", code);
            form.AddField("accessToken", "");

            // ����UnityWebRequest������URL�ͷ���
            UnityWebRequest webRequest = UnityWebRequest.Post(url, form);

            // ����User-Agent����ѡ��
            webRequest.SetRequestHeader("User-Agent", "Apifox/1.0.0 (https://apifox.com)");

            // �������󲢵ȴ���Ӧ
            yield return webRequest.SendWebRequest();

            // ����Ƿ��д�����
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                // ��ӡ������Ϣ
                Debug.LogError(webRequest.error);
                errorBack?.Invoke();
            }
            else
            {
                // ����ɹ���ʹ��webRequest.downloadHandler.text��ȡ��Ӧ����

                DataManager.playerResponse = JsonUtility.FromJson<PlayerResponse>(webRequest.downloadHandler.text);

                Debug.Log(webRequest.downloadHandler.text);
                callBack?.Invoke();
            }
        }

        static IEnumerator GetNPCAllList(string url, Action callBack, Action errorBack)
        {
            Debug.Log($"token:{DataManager.getToken()}");

            // �������û�б����ݣ���������Ȼ����һ���յ�WWWForm�����Է���UnityWebRequest.Post�Ĳ���Ҫ��
            WWWForm form = new WWWForm();
            //form.AddField("token", token);
            UnityWebRequest webRequest = UnityWebRequest.Post(url, form);

            // ����User-Agent����Ȼ��Unity���ⲻ�Ǳ���ģ���Ϊ�˱���һ���ԣ�������Ȼ�����������
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

            // ����User-Agent
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

