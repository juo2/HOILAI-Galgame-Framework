using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDK
{
    public class SDKManager : MonoBehaviour
    {

        static SDKManager m_Instance;
        public static SDKManager Instance
        {
            get
            {
                if (m_Instance != null)
                    return m_Instance;
                GameObject go = new GameObject("SDKManager");
                //go.hideFlags = HideFlags.HideInHierarchy;
                m_Instance = go.AddComponent<SDKManager>();

                UnityEngine.Object.DontDestroyOnLoad(go);
                return m_Instance;
            }
        }

        public void Login()
        {

            Debug.Log("SDK Login");

            // 检查当前平台是否为 Android
            if (Application.platform == RuntimePlatform.Android)
            {
                // 使用AndroidJavaClass访问UnityPlayer类
                AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                // 获取当前的Activity
                AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                // 使用AndroidJavaObject调用Android方法
                AndroidJavaObject androidInterface = new AndroidJavaObject("com.example.yourpackage.YourAndroidClass");
                androidInterface.Call("yourMethodName", currentActivity);
            }
        }

        // 被Android调用的方法
        public void LoginRequest(string message)
        {
            Debug.Log("Received message from Android: " + message);
            // 在这里处理消息，比如更新UI等
        }



        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}


