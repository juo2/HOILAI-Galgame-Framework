using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XModules.Data
{
    public static class DataManager
    {
        public static Dictionary<string,ChatResponse> chatResponseDic = new Dictionary<string, ChatResponse>();

        public static NPCResponse npcResponse = null;
        public static PlayerResponse playerResponse = null;
        public static SessionResponse sessionResponse = null;
        public static OneShotChatResponse oneShotChatResponse = null;

        public static string getPlayerId()
        {
            //临时把token记住
            string id = PlayerPrefs.GetString("TEMP_ID");
            if (string.IsNullOrEmpty(id))
            {
                return playerResponse.data.id;
            }

            return id;
        }

        public static string getToken()
        {
            //临时把token记住
            string token = PlayerPrefs.GetString("TEMP_TOKEN");

            if (string.IsNullOrEmpty(token))
            {
                return playerResponse.data.token;
            }

            return token;
        }

        public static ChatData createChatData(string npcId,string role,string content)
        {
            ChatData chatdata = new ChatData();
            chatdata.userId = getPlayerId();
            chatdata.content = content;
            chatdata.npcId = npcId;
            chatdata.role = role;

            if (chatResponseDic.TryGetValue(npcId, out ChatResponse chatResponse))
            {
                chatResponse.data.Add(chatdata);
            }

            return chatdata;
        }

        public static List<NPCData> getNpcList()
        {
            if (npcResponse == null)
            {
                return new List<NPCData>();
            }
            else
            {
                return npcResponse.data;
            }
        }

        public static NPCData getNpcById(string npcId)
        {
            foreach(var npc in getNpcList())
            {
                if (npcId == npc.id)
                {
                    return npc;
                }
            }

            return null;
        }

        public static List<SessionData> getSessionList()
        {
            return sessionResponse.data;
        }

        public static void addChatResponse(string npcId, ChatResponse chatResponse)
        {
            if (!chatResponseDic.ContainsKey(npcId))
            {
                chatResponseDic[npcId] = chatResponse;
            }
        }

        public static bool IsHasChatResponse(string npcId)
        {
            return chatResponseDic.ContainsKey(npcId);
        }

        public static List<ChatData> getChatDatabyNpcId(string npcId)
        {
            if(chatResponseDic.TryGetValue(npcId,out ChatResponse chatResponse))
            {
                return chatResponse.data;
            }

            return null;
        }

        public static string getNpcResponse()
        {
            if (oneShotChatResponse == null)
            {
                return "请求失败";
            }
            else
            {
                return oneShotChatResponse.data.npcResponse;
            }
        }

        public static int getOneShotChatSelect()
        {
            if (oneShotChatResponse == null)
            {
                return 1;
            }
            else
            {
                return oneShotChatResponse.data.select;
            }
        }

    }
}


