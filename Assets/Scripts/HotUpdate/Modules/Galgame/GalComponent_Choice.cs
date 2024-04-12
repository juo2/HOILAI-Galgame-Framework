using UnityEngine;
using UnityEngine.UI;
using XGUI;
using XModules.Data;

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

        bool isMessage = false;
        
        /// <summary>
        /// 这个选项要跳转到的ID
        /// </summary>
        public int JumpID;
        /// <summary>
        /// 显示的文本
        /// </summary>
        public Text _Title;
        public void Init (int _JumpID, string Title,bool _isMessage = false)
        {
            this.JumpID = _JumpID;
            _Title.text = Title;
            isMessage = _isMessage;
        }


        /// <summary>
        /// 当玩家按下了选项
        /// </summary>
        public void Button_Click_JumpTo ()
        {
            ConversationData.JumpNext(JumpID,_Title.text);

            if (isMessage)
            {
                ConversationData.AddHistoryContent(ConversationData.SelfCharacterInfo.characterID, ConversationData.SelfCharacterInfo.name,"", _Title.text);
            }

            XEvent.EventDispatcher.DispatchEvent("CHOICE_COMPLETE");
            XEvent.EventDispatcher.DispatchEvent("NEXT_STEP");
            return;
        }
    }
}