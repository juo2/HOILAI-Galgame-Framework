using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XModules.Data
{
    [Serializable]
    public class OneShotChatResponse
    {
        public string code;
        public string msg;
        public OneShotChatData data;
    }

    [Serializable]
    public class OneShotChatData
    {
        public int select;
        public string npcResponse;
    }
}


