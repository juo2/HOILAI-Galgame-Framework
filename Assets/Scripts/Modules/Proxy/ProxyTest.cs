using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking; // ������������
using UnityEngine.UI;
using XModules.Data;

public class ProxyTest : MonoBehaviour
{
    public string email = "849616969@qq.com";
    string url = "http://23.94.26.242:8080";

    public Button sendCodeBtn;
    public Button loginBtn;
    public InputField inputField;

    public Button npcAllListBtn;
    public Button getChatRecordBtn;
    
    PlayerData playerData;

    // Start is called before the first frame update
    void Start()
    {
        sendCodeBtn.onClick.AddListener(() => 
        { 
            StartCoroutine(SendCodeRequest($"{url}/chat/user/sendCode"));
        });

        loginBtn.onClick.AddListener(() => 
        {
            StartCoroutine(PostRequest($"{url}/chat/user/login", inputField.text));
        });

        npcAllListBtn.onClick.AddListener(() => 
        {
            StartCoroutine(GetNPCAllList($"{url}/chat/npc/npcAllList"));
        });

        getChatRecordBtn.onClick.AddListener(() => 
        {
            StartCoroutine(GetChatRecord($"{url}/chat/chatRecord/getChatRecord", playerData.data.id));
        });
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

        // ����User-Agent����Ȼ��Unity���ⲻ�Ǳ���ģ���Ϊ�˱���һ���ԣ�������Ȼ�����������
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

    IEnumerator PostRequest(string url, string code)
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
        }
        else
        {
            // ����ɹ���ʹ��webRequest.downloadHandler.text��ȡ��Ӧ����
            Debug.Log(webRequest.downloadHandler.text);

            playerData = JsonUtility.FromJson<PlayerData>(webRequest.downloadHandler.text);
        }
    }

    IEnumerator GetNPCAllList(string url)
    {
        // �������û�б����ݣ���������Ȼ����һ���յ�WWWForm�����Է���UnityWebRequest.Post�Ĳ���Ҫ��
        WWWForm form = new WWWForm();

        UnityWebRequest webRequest = UnityWebRequest.Post(url, form);

        // ����User-Agent����Ȼ��Unity���ⲻ�Ǳ���ģ���Ϊ�˱���һ���ԣ�������Ȼ�����������
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

    IEnumerator GetChatRecord(string url,string playerid)
    {

        Debug.Log($"playerData.data.id:{playerData.data.id}");

        WWWForm form = new WWWForm();
        form.AddField("userId", playerid);
        form.AddField("npcId", "1");

        UnityWebRequest webRequest = UnityWebRequest.Post(url, form);

        // ����User-Agent
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
