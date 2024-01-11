using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

public class XBuildPlayer
{
    public static string s_scenePath = "Assets/LauncherGame.unity";
    public static BuildPlayerOpt s_opt;
    public class BuildPlayerOpt : ScriptableObject
    {
        public BuildTarget buildTarget = BuildTarget.Android;
        public string outPath = "out";
        public bool isDevelopment = true;
        public bool isExportProject = false;
        public bool isOnlyScript = false;
        public bool pc7zArchive = false;
        public bool isOnlyMono = false;
        public bool isUpdateCSharp = false;
    }

    static void CopyPlatformAssets(BuildTarget platform)
    {
        string path = string.Format("../A_PlatformStreamingAssets/{0}", platform.ToString());
        string fullPath = Path.Combine(Application.dataPath, path);
        if (!Directory.Exists(fullPath))
        {
            Debug.Log("XBuildPlayer::CopyPlatformAssets. path not exist " + fullPath);
            return;
        }

        string[] files = Directory.GetFiles(fullPath, "*", SearchOption.AllDirectories);
        foreach (var filePath in files)
        {
            string rpath = filePath.Substring(fullPath.Length + 1);
            string tpath = Path.Combine(Application.streamingAssetsPath, rpath);

            if (!Directory.Exists(tpath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(tpath));
            }

            FileUtil.ReplaceFile(filePath, tpath);
        }
    }


    public static void BuildPlayer(BuildPlayerOpt opt)
    {
        EditorApplication.ExecuteMenuItem("XLua/Generate Code");

        BuildPlayerOptions options = new BuildPlayerOptions();
        
        CopyPlatformAssets(opt.buildTarget);
        
        EditorSettings.spritePackerMode = SpritePackerMode.Disabled;

        if (opt.buildTarget == BuildTarget.Android && !opt.isOnlyMono)
        {
            //XAndroidBuilder.BuildWithoutPatch(opt);
        }

        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);

        options.locationPathName = opt.isExportProject ? opt.outPath : opt.outPath + ".apk";
        if (opt.buildTarget == BuildTarget.StandaloneWindows)
        {
            options.locationPathName = string.Format("out_StandaloneWindows/{0}.exe", Application.productName);
        }

        options.target = opt.buildTarget;

        if (options.target == BuildTarget.Android)
        {
            options.targetGroup = BuildTargetGroup.Android;
        }
        if (options.target == BuildTarget.StandaloneWindows)
        {
            options.targetGroup = BuildTargetGroup.Standalone;
        }
        else if (options.target == BuildTarget.iOS)
        {
            options.targetGroup = BuildTargetGroup.iOS;
        }

        if (opt.isDevelopment)
        {
            options.options |= BuildOptions.Development;
        }

        if (opt.isExportProject)
            options.options |= BuildOptions.AcceptExternalModificationsToPlayer;


        if (opt.isOnlyScript)
            options.options |= BuildOptions.BuildScriptsOnly;

        options.scenes = new[] { s_scenePath };

        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;

        BuildPipeline.BuildPlayer(options);

        if (!opt.isUpdateCSharp)
        {
            AssetDatabase.DeleteAsset(s_scenePath);

            EditorApplication.ExecuteMenuItem("XLua/Clear Generated Code");
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        }
      
        if (options.target == BuildTarget.StandaloneWindows && opt.pc7zArchive)
        {
            ProcessStartInfo sinfo = new ProcessStartInfo();
            sinfo.FileName = ".\\A_Tools\\7z.exe";
            sinfo.UseShellExecute = false;
            sinfo.CreateNoWindow = true;
            sinfo.WorkingDirectory = Path.Combine(Application.dataPath, "../");
            sinfo.Arguments = string.Format("a -t7z {0}.7z {1}*  -r -mx=9 -m0=LZMA2 -ms=10m -mf=on -mhc=on -mmt=on ", opt.outPath, Path.GetDirectoryName(options.locationPathName));
            Process.Start(sinfo);
        }
    }
}