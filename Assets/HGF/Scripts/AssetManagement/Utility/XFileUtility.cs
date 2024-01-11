using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using System;
using System.Collections.Generic;

public class XFileUtility
{
    public static bool ExistResource(string path)
    {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS
        try
        {
            return File.Exists(path);
        }
        catch (System.Exception e) { Debug.LogError(e.ToString()); }
#elif UNITY_ANDROID
        WWW www = new WWW(path);
        while (!www.isDone)
            System.Threading.Thread.Sleep(5);
        if (string.IsNullOrEmpty(www.error))
        {           
            www.Dispose();
            return true;
        }
#endif
        return false;
    }

    public static Sprite ReadStreamingImg(string path, out string error)
    {
        error = string.Empty;
        Sprite sprite = null;
        Texture2D texture = null;

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS
        try
        {
            byte[] bytes = File.ReadAllBytes(path);
            texture = new Texture2D(1, 1);
            texture.LoadImage(bytes);
            texture.name = "streaming image";
            sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        }
        catch (System.Exception e) { error = e.ToString(); }
#elif UNITY_ANDROID
        WWW www = new WWW(path);
        while (!www.isDone)
            System.Threading.Thread.Sleep(5);
        error = www.error;
        if (string.IsNullOrEmpty(error))
        {
            texture = www.texture;
   
            sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            www.Dispose();
        }
#endif
        return sprite;
    }

    public static string ReadStreamingFile(string path, out string error)
    {
        error = string.Empty;
        string data = string.Empty;
        
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS
        try
        {
            data = File.ReadAllText(path);
        }
        catch (System.Exception e) { error = e.ToString(); }
#elif UNITY_ANDROID
        WWW www = new WWW(path);
        while (!www.isDone)
            System.Threading.Thread.Sleep(5);
        error = www.error;
        if (string.IsNullOrEmpty(error))
        {
            data = www.text;
            www.Dispose();
        }
#endif
        return data;
    }


    public static AssetBundle ReadStreamingAssetBundle(string path, out string error)
    {
        error = string.Empty;
        AssetBundle data = null;

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS
        try
        {
            if (File.Exists(path))
                data = AssetBundle.LoadFromFile(path);
            else
                error = "file not exist!  " + path;
        }
        catch (System.Exception e) { error = e.ToString(); }
#elif UNITY_ANDROID
        WWW www = new WWW(path);
        while (!www.isDone)
            System.Threading.Thread.Sleep(5);
        error = www.error;
        if (string.IsNullOrEmpty(error))
        {
            data = www.assetBundle;
            www.Dispose();
        }
#endif
        return data;
    }
    
    public static Sprite ReadStreamingImgEx(string fileName, out string error)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            error = "path is error";
            return null;
        }

        return ReadStreamingImg(Path.Combine(AssetManagement.AssetDefine.streamingAssetsPath, fileName), out error);
    }

    public static AssetBundle ReadStreamingAssetBundleEx(string fileName, out string error)
    {
        return ReadStreamingAssetBundle(Path.Combine(AssetManagement.AssetDefine.streamingAssetsPath, fileName), out error);
    }

    public static AssetBundle ReadPersistentAssetBundle(string fileName, out string error)
    {
        error = string.Empty;
        string path = Path.Combine(AssetManagement.AssetDefine.ExternalSDCardsPath, fileName);
        if (File.Exists(path))
        {
            try
            {
                return AssetBundle.LoadFromFile(path);
            }
            catch (Exception ex)
            {
                error = ex.ToString() + path;
            }
        }

        error = "file not exist!  " + path;
        return null;
    }

    public static void WriteText(string path, string data)
    {
        if (File.Exists(path))
            File.Delete(path);
        else
        {
            string parent = Path.GetDirectoryName(path);
            if (!Directory.Exists(parent))
                Directory.CreateDirectory(parent);
        }

        File.WriteAllText(path, data);
    }

    public static void WriteBytes(string path, byte[] bytes)
    {
        if (File.Exists(path))
            File.Delete(path);
        else
        {
            string parent = Path.GetDirectoryName(path);
            if (!Directory.Exists(parent))
                Directory.CreateDirectory(parent);
        }

        File.WriteAllBytes(path, bytes);
    }
    public static string ReadAllText(string fileName, out string error)
    {
        String fileFullPath = Application.persistentDataPath + "/" + fileName;
        if (string.IsNullOrEmpty(fileName) || !File.Exists(fileFullPath))
        {
            error = "FileName Is Null Or The File Not Exist";
            return "";
        }
        string text = File.ReadAllText(fileFullPath);
        if (string.IsNullOrEmpty(text))
        {
            error = "The file content is empty";
            return "";
        }
        error = null;
        return text;
    }
    public static string PersistentDataPath()
    {
        return Application.persistentDataPath;
    }
    public static string FileMd5(string file)
    {
        return MD5Utility.FileMd5(file);
    }

    public static void TraverseFolder(string path,List<string> filePathList)
    {
        DirectoryInfo theFolder = new DirectoryInfo(path);

        if (!theFolder.Exists)
            return;

        foreach(FileInfo nextFile in theFolder.GetFiles())
        {
            if (nextFile.Extension.Contains("meta") || nextFile.Extension.Contains("cs")) continue;
            if(filePathList != null)
                filePathList.Add(nextFile.FullName);
        }

        foreach(DirectoryInfo nextFolder in theFolder.GetDirectories())
        {
            TraverseFolder(nextFolder.FullName, filePathList);
        }
    }
}
