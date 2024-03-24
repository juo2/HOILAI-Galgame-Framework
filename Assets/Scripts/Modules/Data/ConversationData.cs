using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

namespace XModules.Data
{
    public static class ConversationData
    {
        /// <summary>
        /// 存储整个剧本的XML文档
        /// </summary>

        [Serializable]
        public class Struct_PlotData
        {
            public List<XElement> ListMainPlot = new();
            public class Struct_Choice
            {
                public string Title;
                public int JumpID;
            }
            public class Struct_CharacterInfo
            {
                public string characterID;
                public string name;
                public string image;
                public bool isSelf = false;
            }
            public List<Struct_CharacterInfo> CharacterInfoList = new();
            public List<Struct_Choice> ChoiceTextList = new();
            /// <summary>
            /// 当前的剧情节点
            /// </summary>
            public XElement NowPlotDataNode;

            /// <summary>
            /// 当前是否为分支剧情节点
            /// </summary>
            public int NextJumpID;

            public class HistoryContent
            {
                public string id;
                public string speaker;
                public string content;
                public string optContent;
            }
            public List<HistoryContent> historyContentList = new List<HistoryContent>();
        }

        public static Struct_PlotData PlotData = new();

        public static Struct_PlotData.Struct_CharacterInfo SelfCharacterInfo = null;

        public static Struct_PlotData.Struct_CharacterInfo TempNpcCharacterInfo = null;

        public static List<string> cacheOutMessageList = new List<string>();
        public static int cacheIndex = 0;
        public static int currentCacheIndex = 0;

        public static string tempInputMessage = null;
        public static string webSocketSteamContent = "";
        public static string currentWebSocketSteamContent = "";

        public static string currentStory = null;


        public static Struct_PlotData.Struct_CharacterInfo AddCharacter()
        {
            var characterInfo = new Struct_PlotData.Struct_CharacterInfo();
            var _CharacterId = PlotData.NowPlotDataNode.Attribute("CharacterID").Value;
            characterInfo.name = PlotData.NowPlotDataNode.Attribute("CharacterName").Value;
            characterInfo.image = PlotData.NowPlotDataNode.Attribute("CharacterImage").Value;
            characterInfo.characterID = $"{currentStory}_{ _CharacterId}";
            characterInfo.isSelf = PlotData.NowPlotDataNode.Attribute("IsSelf").Value == "True";

            if (DataManager.getNpcById(characterInfo.characterID) == null)
            {
                Debug.LogError($"前后端数据不对应 characterInfo.characterID:{characterInfo.characterID}");
            }

            if (characterInfo.isSelf)
            {
                SelfCharacterInfo = characterInfo;
            }
            else
            {
                TempNpcCharacterInfo = characterInfo;
            }

            return characterInfo;
        }

        public static Struct_PlotData.Struct_CharacterInfo GetCharacterObjectByName(string _ID)
        {
            string ID = $"{currentStory}_{ _ID}";
            return PlotData.CharacterInfoList.Find(t => t.characterID == ID);
        }

        /// <summary>
        /// 销毁一个角色
        /// </summary>
        /// <param name="ID"></param>
        public static void DestroyCharacterByID(string _ID)
        {
            string ID = $"{currentStory}_{ _ID}";

            var _ = PlotData.CharacterInfoList.Find(t => t.characterID == ID);
            //SendCharMessage(ID, "Quit");
            PlotData.CharacterInfoList.Remove(_);
        }

        /// <summary>
        /// 重置
        /// </summary>
        public static void ResetPlotData()
        {
            PlotData = new Struct_PlotData();
            IsCanJump = true;
            IsSpeak = false;
            SelfCharacterInfo = null;
            TempNpcCharacterInfo = null;
        }

        /// <summary>
        ///是否可以跳过 
        /// </summary>
        public static bool IsCanJump = true;

        /// <summary>
        /// 当前是否正在发言
        /// 如果为假则可以开始下一句
        /// 当这个文本快结束的时候也为True
        /// </summary>
        public static bool IsSpeak;

        public static void JumpNext(int jumpID,string title)
        {
            PlotData.NextJumpID = jumpID;
            IsCanJump = true;
            if (jumpID == -1)
            {
                return;
            }
            PlotData.NextJumpID = jumpID;

            int count = PlotData.historyContentList.Count;
            if (count > 0)
            {
                var historyContent = PlotData.historyContentList[count - 1];
                historyContent.optContent = title;
            }
        }

        public static void AddHistoryContent(string id, string speaker, string content = "",string optContent = "")
        {
            Struct_PlotData.HistoryContent historyContent = new Struct_PlotData.HistoryContent();
            historyContent.id = id;
            historyContent.speaker = speaker;
            historyContent.content = content;
            historyContent.optContent = optContent;

            PlotData.historyContentList.Add(historyContent);
        }

        public static void ClearHistoryContent()
        {
            PlotData.historyContentList.Clear();
        }

        public static List<Struct_PlotData.HistoryContent> GetHistoryContentList()
        {
            return PlotData.historyContentList;
        }

        public static void ClearCacheOneChar()
        {
            webSocketSteamContent = "";
            currentWebSocketSteamContent = "";
            cacheIndex = 0;
            currentCacheIndex = 0;
            cacheOutMessageList.Clear();
        }

        public static string getCacheOneChar()
        {
            Debug.Log($"getCacheOneChar cacheOutMessageList.Count:{cacheOutMessageList.Count}");

            for (int i = cacheIndex; i < cacheOutMessageList.Count; i++)
            {
                webSocketSteamContent += cacheOutMessageList[i];
            }

            cacheIndex = cacheOutMessageList.Count;

            Debug.Log($"getCacheOneChar webSocketSteamContent.Length:{webSocketSteamContent.Length}");

            if (currentCacheIndex < webSocketSteamContent.Length)
            {
                // 检测当前字符是否为 '['
                if (webSocketSteamContent[currentCacheIndex] == '[')
                {
                    // 检查剩余的字符串是否开始为 "[Done]"
                    string remainingContent = webSocketSteamContent.Substring(currentCacheIndex);
                    if (remainingContent.StartsWith("[DONE]"))
                    {
                        currentCacheIndex += "[DONE]".Length; // 更新索引跳过 "[Done]"
                        currentWebSocketSteamContent += "[DONE]"; // 将 "[Done]" 添加到当前WebSocket流内容
                        return "[DONE]"; // 直接返回 "[Done]"
                    }
                }

                // 继续原来的逻辑，返回下一个字符
                char targetOut = webSocketSteamContent[currentCacheIndex];
                currentCacheIndex++;
                Debug.Log($"getCacheOneChar currentCacheIndex:{currentCacheIndex}");
                currentWebSocketSteamContent += targetOut;
                Debug.Log($"getCacheOneChar currentWebSocketSteamContent:{currentWebSocketSteamContent}");
                return targetOut.ToString();
            }

            // 如果没有更多的字符可以返回，则可能需要处理这种情况
            return string.Empty; // 或者返回null或其他合适的值
        }

        public static void completeCacheOneChar()
        {
            //cacheOutMessageList.Clear();
        }

        public static int getOneShotChatSelect()
        {
            var strArray = webSocketSteamContent.Split("|");

            if (strArray.Length == 2)
            {
                return (int)Char.GetNumericValue(strArray[1][0]);;
            }
            else
            {
                Debug.LogError("webSocketSteamContent 没找到 分割号 |");
                return 0;
            }
        }
    }
}