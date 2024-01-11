using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class ShaderExamine : EditorWindow
{
    class ReShaderInfo
    {
        public Shader rawshaer { get; set; }
        public Shader repshaer { get; set; }
        public bool foldout { get; set; }
    }
    private Dictionary<Shader, List<Renderer>> m_AllRendererMat;

    private List<ReShaderInfo> m_shaders;

    void StartCheck()
    {
        m_AllRendererMat = new Dictionary<Shader, List<Renderer>>();
        Renderer[] renderers = GameObject.FindObjectsOfType<Renderer>();
        int count = 0;
        foreach (var renderer in renderers)
        {
            Material[] mats = renderer.sharedMaterials;
            foreach (var mat in mats)
            {
                if (mat.shader != null)
                {
                    List<Renderer> list;
                    if (!m_AllRendererMat.TryGetValue(mat.shader, out list))
                    {
                        list = new List<Renderer>();
                        m_AllRendererMat.Add(mat.shader, list);
                    }

                    list.Add(renderer);
                    break;
                }
            }


            EditorUtility.DisplayProgressBar("检查着色器", renderer.name, (float)++count / (float)renderers.Length);
        }

        m_shaders = new List<ReShaderInfo>();
        foreach (var item in m_AllRendererMat.Keys.ToArray())
            m_shaders.Add(new ReShaderInfo { rawshaer = item });
        m_shaders.Sort((ReShaderInfo s1, ReShaderInfo s2) => { return m_AllRendererMat[s2.rawshaer].Count.CompareTo(m_AllRendererMat[s1.rawshaer].Count); });

        EditorUtility.ClearProgressBar();
    }

    void RepShader(ReShaderInfo info)
    {
        List<Renderer> rlist = m_AllRendererMat[info.rawshaer];

        foreach (var item in rlist)
        {
            Material[] mats = item.sharedMaterials;
            foreach (var mat in mats)
            {
                if (AssetDatabase.IsNativeAsset(mat))
                {
                    mat.shader = info.repshaer;
                }
            }
            //item.sharedMaterials = mats;
        }
    }

    Vector2 scrollPos;
    bool errorShow = true;
    void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space();
        if (GUILayout.Button("检测场景上所有着色器！"))
        {
            StartCheck();
        }

        errorShow = EditorGUILayout.ToggleLeft("只显示非法着色器!", errorShow);

        //EditorGUILayout.Space();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        if (m_shaders == null || m_AllRendererMat == null)
        {
            return;
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        foreach (var item in m_shaders)
        {
            if (errorShow && item.rawshaer.name.StartsWith("X_Shader/"))
            {
                continue;
            }

            List<Renderer> rlist = m_AllRendererMat[item.rawshaer];
            EditorGUILayout.BeginHorizontal();
            item.foldout = EditorGUILayout.Foldout(item.foldout, "详情");
            EditorGUILayout.ObjectField(item.rawshaer, typeof(Shader),false);
            EditorGUILayout.LabelField(rlist.Count.ToString(), GUILayout.Width(50));

            if (GUILayout.Button("<-- 替换 -->"))
            {
                if (item.repshaer == null)
                {
                    EditorUtility.DisplayDialog("替换", "目标着色器不存在", "ok");
                    return;
                }
                RepShader(item);
            }

            item.repshaer = (Shader)EditorGUILayout.ObjectField(item.repshaer, typeof(Shader), false);
            EditorGUILayout.EndHorizontal();

            if (item.foldout)
                foreach (var renderer in rlist)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUI.indentLevel += 5;
                    EditorGUILayout.ObjectField(renderer, typeof(Renderer),false);
                    EditorGUI.indentLevel -= 5;
                    EditorGUILayout.EndHorizontal();
                }
        }
        EditorGUILayout.EndScrollView();
    }


















    [MenuItem("XGame/Examine/Shader")]
    static void Open()
    {
        ShaderExamine se = EditorWindow.GetWindow<ShaderExamine>();
        se.Show();
    }
}