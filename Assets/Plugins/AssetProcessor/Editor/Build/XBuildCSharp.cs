using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using UnityEditor.SceneManagement;
using HybridCLR.Editor.Commands;

/// <summary>
/// 打包CSharp
/// </summary>
public class XBuildCSharp
{ 
 
    static string s_outputNameCSharp = "00000000000000000000000000000001.asset";
    
    static AssetBundleBuild CollectionCSharpAssetBundleBuilds(BuildCSharpParameter buildCSharpStruct)
    {
        CompileDllCommand.CompileDll(buildCSharpStruct.buildTarget);

        Thread.Sleep(100);

        AssetBundleBuild abb = new AssetBundleBuild();
        abb.assetBundleName = s_outputNameCSharp;
        abb.assetNames = new string[0];
        abb.addressableNames = new string[0];

        AssetDatabase.Refresh();

        string[] dllNames = new string[] { "HotUpdate.dll"};

        foreach (var dllName in dllNames)
        {
            string dllPath = Path.Combine(Application.dataPath, string.Format("../HybridCLRData/HotUpdateDlls/{0}/{1}", buildCSharpStruct.buildTarget, dllName));

            Debug.Log($"HybridCLRData dllPath:{dllPath}");

            if (File.Exists(dllPath))
            {
                string projectPath = string.Format("Assets/{0}", dllName.Replace(".dll", ".bytes"));
                string targetPath = XBuildUtility.GetFullPath(projectPath);
                AssetDatabase.DeleteAsset(projectPath);

                FileUtil.CopyFileOrDirectory(dllPath, targetPath);

                byte[] bytes = File.ReadAllBytes(targetPath);

                //if (buildLuaStruct.buildTarget != BuildTarget.StandaloneWindows)
                //{
                //    //77,90,144
                //    bytes[0] = System.Convert.ToByte('X');
                //    bytes[1] = System.Convert.ToByte('X');
                //    bytes[2] = System.Convert.ToByte('X');
                //}

                Thread.Sleep(100);

                File.WriteAllBytes(targetPath, bytes);
                ArrayUtility.Add<string>(ref abb.assetNames, projectPath);
                ArrayUtility.Add<string>(ref abb.addressableNames, dllName);

                //out put
                string outputPath = Path.Combine(buildCSharpStruct.outputPath, string.Format("{0}/", XBuildUtility.GetPlatformAtBuildTarget(buildCSharpStruct.buildTarget)));
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

    public static bool Build(BuildCSharpParameter parameter)
    {
        UnityEditor.EditorSettings.spritePackerMode = SpritePackerMode.Disabled;

        //获取本地svn库版本
        string version = XBuildUtility.GetGitCommitID();

        //去掉加载时间ab包内的后缀名
        if ((parameter.buildAssetBundleOptions & BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension) !=
            BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension)
            parameter.buildAssetBundleOptions |= BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension;

        List<AssetBundleBuild> list;
        //AssetBundleBuild lua = CollectionAssetBundleBuilds(parameter);
        AssetBundleBuild abbCsharp = CollectionCSharpAssetBundleBuilds(parameter);
        {
            list = new List<AssetBundleBuild>() { abbCsharp };
        }

        parameter.outputPath = !string.IsNullOrEmpty(parameter.outputPath) ? parameter.outputPath : Path.Combine(Application.dataPath, "../A_Build/");
        string outputPath = Path.Combine(parameter.outputPath, string.Format("{0}/00", XBuildUtility.GetPlatformAtBuildTarget(parameter.buildTarget)));
        parameter.outputPath = outputPath;

        bool result = XBuildUtility.BuildWriteInfo(list, outputPath, parameter.buildAssetBundleOptions, parameter.buildTarget,
            parameter.isClearFolder, BuildResourceParameter.NameType.NONE, version);

        AssetDatabase.Refresh();

        return result;

    }

    [MenuItem("Tools/buildCSharp")]
    static void SBuild()
    {
        string version = XBuildUtility.GetGitCommitID();
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