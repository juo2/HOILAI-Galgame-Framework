using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
public class XAssetsFiles : ISerializationCallbackReceiver
{
    public static XAssetsFiles s_BuildingtAssets;
    public static XAssetsFiles s_CurrentAssets;
    public static XVersionFile s_CurrentVersion;
    public static string s_00Version;
    public static string s_il2cppMd5;
    public enum FileOptions : int
    {
        NONE = 0,
        //首包/内置文件
        BUILDING = 1,
        //DLL 文件
        DLL = 2,
        //lua 文件    
        LUA = 4,
        //安装包文件  
        INSTALL = 8,
        //启动更新
        LAUNCHDOWNLOAD = 16,
        //所有
        All = ~0,
    }




    [System.Serializable]
    public class FileStruct
    {
        //文件路径
        public string path;
        //md5
        public string md5;
        //文件大小
        public int size;
        //包标记
        public sbyte tag = -1;
        //下载优先级
        public short priority = -1;
        //文件选项
        public FileOptions options = FileOptions.NONE;
    }

    public int p_FileCount;
    public List<FileStruct> p_AllFiles;
    private Dictionary<string, FileStruct> m_AllFilesMap = new Dictionary<string, FileStruct>();
    public Dictionary<string, FileStruct> allFilesMap { get { return m_AllFilesMap; } }
    public void OnBeforeSerialize()
    {
        p_FileCount = p_AllFiles != null ? p_AllFiles.Count : 0;
    }

    public void OnAfterDeserialize()
    {
        m_AllFilesMap.Clear();
        foreach (var item in p_AllFiles)
            m_AllFilesMap.Add(item.path, item);
    }

#if UNITY_EDITOR
    public void Clear()
    {
        if (p_AllFiles == null)
            p_AllFiles = new List<FileStruct>();
        p_AllFiles.Clear();
        if (m_AllFilesMap == null)
            m_AllFilesMap = new Dictionary<string, FileStruct>();
        m_AllFilesMap.Clear();
    }

    public void Add(FileStruct fs)
    {
        if (!m_AllFilesMap.ContainsKey(fs.path))
        {
            p_AllFiles.Add(fs);
            m_AllFilesMap.Add(fs.path, fs);
        }
    }
#endif
}

[System.Serializable]
public class XVersionFile
{
    [System.Serializable]
    public struct VersionStruct
    {
        public string gitVer;
        //public string buildDate;
    }

    //public VersionStruct p_LuaVersion;
    public VersionStruct p_DevVersion;
    //public VersionStruct p_ArtVersion;

    public string p_files_md5;
    public string p_manifest_md5;
}


public class XAssetManifest : ScriptableObject, ISerializationCallbackReceiver
{
    [System.Serializable]
    struct AssetBundleInfo
    {
        public uint m_AssetBundleCrc;
        //public string m_AssetBundleMd5;
        public int[] m_AssetBundleDependencies;
    }

    [SerializeField]
    private List<string> m_AssetBundleNames = new List<string>();
    [SerializeField]
    private List<AssetBundleInfo> m_AssetBundleInfos = new List<AssetBundleInfo>();
    private Dictionary<string, int> m_AssetBundleOrmName = new Dictionary<string, int>(10000);

    [SerializeField]
    private List<string> m_AssetRomKeySer = new List<string>();
    [SerializeField]
    private List<int> m_AssetRomIndexSer = new List<int>();
    //资源映射
    private Dictionary<string, int> m_AssetsOrmBundles = new Dictionary<string, int>(100000);
    public Dictionary<string, int> assetsOrmBundles { get { return m_AssetsOrmBundles; } }
    public void OnBeforeSerialize()
    {
        m_AssetRomKeySer.Clear();
        m_AssetRomIndexSer.Clear();
        foreach (var item in m_AssetsOrmBundles)
        {
            m_AssetRomKeySer.Add(item.Key);
            m_AssetRomIndexSer.Add(item.Value);
        }
    }

    public void OnAfterDeserialize()
    {
        m_AssetsOrmBundles.Clear();
        for (int i = 0; i != Mathf.Min(m_AssetRomKeySer.Count, m_AssetRomIndexSer.Count); i++)
            m_AssetsOrmBundles.Add(m_AssetRomKeySer[i], m_AssetRomIndexSer[i]);

        m_AssetRomKeySer.Clear();
        m_AssetRomIndexSer.Clear();

        m_AssetBundleOrmName.Clear();
        for (int i = 0; i < m_AssetBundleNames.Count; i++)
            m_AssetBundleOrmName.Add(m_AssetBundleNames[i], i);
    }

    int GetAssetBundleIdxAlName(string assetBundleName)
    {
        return m_AssetBundleOrmName.ContainsKey(assetBundleName) ? m_AssetBundleOrmName[assetBundleName] : m_AssetBundleNames.IndexOf(assetBundleName);
    }

    int[] GetAssetBundleIdxAlName(string[] assetBundleNames)
    {
        if (assetBundleNames != null && assetBundleNames.Length > 0)
        {
            int[] depidxs = new int[assetBundleNames.Length];
            for (int i = 0; i < assetBundleNames.Length; i++)
                depidxs[i] = GetAssetBundleIdxAlName(assetBundleNames[i]);
            return depidxs;
        }
        return null;
    }

    string GetAssetNameAtIdx(int idx)
    {
        if (idx < 0 || idx > m_AssetBundleNames.Count - 1)
            return null;
        return m_AssetBundleNames[idx];
    }

    string[] GetAssetNameAtIdx(int[] idxs)
    {
        if (idxs != null && idxs.Length > 0)
        {
            string[] result = new string[idxs.Length];
            for (int i = 0; i < idxs.Length; i++)
                result[i] = GetAssetNameAtIdx(idxs[i]);
            return result;
        }
        return null;
    }

    void GetAssetNameAtIdx(int[] idxs, List<string> result)
    {
        if (idxs != null && idxs.Length > 0)
        {
            for (int i = 0; i < idxs.Length; i++)
                result.Add(GetAssetNameAtIdx(idxs[i]));
        }
    }

#if XConfigMode
    public List<string> GetConfigImages()
    {
        List<string> configImageList = new List<string>();

        foreach(var bundleName in m_AssetBundleNames)
        {
            if(bundleName.Contains("single") && !bundleName.Contains("prefab"))
            {
                int ormName = m_AssetBundleOrmName[bundleName];

                foreach(var keyValuePair in m_AssetsOrmBundles)
                {
                    if(keyValuePair.Value == ormName && !keyValuePair.Key.Contains("spriteatlas"))
                    {
                        configImageList.Add(keyValuePair.Key);
                    }
                }
            }
        }

        return configImageList;
    }

    public List<string> GetConfigAudio()
    {
        List<string> configImageList = new List<string>();

        foreach (var bundleName in m_AssetBundleNames)
        {
            if (bundleName.Contains("01/art/audio/humansound"))
            {
                int ormName = m_AssetBundleOrmName[bundleName];

                foreach (var keyValuePair in m_AssetsOrmBundles)
                {
                    if (keyValuePair.Value == ormName)
                    {
                        configImageList.Add(keyValuePair.Key);
                    }
                }
            }
        }

        return configImageList;
    }

    public List<string> GetConfigBgm()
    {
        List<string> configImageList = new List<string>();

        foreach (var bundleName in m_AssetBundleNames)
        {
            if (bundleName.Contains("01/art/audio/backgroundmusic"))
            {
                int ormName = m_AssetBundleOrmName[bundleName];

                foreach (var keyValuePair in m_AssetsOrmBundles)
                {
                    if (keyValuePair.Value == ormName)
                    {
                        configImageList.Add(keyValuePair.Key);
                    }
                }
            }
        }

        return configImageList;
    }

    public List<string> GetConfigVideo()
    {
        List<string> configImageList = new List<string>();

        foreach (var bundleName in m_AssetBundleNames)
        {
            if (bundleName.Contains("01/art/video"))
            {
                int ormName = m_AssetBundleOrmName[bundleName];

                foreach (var keyValuePair in m_AssetsOrmBundles)
                {
                    if (keyValuePair.Value == ormName)
                    {
                        configImageList.Add(keyValuePair.Key);
                    }
                }
            }
        }

        return configImageList;
    }
#endif

    public string GetAssetBundleNameAtAssetName(string assetName)
    {
        int idx = -1;
        if (!m_AssetsOrmBundles.TryGetValue(assetName, out idx))
            return string.Empty;
        if (idx < 0 || idx > m_AssetBundleNames.Count - 1)
            return string.Empty;
        return m_AssetBundleNames[idx];
    }


    string[] temp = new string[0];
    public string[] GetAllDependencies(string assetBundleName)
    {
        int idx = GetAssetBundleIdxAlName(assetBundleName);
        if (idx < 0 || idx > m_AssetBundleNames.Count - 1)
            return temp;
        if (m_AssetBundleInfos[idx].m_AssetBundleDependencies != null && m_AssetBundleInfos[idx].m_AssetBundleDependencies.Length > 0)
            return GetAssetNameAtIdx(m_AssetBundleInfos[idx].m_AssetBundleDependencies);
        return temp;
    }

    public void GetAllDependencies(string assetBundleName, List<string> result)
    {
        int idx = GetAssetBundleIdxAlName(assetBundleName);
        if (idx < 0 || idx > m_AssetBundleNames.Count - 1)
            return;
        if (m_AssetBundleInfos[idx].m_AssetBundleDependencies != null && m_AssetBundleInfos[idx].m_AssetBundleDependencies.Length > 0)
            GetAssetNameAtIdx(m_AssetBundleInfos[idx].m_AssetBundleDependencies, result);
    }

    public bool ContainsAsset(string assetName)
    {
        return !string.IsNullOrEmpty(GetAssetBundleNameAtAssetName(assetName));
    }

    public uint GetAssetBundleCrc(string assetBundleName)
    {
        int idx = GetAssetBundleIdxAlName(assetBundleName);
        if (idx < 0 || idx > m_AssetBundleNames.Count - 1)
            return 0;
        return m_AssetBundleInfos[idx].m_AssetBundleCrc;
    }

#if UNITY_EDITOR
    public void EditorInitManifest(string folder)
    {
        AssetsFileOrm orm = AssetsFileOrm.OpenAll(folder);

        string[] folders = System.IO.Directory.GetDirectories(folder);
        foreach (var folder_child in folders)
        {
            string folderName = System.IO.Path.GetFileName(folder_child);
            string manifestPath = System.IO.Path.Combine(folder_child, folderName);
            AssetsFileOrm.FileOrm assetFileOrm = orm.allFileOrm.ContainsKey(folderName) ? orm.allFileOrm[folderName] : null;

            if (!System.IO.File.Exists(manifestPath))
                continue;

            AssetBundle ab = AssetBundle.LoadFromFile(manifestPath);

            if (ab == null)
                continue;

            AssetBundleManifest manifest = ab.LoadAsset<AssetBundleManifest>("assetbundlemanifest");
            ab.Unload(false);

            string[] abNames = manifest.GetAllAssetBundles();

            //先记录包名
            for (int i = 0; i < abNames.Length; i++)
            {
                //将所有ab包的名字加上目录名  如 00/name 01/name 02/name
                string newABName = folderName + "/" + abNames[i];
                if (!m_AssetBundleNames.Contains(newABName))
                    m_AssetBundleNames.Add(newABName);
            }

            //分析依赖
            for (int i = 0; i < abNames.Length; i++)
            {
                string newABName = folderName + "/" + abNames[i];
                string assetBundleName = abNames[i];
                string abPath = System.IO.Path.Combine(folder_child, assetBundleName);
                uint crc = 0;
                AssetBundleInfo info = new AssetBundleInfo();
                string[] deps = manifest.GetAllDependencies(assetBundleName);
                for (int j = 0; j < deps.Length; j++)
                    deps[j] = folderName + "/" + deps[j];
                info.m_AssetBundleDependencies = GetAssetBundleIdxAlName(deps);
                if (UnityEditor.BuildPipeline.GetCRCForAssetBundle(abPath, out crc))
                    info.m_AssetBundleCrc = crc;
                m_AssetBundleInfos.Insert(m_AssetBundleNames.IndexOf(newABName), info);
            }

            //资源映射包名
            if (assetFileOrm != null && assetFileOrm.p_AssetBundleList != null)
            {
                foreach (var bundleInfo in assetFileOrm.p_AssetBundleList)
                {
                    string assetBundleName = folderName + "/" + bundleInfo.p_AssetHashBundleName.ToLower();
                    int index = GetAssetBundleIdxAlName(assetBundleName);
                    if (bundleInfo.p_Assets != null)
                    {
                        foreach (var asset in bundleInfo.p_Assets)
                        {
                            if (!m_AssetsOrmBundles.ContainsKey(asset.p_AssetName))
                            {
                                m_AssetsOrmBundles.Add(asset.p_AssetName, index);
                            }
                            else
                            {
                                string error = string.Format("XAssetManifest::EditorInitManifest() 重复的资源命名 [{0}]   [{1}]", asset.p_AssetName, asset.p_AssetPath);
                                Debug.LogWarning(error);
                            }


                            //脚本映射一个资源就行了
                            if (folderName == "00")
                                break;
                        }
                    }
                }
            }
        }
    }
#endif
}
