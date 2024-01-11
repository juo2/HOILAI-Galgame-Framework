using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using XAudio;

namespace XGUI
{
    public class XButtonMusic : MonoBehaviour, IPointerClickHandler
    {
        public string assetName = "panelOpen.wav";
        public void OnPointerClick(PointerEventData eventData)
        {
            XAudioManager.instance.PlayUIMusic(assetName);
        }
    }
}

