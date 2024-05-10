using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using XGUI;

namespace XModules.Main
{
    public class ProfileView : XBaseView
    {
        [SerializeField]
        XButton teamBtn;

        [SerializeField]
        XButton feedBackBtn;

        //[SerializeField]
        //XButton deleteBtn;

        [SerializeField]
        XButton editBtn;

        [SerializeField]
        XButton iconBtn;

        [SerializeField]
        Image icon;

        //[SerializeField]
        //XImage icon;

        //[SerializeField]
        //XText nameLabel;

        //[SerializeField]
        //XText idLabel;

        // Start is called before the first frame update
        void Start()
        {
            string teamTitle = "Terms of Service";
            string teamContent = "Lret updstod. apr 01, 2021\nIhisfrcyPolcy de.cres our po cicsand pronodires co thn oo act ica, aie anddeaceura o Your intonnalion when youle tne Sony ce ed tels fou nbout Yocrincy rghis ardhow fhelhw crotccs Yo!Wn uae Your fe ronnl dta to prey co andimprovo tho sarwinn. By rng tho sar ino.You cgmo to the cdllcction and iza o!intomation in ascondenen y ith thisPryncy Palicy.";

            string feedBackTitle = "feed Back";
            string feedBackContent = "feed Back";

            teamBtn.onClick.AddListener(() =>
            {
                XGUIManager.Instance.OpenView("InfoView", UILayer.BaseLayer, null, teamTitle, teamContent);
            });

            feedBackBtn.onClick.AddListener(() =>
            {
                XGUIManager.Instance.OpenView("InfoView",UILayer.BaseLayer,null, feedBackTitle, feedBackContent);

            });

            editBtn.onClick.AddListener(() =>
            {
                XGUIManager.Instance.OpenView("EditorProfileWindow");
            });

            iconBtn.onClick.AddListener(() => 
            {
#if UNITY_EDITOR

                SDK.PhotoData photoData = new SDK.PhotoData();
                photoData.path = Path.Combine(Application.dataPath, $"Art/Scenes/Game/Texture2D/bg1.jpg");
                photoData.exData = "ProfileView";

                string json = JsonUtility.ToJson(photoData);
                XEvent.EventDispatcher.DispatchEvent("LOAD_IMAGE", json);
#else
                SDK.SDKManager.Instance.Photo("ProfileView");
#endif
            });
        }

        private void OnEnable()
        {
            XEvent.EventDispatcher.AddEventListener("LOAD_IMAGE", LoadImage, this);
        }

        private void OnDisable()
        {
            XEvent.EventDispatcher.RemoveEventListener("LOAD_IMAGE", LoadImage, this);
        }

        public override void OnEnableView()
        {
            base.OnEnableView();

            //if (!loadXmlData)
            //    return;

            ////开始游戏
            //Button_Click_NextPlot();
        }

        public override void OnDisableView()
        {
            base.OnDisableView();
        }

        void LoadImage(string json)
        {
            Debug.Log($"LoadImage json:{json}");

            SDK.PhotoData photoData = JsonUtility.FromJson<SDK.PhotoData>(json);

            Debug.Log($"LoadImage photoData.exData:{photoData.exData}");

            if (photoData.exData == "ProfileView")
            {
                StartCoroutine(LoadImageUri($"file://{photoData.path}"));
            }
        }


        // 在Unity中加载图片
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

                Debug.Log("LoadImageUri request");

                // 获取下载好的纹理
                Texture2D texture = DownloadHandlerTexture.GetContent(request);

                Debug.Log($"LoadImageUri texture:{texture}");

                Sprite sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                Debug.Log($"LoadImageUri sprite:{sprite}");

                icon.sprite = sprite;

                Debug.Log($"LoadImageUri icon:{icon}");

            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}


