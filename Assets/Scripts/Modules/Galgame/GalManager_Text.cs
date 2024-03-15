using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XModules.Data;

namespace XModules.GalManager
{
    public class GalManager_Text : MonoBehaviour
    {
        public const float DefaultSpeed = 0.045f;
        public const float FastSpeed = 0.02f;
        /// <summary>
        /// 当前是否剧情加速
        /// </summary>
        public static bool IsFastMode;

        

        /// <summary>
        /// 文本内容打字机动画事件
        /// </summary>
        public static Tweener TextAnimateEvemt;

        /// <summary>
        /// 文本内容
        /// </summary>
        public Text Text_TextContent;

        /// <summary>
        /// 发言人
        /// </summary>
        public Text Text_CharacterName;

        /// <summary>
        /// 设置对话内容
        /// </summary>
        /// <param name="TextContent"></param>
        public void SetText_Content (string TextContent)
        {
            Text_TextContent.text = TextContent;
        }
        /// <summary>
        /// 设置发言人的名称
        /// </summary>
        public void SetText_CharacterName (string CharacterName)
        {

            Text_CharacterName.text = $"<b>{CharacterName}</b><size=45></size>";
        }


        public void ForceTextContent(string TextContent, string CharacterName, UnityAction CallBack = null)
        {
            TextAnimateEvemt.Kill();
            ConversationData.IsSpeak = true;
            SetText_Content(string.Empty);//先清空内容
            SetText_CharacterName(CharacterName);
            TextAnimateEvemt = Text_TextContent.DOText(TextContent, TextContent.Length * (IsFastMode ? FastSpeed : DefaultSpeed)).SetEase(Ease.Linear).OnComplete(() =>
            {
                ConversationData.IsSpeak = false;
                CallBack?.Invoke();
            });
        }

        /// <summary>
        /// 开始发言
        /// </summary>
        /// <param name="TextContent">文本内容</param>
        /// <param name="CharacterName">发言人名称</param>
        /// <param name="CharacterIdentity">发言人所属</param>
        /// <param name="CallBack">回调事件</param>
        /// <returns></returns>
        public Tweener StartTextContent (string TextContent, string CharacterName, UnityAction CallBack = null)
        {
            if (ConversationData.IsSpeak && Text_TextContent.text.Length >= TextContent.Length * 0.75f && ConversationData.IsCanJump)//当前还正在发言
            {
                //但是 ，如果当前到了总文本的三分之二，也可以下一句
                SetText_Content(TextContent);
                ConversationData.IsSpeak = false;
                TextAnimateEvemt.Kill();
                SetText_CharacterName(CharacterName);
                return TextAnimateEvemt;
            }
            else if (ConversationData.IsSpeak)
            {
                return TextAnimateEvemt;
            }

            ConversationData.IsSpeak = true;
            SetText_Content(string.Empty);//先清空内容
            SetText_CharacterName(CharacterName);
            TextAnimateEvemt = Text_TextContent.DOText(TextContent, TextContent.Length * (IsFastMode ? FastSpeed : DefaultSpeed)).SetEase(Ease.Linear).OnComplete(() =>
            {
                ConversationData.IsSpeak = false;
                CallBack?.Invoke();
            });
            return TextAnimateEvemt;
        }

    }
}
