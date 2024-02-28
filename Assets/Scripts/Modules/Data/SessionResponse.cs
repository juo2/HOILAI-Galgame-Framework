using System;
using System.Collections.Generic;


namespace XModules.Data
{

    [Serializable]
    public class SessionResponse
    {
        public string code; // 错误码
        public string msg; // 错误信息
        public List<SessionData> data; // 数据数组
    }

    [Serializable]
    public class SessionData
    {
        public string id; // 会话id，可选
        public string userId; // 用户id，可选
        public string npcId; // NPCID，可选
        public string createTime; // 创建时间，可选
        public string updateTime; // 更新时间，可选
    }
}
