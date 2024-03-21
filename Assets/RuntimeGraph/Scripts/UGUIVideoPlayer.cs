

using UnityEngine;
using XGUI;

public class UGUIVideoPlayer : MonoBehaviour
{
    XVideoPlayer videoPlayer;

    public XButton closeBtn;

    // Start is called before the first frame update
    private void Awake()
    {
        videoPlayer = GetComponent<XVideoPlayer>();
        videoPlayer.onFinish = () => {
            gameObject.SetActive(false);
        };

        closeBtn.onClick.AddListener(() => 
        {
            videoPlayer.PlayerPause();
            gameObject.SetActive(false);
        });
    }

    // Update is called once per frame
    void Update()
    {
    }


    public void Play(string asstName)
    {
        gameObject.SetActive(true);
        videoPlayer.pathType = XVideoPlayer.PathType.AssetBundle;
        videoPlayer.fileName = asstName;
        videoPlayer.SetVolume(1);
    }
}