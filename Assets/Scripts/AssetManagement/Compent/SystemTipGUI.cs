using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
/// <summary>
/// 一个基本的飘字GUI
/// </summary>
public class SystemTipGUI : MonoBehaviour
{
    public class TextGUI
    {
        public RectTransform p_Transform;
        public Text p_Text;
        public CanvasGroup p_CanvasGroup;
    }


    public static int s_MaxTipNum = 8;
    private List<TextGUI> m_Pool = new List<TextGUI>();
    private List<TextGUI> m_Curr = new List<TextGUI>();

    private Queue<string> m_Queue = new Queue<string>();
    private HashSet<string> m_Showings = new HashSet<string>();
    private float m_Spos = 80;
    public void Add(string tip)
    {
        if (m_Queue.Contains(tip))
            return;
        if (m_Showings.Contains(tip))
            return;
        m_Queue.Enqueue(tip);
        PlayTipOne();
    }

    TextGUI CreateTextGUI()
    {
        TextGUI textGUI = new TextGUI();
        GameObject textGO = new GameObject("GUI_Tip_Text");
        textGUI.p_Text = textGO.AddComponent<Text>();
        textGUI.p_Text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        textGUI.p_Text.fontSize = 22;
        textGUI.p_Text.alignment = TextAnchor.MiddleCenter;

        textGUI.p_CanvasGroup = textGO.AddComponent<CanvasGroup>();

        textGO.AddComponent<Outline>();

        ContentSizeFitter contentSizeFitter = textGO.AddComponent<ContentSizeFitter>();
        contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        textGO.transform.SetParent(transform);
        textGUI.p_Transform = textGO.GetComponent<RectTransform>();
        textGUI.p_Transform.localPosition = Vector3.zero;
        textGUI.p_Transform.localRotation = Quaternion.identity;
        textGUI.p_Transform.localScale = Vector3.one;
        textGUI.p_Transform.anchorMin = new Vector2(0f, 0.5f);
        textGUI.p_Transform.anchorMax = new Vector2(1f, 0.5f);
        textGUI.p_Transform.offsetMin = textGUI.p_Transform.offsetMax = Vector2.zero;

        textGUI.p_Transform.anchoredPosition = new Vector2(0, m_Spos);
        
        return textGUI;
    }



    void PlayTipOne()
    {

        if (m_Curr.Count >= s_MaxTipNum || m_Queue.Count < 1)
            return;


        TextGUI textGUI = null;
        if (m_Pool.Count > 0)
        {
            textGUI = m_Pool[0];
            m_Pool.RemoveAt(0);
            textGUI.p_Transform.anchoredPosition = new Vector2(0, m_Spos);
            textGUI.p_Transform.DOKill();
        }
        else
        {
            textGUI = CreateTextGUI();
        }

        textGUI.p_CanvasGroup.alpha = 1;
        string str = m_Queue.Dequeue();
        if (!m_Showings.Contains(str))
            m_Showings.Add(str);
        textGUI.p_Text.text = str;
        textGUI.p_CanvasGroup.DOFade(0, 1).SetDelay(5).OnComplete(() => { OnFadeComplete(textGUI,str); });

        m_Curr.Add(textGUI);
        if (m_Curr.Count > 0)
        {
            for (int i = 0; i < m_Curr.Count; i++)
            {
                Vector2 targetPos = new Vector2(0, (m_Curr.Count - i) * 30 + m_Spos);
                m_Curr[i].p_Transform.DOAnchorPos(targetPos, 0.2f);
            }
        }
    }

    void OnFadeComplete(TextGUI textGUI,string str)
    {
        if (m_Showings.Contains(str))
            m_Showings.Remove(str);
        m_Curr.Remove(textGUI);
        m_Pool.Add(textGUI);
        PlayTipOne();
    }


    public static void ShowTip(string content)
    {
        if (Debug.isDebugBuild)
        {
            SystemTipGUI.Instance.Add(content);
        }
    }


    //string textStr;
    //public void OnGUI()
    //{
    //    if (GUILayout.Button("Add"))
    //    {
    //        SystemTipGUI.Instance.Add("<color=red>Time.frameCount: </color>" + Time.frameCount);
    //    }
    //}


    static SystemTipGUI s_Instance;
    public static SystemTipGUI Instance
    {
        get
        {
            if (s_Instance == null)
            {
                GameObject go = new GameObject("SystemTipGUI");
                DontDestroyOnLoad(go);
                Canvas canvas = go.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 999;

                CanvasScaler canvasScaler = go.AddComponent<CanvasScaler>();
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.referenceResolution = new Vector2(1280, 720);
                canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                canvasScaler.matchWidthOrHeight = 0.5f;
                s_Instance = go.AddComponent<SystemTipGUI>();

                GameObject xgame = GameObject.Find("xgame");
                if (xgame != null) go.transform.SetParent(xgame.transform);
            }
            return s_Instance;
        }
    }

}
