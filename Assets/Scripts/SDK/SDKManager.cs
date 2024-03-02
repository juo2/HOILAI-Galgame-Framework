using System.Collections;
using System.Collections.Generic;

#if UNITY_IOS
using System.Runtime.InteropServices;
#endif

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

#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern void _Login_Internal();

        [DllImport("__Internal")]
        private static extern void _Photo_Internal();
#endif


        public void Login()
        {
            Debug.Log("SDK Login");
#if UNITY_ANDROID
            using (AndroidJavaClass testClass = new AndroidJavaClass("com.unity3d.player.UnityAndroidBridge"))
            {
                testClass.CallStatic("login");
            }
#elif UNITY_IOS
            _Login_Internal();
#endif
        }

        public void Photo()
        {
            Debug.Log("SDK Photo");
#if UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                using (AndroidJavaClass testClass = new AndroidJavaClass("com.unity3d.player.UnityAndroidBridge"))
                {
                    testClass.CallStatic("getPhoto");
                }
            }
#elif UNITY_IOS
            _Photo_Internal();
#endif
        }


        public void LoginRequest(string message)
        {
            Debug.Log("LoginRequest Received message from Android: " + message);
        }

        public void PhotoRequest(string message)
        {
            Debug.Log("PhotoRequest Received message from Android: " + message);
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


