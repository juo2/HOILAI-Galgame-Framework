using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class XBuildCommandLine
{
    static Dictionary<string, string> GetInputCommand(string[] args)
    {
        Dictionary<string, string> result = new Dictionary<string, string>();
        string key = string.Empty;
        foreach (var item in args)
        {
            if (item.StartsWith("-"))
            {
                key = item.Trim();
            }
            else if (!string.IsNullOrEmpty(item) && !string.IsNullOrEmpty(key))
            {
                result.Add(key, item.Trim());
            }
        }
        return result;
    }


    public static void BuildDevDll()
    {
        EditorApplication.ExecuteMenuItem("XLua/Generate Code");
    }

    // 用于确定文件是否为支持的资源类型
    private static bool IsSupportedType(string filePath)
    {
        // 示例：仅导入图片和模型文件
        // 可以根据需要调整条件
        string extension = Path.GetExtension(filePath).ToLower();
        switch (extension)
        {
            case ".png":
            case ".jpg":
            case ".jpeg":
            case ".tga":
            case ".fbx":
            case ".obj":
                return true;
            default:
                return false;
        }
    }


    //[MenuItem("Tools/Reimport Directory")]
    public static void BuildReImport()
    {
        ImportDirectory(Path.Combine(Application.dataPath, "GUI/Modules/wyyglzj/Images/Single"));
    }

    private static void ImportDirectory(string path)
    {
        // 获取目录下的所有文件（包括子目录中的文件）
        string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);

        foreach (string file in files)
        {
            // 将绝对路径转换为相对于项目的Assets目录的路径
            string assetPath = file.Replace("\\", "/").Replace(Application.dataPath, "Assets");

            // 可以在这里添加更多的过滤条件，比如只导入特定类型的文件
            if (IsSupportedType(assetPath))
            {
                // 导入资产
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.Default);
                Debug.Log($"Reimported: {assetPath}");
            }
        }

        // 刷新AssetDatabase以确保所有变更都已应用
        AssetDatabase.Refresh();
    }

    public static void BuildDevProject()
    {
        Dictionary<string, string> command = GetInputCommand(System.Environment.GetCommandLineArgs());
        Debug.Log("command Length: " + command.Count);
        foreach (var item in command)
            Debug.LogFormat("key:[{0}]  value:[{1}]", item.Key, item.Value);

        string outPath = command.ContainsKey("-buildPath") ? command["-buildPath"] : Path.Combine(Application.dataPath, "../A_Build/");
        bool isClear = command.ContainsKey("-isClear") && command["-isClear"] == "true" ? true : false;

        BuildTarget target = BuildTarget.Android;
        if (command.ContainsKey("-platform"))
        {
            try
            {
                target = (BuildTarget)System.Enum.Parse(typeof(BuildTarget), command["-platform"]);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e);
            }
        }

        //切换平台
        SwitchActiveBuildTarget(target);

        //        if (isUpdateCSharp || isUpdateOther)
        //        {
        //            if (target == BuildTarget.Android)
        //            {
        //                //打个apk
        //                XBuildPlayer.BuildPlayer(new XBuildPlayer.BuildPlayerOpt { buildTarget = BuildTarget.Android, isUpdateCSharp = isUpdateCSharp });
        //            }
        //            else if (target == BuildTarget.StandaloneWindows)
        //            {
        //                //打个pc包
        //                XBuildPlayer.BuildPlayer(new XBuildPlayer.BuildPlayerOpt { buildTarget = BuildTarget.StandaloneWindows, isDevelopment = false, pc7zArchive = true });
        //            }
        //        }
        bool result = false;

        BuildResourceParameter parameter = new BuildResourceParameter();
        parameter.buildTarget = target;

        parameter.buildBundleName = BuildResourceParameter.NameType.NONE;
        parameter.buildAssetBundleOptions = BuildAssetBundleOptions.ChunkBasedCompression;
        parameter.isClearFolder = isClear;
        parameter.outputPath = outPath;
        result = XBuildDevelopment.Build(parameter);
        if (!result) EditorApplication.Exit(1);
        EditorSettings.spritePackerMode = SpritePackerMode.Disabled;
        string manifestPath = Path.Combine(outPath, XBuildUtility.GetPlatformAtBuildTarget(target));
        XBuildUtility.BuildAssetManifest(manifestPath, target);
    }


    public static void BuildArtProject()
    {
        Dictionary<string, string> command = GetInputCommand(System.Environment.GetCommandLineArgs());
        Debug.Log("command Length: " + command.Count);
        foreach (var item in command)
            Debug.LogFormat("key:[{0}]  value:[{1}]", item.Key, item.Value);

        string outPath = command.ContainsKey("-buildPath") ? command["-buildPath"] : Path.Combine(Application.dataPath, "../A_Build/");
        bool isClear = command.ContainsKey("-isClear") && command["-isClear"] == "true" ? true : false;
        BuildTarget target = BuildTarget.Android;
        if (command.ContainsKey("-platform"))
        {
            try
            {
                target = (BuildTarget)System.Enum.Parse(typeof(BuildTarget), command["-platform"]);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e);
            }
        }


        //切换平台
        SwitchActiveBuildTarget(target);

        BuildResourceParameter parameter = new BuildResourceParameter();
        parameter.version = command.ContainsKey("-version") ? command["-version"] : string.Empty;
        parameter.buildTarget = target;
#if UNITY_IOS
        parameter.buildBundleName = BuildResourceParameter.NameType.HASH;
#else
        parameter.buildBundleName = BuildResourceParameter.NameType.NONE;
#endif
        parameter.buildAssetBundleOptions = BuildAssetBundleOptions.ChunkBasedCompression;

        if (command.ContainsKey("-forceRebuild") && command["-platform"] == "true")
        {
            parameter.buildAssetBundleOptions |= BuildAssetBundleOptions.ForceRebuildAssetBundle;
        }



        parameter.isClearFolder = isClear;
        parameter.outputPath = outPath;
        //bool result = XBuildArt.Build(parameter);
        //if (!result) EditorApplication.Exit(1);
    }

    public static void BuildOrmAndCompare()
    {
        Dictionary<string, string> command = GetInputCommand(System.Environment.GetCommandLineArgs());
        Debug.Log("command Length: " + command.Count);
        foreach (var item in command)
            Debug.LogFormat("key:[{0}]  value:[{1}]", item.Key, item.Value);

        string buildTarget = EditorUserBuildSettings.activeBuildTarget.ToString();
        Debug.Log("Editor activeBuildTarget: " + buildTarget);
        BuildTarget target = BuildTarget.Android;
        if (command.ContainsKey("-platform") && buildTarget.Contains(command["-platform"]))
        {
            try
            {
                target = (BuildTarget)System.Enum.Parse(typeof(BuildTarget), command["-platform"]);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e);
                EditorApplication.Exit(1);
            }
        }
        else
        {
            Debug.LogWarning("error: seting platform Exception, Check whether the corresponding platforms are consistent !!!");
            EditorApplication.Exit(1);
        }

        string targetName = XBuildUtility.GetPlatformAtBuildTarget(target);
        string targetPath = command.ContainsKey("-targetPath") ? @command["-targetPath"] : string.Empty;
        Debug.Log("targetPath:" + targetPath);
        if (string.IsNullOrEmpty(targetPath) || !Directory.Exists(targetPath))
        {
            Debug.LogWarning("error: Folder does not exist !!!");
            EditorApplication.Exit(1);
        }

        string webUrl = command.ContainsKey("-webUrl") ? @command["-webUrl"] : string.Empty;
        Debug.Log("webPath:" + webUrl);
        bool isSaveCsv = command.ContainsKey("-isSaveCsv") && command["-isSaveCsv"] == "true" ? true : false;
        Debug.Log("是否保存Build列表:" + isSaveCsv);

        BuildOrmAndPushParameter parameter = new BuildOrmAndPushParameter();
        parameter.buildTarget = target;
        parameter.ormPath = targetPath;
        parameter.webPath = webUrl;
        parameter.isSaveCsv = isSaveCsv;

        Debug.Log("build step 1: 刷新首包");
        EditorSettings.spritePackerMode = SpritePackerMode.Disabled;
        XBuildUtility.BuildAssetManifest(targetPath, parameter.buildTarget);

        Debug.Log("build step 2: 启动更新差异文件列表");
        AssetBundleCompare.OneKeyUpdate(parameter);
    }
    private static void SwitchActiveBuildTarget(BuildTarget target)
    {
        BuildTargetGroup buildTargetGroup = BuildTargetGroup.Standalone;
        if (target == BuildTarget.Android)
        {
            buildTargetGroup = BuildTargetGroup.Android;
        }
        else if (target == BuildTarget.StandaloneWindows)
        {
            buildTargetGroup = BuildTargetGroup.Standalone;
        }
        else if (target == BuildTarget.iOS)
        {
            buildTargetGroup = BuildTargetGroup.iOS;
        }
        EditorUserBuildSettings.SwitchActiveBuildTarget(buildTargetGroup, target);
    }
}