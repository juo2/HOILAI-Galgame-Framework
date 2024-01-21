using System;
using UnityEngine;
using System.Collections;
using System.IO;
using AssetManagement;
using RenderHeads.Media.AVProVideo;
using UnityEngine.Video;
using UnityEngine.UI;
using DG.Tweening;
using XGUI;

#if UNITY_WEBGL
using WeChatWASM;
#endif

[RequireComponent(typeof(DisplayUGUI))]
public class XVideoPlayer : MonoBehaviour
{
    [System.Serializable]
    public enum PathType
    {
        StreamingAssetsFolder,
        PersistentDataFolder,
        AssetBundle,
    }

    [Serializable]

    public enum SizeMode
    {
        FullScreen,
        WindowSize,
    }

    public Action onReady;
    public Action onFinish;
    public Action onError;
    public bool isLoop = false;
    public bool isAutoPlay = true;
    public DisplayUGUI displayUGUI { get; private set; }
    public VideoPlayer videoPlayer { get; private set; }
    [SerializeField] private string m_FilePath = string.Empty;
    public PathType pathType = PathType.StreamingAssetsFolder;

    private RenderTexture m_RenderTexture;
    private RenderTextureFormat renderTextureFormat = RenderTextureFormat.RGB565;
    private RawImage m_RawImage;
    public float floatClearColorChangeSpeed = 0f; //淡入时间
    public float floatBackColorChangeSpeed = 2f; //淡出时间
    private bool m_isFade; 

    Tween fadeClearTween;
    Tween fadeBackTween;
    //是否淡入淡出
    public bool isFade { get { return m_isFade; }set { m_isFade = value; } }
    public RawImage rawImage
    {
        get { return m_RawImage; }
        set { m_RawImage = value; }
    }
    private string lastFileName;
    public string fileName
    {
        get { return m_FilePath; }
        set
        {
            if (m_FilePath == value) return;
            lastFileName = m_FilePath;
            m_FilePath = value;

#if UNITY_WEBGL
            AutoPlayVideo();
#else
            RefreshSource();
#endif
        }
    }

    private void AutoPlayVideo()
    {
        Debug.Log("AssetDefine.BuildinAssetPath:" + AssetDefine.BuildinAssetPath);

        WX.InitSDK((int code) => {
            var systemInfo = WX.GetSystemInfoSync();
            var video = WX.CreateVideo(new WXCreateVideoParam()
            {
                src =  Path.Combine(AssetDefine.BuildinAssetPath, "firstLogin.mp4"),
                controls = false,
                showProgress = false,
                showProgressInControlMode = false,
                autoplay = true,
                showCenterPlayBtn = false,
                underGameView = true,
                width = ((int)systemInfo.screenWidth),
                height = ((int)systemInfo.screenHeight),
            });
            video.OnPlay(() => {
                Debug.Log("video on play");
                Debug.Log("video on play");
                Debug.Log("video on play");
                Debug.Log("video on play");
                if (onReady != null)
                    try { onReady.Invoke(); } catch (Exception e) { XLogger.ERROR_Format(e.ToString()); }
            });
            video.OnError(() => {
                Debug.Log("video on error");
                Debug.Log("video on error");
                Debug.Log("video on error");
                Debug.Log("video on error");
                Debug.Log("video on error");
            });
            video.OnEnded(() => {
                Debug.Log("video on OnEnded");
                Debug.Log("video on OnEnded");
                Debug.Log("video on OnEnded");
                Debug.Log("video on OnEnded");
                Debug.Log("video on OnEnded");
                if (onFinish != null)
                    try { onFinish.Invoke(); } catch (Exception e) { XLogger.ERROR_Format(e.ToString()); }
            });
        });
    }


    void Awake()
    {
        if (Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            videoPlayer = gameObject.TryGetComponent<VideoPlayer>();
            videoPlayer.source = VideoSource.Url;
            videoPlayer.renderMode = VideoRenderMode.CameraFarPlane;
            videoPlayer.targetCamera = XCamera.guiCamera;
            videoPlayer.aspectRatio = VideoAspectRatio.FitHorizontally;
            videoPlayer.frameReady += (VideoPlayer source, long frameIdx) =>
            {
                if (onReady != null)
                    try { onReady.Invoke(); } catch (Exception e) { XLogger.ERROR_Format(e.ToString()); }

            };

            videoPlayer.loopPointReached += (VideoPlayer source) =>
            {
                if (onFinish != null)
                    try { onFinish.Invoke(); } catch (Exception e) { XLogger.ERROR_Format(e.ToString()); }

            };

            videoPlayer.errorReceived += (VideoPlayer source, string message) =>
            {
                if (onError != null)
                    try { XLogger.ERROR_Format("XVideoPlayer::videoplayer. err = {0}", message); onError.Invoke(); } catch (Exception e) { XLogger.ERROR_Format(e.ToString()); }

            };

            displayUGUI = gameObject.GetComponent<DisplayUGUI>();
            if (displayUGUI)
            {
                displayUGUI.enabled = false;
                displayUGUI = null;
            }
        }
        else
        {
            displayUGUI = gameObject.TryGetComponent<DisplayUGUI>();
            displayUGUI._scaleMode = ScaleMode.ScaleAndCrop;
        }
    }

    void OnEnable()
    {
        if (!XVideoManager.Instance) return;


        if (displayUGUI)
        {
            XVideoManager.Instance.AddVideoPlayer(this);
            XVideoManager.Instance.mediaPlayer.Events.AddListener(OnVideoEvent);
            displayUGUI._mediaPlayer = XVideoManager.Instance.mediaPlayer;
        }
        else if (videoPlayer)
            videoPlayer.enabled = true;

#if UNITY_WEBGL
        AutoPlayVideo();
#else
        RefreshSource();
#endif

    }


    void OnDisable()
    {
        if (!XVideoManager.Instance) return;

        if (displayUGUI)
        {
            XVideoManager.Instance.CloseVideo(fileName);
            XVideoManager.Instance.RemoveVideoPlayer(this);
            XVideoManager.Instance.mediaPlayer.Events.RemoveListener(OnVideoEvent);
            displayUGUI._mediaPlayer = null;
        }
        else if (videoPlayer)
            videoPlayer.enabled = false;
    }

    void OnDestroy()
    {
        onReady = null;
        onFinish = null;
        onError = null;
    }

    void RefreshSource()
    {
        RefreshFade(true);
        if (displayUGUI)
            XVideoManager.Instance.mediaPlayer.m_Loop = isLoop;
        else if (videoPlayer)
            videoPlayer.isLooping = isLoop;

        StopAllCoroutines();
        if (pathType == PathType.StreamingAssetsFolder)
        {
            if (string.IsNullOrEmpty(m_FilePath))
            {
                if (displayUGUI)
                    XVideoManager.Instance.CloseVideo(lastFileName);
                else if (videoPlayer)
                    videoPlayer.enabled = false;
            }
            else
            {
                if (displayUGUI)
                {
                    XVideoManager.Instance.PlayVideo(fileName);
                    XVideoManager.Instance.mediaPlayer.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, m_FilePath, isAutoPlay);
                }
                else if (videoPlayer)
                {
                    videoPlayer.enabled = true;
                    videoPlayer.url = Path.Combine(Application.streamingAssetsPath, m_FilePath);
                }
            }
        }
        else if (pathType == PathType.PersistentDataFolder)
        {
            if (string.IsNullOrEmpty(m_FilePath))
            {
                if (videoPlayer)
                    videoPlayer.enabled = false;
                else if (displayUGUI)
                    XVideoManager.Instance.CloseVideo(lastFileName);
            }
            else
            {
                if (videoPlayer)
                {
                    videoPlayer.url = fileName;
                    videoPlayer.enabled = true;
                    videoPlayer.prepareCompleted -= OnPrepareCompleted;
                    videoPlayer.prepareCompleted += OnPrepareCompleted;
                }
                else if (displayUGUI)
                {
                    if (videoPlayer)
                        videoPlayer.enabled = false;

                    XVideoManager.Instance.PlayVideo(fileName);
                    XVideoManager.Instance.mediaPlayer.OpenVideoFromFile(MediaPlayer.FileLocation.AbsolutePathOrURL, fileName, isAutoPlay);
                }
            }
        }
        else if (pathType == PathType.AssetBundle)
        {
            if (string.IsNullOrEmpty(m_FilePath))
            {
                if (videoPlayer)
                    videoPlayer.enabled = false;
                else if (displayUGUI)
                    XVideoManager.Instance.CloseVideo(lastFileName);
            }
            else
            {
                StartCoroutine(LoadCoroutine(m_FilePath));
            }
        }
    }

    IEnumerator LoadCoroutine(string assetName)
    {
        assetName = Path.GetFileNameWithoutExtension(assetName) + ".bytes";
        string fileName = AssetUtility.GetAssetMD5(assetName) + ".mp4";
        string md5path = Path.Combine(AssetDefine.TempVideoPath, fileName);
        if (!File.Exists(md5path))
        {
            TextAsset buff;
            AssetInternalLoader loader = AssetUtility.LoadAsset<TextAsset>(assetName);
            if (loader == null)
            {
                Debug.LogErrorFormat((string.Format("asset is null {0}", assetName)));
                CallError();
                yield break;
            }

            while (!loader.IsDone())
            {
                yield return 0;
            }

            if (!string.IsNullOrEmpty(loader.Error))
            {
                Debug.LogErrorFormat(loader.Error);
                yield break;
            }

            buff = loader.GetRawObject<TextAsset>();
            AssetUtility.DestroyAsset(buff);

            if (buff)
            {
                if (!Directory.Exists(AssetDefine.TempVideoPath))
                    Directory.CreateDirectory(AssetDefine.TempVideoPath);
                File.WriteAllBytes(md5path, buff.bytes);
            }
            else
            {
                CallError();
                yield break;
            }
        }
        RefreshFade(false);
        string videoPath = md5path;//Path.GetDirectoryName(md5path);//+ "/" + fileName;
        if (videoPlayer)
        {
            videoPlayer.url = videoPath;
            videoPlayer.enabled = true;
            videoPlayer.prepareCompleted -= OnPrepareCompleted;
            videoPlayer.prepareCompleted += OnPrepareCompleted;
        }
        else if (displayUGUI)
        {
            if (videoPlayer)
                videoPlayer.enabled = false;
           
            XVideoManager.Instance.PlayVideo(fileName);
            XVideoManager.Instance.mediaPlayer.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToPersistentDataFolder, videoPath, isAutoPlay);
        }
    }

    private void OnPrepareCompleted(VideoPlayer source)
    {
        if (onReady != null)
        {
            try
            {
                onReady.Invoke();
            }
            catch (Exception e)
            {
                XLogger.ERROR_Format(e.ToString());
            }
        }
    }    
    private void OnVideoEvent(MediaPlayer mp, MediaPlayerEvent.EventType type, ErrorCode error)
    {
        if (type == MediaPlayerEvent.EventType.FirstFrameReady)
        {
            if (onReady != null)
            {
                try
                {
                    onReady.Invoke();
                }
                catch (Exception e)
                {
                    XLogger.ERROR_Format(e.ToString());
                }
            }
        }
        else if (type == MediaPlayerEvent.EventType.FinishedPlaying)
        {
            if (onFinish != null)
            {
                try
                {
                    onFinish.Invoke();
                }
                catch (Exception e)
                {
                    XLogger.ERROR_Format(e.ToString());
                }
            }
        }

        if (error != ErrorCode.None)
        {
            CallError();
        }
        //XLogger.DEBUG_Format("XVideoPlayer.OnVideoEvent: {0}/{1}", type, error);
    }


    private void CallError()
    {
        if (onError != null)
        {
            try
            {
                onError.Invoke();
            }
            catch (Exception e)
            {
                XLogger.ERROR_Format(e.ToString());
            }
        }
    }
    public void PlayerPause()
    {
        if (displayUGUI != null && displayUGUI._mediaPlayer != null)
            displayUGUI._mediaPlayer.Pause();
        else if (videoPlayer)
            videoPlayer.Pause();
    }

    public void PlayerPlay()
    {
        if (displayUGUI != null && displayUGUI._mediaPlayer != null)
            displayUGUI._mediaPlayer.Play();
        else if (videoPlayer)
            videoPlayer.Play();
    }

    public void SetVolume(float volume = 1f)
    {
        if (displayUGUI != null && displayUGUI._mediaPlayer != null)
            displayUGUI._mediaPlayer.m_Volume = volume;
        else if (videoPlayer)
            videoPlayer.SetDirectAudioVolume(0, volume);
    }

    //刷新淡出
    public void RefreshFade(bool isClear)
    {
        if (!m_isFade)
            return;
        if(isClear)
            FadeToClear();
        else
            FadeToBlack();
    }

    /// <summary>
    ///淡入
    /// </summary>
    private void FadeToClear()
    {
        if (!m_isFade) 
            return;
        if (displayUGUI)
        {
            if (fadeClearTween != null)
                fadeClearTween.Kill(false);
            fadeClearTween = displayUGUI.DOColor(Color.clear, floatClearColorChangeSpeed);
        }
        else if(videoPlayer)
        {
            if (fadeClearTween != null)
                fadeClearTween.Kill(false);
            fadeClearTween = m_RawImage.DOColor(Color.clear, floatClearColorChangeSpeed);
        }
    }

    /// <summary>
    /// 淡出
    /// </summary>
    private void FadeToBlack()
    {
        if (displayUGUI)
        {
            if (fadeBackTween != null)
                fadeBackTween.Kill(false);
            fadeBackTween = displayUGUI.DOColor(Color.white, floatBackColorChangeSpeed);
        }
        else if (videoPlayer)
        {
            if (fadeBackTween != null)
                fadeBackTween.Kill(false);
            fadeBackTween = m_RawImage.DOColor(Color.white, floatBackColorChangeSpeed);
        }
    }

    //设置显示模式
    public void SetSizeMode(SizeMode mode = SizeMode.FullScreen)
    {
        if (displayUGUI != null && displayUGUI._mediaPlayer != null)
        {
            displayUGUI._scaleMode = mode == SizeMode.FullScreen ? ScaleMode.ScaleAndCrop : ScaleMode.StretchToFill;
            if (m_RawImage)
                m_RawImage.SetActive(false);
        }
        if (videoPlayer)
        {
            if (mode == SizeMode.FullScreen)
            {
                videoPlayer.renderMode = VideoRenderMode.CameraFarPlane;
            }
            else if (mode == SizeMode.WindowSize)
            {
                videoPlayer.renderMode = VideoRenderMode.RenderTexture;
                if (m_RenderTexture == null && m_RawImage != null)
                {
                    m_RawImage.SetActive(true);
                    Vector2 size = m_RawImage.rectTransform.rect.size;
                    RenderTextureDescriptor desc = new RenderTextureDescriptor((int)size.x, (int)size.y, renderTextureFormat);
                    m_RenderTexture = RenderTexture.GetTemporary(desc);
                    videoPlayer.targetTexture = m_RenderTexture;
                    videoPlayer.aspectRatio = VideoAspectRatio.Stretch;
                }
                m_RawImage.texture = m_RenderTexture;
            }
        }
    }
}

