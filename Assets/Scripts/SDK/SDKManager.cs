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
                using (AndroidJavaClass testClass = new AndroidJavaClass("com.unity3d.player.Login"))
                {
                    testClass.CallStatic("login");
                }
            }
        }

        // ��Android���õķ���
        public void LoginRequest(string message)
        {
            Debug.Log("LoginRequest Received message from Android: " + message);
            // �����ﴦ����Ϣ���������UI��
        }


        public void Photo()
        {
            Debug.Log("SDK Photo");

            // ��鵱ǰƽ̨�Ƿ�Ϊ Android
            if (Application.platform == RuntimePlatform.Android)
            {

            }
        }

        public void PhotoRequest(string message)
        {
            Debug.Log("PhotoRequest Received message from Android: " + message);
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


