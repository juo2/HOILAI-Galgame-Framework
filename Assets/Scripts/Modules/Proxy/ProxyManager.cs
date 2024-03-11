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
        static string url = "http://ai.sorachat.site";

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

        public static void GetUserSessionList(Action callBack = null, Action errorBack = null)
        {
            TimerManager.AddCoroutine(GetUserSessionList($"{url}/chat/chatRecord/getUserSessionList", callBack, errorBack));
        }

        public static void GetChatRecord(string npcId,Action callBack = null, Action errorBack = null)
        {
            TimerManager.AddCoroutine(GetChatRecord($"{url}/chat/chatRecord/getChatRecord", npcId,callBack, errorBack));
        }

        public static void DeleteUserSession(string userSessionId, Action callBack = null, Action errorBack = null)
        {
            TimerManager.AddCoroutine(DeleteUserSession($"{url}/chat/chatRecord/deleteUserSession", userSessionId, callBack, errorBack));
        }

        public static void StreamOneShotChat(string npcId,string textContent,string question,string options, Action callBack = null, Action errorBack = null)
        {
            TimerManager.AddCoroutine(StreamOneShotChat($"{url}/chat/chatRecord/streamOneShotChat", npcId, textContent, question, options, callBack, errorBack));
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

                BasicResponse basicResponse = JsonUtility.FromJson<BasicResponse>(webRequest.downloadHandler.text);
                if(basicResponse.code == "0")
                {
                    Debug.Log("<color=#4aff11>SendCodeRequest ����ɹ�!!!</color>");
                    callBack?.Invoke();
                }
                else
                {
                    errorBack?.Invoke();
                }
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

                Debug.Log(webRequest.downloadHandler.text);

                PlayerResponse playerResponse = JsonUtility.FromJson<PlayerResponse>(webRequest.downloadHandler.text);
                if (playerResponse.code == "0")
                {
                    DataManager.playerResponse = playerResponse;
                    Debug.Log("<color=#4aff11>LoginRequest ����ɹ�!!!</color>");
                    callBack?.Invoke();
                }
                else
                {
                    errorBack?.Invoke();
                }
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
                Debug.Log(webRequest.downloadHandler.text);
                
                NPCResponse npcResponse = JsonUtility.FromJson<NPCResponse>(webRequest.downloadHandler.text);

                if (npcResponse.code == "0")
                {
                    DataManager.npcResponse = npcResponse;
                    Debug.Log("<color=#4aff11>GetNPCAllList ����ɹ�!!!</color>");
                    callBack?.Invoke();
                }
                else
                {
                    errorBack?.Invoke();
                }
            }
        }

        static IEnumerator GetUserSessionList(string url, Action callBack, Action errorBack)
        {

            Debug.Log($"playerResponse.data.id:{DataManager.getPlayerId()}");

            WWWForm form = new WWWForm();
            form.AddField("userId", DataManager.getPlayerId());

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

                Debug.Log(webRequest.downloadHandler.text);

                SessionResponse sessionResponse = JsonUtility.FromJson<SessionResponse>(webRequest.downloadHandler.text);

                if (sessionResponse.code == "0")
                {
                    DataManager.sessionResponse = sessionResponse;
                    Debug.Log("<color=#4aff11>GetUserSessionList ����ɹ�!!!</color>");
                    callBack?.Invoke();
                }
                else
                {
                    errorBack?.Invoke();
                }
            }
        }

        static IEnumerator GetChatRecord(string url, string npcId, Action callBack, Action errorBack)
        {

            Debug.Log($"playerResponse.data.id:{DataManager.getPlayerId()}");

            WWWForm form = new WWWForm();
            form.AddField("userId", DataManager.getPlayerId());
            form.AddField("npcId", npcId);

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

                Debug.Log(webRequest.downloadHandler.text);

                ChatResponse chatResponse = JsonUtility.FromJson<ChatResponse>(webRequest.downloadHandler.text);

                if (chatResponse.code == "0")
                {
                    DataManager.addChatResponse(npcId, chatResponse);
                    Debug.Log("<color=#4aff11>GetChatRecord ����ɹ�!!!</color>");
                    callBack?.Invoke();
                }
                else
                {
                    errorBack?.Invoke();
                }
            }
        }

        static IEnumerator DeleteUserSession(string url, string userSessionId, Action callBack, Action errorBack)
        {

            Debug.Log($"userSessionId:{userSessionId}");

            WWWForm form = new WWWForm();
            form.AddField("userSessionId", userSessionId);

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

                Debug.Log(webRequest.downloadHandler.text);

                BasicResponse basicResponse = JsonUtility.FromJson<BasicResponse>(webRequest.downloadHandler.text);
                if (basicResponse.code == "0")
                {
                    Debug.Log("<color=#4aff11>DeleteUserSession ����ɹ�!!!</color>");
                    callBack?.Invoke();

                }
                else
                {
                    errorBack?.Invoke();
                }
            }
        }

        static IEnumerator StreamOneShotChat(string url,string npcId, string textContent, string question, string options, Action callBack, Action errorBack)
        {

            Debug.Log($"npcId:{npcId}");

            WWWForm form = new WWWForm();
            form.AddField("userId",DataManager.getPlayerId());
            form.AddField("npcId", npcId);
            form.AddField("textContent", textContent);
            form.AddField("question", question);
            form.AddField("options", options);

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

                Debug.Log(webRequest.downloadHandler.text);

                DataManager.oneShotChatResponse = JsonUtility.FromJson<OneShotChatResponse>(webRequest.downloadHandler.text);
                if (DataManager.oneShotChatResponse.code == "0")
                {
                    Debug.Log("<color=#4aff11>StreamOneShotChat ����ɹ�!!!</color>");
                    callBack?.Invoke();

                }
                else
                {
                    errorBack?.Invoke();
                }
            }
        }

        

    }

}

