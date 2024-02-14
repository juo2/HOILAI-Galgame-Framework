using UnityEngine;
using UnityEngine.UI;
using XGUI;

namespace XModules.GalManager
{ 
    /// <summary>
    /// 选项类
    /// </summary>
    public class GalComponent_Choice : MonoBehaviour
    {
        private XButton xButton;

        private void Awake()
        {
            xButton = GetComponent<XButton>();
            xButton.onClick.AddListener(Button_Click_JumpTo);
        }

        /// <summary>
        /// 这个选项要跳转到的ID
        /// </summary>
        public string _JumpID;

        /// <summary>
        /// 显示的文本
        /// </summary>
        public Text _Title;
        public void Init (string JumpID, string Title)
        {
            _JumpID = JumpID;
            _Title.text = Title;
        }
        /// <summary>
        /// 当玩家按下了选项
        /// </summary>
        public void Button_Click_JumpTo ()
        {

            ConversationView.PlotData.NowJumpID = _JumpID;
            ConversationView.PlotData.IsBranch = true;
            GalManager_Text.IsCanJump = true;
            if (_JumpID == "-1")
            {
                return;
            }
            //this.gameObject.transform.parent.GetComponent<GalManager_Choice>().Button_Click_Choice();
            //GameObject.Find("EventSystem").GetComponent<GalManager>().Button_Click_NextPlot();


            XEvent.EventDispatcher.DispatchEvent("NEXT_STEP");
            XEvent.EventDispatcher.DispatchEvent("CHOICE_COMPLETE");

            return;
        }
    }
}