using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XGUI
{
    public class XLayer : MonoBehaviour
    {

        public Vector2 offsetMax = Vector2.zero;

        public Vector2 offsetMin = Vector2.zero;

        RectTransform rectTransform = null;

        // Start is called before the first frame update
        void Start()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        // Update is called once per frame
        void Update()
        {
            if (rectTransform != null)
            {
                rectTransform.offsetMax = offsetMax;
                rectTransform.offsetMin = offsetMin;

            }    

        }
    }
}
