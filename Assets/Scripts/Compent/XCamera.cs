using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XGUI
{
    public class XCamera : MonoBehaviour
    {
        public static Camera guiCamera;

        public static void Init()
        {
            GameObject cameraGo = new GameObject();
            cameraGo.name = "Main Camera";
            guiCamera = cameraGo.AddComponent<Camera>();
            //cameraGo.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();

            guiCamera.orthographic = false;
            guiCamera.fieldOfView = 60;
            guiCamera.nearClipPlane = 0.3f;
            guiCamera.farClipPlane = 1000f;

            guiCamera.transform.position = new Vector3(0, 1, -10);
            guiCamera.AddComponent<AudioListener>();

            DontDestroyOnLoad(cameraGo);
        }

    }
}


