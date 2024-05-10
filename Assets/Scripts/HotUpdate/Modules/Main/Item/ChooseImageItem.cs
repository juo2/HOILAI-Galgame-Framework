using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using XGUI;

namespace XModules.Main.Item
{
    public class ChooseImageItem : MonoBehaviour
    {
        [SerializeField]
        Image pic;

        [SerializeField]
        XButton upload;

        [SerializeField]
        XButton select;

        [SerializeField]
        XButton change;

        int index = -1;
        string uri = string.Empty;

        public void Refresh(int _index,string _uri)
        {
            Debug.Log("Refresh RefreshRefresh Refresh Refresh");

            index = _index;
            uri = _uri;

            if (index == 0)
            {
                upload.SetActive(true);
                select.SetActive(false);
                change.SetActive(false);
                pic.SetActive(false);
            }
            else
            {
                upload.SetActive(false);
                select.SetActive(true);
                change.SetActive(true);
                pic.SetActive(true);

                StartCoroutine(LoadImageUri(uri));
            }
        }

        IEnumerator LoadImageUri(string uri)
        {
            // 使用 UnityWebRequestTexture 获取纹理
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(uri);
            yield return request.SendWebRequest();

            // 检查是否有错误发生
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error loading image: " + request.error);
            }
            else
            {
                // 获取下载好的纹理
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                Sprite sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                pic.sprite = sprite;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            upload.onClick.AddListener(() => 
            {

#if UNITY_EDITOR
                XEvent.EventDispatcher.DispatchEvent("LOAD_IMAGE", (Path.Combine(Application.dataPath, "Art/Scenes/Game/Texture2D/bg1.jpg")));
#else
                SDK.SDKManager.Instance.Photo(index.ToString());
#endif

            });

            select.onClick.AddListener(() => 
            {
                Debug.Log($"index : {index}    uri : {uri}");
            });

            change.onClick.AddListener(() => 
            { 
                
            });
        }

        // Update is called once per frame
        void Update()
        {

        }

    }
}



