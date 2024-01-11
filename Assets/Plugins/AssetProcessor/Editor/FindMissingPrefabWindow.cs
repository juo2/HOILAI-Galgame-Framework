using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FindMissingPrefabWindow : EditorWindow
{
    public string m_FindPath;
    private int m_MissingCoute = 0;
    private Vector2 m_ScrollPos;
    private Dictionary<string, List<string>> m_PrefabPathDic = new Dictionary<string, List<string>>();

    [MenuItem("Window/Find Missing Prefab")]
    public static void OpenWindow()
    {
        GetWindow<FindMissingPrefabWindow>().Show();
    }
    public void OnGUI()
    {
        m_FindPath = EditorGUILayout.TextField("输入查找的路径：", m_FindPath, GUILayout.Height(20));
        if (GUILayout.Button("开始查找", GUILayout.Height(30)))
        {
            if (string.IsNullOrEmpty(m_FindPath) || !AssetDatabase.IsValidFolder(m_FindPath))
            {
                string title = "输入的文件夹路径为空。请输入正确的路径，例如：Assets/GameObject";
                EditorUtility.DisplayDialog("Error", title, "确定");
                return;
            }
            else
            {
                m_PrefabPathDic.Clear();
                FindMissing();
            }
        }

        if (m_PrefabPathDic.Count <= 0)
        {
            EditorGUILayout.HelpBox("请输入查找的目录", MessageType.Info);
            return;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("目录" + m_FindPath + "搜索结果：");

        m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos);
        EditorGUILayout.BeginVertical();

        foreach(var dicpath in m_PrefabPathDic)
        {
            if(dicpath.Value.Count > 0)
            {
                GameObject item = AssetDatabase.LoadMainAssetAtPath(dicpath.Key) as GameObject;
                for (int i = 0; i < dicpath.Value.Count; i++)
                {
                    Transform nodeObject = item.transform.Find(dicpath.Value[i]);
                    if (nodeObject)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.ObjectField(item, typeof(GameObject), false);
                        EditorGUILayout.ObjectField(nodeObject.gameObject, typeof(GameObject), false);
                        if (GUILayout.Button("删除", GUILayout.Width(50)))
                        {
                            RemoveMissing(dicpath.Key, dicpath.Value[i], true);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("一键删除", GUILayout.Height(30)))
        {
            BatchRemove();
        }
    }
    private string[] GetPrefabsByPath()
    {
        string[] temps = AssetDatabase.FindAssets("t:Prefab", new string[] { m_FindPath });
        List<string> result = new List<string>();
        foreach (string name in temps)
        {
            string path = AssetDatabase.GUIDToAssetPath(name);
            result.Add(path);
        }
        return result.ToArray();
    }
    private string GetNodePath(GameObject node)
    {
        Transform[] tss = node.GetComponentsInParent<Transform>();
        string str = node.name;
        foreach (var item in tss)
        {
            str = item.name + "/" + str;
        }

        return str;
    }
    private void FindMissing()
    {
        string[] allPrefabs = GetPrefabsByPath();

        int count = 0;
        m_MissingCoute = 0;
        EditorUtility.DisplayCancelableProgressBar("Processing...", "开始查找", 0);

        foreach (string prefabPath in allPrefabs)
        {
            AssetDatabase.ImportAsset(prefabPath);
            Object prefabObj = AssetDatabase.LoadMainAssetAtPath(prefabPath);
            if (prefabObj == null)
            {
                Debug.Log("prefab: " + prefabPath + " null?");
                continue;
            }

            GameObject gameObject = prefabObj as GameObject;
            if (EditorUtility.DisplayCancelableProgressBar("Processing...", gameObject.name, ++count / (float)allPrefabs.Length))
            {
                EditorUtility.ClearProgressBar();
                return;
            }

            FindMissingPrefab(gameObject, prefabPath, true);
        }
        EditorUtility.ClearProgressBar();

        if (m_MissingCoute > 0)
        {
            string title = string.Format("路径:{0}下有{1}个Missing Prefab", m_FindPath, m_MissingCoute);
            EditorUtility.DisplayDialog("Processing", title, "确定");
        }
        else
        {
            EditorUtility.DisplayDialog("Processing", "该路径未有引用丢失", "确定");
        }
    }
    private void FindMissingPrefab(GameObject node, string prefabName, bool isRoot)
    {
        if (node.name.Contains("Missing Prefab") || PrefabUtility.IsPrefabAssetMissing(node) || PrefabUtility.IsDisconnectedFromPrefabAsset(node))
        {
            AddMissing(node, prefabName);
            return;
        }

        if (!isRoot)
        {
            if (PrefabUtility.IsAnyPrefabInstanceRoot(node))
            {
                return;
            }

            GameObject root = PrefabUtility.GetNearestPrefabInstanceRoot(node);
            if (root == node)
            {
                return;
            }
        }


        foreach (Transform childT in node.transform)
        {
            FindMissingPrefab(childT.gameObject, prefabName, false);
        }
    }
    private void AddMissing(GameObject node, string prefabName)
    {
        ++m_MissingCoute;
        string nodePath = GetNodePath(node);
        if (m_PrefabPathDic.ContainsKey(prefabName))
        {
            m_PrefabPathDic[prefabName].Add(nodePath);
        }
        else
        {
            List<string> list = new List<string>();
            list.Add(nodePath);
            m_PrefabPathDic.Add(prefabName, list);
        }
    }
    private void RemoveMissing(string rootPath, string nodePath, bool isClear = true)
    {
        GameObject copyGo;
        Object prefabObj = AssetDatabase.LoadMainAssetAtPath(rootPath);
        copyGo = Instantiate(prefabObj as GameObject);
        copyGo.name = prefabObj.name;
        GameObject node = copyGo.transform.Find(nodePath).gameObject;
        DestroyImmediate(node);
        SaveAsPrefabAsset(copyGo, rootPath);
        DestroyImmediate(copyGo);
        Debug.Log("prefab: " + copyGo.name + "删除引用丢失节点name ----->" + node.name);

        if (isClear)
        {
            m_PrefabPathDic[rootPath].Remove(nodePath);
        }
    }
    private void SaveAsPrefabAsset(GameObject rootGo, string prefabPath)
    {
        PrefabUtility.SaveAsPrefabAsset(rootGo, prefabPath);
    }
    private void BatchRemove()
    {
        if(m_PrefabPathDic.Count <= 0)
        {
            return;
        }

        int count = 0;
        EditorUtility.DisplayCancelableProgressBar("Processing...", "开始删除", 0);
        foreach (var dicpath in m_PrefabPathDic)
        {
            if (dicpath.Value.Count > 0)
            {
                GameObject item = AssetDatabase.LoadMainAssetAtPath(dicpath.Key) as GameObject;
                for (int i = 0; i < dicpath.Value.Count; i++)
                {
                    Transform nodeObject = item.transform.Find(dicpath.Value[i]);
                    if (nodeObject)
                    {
                        RemoveMissing(dicpath.Key, dicpath.Value[i], false);
                    }
                }
            }
            EditorUtility.DisplayCancelableProgressBar("Processing...", dicpath.Key, ++count / (float)m_PrefabPathDic.Count);
        }

        EditorUtility.ClearProgressBar();
        m_PrefabPathDic.Clear();
    }
}