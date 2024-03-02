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

            // ��鵱ǰƽ̨�Ƿ�Ϊ Android
            if (Application.platform == RuntimePlatform.Android)
            {
                // ʹ��AndroidJavaClass����UnityPlayer��
                AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                // ��ȡ��ǰ��Activity
                AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                // ʹ��AndroidJavaObject����Android����
                AndroidJavaObject androidInterface = new AndroidJavaObject("com.example.yourpackage.YourAndroidClass");
                androidInterface.Call("yourMethodName", currentActivity);
            }
        }

        // ��Android���õķ���
        public void LoginRequest(string message)
        {
            Debug.Log("Received message from Android: " + message);
            // �����ﴦ����Ϣ���������UI��
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


