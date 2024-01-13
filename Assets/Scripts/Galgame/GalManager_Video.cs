using System;
using UnityEngine;
using UnityEngine.UI;
using XGUI;

namespace ScenesScripts.GalPlot
{
    public class GalManager_Video : MonoBehaviour
    {
        XVideoPlayer videoPlayer;

        public Action onFinish;

        public GameObject conversationGo;

        private void Awake()
        {
            videoPlayer = GetComponent<XVideoPlayer>();

            videoPlayer.onReady = () => {
                conversationGo.SetActive(false);
            };

            videoPlayer.onFinish = ()=> {
                onFinish?.Invoke();
                conversationGo.SetActive(true);
            };
        }

        public void Play(string asstName)
        {
            videoPlayer.pathType = XVideoPlayer.PathType.AssetBundle;
            videoPlayer.fileName = asstName;
            videoPlayer.SetVolume(1);



        }

        private void OnDestroy()
        {
            videoPlayer.onFinish = null;
        }
    }
}