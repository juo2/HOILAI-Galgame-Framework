using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking; // 用于网络请求

public class ProxyTest : MonoBehaviour
{

    public string email = "849616969@qq.com";

    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(SendCodeRequest("http://23.94.26.242:8080/chat/user/sendCode"));
        //StartCoroutine(PostRequest("http://23.94.26.242:8080/chat/user/login"));
        //StartCoroutine(GetNPCAllList("http://23.94.26.242:8080/chat/npc/npcAllList"));
        StartCoroutine(GetChatRecord("http://23.94.26.242:8080/chat/chatRecord/getChatRecord"));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator SendCodeRequest(string url)
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
        }
        else
        {
            Debug.Log(webRequest.downloadHandler.text);
        }
    }

    IEnumerator PostRequest(string url)
    {
        // 使用WWWForm来构建表单数据
        WWWForm form = new WWWForm();
        form.AddField("loginType", "1");
        form.AddField("email", email);
        form.AddField("code", "9449");
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
        }
        else
        {
            // 请求成功，使用webRequest.downloadHandler.text获取响应内容
            Debug.Log(webRequest.downloadHandler.text);
        }
    }

    IEnumerator GetNPCAllList(string url)
    {
        // 这个请求没有表单数据，但我们依然创建一个空的WWWForm对象，以符合UnityWebRequest.Post的参数要求
        WWWForm form = new WWWForm();

        UnityWebRequest webRequest = UnityWebRequest.Post(url, form);

        // 设置User-Agent，虽然在Unity中这不是必需的，但为了保持一致性，我们仍然包含这个步骤
        webRequest.SetRequestHeader("User-Agent", "Apifox/1.0.0 (https://apifox.com)");

        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(webRequest.error);
        }
        else
        {
            Debug.Log(webRequest.downloadHandler.text);
        }
    }

    IEnumerator GetChatRecord(string url)
    {
        WWWForm form = new WWWForm();
        form.AddField("userId", "1761765043705843714");
        form.AddField("npcId", "1");

        UnityWebRequest webRequest = UnityWebRequest.Post(url, form);

        // 设置User-Agent
        webRequest.SetRequestHeader("User-Agent", "Apifox/1.0.0 (https://apifox.com)");

        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(webRequest.error);
        }
        else
        {
            Debug.Log(webRequest.downloadHandler.text);
        }
    }

}
