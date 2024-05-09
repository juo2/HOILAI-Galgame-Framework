using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XGUI
{
    public class XLayoutItem : MonoBehaviour
    {
        public float height;
        public float width;
        public int index = 0;
        RectTransform m_rectTransform;

        public RectTransform rectTransform
        {
            get
            { 
                if(m_rectTransform == null)
                {
                    m_rectTransform = GetComponent<RectTransform>();
                }

                return m_rectTransform;
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