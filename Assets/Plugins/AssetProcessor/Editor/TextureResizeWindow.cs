using UnityEngine;
using UnityEditor;
using System.IO;

public class TextureResizeWindow : EditorWindow
{

    enum ResizeType
    {
        WidthHeight,
        Ratio,
    }
    private Texture2D[] m_Textures;
    private Vector2 m_ScrollPos;
    private int m_TargetWidth;
    private int m_TargetHeigh;
    private ResizeType m_ResizeType;
    private float m_RatioValue = 0.5f;
    void OnEnable()
    {
        RefreshData();
    }

    void OnSelectionChange()
    {
        RefreshData();
        Repaint();
    }


    void RefreshData()
    {
        m_Textures = Selection.GetFiltered<Texture2D>(SelectionMode.Assets);
        if (m_Textures != null && m_Textures.Length > 0)
        {
            m_TargetWidth = m_Textures[0].width;
            m_TargetHeigh = m_Textures[0].height;
        }
    }

    void OnGUI()
    {
        if (m_Textures == null || m_Textures.Length < 1)
        {
            EditorGUILayout.HelpBox("请选择贴图", MessageType.Info);
            return;
        }

        EditorGUILayout.Space();
        m_ResizeType = (ResizeType)EditorGUILayout.EnumPopup("缩放方式:", m_ResizeType);
        EditorGUILayout.Space();
        GUILayout.BeginHorizontal();
        if (m_ResizeType == ResizeType.WidthHeight)
        {
            EditorGUILayout.LabelField("size:", GUILayout.Width(80));
            m_TargetWidth = EditorGUILayout.IntField(m_TargetWidth);
            EditorGUILayout.LabelField("x", GUILayout.Width(10));
            m_TargetHeigh = EditorGUILayout.IntField(m_TargetHeigh);
        }
        else
        {

            EditorGUILayout.LabelField("ratio：", GUILayout.Width(80));
            m_RatioValue = EditorGUILayout.Slider(m_RatioValue, 0.05f, 0.99f);
        }

        if (GUILayout.Button("确定调整"))
        {
            ReSizeTextures();
        }
        GUILayout.EndHorizontal();
        EditorGUILayout.Space();
        m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos);
        EditorGUILayout.BeginVertical();
        foreach (var item in m_Textures)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(string.Format("{0}x{1}:", item.width, item.height), GUILayout.Width(80));
            EditorGUILayout.ObjectField(item, typeof(Texture2D), false);
            GUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
    }


    void ReSizeTextures()
    {
        int count = 0;
        foreach (var texture in m_Textures)
        {
            string projectPath = AssetDatabase.GetAssetPath(texture);
            string path = Path.Combine(Application.dataPath, projectPath.Substring(7)).Replace("\\", "/");
            Texture2D ntex = new Texture2D(2, 2);
            ntex.LoadImage(File.ReadAllBytes(path));

            int width = m_TargetWidth;
            int height = m_TargetHeigh;
            if (m_ResizeType == ResizeType.Ratio)
            {
                width = (int)(ntex.width * m_RatioValue);
                height = (int)(ntex.width * m_RatioValue);
            }

            TextureScale.Bilinear(ntex, width, height);

            File.WriteAllBytes(path, ntex.EncodeToPNG());

            Object.DestroyImmediate(ntex);


            EditorUtility.DisplayProgressBar("resize Texture", path, (float)++count / (float)m_Textures.Length);
        }

        EditorUtility.ClearProgressBar();

        AssetDatabase.Refresh();
    }



    [MenuItem("Window/TextureResize")]
    static void Open()
    {
        EditorWindow.GetWindow<TextureResizeWindow>().Show();
    }
}