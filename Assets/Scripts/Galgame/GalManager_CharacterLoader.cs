using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XGUI;
using static XGUI.XLoader;

namespace ScenesScripts.GalPlot
{

    public class GalManager_CharacterLoader : MonoBehaviour
    {

        private string m_ImageName = null;

        private string m_StartOrOutside = null;

        private string m_MessageContent = null;

        private XLoader m_Loader;

        private GalManager_CharacterImg m_characterImg = null;

        private GalManager_CharacterAnimate m_characterAnimate = null;

        // Start is called before the first frame update
        void Awake()
        {
            m_Loader = GetComponent<XLoader>();
            m_Loader.onCreateRenderer.AddListener(onCreateRenderer);
            m_Loader.StartLoad();
        }

        void onCreateRenderer(LoaderItemRenderer loaderItem)
        {
            m_characterImg = loaderItem.gameObject.GetComponent<GalManager_CharacterImg>();
            m_characterAnimate = loaderItem.gameObject.GetComponent<GalManager_CharacterAnimate>();

            //Debug.Log("GalManager_CharacterLoader onCreateRenderer");

            if (!string.IsNullOrEmpty(m_ImageName))
                m_characterImg.SetImage(m_ImageName);

            if (!string.IsNullOrEmpty(m_StartOrOutside))
                m_characterAnimate.Animate_StartOrOutside = m_StartOrOutside;

            if (!string.IsNullOrEmpty(m_MessageContent))
            {
                m_characterAnimate.Animate_type = m_MessageContent;
                m_characterAnimate.HandleMessgae();
            }
        }

        /// <summary>
        /// »»Í¼Æ¬
        /// </summary>
        public void SetImage(string imageName)
        {
            m_ImageName = imageName;

            if (m_characterImg != null)
            {
                //Debug.Log("GalManager_BackImg SetImage ImageName");
                m_characterImg.SetImage(imageName);
            }
        }

        public void Set_Animate_StartOrOutside(string startOrOutside)
        {
            m_StartOrOutside = startOrOutside;

            if (m_characterAnimate != null)
            {
                m_characterAnimate.Animate_StartOrOutside = startOrOutside;
            }
        }

        public void HandleMessage(string MessageContent)
        {
            m_MessageContent = MessageContent;

            if (m_characterAnimate != null)
            {
                m_characterAnimate.Animate_type = MessageContent;
                m_characterAnimate.HandleMessgae();
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}
