using System;
using UnityEngine;

namespace XModules.Data
{
    [Serializable]
    public class WebStreamSocketRequest
    {
        public string userId;
        public string npcId;
        public string textContent;
        public string question;
        public string options;
        public string storyId;
    }

    [Serializable]
    public class WebStreamSocketLoopRequest
    {
        public string userId;
        public string npcId;
        public string question;
        public string storyId;
    }
}