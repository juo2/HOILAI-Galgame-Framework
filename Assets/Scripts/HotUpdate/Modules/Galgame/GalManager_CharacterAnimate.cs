
using Common.Game;
using DG.Tweening;
//using TetraCreations.Attributes;
using UnityCustom;
using UnityEngine;
using UnityEngine.UI;

namespace XModules.GalManager
{
    public class GalManager_CharacterAnimate : MonoBehaviour
    {
        /// <summary>
        /// 出入场出场动画
        /// </summary>
        [StringInList("ToShow", "Outside_ToLeft", "Outside_ToRight")] public string Animate_StartOrOutside = "Outside_ToRight";
        /// <summary>
        /// 动画
        /// <para>Shake：颤抖</para>
        /// <para>Shake-Y-Once：向下抖动一次</para>
        /// <para>ToGrey：变灰</para>
        /// <para>To - ：不解释了，移动到指定位置</para>
        /// </summary>
        [StringInList("Shake", "Shake_Y_Once", "ToLeft", "ToCenter", "ToRight")] public string Animate_type = "Shake";
        /// <summary>
        /// 角色立绘
        /// </summary>
        private Image CharacterImg;
        //[Title("注意，主画布的名称必须是MainCanvas")]
        public Canvas MainCanvas;

        private float m_SpriteWidth = 800;

        [SerializeField]
        Transform rightTran;

        [SerializeField]
        Transform leftTran;

        private void Awake ()
        {
            CharacterImg = this.gameObject.GetComponent<Image>();
            if (MainCanvas == null) MainCanvas = GameObject.Find("MainCanvas").GetComponent<Canvas>();
        }
        //[Button(nameof(Start), "重新执行入场动画")]
        private void Start ()
        {
            //HandleInOrOutsideMessgae("ToShow");
            HandleMessgae("Shake_Y_Once");
        }

        public void HandleMessgaeTemp(bool isSelf)
        {
            if (isSelf)
            {
                CharacterImg.rectTransform.localPosition = rightTran.localPosition;
            }
            else
            {
                CharacterImg.rectTransform.localPosition = leftTran.localPosition;
            }
        }

        //[Button(nameof(Start), "重新执行及时动画")]
        public void HandleMessgae (string tmp)
        {
            var _rect = CharacterImg.GetComponent<RectTransform>();
            switch (tmp)
            {
                case "Shake":
                {
                    _rect.DOShakePosition(0.5f, 30f);
                    break;
                }
                case "Shake_Y_Once":
                {
                    _rect.DOAnchorPosY(_rect.anchoredPosition.y - 50f, 0.6f).OnComplete(() =>
                    {
                        _rect.DOAnchorPosY(_rect.anchoredPosition.y + 50f, 0.6f);
                    });
                    break;
                }
            }
        }
        /// <summary>
        /// 处理出场动画消息
        /// </summary>
        public void HandleInOrOutsideMessgae (string tmp)
        {
            CharacterImg.color = new Color32(255, 255, 255, 0);//完全透明
            var rect = this.gameObject.GetComponent<RectTransform>();

            Vector3 orginPos = rect.localPosition;

            switch (tmp)
            {
                //逐渐显示
                case "ToShow":
                    {
                        break;
                    }
                //从屏幕边缘滑到左侧
                case "Outside_ToLeft":
                    {
                        rect.anchoredPosition = new Vector2(-300, rect.anchoredPosition.y);
                        rect.DOAnchorPos(orginPos, 1.0f);
                        break;
                    }
                //从屏幕边缘滑到右侧
                case "Outside_ToRight":
                    {
                        rect.anchoredPosition = new Vector2(300, rect.anchoredPosition.y);
                        rect.DOAnchorPos(orginPos, 1.0f);
                        break;
                    }
            }
            //都需要指定的
            {
                CharacterImg.DOFade(1, 0.7f);
            }
        }
        
    }

}
