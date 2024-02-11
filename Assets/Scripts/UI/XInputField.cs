using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace XGUI
{
    public class XInputField : InputField
    {
        public UnityAction OnPointerClickCallBack;

        void ClearEvent()
        {
            if (onValueChanged != null)
            {
                onValueChanged.RemoveAllListeners();
                onValueChanged = null;
            }


            if (onEndEdit != null)
            {
                onEndEdit.RemoveAllListeners();
                onEndEdit = null;
            }

            if (OnPointerClickCallBack != null)
            {
                OnPointerClickCallBack = null;
            }
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            if (OnPointerClickCallBack != null)
            {
                OnPointerClickCallBack.Invoke();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            this.ClearEvent();
        }

    }
}


