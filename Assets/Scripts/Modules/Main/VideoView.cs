using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XGUI;

namespace XModules.Main
{
    public class VideoView : XBaseView
    {
        XVideoPlayer videoPlayer;

        // Start is called before the first frame update
        private void Awake()
        {
            videoPlayer = GetComponent<XVideoPlayer>();

            //videoPlayer.onReady = () => {
            //    //conversationGo.SetActive(false);
            //};

            videoPlayer.onFinish = () => {
                //onFinish?.Invoke();
                //conversationGo.SetActive(true);
                //gameObject.SetActive(false);
                XGUIManager.Instance.SetActivateLayer(UILayer.BaseLayer, true);

                XGUIManager.Instance.CloseView("VideoView");
            };
        }

        

        // Update is called once per frame
        void Update()
        {

        }

        public override void OnEnableView()
        {
            base.OnEnableView();

            XGUIManager.Instance.SetActivateLayer(UILayer.BaseLayer, false);

            string videoName = viewArgs[0] as string;

            Debug.Log($"videoName:{videoName}");

            Debug.Log($"finishAction:{finishAction}");

            Play(videoName);
        }


        public void Play(string asstName)
        {
            videoPlayer.pathType = XVideoPlayer.PathType.AssetBundle;
            videoPlayer.fileName = asstName;
            videoPlayer.SetVolume(1);
        }
    }

}

