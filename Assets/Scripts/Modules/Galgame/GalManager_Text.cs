using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XModules.Data;
using static XModules.Data.ConversationData;
    
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

        private string currentContent = ""; // 当前应该显示的内容
        private string additionalContent = ""; // 新附加的内容
        private float timer = 0f; // 计时器，用于控制字符的显示速度
        private bool isAnimating = false; // 是否正在播放动画

        private UnityAction finishFunc = null;

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

        public void StreamTextContent(string CharacterName,bool isNeedShowFinish = true)
        {
           
            Debug.Log($"Enter StreamTextContent ------------------------------------ ConversationData.IsSpeak:{ConversationData.IsSpeak}");

            if (ConversationData.IsSpeak)
            {
                return;
            }

            //先把数据清空
            ClearCacheOneChar();

            KillTween();
            SetText_Content(string.Empty);//先清空内容
            SetText_CharacterName(CharacterName);
            ConversationData.IsSpeak = true;

            StartCoroutine(StreamTextContentInternal(isNeedShowFinish));
        }


        IEnumerator StreamTextContentInternal(bool isNeedShowFinish)
        {
            bool isDone = false;
            bool isShowFinish = false;

            string targetChar = getCacheOneChar();

            Debug.Log($"targetChar33333333333333:{targetChar}");

            if(isNeedShowFinish)
            {
                if (currentWebSocketSteamContent.Contains("|"))
                {
                    isShowFinish = true;
                    targetChar = "";
                    Debug.Log("[isShowFinish][isShowFinish][isShowFinish][isShowFinish][isShowFinish][isShowFinish][isShowFinish][isShowFinish]");
                }
            }

            if (currentWebSocketSteamContent.Contains("[DONE]"))
            {
                isDone = true;
                targetChar = "";
                Debug.Log("[DONE][DONE][DONE][DONE][DONE][DONE][DONE][DONE]");
            }

            Text_TextContent.text = Text_TextContent.text + targetChar;

            if ( (isShowFinish || !isNeedShowFinish) && isDone)
            {
                yield return new WaitForSeconds(DefaultSpeed);
                ConversationData.IsSpeak = false;
                completeCacheOneChar();
                XEvent.EventDispatcher.DispatchEvent("STREAM_FINISH");
            }
            else
            {
                yield return new WaitForSeconds(DefaultSpeed);
                yield return StreamTextContentInternal(isNeedShowFinish);
            }
        }

        public void ForceTextContent(string TextContent, string CharacterName, UnityAction CallBack = null)
        {
            KillTween();
            ConversationData.IsSpeak = true;
            SetText_Content(string.Empty);//先清空内容
            SetText_CharacterName(CharacterName);
            DoText(TextContent);

            finishFunc = () => 
            {
                ConversationData.IsSpeak = false;
                CallBack?.Invoke();
            };
        }

        /// <summary>
        /// 开始发言
        /// </summary>
        /// <param name="TextContent">文本内容</param>
        /// <param name="CharacterName">发言人名称</param>
        /// <param name="CharacterIdentity">发言人所属</param>
        /// <param name="CallBack">回调事件</param>
        /// <returns></returns>
        public void StartTextContent (string TextContent, string CharacterName, UnityAction CallBack = null)
        {
            if (ConversationData.IsSpeak && Text_TextContent.text.Length >= TextContent.Length * 0.75f && ConversationData.IsCanJump)//当前还正在发言
            {
                //但是 ，如果当前到了总文本的三分之二，也可以下一句
                KillTween();
                SetText_CharacterName(CharacterName);
                return;
            }
            else if (ConversationData.IsSpeak)
            {
                return;
            }

            ConversationData.IsSpeak = true;
            SetText_Content(string.Empty);//先清空内容
            SetText_CharacterName(CharacterName);

            DoText(TextContent);
            finishFunc =() =>
            {
                ConversationData.IsSpeak = false;
                CallBack?.Invoke();
            };
        }

        public void KillTween()
        {
            isAnimating = false;
            additionalContent = ""; // 清空附加内容
            currentContent = "";
        }

        public void DoText(string content)
        {
            // 如果动画正在进行中，则新内容追加到目标内容之后
            if (isAnimating)
            {
                return;
            }
            else
            {
                additionalContent = content; // 设置新附加的内容
                currentContent = Text_TextContent.text; // 保留当前已显示的内容
                isAnimating = true; // 开始动画
            }

            timer = DefaultSpeed; // 重置计时器
        }

        private void Update()
        {
            if (!isAnimating || string.IsNullOrEmpty(additionalContent))
            {
                return;
            }

            // 更新计时器
            timer -= Time.deltaTime;

            if (timer <= 0f && currentContent.Length < Text_TextContent.text.Length + additionalContent.Length)
            {
                // 时间到了，显示下一个字符
                currentContent += additionalContent[0]; // 追加下一个字符到当前内容中
                additionalContent = additionalContent.Substring(1); // 更新附加内容
                Text_TextContent.text = currentContent; // 更新显示的文本
                timer = DefaultSpeed; // 重置计时器
            }

            if (string.IsNullOrEmpty(additionalContent))
            {
                // 所有附加字符都已显示，停止动画
                isAnimating = false;
                currentContent = "";

                finishFunc?.Invoke();
            }
        }

    }
}
