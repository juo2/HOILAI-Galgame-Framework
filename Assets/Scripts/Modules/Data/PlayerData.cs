using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XModules.Data
{
    [Serializable]
    public class PlayerData
    {
        [Serializable]
        public class Data
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
            public string tokenExpire;
        }

        public string code;
        public string msg;
        public Data data;
    }
}


