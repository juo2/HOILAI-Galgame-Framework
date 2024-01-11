using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;
using System.IO;

public class AssetRefFindWindow : EditorWindow
{
    public class RefInfo
    {
        //public Object p_Object;
        public string p_AssetPath;
        public HashSet<string> p_ByAssetsIds;
        public bool p_IsShowRef;
    }


    private Dictionary<string, RefInfo> m_FindAssets;
    private string m_FindFolder = "Assets";
    private bool m_IsFindIng = false;
    void RefreshSelected()
    {
        m_FindAssets = new Dictionary<string, RefInfo>();
        foreach (var guid in Selection.assetGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (AssetDatabase.IsValidFolder(path))
            {
                string folder = GetFullPath(path);
                if (!Directory.Exists(folder))
                {
                    Debug.LogWarningFormat("{0} not exist!", folder);
                    continue;
                }


                string[] files = Directory.GetFiles(folder, "*", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    if (Path.GetExtension(file) == ".meta")
                    {
                        continue;
                    }
                    string proPath = GetPorjectPath(file);
                    string fileguid = AssetDatabase.AssetPathToGUID(GetPorjectPath(file));
                    m_FindAssets.Add(fileguid, new RefInfo { p_AssetPath = proPath });
                }
            }
            else
            {
                m_FindAssets.Add(guid, new RefInfo { p_AssetPath = path });
            }
        }

    }

    Vector2 scrollPos = Vector2.zero;
    bool onlyShow;
    bool objectShow;
    void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("需要搜索的目录。目录之间以|分开例：'Assets/a|Assets/b' 路径越多将越慢!", MessageType.Info, true);
        EditorGUILayout.BeginHorizontal();
        m_FindFolder = EditorGUILayout.TextField("搜索的目录：", m_FindFolder);

        if (GUILayout.Button("刷新选中"))
        {
            RefreshSelected();
        }
        EditorGUI.BeginDisabledGroup(m_FindAssets == null || m_FindAssets.Count < 1);
        if (GUILayout.Button(m_IsFindIng ? "取消搜索" : "搜索", GUILayout.Width(200)))
        {
            m_IsFindIng = !m_IsFindIng;
            StartFind();
        }
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        onlyShow = EditorGUILayout.Toggle("只显示被引用的", onlyShow);
        objectShow = EditorGUILayout.Toggle("对象格式显示", objectShow);
        EditorGUILayout.EndHorizontal();

        if (m_FindAssets != null)
        {
            EditorGUILayout.Space();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            EditorGUILayout.BeginVertical();
            foreach (var item in m_FindAssets)
            {
                if (onlyShow)
                {
                    if (item.Value.p_ByAssetsIds == null || item.Value.p_ByAssetsIds.Count < 1)
                    {
                        continue;
                    }
                }


                string str = item.Value.p_AssetPath;
                if (item.Value.p_ByAssetsIds != null && item.Value.p_ByAssetsIds.Count > 0)
                {
                    str = string.Format("({0}) {1}", item.Value.p_ByAssetsIds.Count, item.Value.p_AssetPath);
                }
                EditorGUILayout.BeginHorizontal();
                item.Value.p_IsShowRef = EditorGUILayout.Foldout(item.Value.p_IsShowRef, str);
                if (objectShow)
                {
                    Object rawObject = AssetDatabase.LoadAssetAtPath<Object>(item.Value.p_AssetPath);
                    EditorGUILayout.ObjectField(rawObject, typeof(Object), false, GUILayout.Width(200));
                }


                EditorGUILayout.EndHorizontal();
                if (item.Value.p_IsShowRef)
                {

                    EditorGUI.indentLevel += 2;
                    if (item.Value.p_ByAssetsIds != null)
                    {
                        foreach (var id in item.Value.p_ByAssetsIds)
                        {
                            string objPath = AssetDatabase.GUIDToAssetPath(id);
                            if (objectShow)
                            {
                                Object rawObject = AssetDatabase.LoadAssetAtPath<Object>(objPath);
                                EditorGUILayout.ObjectField(rawObject, typeof(Object), false);
                            }
                            else
                            {
                                EditorGUILayout.BeginHorizontal();
                                if (GUILayout.Button("    " + objPath, "Label"))
                                {
                                    EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(objPath));
                                }
                                EditorGUILayout.EndHorizontal();
                            }
                        }

                    }
                    EditorGUI.indentLevel -= 2;

                }

            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }
    }

    void StartFind()
    {
        string[] folders = m_FindFolder.Split('|');
        if (string.IsNullOrEmpty(m_FindFolder) || folders.Length == 0)
        {
            EditorUtility.DisplayDialog("提示", "搜索目录不存在！", "我知道了");
            return;
        }

        for (int i = 0; i < folders.Length; i++)
        {
            string fullPath = GetFullPath(folders[i]);
            if (!Directory.Exists(fullPath))
            {
                Debug.LogWarningFormat("{0} not exist!", fullPath);
                continue;
            }
            string[] files = Directory.GetFiles(fullPath, "*", SearchOption.AllDirectories);
            int count = 0;
            foreach (var item in files)
            {
                string ext = Path.GetExtension(item);
                EditorUtility.DisplayProgressBar("查找引用", item, (float)++count / (float)files.Length);
                if (ext == ".meta")
                    continue;
                CheckDef(GetPorjectPath(item));
                
            }
        }

        EditorUtility.ClearProgressBar();
        m_IsFindIng = false;
    }


    void CheckDef(string path)
    {
        string assetid = AssetDatabase.AssetPathToGUID(path);
        if (m_FindAssets.ContainsKey(assetid)) return;

        string[] deps = AssetDatabase.GetDependencies(path);

        foreach (var item in deps)
        {
            string guid = AssetDatabase.AssetPathToGUID(item);
            if (m_FindAssets.ContainsKey(guid))
            {
                RefInfo refInfo = m_FindAssets[guid];
                if (refInfo.p_ByAssetsIds == null)
                    refInfo.p_ByAssetsIds = new HashSet<string>();

                if (!refInfo.p_ByAssetsIds.Contains(assetid))
                {
                    refInfo.p_ByAssetsIds.Add(assetid);
                }
            }
        }
    }

    static string GetFullPath(string projectPath)
    {
        return Path.Combine(Application.dataPath, projectPath.Substring(7)).Replace("\\", "/");
    }

    static string GetPorjectPath(string fullPath)
    {
        return fullPath.Substring(Application.dataPath.Length - 6).Replace("\\", "/");
    }


    [MenuItem("Window/AssetRefFindWindow")]
    static void Open()
    {
        EditorWindow.GetWindow<AssetRefFindWindow>().Show();
    }
}