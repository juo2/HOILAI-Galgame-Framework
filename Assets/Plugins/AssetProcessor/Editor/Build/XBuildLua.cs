using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using UnityEditor.SceneManagement;

/// <summary>
/// 打包Lua
/// </summary>
public class XBuildLua
{ 
 
    static string s_tempFolder = "Assets/X_Building_Lua";
    static string s_outputName = "00000000000000000000000000000000.asset";
    static string s_outputNameCSharp = "00000000000000000000000000000001.asset";
    static string s_outputNameIl2cpp = "00000000000000000000000000000002";
    static string s_il2cppName = "InitIl2cpp";
   
    static AssetBundleBuild CollectionCSharpAssetBundleBuilds(BuildLuaParameter buildLuaStruct)
    {
        AssetBundleBuild abb = new AssetBundleBuild();
        abb.assetBundleName = s_outputNameCSharp;
        abb.assetNames = new string[0];
        abb.addressableNames = new string[0];

        if (!buildLuaStruct.isUpdateCSharp)
        {
            return abb;
        }

        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);

        AssetDatabase.Refresh();

        string[] dllNames = new string[] { "Assembly-CSharp.dll", "Assembly-CSharp-firstpass.dll", "Unity.TextMeshPro.dll" };

        foreach (var dllName in dllNames)
        {
#if UNITY_2018 || UNITY_2018_OR_NEWER
            string dllPath = Path.Combine(Application.dataPath, string.Format("../Library/PlayerDataCache/Android/Data/Managed/{0}", dllName));
            if (buildLuaStruct.buildTarget == BuildTarget.StandaloneWindows)
            {
                dllPath = Path.Combine(Application.dataPath, string.Format("../Library/PlayerDataCache/Win/Data/Managed/{0}", dllName));
            }
#else
            string dllPath = Path.Combine(Application.dataPath, string.Format("../Temp/StagingArea/assets/bin/Data/Managed/{0}", dllName));
            if (buildLuaStruct.buildTarget == BuildTarget.StandaloneWindows)
            {
                dllPath = Path.Combine(Application.dataPath, string.Format("../Temp/StagingArea/{0}_Data/Managed/{1}", Application.productName, dllName));
            }
#endif

            if (dllName.Contains("Assembly-CSharp.dll"))
            {
                string str = Encoding.Default.GetString(File.ReadAllBytes(dllPath));

                if (str.Contains(s_il2cppName))
                {
                    Debug.LogError("这个dll是il2cpp的，这个包不打了!!!!!!!!!");
                    return abb;
                }
            }

            if (File.Exists(dllPath))
            {
                string projectPath = string.Format("Assets/{0}", dllName.Replace(".dll", ".bytes"));
                string targetPath = XBuildUtility.GetFullPath(projectPath);
                AssetDatabase.DeleteAsset(projectPath);

                FileUtil.CopyFileOrDirectory(dllPath, targetPath);

                byte[] bytes = File.ReadAllBytes(targetPath);

                if (buildLuaStruct.buildTarget != BuildTarget.StandaloneWindows)
                {
                    //77,90,144
                    bytes[0] = System.Convert.ToByte('X');
                    bytes[1] = System.Convert.ToByte('X');
                    bytes[2] = System.Convert.ToByte('X');
                }

                Thread.Sleep(100);

                File.WriteAllBytes(targetPath, bytes);
                ArrayUtility.Add<string>(ref abb.assetNames, projectPath);
                ArrayUtility.Add<string>(ref abb.addressableNames, dllName);

                //out put
                string outputPath = Path.Combine(buildLuaStruct.outputPath, string.Format("{0}/", XBuildUtility.GetPlatformAtBuildTarget(buildLuaStruct.buildTarget)));
                outputPath = Path.Combine(outputPath, dllName);
                if (File.Exists(outputPath)) File.Delete(outputPath);
                string dirPath = Path.GetDirectoryName(outputPath);
                if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
                File.WriteAllBytes(outputPath, bytes);
            }
        }

        AssetDatabase.Refresh();
        return abb;
    }


    //每次都要unity导入资源比较慢
    static AssetBundleBuild CollectionAssetBundleBuilds(BuildLuaParameter buildLuaStruct)
    {
        if (AssetDatabase.IsValidFolder(s_tempFolder))
            AssetDatabase.DeleteAsset(s_tempFolder);

        string dataPath = Application.dataPath;
        string projectFolder = dataPath;
        projectFolder = projectFolder.Substring(0, projectFolder.Length - 6);
        projectFolder = Path.Combine(projectFolder, s_tempFolder);

        //配置
        AssetBundleBuild abb_luaConfig = new AssetBundleBuild();
        abb_luaConfig.assetBundleName = string.IsNullOrEmpty(buildLuaStruct.fileName) ? s_outputName : buildLuaStruct.fileName;
        abb_luaConfig.assetNames = new string[0];
        abb_luaConfig.addressableNames = new string[0];

        if (!buildLuaStruct.isUpdateLua)
        {
            return abb_luaConfig;
        }

        //代码
        AssetBundleBuild abb_luaCode = new AssetBundleBuild();
        abb_luaCode.assetBundleName = "tempcode.txt";
        abb_luaCode.assetNames = new string[0];
        abb_luaCode.addressableNames = new string[0];

        string[] allLuas = Directory.GetFiles(buildLuaStruct.luaDirectory, "*.lua", SearchOption.AllDirectories);
        int sidx = buildLuaStruct.luaDirectory.Length;
        int psidx = dataPath.Length - 6;
        List<string> luaCodes = new List<string>();
        List<string> luaCfgs = new List<string>();
        foreach (var file in allLuas)
        {
            //将文件从lua工程目录拷贝到Unity工程中进行打包
            string relative = file.Substring(sidx);
            string projectPath = Path.Combine(projectFolder, relative);
            projectPath = projectPath.Replace(Path.GetExtension(projectPath), ".txt");
            string dirPath = Path.GetDirectoryName(projectPath);

            string relativeRp = relative.Replace("\\", "/");
            //game目录下的才打包
            if (!relativeRp.StartsWith("game/")) continue;

            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            FileUtil.CopyFileOrDirectory(file, projectPath);

            string assetPath = projectPath.Substring(psidx).Replace("\\", "/");
            string addressPath = assetPath.Substring(s_tempFolder.Length + 1).Replace("/", ".");
            bool isLuaConfig = relativeRp.StartsWith("game/config");
            if (isLuaConfig)
            {
                ////dbbackup目录下的不打包
                //bool isDbbackup = relativeRp.StartsWith("game/config/dbbackup");
                //if (!isDbbackup)
                //{
                    luaCfgs.Add(projectPath);
                    ArrayUtility.Add<string>(ref abb_luaConfig.assetNames, assetPath);
                    ArrayUtility.Add<string>(ref abb_luaConfig.addressableNames, addressPath);
                //}                
            }
            else
            {
                luaCodes.Add(projectPath);
                ArrayUtility.Add<string>(ref abb_luaCode.assetNames, assetPath);
                ArrayUtility.Add<string>(ref abb_luaCode.addressableNames, addressPath);
            }
        }

        //将代码先打个包
        BuildAssetBundleOptions bbo = buildLuaStruct.buildAssetBundleOptions;
        if ((bbo & BuildAssetBundleOptions.UncompressedAssetBundle) == BuildAssetBundleOptions.UncompressedAssetBundle)
            bbo |= ~BuildAssetBundleOptions.UncompressedAssetBundle;
        else if ((bbo & BuildAssetBundleOptions.ChunkBasedCompression) != BuildAssetBundleOptions.ChunkBasedCompression)
            bbo |= BuildAssetBundleOptions.ChunkBasedCompression;


        //序列化文件
        string sopath = Path.Combine(dataPath, "Scripts/SerializedObjects");
        string[] allsoFiles = Directory.GetFiles(sopath, "*.asset");
        foreach (var file in allsoFiles)
        {
            string path = file.Replace("\\", "/");
            string assetPath = FileUtil.GetProjectRelativePath(path);

            ArrayUtility.Add<string>(ref abb_luaCode.assetNames, assetPath);
            ArrayUtility.Add<string>(ref abb_luaCode.addressableNames, assetPath);
        }

        AssetDatabase.Refresh();


        string tempOutPath = string.Format("{0}/{1}/00/temp", buildLuaStruct.outputPath, XBuildUtility.GetPlatformAtBuildTarget(buildLuaStruct.buildTarget));
        if (Directory.Exists(tempOutPath))
            Directory.Delete(tempOutPath, true);
        Directory.CreateDirectory(tempOutPath);
        Debug.LogFormat("gen luacode {0}", tempOutPath);
        BuildPipeline.BuildAssetBundles(tempOutPath, new AssetBundleBuild[] { abb_luaCode }, bbo, buildLuaStruct.buildTarget);
        string tempcodePath = tempOutPath + "/" + abb_luaCode.assetBundleName;
        if (File.Exists(tempcodePath))
        {
            byte[] bytes = File.ReadAllBytes(tempcodePath);
            bytes[0] = (byte)'X'; //U
            bytes[1] = (byte)'G'; //n
            bytes[2] = (byte)'A'; //i
            bytes[3] = (byte)'M'; //t
            bytes[4] = (byte)'E'; //y
            bytes[5] = (byte)'X'; //F
            bytes[6] = (byte)'X'; //S
            File.WriteAllBytes(tempcodePath, bytes);
            string luacodeppath = Path.Combine(projectFolder, abb_luaCode.assetBundleName);
            FileUtil.CopyFileOrDirectory(tempcodePath, luacodeppath);
            FileUtil.DeleteFileOrDirectory(tempOutPath);

            luacodeppath = FileUtil.GetProjectRelativePath(luacodeppath).Replace("\\", "/");
            ArrayUtility.Add<string>(ref abb_luaConfig.assetNames, luacodeppath);
            ArrayUtility.Add<string>(ref abb_luaConfig.addressableNames, abb_luaCode.assetBundleName);

        }
        else
            Debug.LogErrorFormat("luacode not exist! {0}", tempcodePath);

        AssetDatabase.Refresh();
        return abb_luaConfig;
    }

    //将所有lua打成一个字节文件 //启动时gc太大
    static AssetBundleBuild CollectionAssetBundleBuildsEx(BuildLuaParameter buildLuaStruct)
    {
        if (AssetDatabase.IsValidFolder(s_tempFolder))
            AssetDatabase.DeleteAsset(s_tempFolder);



        string dataPath = Application.dataPath;
        string projectFolder = dataPath;
        projectFolder = projectFolder.Substring(0, projectFolder.Length - 6);
        projectFolder = Path.Combine(projectFolder, s_tempFolder);

        if (!Directory.Exists(projectFolder))
            Directory.CreateDirectory(projectFolder);

        AssetBundleBuild abb = new AssetBundleBuild();
        abb.assetBundleName = string.IsNullOrEmpty(buildLuaStruct.fileName) ? s_outputName : buildLuaStruct.fileName;
        abb.assetNames = new string[0];
        abb.addressableNames = new string[0];

        string[] allLuas = Directory.GetFiles(buildLuaStruct.luaDirectory, "*.lua", SearchOption.AllDirectories);
        int sidx = buildLuaStruct.luaDirectory.Length;
        int psidx = dataPath.Length - 6;


        MemoryStream luaBytes = new MemoryStream();
        foreach (var file in allLuas)
        {
            string relative = file.Substring(sidx);
            string projectPath = Path.Combine(projectFolder, relative);
            projectPath = projectPath.Replace(Path.GetExtension(projectPath), ".txt");
            string assetPath = projectPath.Substring(psidx).Replace("\\", "/");
            string addressPath = assetPath.Substring(s_tempFolder.Length + 1).Replace("/", ".");
            luaBytes.Write(System.BitConverter.GetBytes(addressPath.Length), 0, 2);

            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(addressPath);
            luaBytes.Write(bytes, 0, bytes.Length);
            bytes = File.ReadAllBytes(file);
            luaBytes.Write(System.BitConverter.GetBytes(bytes.Length), 0, 4);
            luaBytes.Write(bytes, 0, bytes.Length);

        }


        string fullBytesPath = Path.Combine(projectFolder, "code.bytes");
        string projectBytesPath = fullBytesPath.Substring(psidx).Replace("\\", "/");

        File.WriteAllBytes(fullBytesPath, luaBytes.ToArray());


        ArrayUtility.Add<string>(ref abb.assetNames, projectBytesPath);
        ArrayUtility.Add<string>(ref abb.addressableNames, "code");

        AssetDatabase.Refresh();


        return abb;
    }

    public static bool Build(BuildLuaParameter parameter)
    {
        UnityEditor.EditorSettings.spritePackerMode = SpritePackerMode.Disabled;

        //获取本地svn库版本
        string version = XBuildUtility.GetSvnVersion(parameter.version);

        if (string.IsNullOrEmpty(parameter.luaDirectory))
        {
            Debug.LogError("BuildLua.Build buildLuaStruct.luaDirectory IsNullOrEmpty");
            return false;
        }

        if (!Directory.Exists(parameter.luaDirectory))
        {
            Debug.LogErrorFormat("BuildLua.Build Lua 工程目录不存在 luaDirectory=\"{0}\"", parameter.luaDirectory);
            return false;
        }

        //去掉加载时间ab包内的后缀名
        if ((parameter.buildAssetBundleOptions & BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension) !=
            BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension)
            parameter.buildAssetBundleOptions |= BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension;

        List<AssetBundleBuild> list;
        AssetBundleBuild lua = CollectionAssetBundleBuilds(parameter);

#if UNITY_IOS
        list = new List<AssetBundleBuild>() { lua };
#else
        //AssetBundleBuild abbCsharp = CollectionCSharpAssetBundleBuilds(parameter);

        {
            list = new List<AssetBundleBuild>() { lua };
        }
#endif

        parameter.outputPath = !string.IsNullOrEmpty(parameter.outputPath) ? parameter.outputPath : Path.Combine(Application.dataPath, "../A_Build/");
        string outputPath = Path.Combine(parameter.outputPath, string.Format("{0}/00", XBuildUtility.GetPlatformAtBuildTarget(parameter.buildTarget)));
        parameter.outputPath = outputPath;

        bool result = XBuildUtility.BuildWriteInfo(list, outputPath, parameter.buildAssetBundleOptions, parameter.buildTarget,
            parameter.isClearFolder, BuildResourceParameter.NameType.NONE, version);

        //bool result = true;

        if (AssetDatabase.IsValidFolder(s_tempFolder))
            AssetDatabase.DeleteAsset(s_tempFolder);

        //AssetDatabase.DeleteAsset(s_CSharpProPath);

        AssetDatabase.Refresh();

        return result;

    }

    [MenuItem("Tools/buildLua")]
    static void SBuild()
    {
        string version = XBuildUtility.GetSvnVersion(string.Empty);
        Debug.Log(version);
    }


    //[MenuItem("Tools/loadab")]
    //static void LoadAB()
    //{
    //    string path = Application.dataPath.Substring(0, Application.dataPath.Length - 6);
    //    path = Path.Combine(path, s_outputName);

    //    AssetBundle ab = AssetBundle.LoadFromFile(path);
    //    TextAsset asset = ab.LoadAsset<TextAsset>("game.main");

    //    Debug.Log(asset);

    //    ab.Unload(false);
    //}

}