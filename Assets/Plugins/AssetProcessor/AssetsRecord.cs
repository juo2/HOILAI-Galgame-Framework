using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

[System.Serializable]
public class AssetsRecord
{
    public static AssetsRecord s_CurrentRecord;
    public List<string> p_Assets;

    public void Record(string assetName)
    {

        if (p_Assets == null) p_Assets = new List<string>();
        if (p_Assets.Contains(assetName)) return;
        RecordByTask(assetName);
        p_Assets.Insert(0, assetName);
    }





    /// <summary>
    /// 记录任务阶段
    /// </summary>
    /// <param name="assetName"></param>
    private StreamWriter sw;

    string getTaskCode = @"
            if MDefine and MDefine.cache.task and GGameScene and GGameScene.isEnter then
                local task =  MDefine.cache.task.get_taskByType()
                if task then return task.cfg.code end
            end";
    System.Reflection.MethodInfo methodinfo;
    public void RecordByTask(string assetName)
    {
        if (assetName.EndsWith(".png") || assetName.EndsWith(".unity")) return;

        if (sw == null)
        {
            string path = Path.Combine(Application.dataPath.Substring(0, Application.dataPath.Length - 6), "taskrecord.csv");
            if (File.Exists(path)) File.Delete(path);
            sw = File.CreateText(path);
            System.Reflection.Assembly Assembly = System.Reflection.Assembly.Load("Assembly-CSharp");
            System.Type luaenvr = Assembly.GetType("LuaEnvironment");
            methodinfo = luaenvr.GetMethod("DoString");
        }

        object reobj = methodinfo.Invoke(null, new object[] { getTaskCode });
        object[] arr = reobj as object[];
        int taskCode = 0;
        if (arr != null && arr.Length > 0)
        {
            Debug.LogFormat("::::::::::::::::录制任务加载：taskCode:{0}  assetName:{1}    ::::::::::::::::", taskCode, assetName);
            taskCode = System.Convert.ToInt32(arr[0]);
        }

        sw.WriteLine("{0},{1}", taskCode, assetName);
        sw.Flush();
    }

}
