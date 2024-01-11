using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class BoneEffectWindow : EditorWindow
{
    private GameObject m_ActiveGameObject;
    private Transform m_ActiveTransform;
    private List<Transform> m_Boneeffs;
    private int m_PageIndex = 1;
    private string[] m_PageNames = new string[] { "抽取骨骼特效", "附加骨骼特效" };

    private Transform m_TargetSkl;
    private Transform m_TargetEff;

    private string m_State;
    void OnEnable()
    {
        Refresh(null);
    }

    void OnSelectionChange()
    {

        if (Selection.activeGameObject != null)
        {
            Refresh(Selection.activeGameObject);
            Repaint();
        }
    }



    void Refresh(GameObject go)
    {
        if (go != null)
        {
            m_ActiveGameObject = go;
            m_ActiveTransform = m_ActiveGameObject.transform;
            m_Boneeffs = new List<Transform>();
            IterationTransform(go.transform, m_Boneeffs);
        }
    }


    void IterationTransform(Transform transform, List<Transform> boneeffs)
    {
        if (transform.childCount < 1)
            return;

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.name.EndsWith("_boneeff"))
            {
                boneeffs.Add(child);
            }
            else
            {
                IterationTransform(child, boneeffs);
            }
        }
    }


    void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space();
        m_PageIndex = GUILayout.Toolbar(m_PageIndex, m_PageNames);
        EditorGUILayout.Space();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        if (m_PageIndex == 0)
        {
            DoDeBoneGUI();
        }
        else
        {
            DoEnBoneGUI();
        }
    }

    Vector2 scrollPos = Vector2.zero;
    void DoDeBoneGUI()
    {
        EditorGUILayout.HelpBox("使用方式：选中模型对象，后点击（抽取）！", MessageType.Info);
        EditorGUI.BeginDisabledGroup(m_Boneeffs == null || m_Boneeffs.Count == 0);
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("抽取"))
            {
                ExtractToNewGameObject();
            }

            EditorGUILayout.EndHorizontal();
        }
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.LabelField(string.Format("数量：{0}", m_Boneeffs == null ? 0 : m_Boneeffs.Count));
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        EditorGUILayout.BeginVertical();
        if (m_Boneeffs != null)
        {
            foreach (var item in m_Boneeffs)
            {
                EditorGUI.BeginDisabledGroup(true);
                {
                    //EditorGUILayout.ObjectField(item.parent, typeof(Transform), true);
                    //EditorGUI.indentLevel += 2;
                    EditorGUILayout.ObjectField(item, typeof(Transform), true);
                    //EditorGUI.indentLevel -= 2;
                    EditorGUI.EndDisabledGroup();
                }
                EditorGUILayout.Space();
            }
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
    }

    void ExtractToNewGameObject()
    {
        GameObject newGo = new GameObject(m_ActiveGameObject.name + "_BoneEffs");
        Transform nTransform = newGo.transform;
        foreach (var item in m_Boneeffs)
        {
            TransformMoveTo(item, nTransform);
        }
    }


    void TransformMoveTo(Transform ts, Transform parent)
    {
        Vector3 opos = ts.localPosition;
        Vector3 oscale = ts.localScale;
        Vector3 orot = ts.localEulerAngles;
        ts.SetParent(parent);
        ts.localPosition = opos;
        ts.localScale = oscale;
        ts.localRotation = Quaternion.Euler(orot);
    }

    void DoEnBoneGUI()
    {
        EditorGUILayout.HelpBox("使用方式：拖入相应对象，后点击（附加）！命名规则  骨骼名_boneeff", MessageType.Info);
        EditorGUI.BeginDisabledGroup(m_TargetSkl == null || m_TargetEff == null);
        if (GUILayout.Button("附加"))
        {
            ExtractGameObjectToSkl();
        }
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.Space();
        EditorGUI.BeginDisabledGroup(true);
        m_TargetSkl = (Transform)EditorGUILayout.ObjectField("选中骨骼：", m_ActiveTransform, typeof(Transform), true);
        EditorGUI.EndDisabledGroup();

        m_TargetEff = (Transform)EditorGUILayout.ObjectField("特效：", m_TargetEff, typeof(Transform), true);

        if (!string.IsNullOrEmpty(m_State))
        {
            EditorGUILayout.HelpBox(m_State, MessageType.Error);
        }
    }

    void ExtractGameObjectToSkl()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        Dictionary<string, Transform> bones = new Dictionary<string, Transform>();
        Transform[] tempBones = m_TargetSkl.gameObject.GetComponentsInChildren<Transform>();
        foreach (var item in tempBones)
        {
            if (bones.ContainsKey(item.name))
            {
                sb.AppendLine(string.Format("发现同名的骨骼：{0}", item.name));
            }
            else
            {
                bones.Add(item.name, item);
            }
        }

        for (int i = 0; i < m_TargetEff.childCount; i++)
        {
            Transform child = m_TargetEff.GetChild(i);
            int index = child.name.IndexOf("_boneeff");
            if (index == -1)
            {
                sb.AppendLine(string.Format("错误的名字：{0}", child.name));
                continue;
            }
            string boneName = child.name.Substring(0, index);

            if (!bones.ContainsKey(boneName))
            {
                sb.AppendLine(string.Format("目标骨骼点不存在：{0}", boneName));
                continue;
            }


            GameObject ngo = GameObject.Instantiate<GameObject>(child.gameObject, bones[boneName]);
            ngo.name = ngo.name.Replace("(Clone)", "");
        }

        m_State = sb.ToString();
    }


    [MenuItem("Window/BoneEffectWindow")]
    static void Open()
    {
        EditorWindow.GetWindow<BoneEffectWindow>().Show();
    }
}