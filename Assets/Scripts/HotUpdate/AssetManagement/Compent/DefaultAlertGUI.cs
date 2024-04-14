using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using Object = UnityEngine.Object;

public class DefaultAlertGUI : MonoBehaviour
{
    public static string s_AssetName = "GUI_MessageBox";
    public static DefaultAlertGUI s_DefaultAlertGUI;
    static DefaultAlertGUI()
    {
        GameObject go = new GameObject("DefaultAlertGUI");
        s_DefaultAlertGUI = go.AddComponent<DefaultAlertGUI>();
        Object.DontDestroyOnLoad(go);
    }

    [Flags]
    public enum ButtonOpt : int
    {
        None = 0,
        Close = 1 << 1,
        Sure = 1 << 2,
        Cancel = 1 << 3,
        All = ~0,
    }


    public GameObject instanceObject { get; private set; }
    public Transform instanceTransform { get; private set; }

    public Text titleText { get; private set; }

    public Text contentText { get; private set; }
    public Button closeBtn { get; private set; }
    public Button sureBtn { get; private set; }
    public Button cancelBtn { get; private set; }
    public ButtonOpt btnResult { get; private set; }

    public Action<int> onClick;

    void Load()
    {
        GameObject rawGO = Resources.Load<GameObject>(s_AssetName);
        if (rawGO == null)
        {
            XLogger.WARNING("DefaultAlertGUI::Load . 包内资源格式异常 GameObject ");
            return;
        }

        instanceObject = GameObject.Instantiate<GameObject>(rawGO, transform);
        instanceTransform = instanceObject.transform;
        InitUI();
    }


    Button InitButton(Transform ts)
    {
        Button xbtn = ts.TryGetComponent<Button>();
        xbtn.transition = Selectable.Transition.None;
        return xbtn;
    }


    void InitUI()
    {

        closeBtn = InitButton((Transform)instanceTransform.FindComponent("", "child/Bg/closeBtn"));
        cancelBtn = InitButton((Transform)instanceTransform.FindComponent("", "child/Bg/layout/canel"));
        sureBtn = InitButton((Transform)instanceTransform.FindComponent("", "child/Bg/layout/sure"));
        titleText = (Text)instanceTransform.FindComponent("Text", "child/Bg/Text");
        contentText = (Text)instanceTransform.FindComponent("Text", "child/Bg/context");

        closeBtn.onClick.RemoveAllListeners();
        closeBtn.onClick.AddListener(() => { btnResult = ButtonOpt.Close; _Close(); if (onClick != null) onClick.Invoke((int)btnResult); });

        cancelBtn.onClick.RemoveAllListeners();
        cancelBtn.onClick.AddListener(() => { btnResult = ButtonOpt.Cancel; _Close(); if (onClick != null) onClick.Invoke((int)btnResult); });

        sureBtn.onClick.RemoveAllListeners();
        sureBtn.onClick.AddListener(() => { btnResult = ButtonOpt.Sure; _Close(); if (onClick != null) onClick.Invoke((int)btnResult); });
    }

    void InitButtonOpt(ButtonOpt opt)
    {
        //noCloseImg.SetActive((opt & ButtonOpt.Close) != ButtonOpt.Close);
        closeBtn.SetActive((opt & ButtonOpt.Close) == ButtonOpt.Close);
        cancelBtn.SetActive((opt & ButtonOpt.Cancel) == ButtonOpt.Cancel);
        sureBtn.SetActive((opt & ButtonOpt.Sure) == ButtonOpt.Sure);
    }


    void SetButtonLabel(Button button, string str)
    {
        
        Text text = button.transform.GetComponentInChildren<Text>();
        if (text) text.text = str;
    }

    DefaultAlertGUI _Open(string title, string content, string sureStr, string cancelStr, ButtonOpt opt)
    {
        if (instanceObject == null)
        {
            Load();
        }

        btnResult = ButtonOpt.None;
        InitButtonOpt(opt);

        //titleText.text = title;
        contentText.text = content;
        
        SetButtonLabel(sureBtn, sureStr);
        SetButtonLabel(cancelBtn, cancelStr);
        return this;
    }

    public IEnumerator Wait()
    {
        while (btnResult == 0)
            yield return 0;
    }

    void _Close()
    {
        if (instanceObject != null)
        {
            Object.DestroyImmediate(instanceObject);
            instanceObject = null;
            instanceTransform = null;
        }
    }

    void OnDestroy()
    {
        if (onClick != null) onClick = null;
    }


    public static DefaultAlertGUI Open(string title, string content, string sureStr = "", string cancelStr = "", ButtonOpt opt = ButtonOpt.All)
    {
        sureStr = string.IsNullOrEmpty(sureStr) ? UpdateConst.GetLanguage(11206) : sureStr;
        cancelStr = string.IsNullOrEmpty(cancelStr) ? UpdateConst.GetLanguage(11207) : cancelStr;
        title = string.IsNullOrEmpty(title) ? UpdateConst.GetLanguage(11301) : title;
        return s_DefaultAlertGUI._Open(title, content, sureStr, cancelStr, opt);
    }

    public static DefaultAlertGUI Open(string title, string content, string sureStr, string cancelStr, int opt)
    {
        return Open(title, content, sureStr, cancelStr, (ButtonOpt)Enum.ToObject(typeof(ButtonOpt), opt));
    }

    public static void Close()
    {
        s_DefaultAlertGUI._Close();
    }
}
