using UnityEngine;
using UnityEngine.UI;
using XGUI;

namespace XModules.GalManager
{

    public class GalManager_CharacterImg : MonoBehaviour
    {
        private XImage CharacterImg;
        private void Awake ()
        {
            CharacterImg = this.gameObject.GetComponent<XImage>();

        }

        /// <summary>
        /// 换图片
        /// </summary>
        public void SetImage(string imageName)
        {
            CharacterImg.spriteAssetName = imageName;
        }
    }
}