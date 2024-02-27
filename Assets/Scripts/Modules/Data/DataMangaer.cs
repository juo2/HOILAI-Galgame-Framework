using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XModules.Data
{
    public static class DataManager
    {
        public static ChatResponse chatResponse = null;
        public static NPCResponse npcResponse = null;
        public static PlayerResponse playerResponse = null;

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
    }
}


