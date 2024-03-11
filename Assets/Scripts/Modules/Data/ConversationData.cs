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

        public static Struct_PlotData.Struct_CharacterInfo AddCharacter()
        {
            var characterInfo = new Struct_PlotData.Struct_CharacterInfo();
            var _CharacterId = PlotData.NowPlotDataNode.Attribute("CharacterID").Value;
            characterInfo.name = PlotData.NowPlotDataNode.Attribute("CharacterName").Value;
            characterInfo.image = PlotData.NowPlotDataNode.Attribute("CharacterImage").Value;
            characterInfo.characterID = _CharacterId;
            characterInfo.isSelf = PlotData.NowPlotDataNode.Attribute("IsSelf").Value == "True";

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

        public static Struct_PlotData.Struct_CharacterInfo GetCharacterObjectByName(string ID)
        {
            return PlotData.CharacterInfoList.Find(t => t.characterID == ID);
        }

        /// <summary>
        /// 销毁一个角色
        /// </summary>
        /// <param name="ID"></param>
        public static void DestroyCharacterByID(string ID)
        {
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
    }
}