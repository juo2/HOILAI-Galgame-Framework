using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XModules.Data
{

    [Serializable]
    public class NPCResponse
    {
        public string code; // 返回码
        public string msg; // 返回信息
        public List<NPCData> data; // 数据
    }

    [Serializable]
    public class NPCData
    {
        public string id; // NPCid
        public string content; // 预设内容
        public string description; // 简单描述
        public string createTime; // 创建时间
    }
    
}


