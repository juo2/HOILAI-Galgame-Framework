using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class SelectedObjectHelper : MonoBehaviour
{
    private GUIStyle m_ButtonGUIStyle;
    List<RaycastResult> m_RaycastResult = new List<RaycastResult>();
    void Update()
    {

        if (Input.touchCount > 0 || (Input.GetMouseButtonDown(1) && Input.GetKey(KeyCode.LeftControl)))
        {
            PointerEventData data = new PointerEventData(EventSystem.current);
            data.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            EventSystem.current.RaycastAll(data, m_RaycastResult);
            if (m_RaycastResult.Count > 0)
            {
#if UNITY_EDITOR
                UnityEditor.EditorGUIUtility.PingObject(m_RaycastResult[0].gameObject);
#endif
            }
        }
    }


    void OnGUI()
    {
        //if (m_ButtonGUIStyle == null)
        //{
        //    m_ButtonGUIStyle = new GUIStyle(GUI.skin.button);
        //    m_ButtonGUIStyle.alignment = TextAnchor.MiddleLeft;
        //    m_ButtonGUIStyle.fontSize = 23;
        //}

        //if (Input.touchCount > 0 || Input.GetKey(KeyCode.LeftControl))
        //{

        //    Color contentColor = GUI.contentColor;
        //    GUI.contentColor = Color.red;
        //    GUILayout.BeginArea(new Rect(Screen.width * 0.5f, 0, Screen.width, Screen.height));
        //    if (m_RaycastResult.Count > 0)
        //    {
        //        for (int i = 0; i < m_RaycastResult.Count; i++)
        //        {
        //            GUILayout.Button(m_RaycastResult[i].ToString(), m_ButtonGUIStyle);
        //        }
        //    }
        //    GUILayout.EndArea();
        //    GUI.contentColor = contentColor;
        //}
       
    }
}
