using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEditor;
using System.Text;
using System.Globalization;


public class BuildLuaParameter
{
    public string outputPath;
    public string fileName;
    public string luaDirectory;
    public string version;
    public bool isClearFolder;
    public BuildTarget buildTarget;
    public BuildAssetBundleOptions buildAssetBundleOptions;
    public bool isUpdateCSharp;
    public bool isUpdateLua = true;
}

public class BuildResourceParameter
{
    public enum NameType
    {
        NONE,            //文件名为包名
        HASH,            //资源包哈希码命名(注意hash不是文件MD5)
        HASH_APPEND,     //资源包哈希码追加在尾部
    }

    public string version;
    public string outputPath;
    public bool isClearFolder;
    public BuildTarget buildTarget;
    public BuildAssetBundleOptions buildAssetBundleOptions;
    public NameType buildBundleName = NameType.HASH;
}

public class BuildOrmAndPushParameter
{
    public string ormPath;
    public string webPath;
    public bool isSaveCsv;
    public BuildTarget buildTarget;
}

public class XBuildUtility
{
    public static string fileMD5(string file)
    {
        try
        {
            FileStream fs = new FileStream(file, FileMode.Open);
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(fs);
            fs.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }

            return sb.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception("md5file() fail, error:" + ex.Message);
        }
    }

    public static int fileSize(string file)
    {
        try
        {
            return (int)new FileInfo(file).Length;
        }
        catch (Exception ex)
        {

            throw new Exception("fileSize() fail, error:" + ex.Message);
        }
    }

    public static void WriteBuildError(string path, string content)
    {
        if (!File.Exists(path))
            File.Create(path);

        File.AppendAllText(path, content);
    }

    public static DateTime ConvertStringToDateTime(string timeStamp)
    {
        DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
        long lTime = long.Parse(timeStamp + "0000");
        TimeSpan toNow = new TimeSpan(lTime);
        return dtStart.Add(toNow);
    }

    public static string ConvertStringToDateStr(string timeStamp)
    {
        if (string.IsNullOrEmpty(timeStamp)) return string.Empty;

        DateTimeFormatInfo dtFormat = new DateTimeFormatInfo();
        dtFormat.ShortDatePattern = "yyyyMMddHHmmss";

        Debug.Log(timeStamp);
        Debug.Log(System.Convert.ToDateTime(timeStamp));
        Debug.Log(System.Convert.ToDateTime(timeStamp, dtFormat));

        return System.Convert.ToDateTime(timeStamp, dtFormat).ToString("yyyy/MM/dd HH:mm:ss");
    }



    /// <summary>
    /// 获取Svn版本号 
    /// </summary>
    /// <returns>
    ///     190
    ///     20181010101010
    /// </returns>
    public static string GetSvnVersion(string version)
    {
        List<string> result = new List<string>();
        string workDirectory = Application.dataPath.Remove(Application.dataPath.LastIndexOf("/Assets", System.StringComparison.Ordinal));
        string dateStr = string.Format("{0:yyyyMMddHHmmss}", System.DateTime.Now);

        if (string.IsNullOrEmpty(version))
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "svn";
            process.StartInfo.Arguments = string.Format("info -r BASE \"{0}\"", workDirectory);
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler((sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data) && !string.IsNullOrEmpty(e.Data.Trim()))
                    result.Add(e.Data);
            });

            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();
            //process.WaitForExit();
            process.Close();

            int count = 0;
            foreach (var svnInfo in result)
                Debug.LogFormat("XBuildUtility::GetSvnVersion() svn:   {0}:{1}", count++, svnInfo);

            if (result.Count >= 6)
            {
                string revisionVer = result[5];
                string[] sps = revisionVer.Split(':');
                if (sps.Length >= 2)
                    return sps[1].Trim() + "\r\n" + dateStr;
            }
            return dateStr + "\r\n" + dateStr;
        }
        else
        {
            return version + "\r\n" + dateStr;
        }


    }


    /// <summary>
    /// 项目路径转全路径
    /// </summary>
    /// <param name="projectPath"></param>
    /// <returns></returns>
    public static string GetFullPath(string projectPath)
    {
        return Path.Combine(Application.dataPath, projectPath.Substring(7)).Replace("\\", "/");
    }

    /// <summary>
    /// 全路径转项目路径
    /// </summary>
    /// <param name="fullPath"></param>
    /// <returns></returns>
    public static string GetPorjectPath(string fullPath)
    {
        return fullPath.Substring(Application.dataPath.Length - 6).Replace("\\", "/");
    }

    public static AssetBundleBuild CreateAssetBundleBuild(string assetBundleName, string[] addressableNames, string[] assetNames, bool addExtName = true)
    {
        assetBundleName = assetBundleName.Substring(7).ToLower();
        string extname = Path.GetExtension(assetBundleName);
        if (!string.IsNullOrEmpty(extname))
            assetBundleName = assetBundleName.Replace(extname, "");
        if (addExtName)
            assetBundleName += ".asset";
        return new AssetBundleBuild { assetBundleName = assetBundleName, addressableNames = addressableNames, assetNames = assetNames };
    }

    public static string GetPlatformAtBuildTarget(BuildTarget target)
    {
        return target.ToString();
    }



    //文件过滤
    static List<string> s_ExtNameFilters = new List<string>() { ".meta", ".cs", ".cginc", ".exr" };
    static List<string> s_FileNameFilters = new List<string>() { "LightingData.asset", "NavMesh.asset" };
    static bool Filter(string file)
    {
        if (Directory.Exists(file)) return false;
        string extName = Path.GetExtension(file);
        string fileName = Path.GetFileName(file);
        return s_ExtNameFilters.Contains(extName) || s_FileNameFilters.Contains(fileName);
    }
    public static void CollectionFolder(List<AssetBundleBuild> outList, string projectPath, bool isAlone = false)
    {
        string fullPath = XBuildUtility.GetFullPath(projectPath);
        if (string.IsNullOrEmpty(projectPath) || !Directory.Exists(fullPath))
            return;

        string[] files = Directory.GetFiles(fullPath, "*", SearchOption.AllDirectories);
        if (files.Length < 1)
            return;

        if (isAlone)
        {
            //一个文件一个包
            foreach (var file in files)
            {
                if (Filter(file)) continue;
                string assetBundleName = XBuildUtility.GetPorjectPath(file);
                string addressableName = Path.GetFileName(file);
                string assetName = XBuildUtility.GetPorjectPath(file);
                outList.Add(XBuildUtility.CreateAssetBundleBuild(assetBundleName, new string[] { addressableName }, new string[] { assetName }));
            }
        }
        else
        {
            //目录下所有文件打成一个包
            string[] addressableNames = new string[0];
            string[] assetNames = new string[0];
            string assetBundleName = projectPath;

            foreach (var file in files)
            {
                if (Filter(file)) continue;

                string addressableName = Path.GetFileName(file);
                string assetName = XBuildUtility.GetPorjectPath(file);
                ArrayUtility.Add<string>(ref addressableNames, addressableName);
                ArrayUtility.Add<string>(ref assetNames, assetName);
            }

            if (addressableNames.Length > 0)
                outList.Add(XBuildUtility.CreateAssetBundleBuild(assetBundleName, addressableNames, assetNames));
        }
    }

    //清除无效的文件
    public static void ClearUnnecessary(string path, AssetBundleBuild[] builds)
    {
        HashSet<string> exist = new HashSet<string>();
        foreach (var item in builds)
            exist.Add(item.assetBundleName);

        string[] files = Directory.GetFiles(path, "*");
        string dirName = Path.GetFileName(path);
        foreach (var item in files)
        {
            string fileExt = Path.GetExtension(item);
            string fileName = Path.GetFileName(item);
            if (fileExt == ".txt" || fileExt == ".manifest" || fileName == dirName)
                continue;

            if (!exist.Contains(fileName))
                File.Delete(item);
        }
    }

    public static bool BuildWriteInfo(
        List<AssetBundleBuild> list,
        string outPath,
        BuildAssetBundleOptions options,
        BuildTarget target,
        bool isClearFolder,
        BuildResourceParameter.NameType buildBundleName,
        string version)
    {
        //buildBundleName = BuildResourceParameter.NameType.NONE;


        //版本文件 格式为  本地svn库版本号 编辑日期
        string versionPath = Path.Combine(outPath, "version.txt");
        //文件映射关系
        string fileORMPath = Path.Combine(outPath, "fileorm.txt");

        if (File.Exists(versionPath))
            File.Delete(versionPath);

        if (File.Exists(fileORMPath))
            File.Delete(fileORMPath);

        if (isClearFolder)
        {
            if (Directory.Exists(outPath))
                Directory.Delete(outPath, true);
        }

        if (!Directory.Exists(outPath))
            Directory.CreateDirectory(outPath);

        AssetBundleBuild[] builds = list.ToArray();

        List<string> hashs = new List<string>();

        //先收集清单
        AssetBundleManifest manifest = null;
        try
        {
            manifest = BuildPipeline.BuildAssetBundles(outPath, builds, options | BuildAssetBundleOptions.DryRunBuild, target);
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
            //EditorApplication.Exit(1);

            return false;
        }


        if (manifest == null)
        {
            //EditorApplication.Exit(1);
            Debug.LogError("manifest is null");

            return false;
        }


        for (int i = 0; i < builds.Length; i++)
        {
            string hash = manifest.GetAssetBundleHash(builds[i].assetBundleName).ToString();
            hashs.Add(hash);
            //纯哈希命名重定包名
            if (buildBundleName == BuildResourceParameter.NameType.HASH)
                builds[i].assetBundleName = hash;
            else if (buildBundleName == BuildResourceParameter.NameType.HASH_APPEND)
                builds[i].assetBundleName += "_" + hash;
        }



        manifest = BuildPipeline.BuildAssetBundles(outPath, builds, options, target);

        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

        AssetsFileOrm.CreateFileOrm(fileORMPath, list, builds, hashs);

        sw.Stop();
        Debug.Log("XBuildUtility:BuildWriteInfo CreateFileOrm time: " + sw.ElapsedMilliseconds * 0.001f);


        ClearUnnecessary(outPath, builds);

        File.WriteAllText(versionPath, version);


        return true;
    }


    ////其它文件的属性不在此配置
    static Dictionary<string, XAssetsFiles.FileOptions> DefaultOptions = new Dictionary<string, XAssetsFiles.FileOptions>()
    {
        {"00/00000000000000000000000000000000.asset",XAssetsFiles.FileOptions.LUA | XAssetsFiles.FileOptions.LAUNCHDOWNLOAD},
        {"00/00000000000000000000000000000001.asset",XAssetsFiles.FileOptions.DLL| XAssetsFiles.FileOptions.LAUNCHDOWNLOAD},
        {"00/00000000000000000000000000000002_arm64-v8a.asset",XAssetsFiles.FileOptions.DLL| XAssetsFiles.FileOptions.LAUNCHDOWNLOAD},
        {"00/00000000000000000000000000000002_armeabi-v7a.asset",XAssetsFiles.FileOptions.DLL| XAssetsFiles.FileOptions.LAUNCHDOWNLOAD},
        {"00/00000000000000000000000000000002_x86.asset",XAssetsFiles.FileOptions.DLL| XAssetsFiles.FileOptions.LAUNCHDOWNLOAD},
        {"00/00000000000000000000000000000002_arm64-v8a_debug.asset",XAssetsFiles.FileOptions.DLL| XAssetsFiles.FileOptions.LAUNCHDOWNLOAD},
        {"00/00000000000000000000000000000002_armeabi-v7a_debug.asset",XAssetsFiles.FileOptions.DLL| XAssetsFiles.FileOptions.LAUNCHDOWNLOAD},
        {"00/00000000000000000000000000000002_x86_debug.asset",XAssetsFiles.FileOptions.DLL| XAssetsFiles.FileOptions.LAUNCHDOWNLOAD},

    };

    //文件列表
    static string BuildFileList(string outPath)
    {

        List<string> buildingFile = new List<string>();
        //配置文件，记录了文件的属性及分包等... 
        string settingPath = Path.Combine(outPath, "files.setting");
        XAssetsFiles settingFiles = null;
        if (File.Exists(settingPath))
            settingFiles = JsonUtility.FromJson<XAssetsFiles>(File.ReadAllText(settingPath));

        XAssetsFiles xAssetsFiles = new XAssetsFiles();
        xAssetsFiles.p_AllFiles = new List<XAssetsFiles.FileStruct>();

        string[] folders = Directory.GetDirectories(outPath);
        foreach (var folder in folders)
        {
            string folderName = Path.GetFileName(folder);

            //文件映射表
            string fileormPath = Path.Combine(folder, "fileorm.txt");
            Dictionary<string, string> fileNameToBundleName = null;
            Dictionary<string, List<string>> bunldeNameToAssetList = null;
            if (File.Exists(fileormPath))
            {
                AssetsFileOrm.FileOrm fileorm = AssetsFileOrm.FileOrm.Load(fileormPath);

                fileNameToBundleName = new Dictionary<string, string>();
                bunldeNameToAssetList = new Dictionary<string, List<string>>();
                foreach (var item in fileorm.p_AssetBundleList)
                    if (!fileNameToBundleName.ContainsKey(item.p_AssetHashBundleName))
                    {
                        fileNameToBundleName.Add(item.p_AssetHashBundleName, item.p_AssetHashBundleName);
                        List<string> fileList = new List<string>();
                        for (int i = 0; i < item.p_Assets.Count; i++)
                        {
                            fileList.Add(item.p_Assets[i].p_AssetPath);
                        }
                        bunldeNameToAssetList[item.p_AssetHashBundleName] = fileList;
                    }
            }


            string[] files = Directory.GetFiles(folder, "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                string fileName = file.Replace(folder + "\\", "");//Path.Get
                                                                  //FileName(file);
                string ext = Path.GetExtension(file);
                if (ext == ".txt" || ext == ".manifest" || fileName == folderName)
                {
                    continue;
                }

                XAssetsFiles.FileStruct fs = new XAssetsFiles.FileStruct();
                fs.path = Path.Combine(folderName, fileName).Replace("\\", "/");
                fs.md5 = fileMD5(file);
                fs.size = fileSize(file);


                //使用设置的包标识
                if (settingFiles != null)
                {
                    string bname = string.Empty;
                    fileName = fileName.Replace("\\", "/");
                    if (fileNameToBundleName != null && fileNameToBundleName.TryGetValue(fileName, out bname))
                    {
                        XAssetsFiles.FileStruct settingFs;
                        if (settingFiles.allFilesMap.TryGetValue(bname, out settingFs) || settingFiles.allFilesMap.TryGetValue(fs.path, out settingFs))
                        {
                            if ((settingFs.options & XAssetsFiles.FileOptions.BUILDING) == XAssetsFiles.FileOptions.BUILDING)
                            {
                                List<string> fileList = bunldeNameToAssetList[fileName];
                                for (int i = 0; i < fileList.Count; i++)
                                {
                                    string exName = Path.GetExtension(fileList[i]);
                                    if (exName != ".txt")
                                    {
                                        buildingFile.Add(fileList[i]);
                                    }
                                }
                            }
                            fs.options = settingFs.options;
                            fs.tag = settingFs.tag;
                            fs.priority = settingFs.priority;
                        }
                    }
                }
                else if (DefaultOptions.ContainsKey(fs.path))
                {
                    fs.options = DefaultOptions[fs.path];
                }

                xAssetsFiles.p_AllFiles.Add(fs);
            }
        }
        string filesPath = Path.Combine(outPath, "files.txt");
        File.WriteAllText(filesPath, JsonUtility.ToJson(xAssetsFiles, true));
        string buildingPath = Path.Combine(outPath, "buildingFiles.txt");
        File.WriteAllLines(buildingPath, buildingFile.ToArray());
        return filesPath;
    }

    //版本文件
    static string BuildVersionFile(string outPath)
    {
        XVersionFile version = new XVersionFile();
        string[] versions = Directory.GetFiles(outPath, "version.txt", SearchOption.AllDirectories);
        foreach (var versionPath in versions)
        {
            string folderName = Path.GetFileName(Path.GetDirectoryName(versionPath));

            string[] lines = File.ReadAllLines(versionPath);
            if (lines.Length >= 2)
            {
                string svnVer = lines[0];
                string date = lines[1];

                /**if (folderName == "00")
                    version.p_LuaVersion = new XVersionFile.VersionStruct { svnVer = svnVer, buildDate = date };
                else**/
                if (folderName == "01")
                    version.p_DevVersion = new XVersionFile.VersionStruct { svnVer = svnVer, buildDate = date };
                else if (folderName == "02")
                    version.p_ArtVersion = new XVersionFile.VersionStruct { svnVer = svnVer, buildDate = date };
            }
        }


        version.p_manifest_md5 = fileMD5(Path.Combine(outPath, "xassetmanifest"));
        version.p_files_md5 = fileMD5(Path.Combine(outPath, "files"));

        string filesPath = Path.Combine(outPath, "version.txt");
        File.WriteAllText(filesPath, JsonUtility.ToJson(version, true));

        return filesPath;
    }

    static AssetBundleBuild[] BuildVersionCtrlFile(string outPath)
    {
        string flist = BuildFileList(outPath);
        string[] buildFiles = new string[] { flist };
        AssetBundleBuild[] abbs = new AssetBundleBuild[0];
        foreach (var item in buildFiles)
        {
            string projectFullPath = Path.Combine(Application.dataPath, Path.GetFileName(item));
            string projectPath = GetPorjectPath(projectFullPath);
            if (File.Exists(item))
            {
                if (File.Exists(projectFullPath))
                    File.Delete(projectFullPath);
                FileUtil.CopyFileOrDirectory(item, projectFullPath);
            }

            string assetBundleName = projectPath;
            string addressableName = Path.GetFileNameWithoutExtension(projectPath);
            string assetName = projectPath;
            ArrayUtility.Add<AssetBundleBuild>(ref abbs, CreateAssetBundleBuild(assetBundleName, new string[] { addressableName }, new string[] { assetName }, false));
        }

        AssetDatabase.Refresh();

        return abbs;
    }

    //清单文件 版本文件 文件列表
    public static void BuildAssetManifest(string outPath, BuildTarget target)
    {
        AssetBundleBuild[] abbs = BuildVersionCtrlFile(outPath);

        XAssetManifest manifest = ScriptableObject.CreateInstance<XAssetManifest>();
        manifest.EditorInitManifest(outPath);
        string projectPath = "Assets/XAssetManifest.asset";
        AssetDatabase.CreateAsset(manifest, projectPath);


        string[] addressableNames = new string[] { Path.GetFileNameWithoutExtension(projectPath) };
        string[] assetNames = new string[] { projectPath };
        string assetBundleName = projectPath;

        ArrayUtility.Add<AssetBundleBuild>(ref abbs, CreateAssetBundleBuild(assetBundleName, addressableNames, assetNames, false));

        for (int i = 0; i < abbs.Length; i++)
            abbs[i].assetBundleName = Path.GetFileNameWithoutExtension(abbs[i].assetBundleName);

        ////清单文件极限压缩
        BuildAssetBundleOptions options = BuildAssetBundleOptions.None | BuildAssetBundleOptions.ForceRebuildAssetBundle;
        //BuildAssetBundleOptions options = BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.ForceRebuildAssetBundle;
        BuildPipeline.BuildAssetBundles(outPath, abbs, options, target);

        string[] deletePaths = new string[0];
        foreach (var item in abbs)
        {
            string assetProjName = item.assetNames[0];
            string buidName = item.assetBundleName;
            if (assetProjName != projectPath)
                AssetDatabase.DeleteAsset(item.assetNames[0]);

            string mfName = Path.Combine(outPath, buidName + ".manifest");
            ArrayUtility.Add<string>(ref deletePaths, mfName);
        }


        ArrayUtility.Add<string>(ref deletePaths, Path.Combine(outPath, Path.GetFileName(outPath)));
        ArrayUtility.Add<string>(ref deletePaths, Path.Combine(outPath, Path.GetFileName(outPath) + ".manifest"));

        foreach (var path in deletePaths)
            if (File.Exists(path))
                File.Delete(path);

        AssetDatabase.Refresh();

        string manifestJson = JsonUtility.ToJson(manifest);
        string xassetmanifestJsonPath = Path.Combine(outPath, "xassetmanifest.json");
        File.WriteAllText(xassetmanifestJsonPath, manifestJson);


        BuildVersionFile(outPath);
    }

    public static byte byteLength = 4;
    static List<byte> randomByteList = new List<byte>();
    static bool CheckIsByteList(byte[] bytes)
    {
        return (bytes[byteLength] == 0x55
            && bytes[byteLength + 1] == 0x6E
            && bytes[byteLength + 2] == 0x69
            && bytes[byteLength + 3] == 0x74
            && bytes[byteLength + 4] == 0x79
            && bytes[byteLength + 5] == 0x46
            && bytes[byteLength + 6] == 0x53);
    }

    public static void BuildByteList(List<AssetBundleBuild> list, string outPath)
    {
        foreach (var item in list)
        {
            string path = Path.Combine(outPath, item.assetBundleName);

            if (File.Exists(path))
            {
                byte[] bytes = File.ReadAllBytes(path);

                if (!CheckIsByteList(bytes))
                {
                    List<byte> byteList = new List<byte>(bytes);

                    randomByteList.Clear();

                    for (byte i = 0; i < XBuildUtility.byteLength; i++)
                    {
                        byte b = (byte)(UnityEngine.Random.Range(0.0f, 1.0f) * (float)byte.MaxValue);
                        randomByteList.Add(b);
                    }

                    byteList.InsertRange(0, randomByteList);

                    File.WriteAllBytes(path, byteList.ToArray());
                }
            }
        }

    }

    public static void BuildDifferAssetInfo(string fpath, List<XAssetsFiles.FileStruct> showFiles)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var item in showFiles)
        {
            sb.AppendFormat("{0},{1},{2}\n", item.path, EditorUtility.FormatBytes(item.size), item.size);
        }

        StreamWriter sw = new StreamWriter(@fpath, false);
        sw.Write(sb.ToString());
        sw.Close();
        sw.Dispose();
    }












    public static void BuildDepAssetInfo(string fpath, List<XAssetsFiles.FileStruct> showFiles, XAssetManifest manifest)
    {
        StreamWriter sw = new StreamWriter(fpath, false);
        sw.WriteLine("路径,类型,引用检查,大小,大小/B,引用文件数");
        foreach (var item in showFiles)
        {
            string path = item.path;
            string sizeF = EditorUtility.FormatBytes(item.size);
            string size = item.size.ToString();
            string typeStr = GetTypeStrByPath(path);
            string[] deps = manifest.GetAllDependencies(path);
            string deperr = GetDepCheck(path, manifest.GetAllDependencies(path));
            sw.WriteLine("{0},{1},{2},{3},{4},{5}", path, typeStr, deperr, sizeF, size, deps.Length);
        }


        sw.Close();
        sw.Dispose();
    }

    public static string GetDepCheck(string path, string[] deps)
    {
        string result = "正常";
        if (deps.Length < 1) return result;

        if (path.StartsWith("02/art/effect"))
        {
            //特效检查错误依赖
            foreach (var item in deps)
            {
                //着色器是共用不管
                if (item.StartsWith("02/shaders"))
                    continue;

                //引用了非特效目录下的内容
                if (!item.StartsWith("02/art/effect"))
                    return "异常";

                //引用了公共目录的资源跳过
                if (item.Contains("com"))
                    continue;

                return "异常";
            }
        }
        else if (path.StartsWith("02/art/charactars"))
        {
            //角色模型检查
            foreach (var item in deps)
            {
                //着色器是共用不管
                if (item.StartsWith("02/shaders"))
                    continue;

                //引用了非角色目录下的内容
                if (!item.StartsWith("02/art/charactars"))
                    return "异常";

                //引用了公共目录的资源跳过
                if (item.Contains("com"))
                    continue;

                if (item.Contains("02/art/charactars/animatorcontrollers"))
                    continue;

                if (item.Contains("02/art/charactars/matcap"))
                    continue;

                if (item.Contains("02/art/charactars/npc/npccommon.asset"))
                    continue;

                if (item.Contains("02/art/charactars/player/anims_new/z_common.asset"))
                    continue;

                return "异常";
            }
        }
        else
        {
            result = "未检查";
        }
        return result;
    }

    public static string GetTypeStrByPath(string path)
    {
        string typeStr = "None";

        if (path.StartsWith("02/art/effect/"))
        {
            typeStr = "特效";
        }
        else if (path.StartsWith("02/art/env/"))
        {
            typeStr = "场景";

        }
        else if (path.StartsWith("02/art/charactars"))
        {
            typeStr = "角色";

        }
        else if (path.StartsWith("02/art/uiavatar"))
        {
            typeStr = "界面展示背景";

        }
        else if (path.StartsWith("02/art/story"))
        {
            typeStr = "剧情";

        }
        else if (path.StartsWith("02/art/video"))
        {
            typeStr = "视频";

        }
        else if (path.StartsWith("02/art/audio"))
        {
            typeStr = "音频";

        }
        else if (path.StartsWith("02/shaders"))
        {
            typeStr = "着色器";

        }
        return typeStr;
    }
}

