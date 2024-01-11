using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEditor;
using System;
using System.Reflection;
public abstract class CSVReport
{
    public static string csvStr = string.Empty;
    protected int line_length = 12;
    public static int lineCount = 0;
    public static string reporttPath = "";

    public static Dictionary<string, int> propertyName2Index = new Dictionary<string, int>()
    {

    };

    public static List<KeyValuePair<string, string>> propertyName2IndexList = new List<KeyValuePair<string, string>>()
        {
            new KeyValuePair<string,string>("path",     "路径"),
            new KeyValuePair<string,string>("filetype",  "文件夹类型" ),
            new KeyValuePair<string,string>("foldername","文件夹名" ),
            new KeyValuePair<string,string>("type",      "类型" ),
            new KeyValuePair<string,string>("texsize",   "尺寸" ),
            new KeyValuePair<string,string>("compress",  "压缩格式" ),
            new KeyValuePair<string,string>("vert",      "顶点" ),
            new KeyValuePair<string,string>("tri",       "面数" ),
            new KeyValuePair<string,string>("length",    "时长(秒)" ),
            new KeyValuePair<string,string>("memory",    "内存(B)" ),
            new KeyValuePair<string,string>( "invaild",   "" ),
        };

    public static HashSet<string> fileTypeSet = new HashSet<string>
        {
            "Scenes",
            "Story",
            "SkyBox",
            //
            "Shaders",
            //
            "Collect",
            "Controller",
            "Drop",
            "MatCap",
            "Monster",
            "Npc",
            "Pass",
            "Pet",
            "PetMount",
            "Player",
            "PlayerPart",
            "UI",
            //
            "Effect/Charactar",
            "Effect/Fight",
            "Effect/UI",
            //
            "UIAvatar",
        };
    public static HashSet<string> fileSubTypeSet = new HashSet<string>
        {
            "Ogg",
            //
            "Anim",
            "Materials",
            "Models",
            "Textures",
            "Mesh",
            //
            "T_Textures",
            "Texture",
            //
            "animals",
            "animation",
            "- Materials",

        };

    public static void Init(string reporttPath,string fileNamePre = "")
    {
        csvStr = string.Empty;
        CSVReport.reporttPath = reporttPath;

        ClearCache();

        propertyName2Index = new Dictionary<string, int>();
        for (int i = 0; i < propertyName2IndexList.Count; i++)
        {
            var pair = propertyName2IndexList[i];
            propertyName2Index[pair.Key] = i;
        }

        CSVReport.WriteTitle();

        SetFileNamePre(fileNamePre);
    }

    //Assets/Art/Charactars/Monster/bh_1035/Models/bh_1035.FBX
    //Assets/Art/Env/Scenes/scene_tg6_zhuxain01/Mesh/bg_lut_tree_tr_t04_sm_old.FBX


    protected static Dictionary<string, string> line_cache = new Dictionary<string, string>();
    public static bool AnyCache(string path)
    {
        return line_cache.ContainsKey(path);
    }

    public static void WriteTitle()
    {
        for (int i = 0; i < propertyName2IndexList.Count; i++)
        {
            if (i > 0)
                csvStr += ",";

            var pair = propertyName2IndexList[i];
            csvStr += pair.Value;
        }
    }
    public static void ClearCache()
    {
        if (line_cache != null)
            line_cache.Clear();
    }

    public static string fileNamePre = "";

    public static void SetFileNamePre(string value)
    {
        fileNamePre = value;
    }
    public static void Output()
    {
        if (!Directory.Exists(reporttPath))
            return;

        try
        {
            string s = string.Format("{0:yyyyMMdd_HHmmss}", System.DateTime.Now);
            string path = string.Empty;
            if (string.IsNullOrEmpty(fileNamePre))
                path = Path.Combine(reporttPath, "report_" + s + ".csv");
            else
                path = Path.Combine(reporttPath, fileNamePre +"_" + s + ".csv");

            File.WriteAllText(Path.Combine(reporttPath, path), csvStr, System.Text.Encoding.UTF8);
            var explorer = reporttPath.Replace('/', '\\');
            System.Diagnostics.Process.Start("explorer.exe", explorer);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
}
public class CSVLine : CSVReport
{
    string[] line;

    public CSVLine()
    {
        line = new string[line_length];
        pathNode = null;
    }
    public CSVLine(string path, bool init = true) : this()
    {
        SplitPath(path);
        if (init)
        {
            SetPath(path);
            SetFolderName();
            SetFileType();
        }
    }

    protected static string[] pathNode;
    public void SplitPath(string path)
    {
        var temp = path.Replace('/', '\\');
        pathNode = temp.Split('\\');
    }

    public void ApplyLine(string path = "", string customLine = "")
    {
        var lineStr = customLine;
        if (string.IsNullOrEmpty(lineStr))
        {
            for (int i = 0; i < line_length; i++)
            {
                lineStr += line[i] + ",";
            }
        }

        csvStr += "\n";
        csvStr += lineStr;
        lineCount++;

        if (!string.IsNullOrEmpty(path) && !line_cache.ContainsKey(path))
        {
            line_cache[path] = lineStr;
        }
    }

    public void SetValue(int index, string property)
    {
        if (index >= line_length)
            return;

        line[index] = property;
    }

    public int Index(string propertyName)
    {
        int index = 10;
        if (propertyName2Index.TryGetValue(propertyName, out index))
            return index;

        return propertyName2Index["invaild"];
    }
    public void SetPath(string property)
    {
        var temp = property.Replace('/', '\\');
        line[Index("path")] = temp;
    }

    public Dictionary<string, string> fileTypeCacheMap = new Dictionary<string, string>();
    public void SetFileType(string property = "")
    {
        if (string.IsNullOrEmpty(property))
        {
            for (int i = 0; i < pathNode.Length; i++)
            {
                var node = pathNode[i];
                if (fileTypeSet.Contains(node))
                {
                    line[Index("filetype")] = node;
                    return;
                }
            }

            return;
        }

        line[Index("filetype")] = property;
    }

    //protected static Dictionary<string, string> folderNameCache = new Dictionary<string, string>();
    public static string GetFolderName(string path)
    {
        //if (folderNameCache.ContainsKey(path))
        //    return folderNameCache[path];

        var foldername = string.Empty;
        var temp = path.Replace('/', '\\');
        var nodes = temp.Split('\\');

        for (int i = nodes.Length - 2; i >= 0; i--)
        {
            var node = nodes[i];
            if (fileSubTypeSet.Contains(node))
                continue;

            foldername = node;
            break;
        }

        //if (!folderNameCache.ContainsKey(path))
        //    folderNameCache[path] = foldername;

        return foldername;
    }

    private static string GetFolderName()
    {
        var nodes = pathNode;
        var foldername = string.Empty;
        for (int i = nodes.Length - 2; i >= 0; i--)
        {
            var node = pathNode[i];
            if (fileSubTypeSet.Contains(node))
                continue;

            foldername = node;
            break;
        }

        return foldername;
    }
    public void SetFolderName(string property = "")
    {
        if (!string.IsNullOrEmpty(property))
        {
            line[Index("foldername")] = property;
            return;
        }

        line[Index("foldername")] = GetFolderName(); 
    }
    public void SetType(string property)
    {
        line[Index("type")] = property;
    }
    public void SetSize(string property)
    {
        line[Index("texsize")] = property;
    }

    public void SetVert(string property)
    {
        line[Index("vert")] = property;
    }
    public void SetTriangle(string property)
    {
        line[Index("tri")] = property;
    }
    public void SetMemory(string property)
    {
        line[Index("memory")] = property;
    }
    public void SetLength(string property)
    {
        line[Index("length")] = property;
    }
    public void SetCompress(string property)
    {
        line[Index("compress")] = property;
    }
}
