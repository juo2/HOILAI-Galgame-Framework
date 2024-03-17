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
        // 创建UnityWebRequest对象
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // 发送请求并等待直到完成
            yield return webRequest.SendWebRequest();

            // 检查是否有错误发生
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                // 处理错误情况，例如输出错误信息
                Debug.LogError(webRequest.error);
            }
            else
            {
                // 成功响应，处理数据，例如将文本内容显示在UI上
                string output = webRequest.downloadHandler.text;

                finishCallBack?.Invoke(output);
            }
        }
    }
}