using AssetManagement;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Common.Game
{
    /// <summary>
    /// 游戏内通用API
    /// </summary>
    public static class GameAPI
    {
        public static void Print (object _Message, string _Type = "debug")
        {
            string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string res = $"[{currentTime}] {_Type} : {_Message}";
            switch (_Type)
            {
                case "debug":
                    Debug.Log(res);
                    return;
                case "warn":
                    Debug.LogWarning(res);
                    return;
                case "error":
                    Debug.LogError(res);
                    return;
                default:
                    Debug.Log(res);
                    break;
            }
        }
    }
}
