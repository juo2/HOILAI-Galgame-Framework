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
                using (AndroidJavaClass testClass = new AndroidJavaClass("com.unity3d.player.Login"))
                {
                    testClass.CallStatic("login");
                }
            }
        }

        // 被Android调用的方法
        public void LoginRequest(string message)
        {
            Debug.Log("LoginRequest Received message from Android: " + message);
            // 在这里处理消息，比如更新UI等
        }


        public void Photo()
        {
            Debug.Log("SDK Photo");

            // 检查当前平台是否为 Android
            if (Application.platform == RuntimePlatform.Android)
            {

            }
        }

        public void PhotoRequest(string message)
        {
            Debug.Log("PhotoRequest Received message from Android: " + message);
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


