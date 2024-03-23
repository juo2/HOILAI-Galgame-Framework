using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using XGUI;

namespace XNode.Story
{
    public class PanelBtnItem : MonoBehaviour
    {
        public XButton btn;
        public Text label;
        public RuntimeStoryGraph runtime;

        // Start is called before the first frame update
        void Start()
        {
            btn.onClick.AddListener(() =>
            {
                StartCoroutine(LoadPlot(label.text));
            });
        }

        public IEnumerator LoadPlot(string storyId)
        {
            yield return null;

            string _PlotText = string.Empty;
            //string filePath = Path.Combine(AssetDefine.BuildinAssetPath, "HGF/Test.xml");

            string random = DateTime.Now.ToString("yyyymmddhhmmss");
            string url = $"http://appcdn.calfchat.top/story/{storyId}.xml?v={random}";

            Debug.Log($"url:{url}");

            UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                _PlotText = www.downloadHandler.text;
            }
            else
            {
                Debug.Log("Error: " + www.error);
            }
            //try
            {
                //GameAPI.Print($"ÓÎÏ·¾ç±¾£º{_PlotText}");
                runtime.LoadPlot(_PlotText);
                runtime.nameField.text = storyId;
            }
        }

        public void Refresh(string name)
        {
            label.text = name;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
