using UnityEngine;
using UnityEditor;

public class ColorExamine : EditorWindow
{
    private Color m_Color = Color.white;
    private uint m_ColorIntVlaue;
    private string m_ColorHtml;
    private string m_Color255Str;
    private string m_Color01Str;

    void OnEnable()
    {
        Refesh();
    }


    void OnGUI()
    {

        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical("box");

        EditorGUI.BeginChangeCheck();
        {
            m_Color = EditorGUILayout.ColorField(m_Color);
            if (EditorGUI.EndChangeCheck())
            {
                Refesh();
            }
        }

        EditorGUI.BeginChangeCheck();
        {
            m_ColorHtml = EditorGUILayout.DelayedTextField(m_ColorHtml);
            if (EditorGUI.EndChangeCheck())
            {
                if (ColorUtility.TryParseHtmlString("#" + m_ColorHtml, out m_Color))
                {
                    Refesh();
                }
            }
        }


        EditorGUI.BeginChangeCheck();
        {
            m_Color255Str = EditorGUILayout.DelayedTextField(m_Color255Str);
            if (EditorGUI.EndChangeCheck())
            {
                string[] arr = m_Color255Str.Split(',');
                if (arr.Length == 4)
                {
                    for (int i = 0; i < arr.Length; i++)
                    {
                        int ivalue;
                        if (int.TryParse(arr[i], out ivalue))
                        {
                            m_Color[i] = ivalue / 0xff;
                        }
                    }

                    Refesh();
                }
            }

        }



        EditorGUI.BeginChangeCheck();
        {
            m_Color01Str = EditorGUILayout.DelayedTextField(m_Color01Str);
            if (EditorGUI.EndChangeCheck())
            {
                string[] arr = m_Color01Str.Split(',');
                if (arr.Length == 4)
                {
                    for (int i = 0; i < arr.Length; i++)
                    {
                        float fvalue;
                        if (float.TryParse(arr[i], out fvalue))
                        {
                            m_Color[i] = fvalue;
                        }
                    }

                    Refesh();
                }
            }
        }

        EditorGUI.BeginChangeCheck();
        {
            string str = EditorGUILayout.DelayedTextField(m_ColorIntVlaue.ToString());
            if (EditorGUI.EndChangeCheck())
            {
                uint uivalue;
                if (uint.TryParse(str, out uivalue))
                {
                    m_ColorIntVlaue = uivalue;
                    uint a = (m_ColorIntVlaue & 0xff000000) >> 24;
                    uint r = (m_ColorIntVlaue & 0xff0000) >> 16;
                    uint g = (m_ColorIntVlaue & 0xff00) >> 8;
                    uint b = (m_ColorIntVlaue & 0xff);
                    m_Color.a = (float)a / 0xff;
                    m_Color.r = (float)r / 0xff;
                    m_Color.g = (float)g / 0xff;
                    m_Color.b = (float)b / 0xff;
                    Refesh();
                }
            }
        }

        EditorGUILayout.EndVertical();
    }



    void Refesh()
    {
        m_ColorHtml = ColorUtility.ToHtmlStringRGBA(m_Color);
        m_Color255Str = string.Format("{0},{1},{2},{3}", (int)(m_Color.r * 0xff), (int)(m_Color.g * 0xff), (int)(m_Color.b * 0xff), (int)(m_Color.a * 0xff));
        m_Color01Str = string.Format("{0:F},{1:F},{2:F},{3:F}", m_Color.r, m_Color.g, m_Color.b, m_Color.a);
        m_ColorIntVlaue = (uint)(m_Color.a * 0xff) << 24 | (uint)(m_Color.r * 0xff) << 16 | (uint)(m_Color.g * 0xff) << 8 | (uint)(m_Color.b * 0xff);
    }



    [MenuItem("XGame/Examine/ColorExamine")]
    static void Open()
    {
        ColorExamine se = EditorWindow.GetWindow<ColorExamine>();
        se.maxSize = new Vector2(376, 106);
        se.Show();
    }
}