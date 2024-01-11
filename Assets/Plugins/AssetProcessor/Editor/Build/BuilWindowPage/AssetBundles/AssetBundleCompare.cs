using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using FileStruct = XAssetsFiles.FileStruct;
using UnityEditor;
using UnityEngine.Events;
using System.Text;

public class AssetBundleCompare
{
    private string m_Path;
    private string m_Path2;
    private bool m_IsSaveCsv = false;

    private AssetBundleManifestDownload m_Download;
    public AssetBundleManifestDownload download { get { return m_Download; } }

    private AssetBundleManifestDownload m_Download2;
    public AssetBundleManifestDownload download2 { get { return m_Download2; } }

    public static List<FileStruct> currentShowFiles { get; private set; }
    public static XAssetsFiles.FileOptions showOptions = XAssetsFiles.FileOptions.LAUNCHDOWNLOAD;
    public static long totalSize { get; private set; }
    public static int showTags = -2;
    public UnityAction<bool> onComplete { get; set; }

    public AssetBundleCompare(BuildOrmAndPushParameter ormParameter)
    {
        this.m_Path = ormParameter.ormPath;
        this.m_Path2 = ormParameter.webPath;
        this.m_IsSaveCsv = ormParameter.isSaveCsv;

        Debug.Log("m_Path " + this.m_Path);
        Debug.Log("m_Path2 " + this.m_Path2);

        m_Download = new AssetBundleManifestDownload();
        m_Download.onComplete += OnLoadComplete;
        m_Download2 = new AssetBundleManifestDownload();
        m_Download2.onComplete += OnLoadComplete;

        m_Download.Download(this.m_Path);        
        m_Download2.Download(this.m_Path2);
    }

    private void OnLoadComplete()
    {
        Debug.Log(string.Format("AssetBundleCompare Complete {0}  {1}  {2}  {3}  {4}  {5}", m_Download.isDonwload, m_Download.error, m_Download.allBundleInfo.Count,
            m_Download2.isDonwload, m_Download2.error, m_Download2.allBundleInfo.Count));

        if (!string.IsNullOrEmpty(m_Download.error) || !string.IsNullOrEmpty(m_Download2.error))
        {
            if (onComplete != null)
                onComplete.Invoke(false);
            return;
        }

        if (m_Download.isDonwload || m_Download.allBundleInfo.Count <= 0 || m_Download2.isDonwload || m_Download2.allBundleInfo.Count <= 0)
            return;

        FilterFiles();
        if (onComplete != null)
            onComplete.Invoke(true);
    }

    private void FilterFiles()
    {
        if (m_Download.files == null)
            return;

        if (currentShowFiles == null)
            currentShowFiles = new List<FileStruct>();
        else
            currentShowFiles.Clear();

        totalSize = 0;
        List<FileStruct> files = m_Download.files.p_AllFiles;

        foreach (var item in files)
        {
            //if (!string.IsNullOrEmpty(m_SearchString) && !m_SearchString.StartsWith("dep:"))
            //{
            //    if (!item.path.Contains(m_SearchString)) continue;
            //}

            //if (onlyChange)
            //{
            FileStruct fs;
            if (m_Download2.files != null)
            {
                if (!m_Download2.files.allFilesMap.TryGetValue(item.path, out fs)) { }
                else if (fs.md5 != item.md5) { }
                else
                    continue;
            }
            //}

            if (showTags != -2 && showTags != item.tag) continue;

            //if (onlyShowRecord && recordAssetsFiles != null)
            //{
            //    if (!recordAssetsFiles.allFilesMap.ContainsKey(item.path))
            //        continue;
            //}

            if (showOptions == XAssetsFiles.FileOptions.All
                || (showOptions == XAssetsFiles.FileOptions.NONE && item.options == XAssetsFiles.FileOptions.NONE)
                || (showOptions & item.options) != XAssetsFiles.FileOptions.NONE
                || showOptions == item.options)
            {
                totalSize += item.size;
                currentShowFiles.Add(item);
            }
        }

        if (this.m_IsSaveCsv)
        {
            string fpath = Path.Combine(Path.GetDirectoryName(Application.dataPath), "A_Build/buildFirst.csv");
            XBuildUtility.BuildDifferAssetInfo(fpath, currentShowFiles);
            Debug.Log("启动更新列表生成路径: " + fpath);
        }
        else
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in currentShowFiles)
            {
                sb.AppendFormat("{0},{1},{2}\n", item.path, EditorUtility.FormatBytes(item.size), item.size);
            }
            Debug.Log(string.Format("启动更新列表 更新数量{0} 更新大小{1}\n {2} ：", currentShowFiles.Count, EditorUtility.FormatBytes(totalSize), sb.ToString()));
        }
    }

    public static void OneKeyUpdate(BuildOrmAndPushParameter parameter)
    {
        AssetBundleCompare abc = new AssetBundleCompare(parameter);
        abc.onComplete = (x) =>
        {
            Debug.Log("build step 2完成回调结果：" + x);
            EditorApplication.Exit(0);
        };
    }
}