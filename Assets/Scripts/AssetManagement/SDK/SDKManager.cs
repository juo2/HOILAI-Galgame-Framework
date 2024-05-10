using System.Collections;
using System.Collections.Generic;

#if UNITY_IOS
using System.Runtime.InteropServices;
#endif

using UnityEngine;
using UnityEngine.Networking;

namespace SDK
{
    public class PhotoData
    {
        public string path;
        public string exData;
    }

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


        public void Login(string param = "test")
        {
            Debug.Log("SDK Login");
#if UNITY_ANDROID
            using (AndroidJavaClass testClass = new AndroidJavaClass("com.unity3d.player.UnityAndroidBridge"))
            {
                testClass.CallStatic("login", param);
            }
#elif UNITY_IOS
            _Login_Internal();
#endif
        }

        public void Photo(string param = "test")
        {
            Debug.Log("SDK Photo");
#if UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                using (AndroidJavaClass testClass = new AndroidJavaClass("com.unity3d.player.UnityAndroidBridge"))
                {
                    testClass.CallStatic("getPhoto", param);
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

        
        public void PhotoRequest(string json)
        {
            PhotoData photoData = JsonUtility.FromJson<PhotoData>(json);

            Debug.Log("PhotoRequest Received message from Android photoData.path: " + photoData.path + "   photoData.exData:" + photoData.exData);

            XEvent.EventDispatcher.DispatchEvent("LOAD_IMAGE", json);
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


