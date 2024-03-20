using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SFB;
using UnityEngine.Networking;
using UnityEngine.Events;

[RequireComponent(typeof(Button))]
public class OpenFileXml : MonoBehaviour, IPointerDownHandler {
    
    public UnityAction<string> finishCallBack;
#if UNITY_WEBGL && !UNITY_EDITOR
    //
    // WebGL
    //
    [DllImport("__Internal")]
    private static extern void UploadFile(string gameObjectName, string methodName, string filter, bool multiple);

    public void OnPointerDown(PointerEventData eventData) {
        UploadFile(gameObject.name, "OnFileUpload", ".xml", false);
    }

    // Called from browser
    public void OnFileUpload(string url) {
        StartCoroutine(OutputRoutine(url));
    }
#else
    //
    // Standalone platforms & editor
    //
    public void OnPointerDown(PointerEventData eventData) { }

    void Start() {
        var button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick() {
        var paths = StandaloneFileBrowser.OpenFilePanel("Title", "", "xml", false);
        if (paths.Length > 0) {
            StartCoroutine(OutputRoutine(new System.Uri(paths[0]).AbsoluteUri));
        }
    }
#endif

    private IEnumerator OutputRoutine(string url)
    {
        // ����UnityWebRequest����
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // �������󲢵ȴ�ֱ�����
            yield return webRequest.SendWebRequest();

            // ����Ƿ��д�����
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                // �������������������������Ϣ
                Debug.LogError(webRequest.error);
            }
            else
            {
                // �ɹ���Ӧ���������ݣ����罫�ı�������ʾ��UI��
                string output = webRequest.downloadHandler.text;

                finishCallBack?.Invoke(output);
            }
        }
    }
}