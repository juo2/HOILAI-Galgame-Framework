using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace XGUI
{
    public enum UILayer
    {
        BaseLayer,
        WindowLayer,
        VideoLayer,
    }

    public class XGUIManager : MonoBehaviour
    {

        Dictionary<UILayer, Transform> uiLayerDic = new Dictionary<UILayer, Transform>();
        Dictionary<string, XModules.XBaseView> viewDic = new Dictionary<string, XModules.XBaseView>();

        EventSystem eventSystem;
        public Canvas xCanvas;

        static XGUIManager m_Instance;
        public static XGUIManager Instance
        {
            get
            {
                if (m_Instance != null)
                    return m_Instance;
                GameObject go = new GameObject("XGUIManager");
                //go.hideFlags = HideFlags.HideInHierarchy;
                m_Instance = go.AddComponent<XGUIManager>();

                UnityEngine.Object.DontDestroyOnLoad(go);
                return m_Instance;
            }
        }

        public void Init()
        {
            XCamera.Init();

            GameObject canvasGo = new GameObject();
            canvasGo.name = "MainCanvas";
            xCanvas = canvasGo.AddComponent<Canvas>();
            xCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            xCanvas.worldCamera = XCamera.guiCamera;
            xCanvas.planeDistance = 100;

            CanvasScaler canvasScaler = xCanvas.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1560, 720);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0.5f;
            canvasScaler.referencePixelsPerUnit = 100;

            xCanvas.AddComponent<GraphicRaycaster>();

            CreateLayers();

            GameObject eventSysGo = new GameObject("EventSystem", typeof(StandaloneInputModule));
            eventSystem = eventSysGo.AddComponent<EventSystem>();
        }

        void CreateLayers()
        {
            UILayer[] enumValues = (UILayer[])Enum.GetValues(typeof(UILayer));
            foreach (UILayer layer in enumValues)
            {
                GameObject go = new GameObject($"{layer}");

                RectTransform rectTransform = go.AddComponent<RectTransform>();
                rectTransform.SetParent(xCanvas.transform);
                rectTransform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                rectTransform.localScale = Vector3.one;

                rectTransform.anchorMax = Vector2.one;
                rectTransform.anchorMin = Vector2.zero;

                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.sizeDelta = Vector2.zero;
                uiLayerDic.Add(layer, go.transform);
            }
        }

        public void SetActivateLayer(UILayer layer,bool res)
        {
            bool hasLayer = uiLayerDic.TryGetValue(layer, out Transform layerTran);
            if (hasLayer)
            {
                layerTran.SetActive(res);
            }
        }


        public void OpenView(string viewName, UILayer layer = UILayer.BaseLayer,Action finishAction = null,params object[] viewArgs )
        {
            bool hasView = viewDic.TryGetValue(viewName, out XModules.XBaseView xBaseView);
            if (!hasView)
            {
#if UNITY_EDITOR
                if (AssetManagement.AssetManager.Instance.AssetLoaderOptions == null)
                    AssetManagement.AssetManager.Instance.Initialize(new GameLoaderOptions());
#endif
                AssetManagement.AssetInternalLoader loader = AssetManagement.AssetUtility.LoadAsset<GameObject>($"{viewName}.prefab");
                loader.onComplete += (AssetManagement.AssetInternalLoader load) => {
                    if (string.IsNullOrEmpty(load.Error))
                    {
                        GameObject rawGo = load.GetRawObject<GameObject>();
                        GameObject viewGo = Instantiate<GameObject>(rawGo);
                        xBaseView = viewGo.GetComponent<XModules.XBaseView>();
                        _openView(viewName, xBaseView, layer, finishAction,viewArgs);
                    }
                    
                    loader = null;

                };
            }
            else
            {
                _openView(viewName, xBaseView, layer, finishAction, viewArgs);
            }
        }

        void _openView(string viewName, XModules.XBaseView xBaseView, UILayer layer, Action finishAction = null,params object[] viewArgs)
        {
            bool hasLayer = uiLayerDic.TryGetValue(layer, out Transform layerTran);
            if (hasLayer)
            {
                xBaseView.gameObject.name = viewName;


                xBaseView.finishAction = finishAction;
                xBaseView.viewArgs = viewArgs;
                xBaseView.OnEnableView();
                RectTransform rectTransform = xBaseView.GetComponent<RectTransform>();
                rectTransform.SetParent(layerTran);
                rectTransform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                rectTransform.localScale = Vector3.one;

                rectTransform.anchorMax = Vector2.one;
                rectTransform.anchorMin = Vector2.zero;

                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.sizeDelta = Vector2.zero;
                viewDic[viewName] = xBaseView;
            }
        }

        public void CloseView(string viewName)
        {
            bool hasView = viewDic.TryGetValue(viewName, out XModules.XBaseView xBaseView);
            if (hasView)
            {
                xBaseView.OnDisableView();
                xBaseView.SetActive(false);
            }
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}


