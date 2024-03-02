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
        

        public static string getPlayerId()
        {
            return playerResponse.data.id;
        }

        public static string getToken()
        {
            return playerResponse.data.token;
        }

        public static ChatData createChatData(string npcId,string role,string content)
        {
            ChatData chatdate = new ChatData();
            chatdate.userId = getPlayerId();
            chatdate.content = content;
            chatdate.npcId = npcId;
            chatdate.role = role;

            return chatdate;
        }

        public static List<NPCData> getNpcList()
        {
            return npcResponse.data;
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

    }
}

