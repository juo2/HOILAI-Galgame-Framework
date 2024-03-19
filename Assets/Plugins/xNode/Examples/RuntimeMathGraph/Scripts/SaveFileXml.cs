using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SFB;
using UnityEngine.Events;

[RequireComponent(typeof(Button))]
public class SaveFileXml : MonoBehaviour, IPointerDownHandler {

    // Sample text data
    public string saveData = "xxxxxxx";
    public string fileName = "sample";
    public bool isCanSave = true;
    public UnityAction preCallBack;

#if UNITY_WEBGL && !UNITY_EDITOR
    //
    // WebGL
    //
    [DllImport("__Internal")]
    private static extern void DownloadFile(string gameObjectName, string methodName, string filename, byte[] byteArray, int byteArraySize);

    // Broser plugin should be called in OnPointerDown.
    public void OnPointerDown(PointerEventData eventData) {

        preCallBack?.Invoke();

        if (!isCanSave)
            return;

        var bytes = Encoding.UTF8.GetBytes(saveData);
        DownloadFile(gameObject.name, "OnFileDownload", $"{fileName}.xml", bytes, bytes.Length);
    }

    // Called from browser
    public void OnFileDownload() {
        //output.text = "File Successfully Downloaded";
    }
#else
    //
    // Standalone platforms & editor
    //
    public void OnPointerDown(PointerEventData eventData) { }

    // Listen OnClick event in standlone builds
    void Start() {
        var button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    public void OnClick() {

        preCallBack?.Invoke();

        if (!isCanSave)
            return;

        Debug.Log("±£´æ");

        var path = StandaloneFileBrowser.SaveFilePanel("Title", "", fileName, "xml");

        if (!string.IsNullOrEmpty(path)) {
            File.WriteAllText(path, saveData);
        }
    }
#endif
}