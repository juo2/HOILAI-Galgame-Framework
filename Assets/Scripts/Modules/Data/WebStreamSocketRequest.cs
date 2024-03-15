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
    }
}