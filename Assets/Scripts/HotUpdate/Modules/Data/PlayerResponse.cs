using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XModules.Data
{
    [Serializable]
    public class PlayerResponse
    {
        public string code;
        public string msg;
        public PlayerData data;
    }

    [Serializable]
    public class PlayerData
    {
        public string id;
        public string uuid;
        public string loginType;
        public string email;
        public string username;
        public string nickname;
        public string avatar;
        public string gender;
        public string createTime;
        public string updateTime;
        public string token;
    }
}


