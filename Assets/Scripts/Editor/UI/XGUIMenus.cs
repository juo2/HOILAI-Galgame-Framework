using UnityEngine;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine.UI;
using XGUI;
using System.Collections;
//using TMPro;

public class XGUIMenus : ScriptableObject
{
    static GameObject CallMenuOptions(string name, MenuCommand menuCommand)
    {
        System.Reflection.Assembly Assembly = System.Reflection.Assembly.Load("UnityEditor.UI");
        System.Type type = Assembly.GetType("UnityEditor.UI.MenuOptions");
        System.Reflection.MethodInfo method = type.GetMethod(name);
        method.Invoke(type, new object[] { menuCommand });
        return Selection.activeGameObject;
    }

    [MenuItem("GameObject/UI/Image")]
    static public void OverWriteImage(MenuCommand menuCommand)
    {
        GameObject go = CallMenuOptions("AddImage", menuCommand);
        go.GetComponent<Image>().raycastTarget = false;
    }

    [MenuItem("GameObject/UI/Text")]
    static public void OverWriteText(MenuCommand menuCommand)
    {
        //换成XText
        GameObject go = CallMenuOptions("AddText", menuCommand);
        DestroyImmediate(go.GetComponent<Text>());

        XText txt = go.AddComponent<XText>();
        txt.fontSize = 24;
        txt.color = Color.white;
        txt.raycastTarget = false;
    }


    [MenuItem("GameObject/XGUI/XEnvironment", priority = 10)]
    static public void AddXEnvironment(MenuCommand menuCommand)
    {
        GameObject go = CallMenuOptions("AddCanvas", menuCommand);
        GameObject camGO = new GameObject("UICamera");
        Camera cam = camGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;

        Canvas canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = cam;

        CanvasScaler scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;
    }

    [MenuItem("GameObject/XGUI/XEnvironment 3D", priority = 10)]
    static public GameObject AddXEnvironment3D(MenuCommand menuCommand)
    {
        GameObject go = CallMenuOptions("AddCanvas", menuCommand);
        GameObject camGO = new GameObject("UICamera");
        Camera cam = camGO.AddComponent<Camera>();
        cam.fieldOfView = 20;
        cam.farClipPlane = 20;
        cam.nearClipPlane = 0.3f;
        //cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.farClipPlane = 3;

        Canvas canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = cam;
        canvas.planeDistance = 1;

        CanvasScaler scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;
        return go;
    }

    [MenuItem("GameObject/XGUI/XButton", priority = 10)]
    static public GameObject AddYellowButton(MenuCommand menuCommand)
    {
        GameObject go = CallMenuOptions("AddButton", menuCommand);
        go.name = "btn";
        Image img = go.GetComponent<Image>();
        Object.DestroyImmediate(img);
        XImage ximg = go.AddComponent<XImage>();
        //ximg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GUI/Modules/Common/Images/Buttons/common_btn_annio7.png");
        ximg.type = Image.Type.Simple;
        ximg.SetNativeSize();

        Button btn = go.GetComponent<Button>();
        Object.DestroyImmediate(btn);
        XButton xbutton = go.AddComponent<XButton>();
        Text text = (Text)xbutton.transform.FindComponent(typeof(Text), "Text (Legacy)");
        text.name = "label";
        GameObject obj = text.gameObject;
        DestroyImmediate(text);
        XText xText = obj.AddComponent<XText>();
        xText.rectTransform.offsetMin = Vector2.zero;
        xText.rectTransform.offsetMax = Vector2.zero;
        xbutton.labelText = xText;
        xbutton.labelText.raycastTarget = false;
        xbutton.transition = Selectable.Transition.None;

        Color color = Color.white;
        xText.fontSize = 24;

        ColorUtility.TryParseHtmlString("#63453c", out color);
        xText.color = color;

        go.AddComponent<XButtonScaleTween>();
        //go.AddComponent<XButtonMusic>();
        return go;
    }

    [MenuItem("GameObject/XGUI/Image", priority = 10)]
    static public void AddNoRaycastImage(MenuCommand menuCommand)
    {
        GameObject go = CallMenuOptions("AddImage", menuCommand);
        Image img = go.GetComponent<Image>();
        img.raycastTarget = false;
    }

    [MenuItem("GameObject/XGUI/XImage", priority = 10)]
    static public void AddXImage(MenuCommand menuCommand)
    {
        GameObject go = CallMenuOptions("AddImage", menuCommand);
        Image img = go.GetComponent<Image>();
        Object.DestroyImmediate(img);
        img = go.AddComponent<XImage>();
        img.raycastTarget = false;
    }

    //[MenuItem("GameObject/XGUI/XImageSequence", priority = 10)]
    //static public void AddXImageSequence(MenuCommand menuCommand)
    //{
    //    GameObject go = CallMenuOptions("AddImage", menuCommand);
    //    Image img = go.GetComponent<Image>();
    //    Object.DestroyImmediate(img);
    //    XImageSequence ximg = go.AddComponent<XImageSequence>();
    //    ximg.raycastTarget = false;
    //    ximg.autoSetNativeSize = false;
    //}

    [MenuItem("GameObject/XGUI/XText", priority = 10)]
    static public GameObject AddXText(MenuCommand menuCommand)
    {
        GameObject go = CallMenuOptions("AddText", menuCommand);
        DestroyImmediate(go.GetComponent<Text>());

        XText txt = go.AddComponent<XText>();
        txt.fontSize = 22;
        txt.color = Color.white;
        txt.raycastTarget = false;

        return go;
    }


    [MenuItem("GameObject/XGUI/XListView", priority = 10)]
    static public GameObject AddListView(MenuCommand menuCommand)
    {
        GameObject go = CallMenuOptions("AddScrollView", menuCommand);
        Transform transform = go.transform;
        go.name = "ListView";
        Object.DestroyImmediate(go.GetComponent<Image>());
        Object.DestroyImmediate(transform.Find("Scrollbar Horizontal").gameObject);
        Object.DestroyImmediate(transform.Find("Scrollbar Vertical").gameObject);

        go.AddComponent<XNoDrawingView>();
        ScrollRect scrollRect = go.GetComponent<ScrollRect>();
        RectTransform content = scrollRect.content;
        RectTransform viewport = scrollRect.viewport;
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = viewport.offsetMax = Vector2.zero;

        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = new Vector2(0, viewport.rect.height);


        Object.DestroyImmediate(scrollRect);

        scrollRect = go.AddComponent<XScrollRect>();
        scrollRect.content = content;
        scrollRect.viewport = viewport;

        go.AddComponent<XListView>().xScrollRect = scrollRect as XScrollRect;

        Mask mask = transform.Find("Viewport").GetComponent<Mask>();
        Image image = transform.Find("Viewport").GetComponent<Image>();
        mask.gameObject.AddComponent<RectMask2D>();
        Object.DestroyImmediate(mask);
        Object.DestroyImmediate(image);
        return go;
    }

    [MenuItem("GameObject/XGUI/XListView Small", priority = 10)]
    static public void AddListViewSmall(MenuCommand menuCommand)
    {
        GameObject go = CallMenuOptions("AddScrollView", menuCommand);
        Transform transform = go.transform;
        go.name = "ListView Small";
        Object.DestroyImmediate(transform.Find("Scrollbar Horizontal").gameObject);
        Object.DestroyImmediate(transform.Find("Scrollbar Vertical").gameObject);

        Object.DestroyImmediate(go.GetComponent<Image>());
        go.AddComponent<XNoDrawingView>();

        ScrollRect scrollRect = go.GetComponent<ScrollRect>();
        RectTransform content = scrollRect.content;

        Object.DestroyImmediate(content.gameObject);

        RectTransform viewport = scrollRect.viewport;
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = viewport.offsetMax = Vector2.zero;



        Object.DestroyImmediate(scrollRect);

        go.AddComponent<XListView>().xScrollRect = scrollRect as XScrollRect;

        viewport.gameObject.name = "Content";
        Mask mask = viewport.gameObject.GetComponent<Mask>();
        Image image = viewport.gameObject.GetComponent<Image>();
        Object.DestroyImmediate(mask);
        Object.DestroyImmediate(image);
    }

    [MenuItem("GameObject/XGUI/XInputField", priority = 10)]
    static public void AddXInputField(MenuCommand menuCommand)
    {
        GameObject go = CallMenuOptions("AddInputField", menuCommand);
        InputField inputField = go.GetComponent<InputField>();
        Text text = inputField.textComponent;
        GameObject placeholder = inputField.placeholder.gameObject;

        GameObject obj = text.gameObject;
        DestroyImmediate(text);
        XText xText = obj.transform.AddComponent<XText>();

        xText.alignment = TextAnchor.MiddleCenter;
        xText.supportRichText = false;
        xText.color = new Color(0, 0, 0, 1);
        xText.fontSize = 24;

        Text placeText = placeholder.GetComponent<Text>();
        DestroyImmediate(placeText);
        XText xPlaceText = placeholder.transform.AddComponent<XText>();

        xPlaceText.alignment = TextAnchor.MiddleCenter;
        xPlaceText.fontStyle = FontStyle.Italic;
        xPlaceText.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        xPlaceText.fontSize = 24;

        DestroyImmediate(inputField);
        XInputField xInputField = go.AddComponent<XInputField>();
        xInputField.placeholder = xPlaceText;
        xInputField.textComponent = xText;
    }

    //[MenuItem("GameObject/XGUI/XProressSize", priority = 10)]
    //static public void AddXProressSize(MenuCommand menuCommand)
    //{
    //    //GameObject viewport = new GameObject("Progress", typeof(XProressSize));

    //    GameObject go = CallMenuOptions("AddPanel", menuCommand);
    //    Object.DestroyImmediate(go.GetComponent<Image>());
    //    go.name = "Progress";

    //    RectTransform rect = go.GetComponent<RectTransform>();
    //    rect.anchorMin = new Vector2(0.5f, 0.5f);
    //    rect.anchorMax = new Vector2(0.5f, 0.5f);
    //    rect.pivot = new Vector2(0.5f, 0.5f);
    //    rect.sizeDelta = new Vector2(200, 20);
    //    rect.anchoredPosition = Vector2.zero;

    //    XProgressSize progress = go.AddComponent<XProgressSize>();

    //    GameObject bg_img = new GameObject("bg_img", typeof(Image));
    //    RectTransform bgRect = bg_img.GetComponent<RectTransform>();
    //    bgRect.SetParent(rect);
    //    bgRect.localScale = Vector3.one;
    //    bgRect.localPosition = Vector3.zero;
    //    InitAnchore(bgRect);
    //    progress.bg_img = bg_img.GetComponent<Image>();
    //    progress.bg_img.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GUI/Modules/Common/Images/SmallPiece/common_id_xtdb.png");
    //    progress.bg_img.type = Image.Type.Sliced;

    //    GameObject pro_img = new GameObject("pro_img", typeof(Image));
    //    pro_img.AddComponent<RectMask2D>();
    //    RectTransform proRect = pro_img.GetComponent<RectTransform>();
    //    proRect.SetParent(rect);
    //    proRect.localScale = Vector3.one;
    //    proRect.localPosition = Vector3.zero;
    //    InitAnchore(proRect);
    //    proRect.sizeDelta = new Vector2(200, 16);
    //    proRect.anchoredPosition = new Vector2(0, -2);
    //    progress.progress_img = pro_img.GetComponent<Image>();
    //    progress.progress_img.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GUI/Modules/Common/Images/SmallPiece/common_id_xt01.png");
    //    progress.progress_img.type = Image.Type.Sliced;

    //    GameObject eff_obj = new GameObject("eff_img", typeof(Image));
    //    RectTransform effRect = eff_obj.GetComponent<RectTransform>();
    //    effRect.SetParent(proRect);
    //    effRect.localScale = Vector3.one;
    //    effRect.localPosition = Vector3.zero;
    //    Image img = eff_obj.GetComponent<Image>();
    //    img.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GUI/Modules/Common/Images/SmallPiece/common_id_xtg.png");
    //    img.SetNativeSize();
    //    effRect.anchorMin = new Vector2(1, 0.5f);
    //    effRect.anchorMax = new Vector2(1, 0.5f);
    //    effRect.anchoredPosition = new Vector2(-26, 0);

    //    GameObject textGo = CallMenuOptions("AddText", menuCommand);
    //    DestroyImmediate(textGo.GetComponent<Text>());
    //    XText text = textGo.AddComponent<XText>();
    //    text.name = "por_txt";
    //    RectTransform textRect = textGo.GetComponent<RectTransform>();
    //    textRect.SetParent(rect);
    //    textRect.localScale = Vector3.one;
    //    textRect.localPosition = Vector3.zero;
    //    text.color = Color.yellow;
    //    text.text = "1/100";
    //    progress.label = text;

    //    //Color color = Color.white;
    //    //ColorUtility.TryParseHtmlString("#FFF0D2FF", out color);
    //    //text.color = color;
    //    //text.fontSize = 18;
    //    //Outline outLine = text.gameObject.AddComponent<Outline>();
    //    //ColorUtility.TryParseHtmlString("#6A2D05FF", out color);
    //    //outLine.effectColor = color;

    //    progress.InitUI();
    //    progress.InitValue(progress.m_value, progress.m_maxValue);
    //}

    //[MenuItem("GameObject/XGUI/XToggle", priority = 10)]
    //static public GameObject AddXToggle(MenuCommand menuCommand)
    //{
    //    GameObject go = CallMenuOptions("AddToggle", menuCommand);
    //    go.name = "XToggle";
    //    Toggle tog = go.GetComponent<Toggle>();
    //    Graphic graphic = tog.graphic;
    //    Object.DestroyImmediate(go.GetComponent<Toggle>());
    //    XToggle xToggle = go.AddComponent<XToggle>();
    //    xToggle.transition = Selectable.Transition.None;
    //    xToggle.graphic = graphic;
    //    Text txt = go.transform.Find("Label").GetComponent<Text>();
    //    GameObject obj = txt.gameObject;
    //    DestroyImmediate(txt);

    //    XText xText = obj.transform.AddComponent<XText>();
    //    xText.fontSize = 24;

    //    Image img = go.transform.Find("Background").GetComponent<Image>();
    //    img.raycastTarget = false;
    //    img.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GUI/Modules/Common/Images/Buttons/common_btn_13.png");
    //    img.SetNativeSize();

    //    Image img2 = go.transform.Find("Background/Checkmark").GetComponent<Image>();
    //    img2.raycastTarget = false;
    //    img2.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GUI/Modules/Common/Images/Buttons/common_btn_12.png");
    //    img2.SetNativeSize();

    //    XText text = go.transform.Find("Label").GetComponent<XText>();
    //    text.raycastTarget = false;

    //    RectTransform rect = go.GetComponent<RectTransform>();
    //    rect.sizeDelta = new Vector2(160, 25);
    //    rect.anchorMax = new Vector2(0.5f, 0.5f);
    //    rect.anchorMin = new Vector2(0.5f, 0.5f);

    //    RectTransform rect2 = go.transform.Find("Background").GetComponent<RectTransform>();
    //    rect2.anchoredPosition = new Vector2(-67.5f, 0);
    //    rect2.sizeDelta = new Vector2(25, 25);
    //    rect2.anchorMax = new Vector2(0.5f, 0.5f);
    //    rect2.anchorMin = new Vector2(0.5f, 0.5f);

    //    RectTransform rect3 = go.transform.Find("Background/Checkmark").GetComponent<RectTransform>();
    //    rect3.sizeDelta = new Vector2(30, 26);
    //    rect3.anchorMax = new Vector2(0.5f, 0.5f);
    //    rect3.anchorMin = new Vector2(0.5f, 0.5f);

    //    RectTransform rect4 = go.transform.Find("Label").GetComponent<RectTransform>();
    //    rect4.anchoredPosition = new Vector2(14, 0);
    //    rect4.sizeDelta = new Vector2(132, 30.75f);
    //    rect4.anchorMax = new Vector2(0.5f, 0.5f);
    //    rect4.anchorMin = new Vector2(0.5f, 0.5f);

    //    go.AddComponent<XButtonMusic>();
    //    go.AddComponent<XButtonScaleTween>();
    //    go.AddComponent<XNoDrawingView>();

    //    return go;
    //}

}