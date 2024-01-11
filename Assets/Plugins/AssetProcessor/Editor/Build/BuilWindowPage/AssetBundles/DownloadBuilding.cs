using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;

public class DownloadBuilding
{


    public static void StartDownload(string url, string path, List<XAssetsFiles.FileStruct> files)
    {
        DownLoadVersionFile(url, path, files);
    }



    private static void DownLoadVersionFile(string path, string savePath, List<XAssetsFiles.FileStruct> files)
    {

        UnityWebRequest uwr = UnityWebRequest.Get(path + "version.txt");
        EditorUtility.DisplayProgressBar("下载版本文件", uwr.url, 0.3f);
        UnityWebRequestAsyncOperation uwrao = uwr.SendWebRequest();
        uwrao.completed += (AsyncOperation ao) =>
        {
            if (string.IsNullOrEmpty(uwr.error))
            {
                File.WriteAllBytes(Path.Combine(savePath, "version.txt"), uwr.downloadHandler.data);
                DownLoadManifestFile(path, savePath, files);
            }
            else
            {
                EditorUtility.DisplayDialog("版本文件下载失败", uwr.error, "确定");
            }
        };
    }

    private static void DownLoadManifestFile(string path, string savePath, List<XAssetsFiles.FileStruct> files)
    {
        UnityWebRequest uwr = UnityWebRequest.Get(path + "xassetmanifest");
        EditorUtility.DisplayProgressBar("下载清单文件", uwr.url, 0.6f);
        UnityWebRequestAsyncOperation uwrao = uwr.SendWebRequest();
        uwrao.completed += (AsyncOperation ao) =>
        {
            if (string.IsNullOrEmpty(uwr.error))
            {
                File.WriteAllBytes(Path.Combine(savePath, "xassetmanifest"), uwr.downloadHandler.data);
                DownLoadFiles(path, savePath, files);
            }
            else
            {
                EditorUtility.DisplayDialog("清单文件下载失败", uwr.error, "确定");
            }
        };
    }

    private static void DownLoadFiles(string path, string savePath, List<XAssetsFiles.FileStruct> files)
    {
        UnityWebRequest uwr = UnityWebRequest.Get(path + "files");
        EditorUtility.DisplayProgressBar("下载文件列表", uwr.url, 0.9f);
        UnityWebRequestAsyncOperation uwrao = uwr.SendWebRequest();
        uwrao.completed += (AsyncOperation ao) =>
        {
            if (string.IsNullOrEmpty(uwr.error))
            {
                File.WriteAllBytes(Path.Combine(savePath, "files"), uwr.downloadHandler.data);
                Download(path, savePath, files, files.Count);
            }
            else
            {
                EditorUtility.DisplayDialog("文件列表下载失败", uwr.error, "确定");
            }
        };
    }

    private static void Download(string url, string path, List<XAssetsFiles.FileStruct> files, int total)
    {
        if (files.Count < 1)
        {
            EditorUtility.ClearProgressBar();
            return;
        }

        XAssetsFiles.FileStruct fs = files[0];
        files.RemoveAt(0);

        string spath = url + fs.path;
        string savePath = Path.Combine(path, fs.path);

        int count = total - files.Count;

        EditorUtility.DisplayProgressBar(string.Format("下载首包文件...[{0}/{1}]", count, total), spath, (float)count / (float)total);

        if (File.Exists(savePath))
        {
            if (XBuildUtility.fileMD5(savePath) == fs.md5)
            {
                Download(url, path, files, total);
                return;
            }
            else
                File.Delete(savePath);
        }


        UnityWebRequest uwr = UnityWebRequest.Get(spath);
        UnityWebRequestAsyncOperation uwrao = uwr.SendWebRequest();

        uwrao.completed += (AsyncOperation ao) =>
        {
            if (!string.IsNullOrEmpty(uwr.error))
            {
                Debug.LogError(uwr.error);
                files.Add(fs);
            }
            else
            {
                string dir = Path.GetDirectoryName(savePath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                File.WriteAllBytes(savePath, uwr.downloadHandler.data);

                if (XBuildUtility.fileMD5(savePath) != fs.md5)
                {
                    File.Delete(savePath);
                    files.Add(fs);
                }
            }

            Download(url, path, files, total);
        };
    }
}