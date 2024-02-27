using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XModules.Data
{
    [Serializable]
    public class ChatResponse
    {
        public string code; // 返回码
        public string msg; // 返回信息
        public List<ChatData> data; // The array of objects

        
    }

    [Serializable]
    public class ChatData
    {
        public string id; // 聊天记录id
        public string userId; // 用户id
        public string npcId; // npcid
        public string role; // 角色
        public string content; // 回复内容
        public string createTime; // 创建时间
    }

    

}


