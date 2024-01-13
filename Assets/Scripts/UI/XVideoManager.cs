using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RenderHeads.Media.AVProVideo;

public class XVideoManager : MonoBehaviour
{
    public static XVideoManager s_Instance;
    public static XVideoManager Instance { get { return s_Instance; } }
    private static bool isDestroy = false;
    public static void Initialize()
    {
        GameObject go = new GameObject("VideoManager");
        s_Instance = go.AddComponent<XVideoManager>();
        Object.DontDestroyOnLoad(go);
    }

    private HashSet<XVideoPlayer> m_ActivedPlayers = new HashSet<XVideoPlayer>();
    public MediaPlayer mediaPlayer { get; private set; }
    public string curAssetName { get; private set; }
    private void Awake()
    {
        GameObject go = new GameObject("MediaPlayer-1");
        mediaPlayer = go.AddComponent<MediaPlayer>();
        mediaPlayer.m_AutoOpen = false;
        go.transform.SetParent(transform);
    }


    public void AddVideoPlayer(XVideoPlayer vp)
    {
        if (isDestroy) return;
        if (!m_ActivedPlayers.Contains(vp))
        {
            m_ActivedPlayers.Add(vp);

            if (!mediaPlayer.enabled)
                mediaPlayer.enabled = true;
        }
    }

    public void RemoveVideoPlayer(XVideoPlayer vp)
    {
        if (isDestroy) return;
        if (m_ActivedPlayers.Contains(vp))
        {
            m_ActivedPlayers.Remove(vp);

            if (m_ActivedPlayers.Count <= 0)
            {
                //mediaPlayer.CloseVideo();
                mediaPlayer.enabled = false;
            }
        }
    }

    public void PlayVideo(string assetName)
    {
        curAssetName = assetName;
    }

    public void CloseVideo(string assetName)
    {
        if (assetName == curAssetName)
        {
            mediaPlayer.CloseVideo();
            curAssetName = string.Empty;
        }
    }

    private void OnDestroy()
    {
        isDestroy = true;
    }


}
