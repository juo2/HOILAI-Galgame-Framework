using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using AssetBundleInfo = AssetsFileOrm.FileOrm.AssetBundleInfo;

public class AssetBundleManifestDownload
{
    private string m_Path;
    public bool isDonwload { get; set; }

    public XVersionFile version { get; set; }
    public XAssetManifest manifest { get; set; }
    public XAssetsFiles files { get; set; }
    public UnityAction onComplete { get; set; }
    public UnityAction<string, float> onProgress { get; set; }
    public float progress { get; set; }
    public string progressStr { get; private set; }
    public string error { get; set; }

    private Dictionary<string, AssetBundleInfo> m_AllBundleInfo = new Dictionary<string, AssetBundleInfo>();
    public Dictionary<string, AssetBundleInfo> allBundleInfo { get { return m_AllBundleInfo; } }

    private Dictionary<string, string> m_AllBundleNameToHash = new Dictionary<string, string>();
    public Dictionary<string, string> allBundleNameToHash { get { return m_AllBundleNameToHash; } }
    public void Download(string path)
    {
        progress = 0;
        error = string.Empty;
        m_Path = path;
        isDonwload = true;
        m_AllBundleInfo.Clear();
        if (path.StartsWith("http"))
        {
            DownLoadVersionFile();
        }
        else
        {
            LoadLocal();
        }
    }


    void LoadLocal()
    {
        string tpath = Path.Combine(m_Path, "version.txt");
        version = JsonUtility.FromJson<XVersionFile>(File.ReadAllText(tpath));

        tpath = Path.Combine(m_Path, "xassetmanifest");
        AssetBundle ab = AssetBundle.LoadFromFile(tpath);
        if (ab)
        {
            manifest = ab.LoadAsset<XAssetManifest>(ab.GetAllAssetNames()[0]);
            ab.Unload(false);
            ab = null;
        }

        tpath = Path.Combine(m_Path, "files");
        ab = AssetBundle.LoadFromFile(tpath);
        if (ab)
        {
            TextAsset textAsset = ab.LoadAsset<TextAsset>(ab.GetAllAssetNames()[0]);
            if (textAsset != null)
            {
                files = JsonUtility.FromJson<XAssetsFiles>(textAsset.text);
                Resources.UnloadAsset(textAsset);
            }
            ab.Unload(true);
            ab = null;
        }


        for (int i = 0; i < 3; i++)
        {
            string fname = string.Format("0{0}", i);
            tpath = m_Path + "/" + fname + "/fileorm.txt";
            if (File.Exists(tpath))
            {
                AssetsFileOrm.FileOrm fo = AssetsFileOrm.FileOrm.Load(tpath);
                FileOrmHnale(fname, fo);
            }
        }

        OnDownloadFinish();
    }



    void DownLoadVersionFile()
    {
        string tpath = Path.Combine(m_Path, "version.txt");
        UnityWebRequest uwr = UnityWebRequest.Get(tpath);
        OnProgress(uwr.url, 0.1f);
        UnityWebRequestAsyncOperation uwrao = uwr.SendWebRequest();
        uwrao.completed += (AsyncOperation ao) =>
        {
            if (string.IsNullOrEmpty(uwr.error))
            {
                version = JsonUtility.FromJson<XVersionFile>(uwr.downloadHandler.text);
                DownLoadManifestFile();
            }
            else
                OnError(uwr.error);
        };
    }

    void DownLoadManifestFile()
    {
        UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(m_Path + "xassetmanifest");
        OnProgress(uwr.url, 0.3f);
        UnityWebRequestAsyncOperation uwrao = uwr.SendWebRequest();
        uwrao.completed += (AsyncOperation ao) =>
        {
            if (string.IsNullOrEmpty(uwr.error))
            {
                AssetBundle ab = ((DownloadHandlerAssetBundle)uwr.downloadHandler).assetBundle;
                if (ab)
                {
                    manifest = ab.LoadAsset<XAssetManifest>(ab.GetAllAssetNames()[0]);
                    ab.Unload(false);
                }

                DownLoadFiles();
            }
            else
                OnError(uwr.error);
        };
    }

    void DownLoadFiles()
    {
        UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(m_Path + "files");
        OnProgress(uwr.url, 0.5f);
        UnityWebRequestAsyncOperation uwrao = uwr.SendWebRequest();
        uwrao.completed += (AsyncOperation ao) =>
        {
            if (string.IsNullOrEmpty(uwr.error))
            {
                AssetBundle ab = ((DownloadHandlerAssetBundle)uwr.downloadHandler).assetBundle;
                if (ab)
                {
                    TextAsset textAsset = ab.LoadAsset<TextAsset>(ab.GetAllAssetNames()[0]);
                    if (textAsset != null)
                    {
                        files = JsonUtility.FromJson<XAssetsFiles>(textAsset.text);
                        Resources.UnloadAsset(textAsset);
                    }
                    ab.Unload(true);
                }
                //DownloadFileorm();

                DownloadFileorm(1);
            }
            else
                OnError(uwr.error);
        };
    }

    void DownloadFileorm(int idx = 0)
    {
        string fname = string.Format("0{0}", idx);
        UnityWebRequest uwr = UnityWebRequest.Get(m_Path + fname + "/fileorm.txt");
        OnProgress(uwr.url, 0.7f + idx * 0.1f);
        UnityWebRequestAsyncOperation uwrao = uwr.SendWebRequest();
        uwrao.completed += (AsyncOperation ao) =>
        {
            AssetsFileOrm.FileOrm fo = null;
            try
            {
                fo = AssetsFileOrm.FileOrm.Load(uwr.url, uwr.downloadHandler.text);
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("{0} {1}", e.ToString(), uwr.url);
            }

            if (fo == null)
            {
                Debug.LogErrorFormat("{1}", uwr.url);
            }

            FileOrmHnale(fname, fo);
            idx++;
            if (idx < 2)
            {
                DownloadFileorm(idx);
            }
            else
                OnDownloadFinish();
        };
    }

    void FileOrmHnale(string fname, AssetsFileOrm.FileOrm fo)
    {
        foreach (var item in fo.p_AssetBundleList)
        {
            string assetBundleName = fname + "/" + item.p_AssetHashBundleName;
            if (m_AllBundleInfo.ContainsKey(assetBundleName))
            {
                Debug.LogWarning("AssetBundleManifestDownload::FileOrmHnale 相同的Hash值: " + item.p_AssetBundleName + " -> " + m_AllBundleInfo[assetBundleName].p_AssetBundleName + " -> " + item.p_AssetHashBundleName);
                continue;
            }

            if (!allBundleNameToHash.ContainsKey(item.p_AssetBundleName))
            {
                allBundleNameToHash.Add(item.p_AssetBundleName, assetBundleName);
            }

            m_AllBundleInfo.Add(assetBundleName, item);
        }
    }


    void OnError(string str)
    {
        error += str;
        OnDownloadFinish();
    }

    void OnDownloadFinish()
    {
        isDonwload = false;
        OnProgress("", 1);
        if (onComplete != null)
        {
            onComplete.Invoke();
        }
    }


    void OnProgress(string desc, float v)
    {
        progress = v;
        progressStr = desc;

        if (onProgress != null)
        {
            onProgress.Invoke(desc, v);
        }
    }
}
