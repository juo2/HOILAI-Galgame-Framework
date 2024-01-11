using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;

public class AssetBundleListUpload : EditorWindow
{



    public static void Open(List<XAssetsFiles.FileStruct> list = null)
    {
        AssetBundleListUpload upload = EditorWindow.GetWindow<AssetBundleListUpload>();
        upload.minSize = upload.maxSize = new Vector2(450, 106);

        upload.fileList = list;
        upload.RefreshData();
    }



    [System.Serializable]
    public struct AssetBundleInfo
    {
        public string name;
        public int size;
    }

    [System.Serializable]
    public class JsonData
    {
        public List<AssetBundleInfo> list = new List<AssetBundleInfo>();
    }


    private List<XAssetsFiles.FileStruct> fileList;
    private List<string> textLines;
    private JsonData jsonData;
    public string url = string.Empty;


    void RefreshData()
    {
        if (fileList != null && fileList.Count > 0)
        {
            jsonData = new JsonData();
            foreach (var fs in fileList)
            {
                jsonData.list.Add(new AssetBundleInfo { name = fs.path, size = fs.size });
            }
        }
        else if (textLines != null && textLines.Count > 0)
        {
            jsonData = new JsonData();
            foreach (var str in textLines)
            {
                string[] rows = str.Split(',');
                string name = rows[0];
                int size = 0;
                int.TryParse(rows[2], out size);
                jsonData.list.Add(new AssetBundleInfo { name = name, size = size });
            }
        }
    }


    void OnGUI()
    {
        if (jsonData == null || jsonData.list.Count <= 0)
        {
            EditorGUILayout.HelpBox("请选择csv文件上传", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("可直接上传 数量：" + jsonData.list.Count, MessageType.Info);
        }


        EditorGUILayout.BeginHorizontal();
        {
            EditorGUIUtility.labelWidth = 40;
            EditorGUI.BeginChangeCheck();
            url = EditorGUILayout.TextField("上传：", url);
            if (EditorGUI.EndChangeCheck())
                url = UnityWebRequest.UnEscapeURL(url);

            bool active = jsonData == null || jsonData.list.Count <= 0 || string.IsNullOrEmpty(url);
            EditorGUI.BeginDisabledGroup(active);
            if (GUILayout.Button("上传"))
            {
                Upload();
            }
            EditorGUI.EndDisabledGroup();
        }
        EditorGUILayout.EndHorizontal();

        bool active2 = jsonData == null || jsonData.list.Count <= 0;
        EditorGUI.BeginDisabledGroup(!active2);
        if (GUILayout.Button("选择csv", GUILayout.Height(40)))
        {
            string path = EditorUtility.OpenFilePanel("选择csv", Path.Combine(Application.dataPath,"../"), "csv");
            if (!string.IsNullOrEmpty(path))
            {
                textLines = new List<string>(File.ReadAllLines(path));
                RefreshData();
            }
        }
        EditorGUI.EndDisabledGroup();
    }



    private void Upload()
    {
        url = UnityWebRequest.UnEscapeURL(url);
        string upurl = url.Replace("detail", "submitData");
        upurl += "/buildingAssets";
        string json = "";

        if (jsonData != null && jsonData.list.Count > 0)
        {
            json = EditorJsonUtility.ToJson(jsonData);
            json = json.Substring(0, json.Length - 1).Substring(8);
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("data", json);
            UnityWebRequest uwr = UnityWebRequest.Post(upurl, dict);
            UnityWebRequestAsyncOperation uwao = uwr.SendWebRequest();
            uwao.completed += (AsyncOperation async) =>
            {
                if (!string.IsNullOrEmpty(uwr.error))
                {
                    EditorUtility.DisplayDialog("出错了", uwr.error, "我知道了");
                }
                else
                {
                    EditorUtility.DisplayDialog("上传成功", uwr.downloadHandler.text, "我知道了");
                }
            };
        }
    }
}