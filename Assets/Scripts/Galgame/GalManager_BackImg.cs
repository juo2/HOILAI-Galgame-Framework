using UnityEngine;
using UnityEngine.UI;
using XGUI;

namespace ScenesScripts.GalPlot
{

    public class GalManager_BackImg : MonoBehaviour
    {
        private XImage BackImg;
        private void Awake()
        {
            BackImg = this.gameObject.GetComponent<XImage>();
        }

        /// <summary>
        /// 换图片
        /// </summary>
        public void SetImage (string imageName)
        {
            Debug.Log("GalManager_BackImg SetImage ImageName");
            BackImg.spriteAssetName = imageName;
        }
    }
}