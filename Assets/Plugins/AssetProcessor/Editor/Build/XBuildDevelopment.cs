using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// 打包程序开发工程
/// </summary>
public class XBuildDevelopment
{
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

    static void CollectionFile(List<AssetBundleBuild> outList, string projectPath)
    {
        string assetBundleName = Path.Combine(Path.GetDirectoryName(projectPath), Path.GetFileNameWithoutExtension(projectPath)).Replace("\\", "/");
        string addressableName = Path.GetFileName(projectPath);
        string assetName = projectPath;
        outList.Add(XBuildUtility.CreateAssetBundleBuild(assetBundleName, new string[] { addressableName }, new string[] { assetName }));
    }

    static void CollectionFolderAll(List<AssetBundleBuild> outList, string projectPath, bool isAlone = false)
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

    static void CollectionCharactars(List<AssetBundleBuild> outList)
    {
        string path = XBuildUtility.GetFullPath("Assets/Art/Charactars");
        if (!Directory.Exists(path))
            return;
        string[] folders = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
        foreach (var folder in folders)
        {
            if (Filter(folder)) continue;
            string folderName = Path.GetFileName(folder);
            if (folderName == "Player")
            {
                //Assets/Art/Charactars/Player/../
                string[] childFolders = Directory.GetDirectories(folder);
                foreach (var childFolder in childFolders)
                {
                    string childFolderName = Path.GetFileName(childFolder);
                    foreach (var item in Directory.GetDirectories(childFolder))
                    {
                        foreach (var item2 in Directory.GetDirectories(item))
                            CollectionFolderAll(outList, XBuildUtility.GetPorjectPath(item2), false);
                    }
                }
            }
            else if (folderName == "Skeleton" || folderName == "Decal")
            {
                //Assets/Art/Charactars/../../
                CollectionFolderAll(outList, XBuildUtility.GetPorjectPath(folder), false);
            }
            else
            {
                //Assets/Art/Charactars/../
                string[] childFolders = Directory.GetDirectories(folder);
                foreach (var childFolder in childFolders)
                    CollectionFolderAll(outList, XBuildUtility.GetPorjectPath(childFolder), false);
            }

        }
    }

    /// <summary>
    /// UI模型
    /// </summary>
    /// <param name="outList"></param>
    static void CollectionUIAvatar(List<AssetBundleBuild> outList)
    {
        string path = XBuildUtility.GetFullPath("Assets/Art/UIAvatar");
        if (!Directory.Exists(path))
            return;
        string[] folders = Directory.GetDirectories(path, "*");

        foreach (var folder in folders)
        {
            if (Filter(folder)) continue;
            CollectionFolderAll(outList, XBuildUtility.GetPorjectPath(folder));
        }
    }

    /// <summary>
    /// 视频
    /// </summary>
    /// <param name="outList"></param>
    static void CollectionVideo(List<AssetBundleBuild> outList)
    {
        string path = XBuildUtility.GetFullPath("Assets/Art/Video");
        if (!Directory.Exists(path))
            return;
        string[] files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
        if (files.Length < 1)
            return;

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
    

    /// <summary>
    /// 场景
    /// </summary>
    /// <param name="outList"></param>
    static void CollectionEnvironment(List<AssetBundleBuild> outList)
    {
        string path = XBuildUtility.GetFullPath("Assets/Art/Scenes");
        if (!Directory.Exists(path))
            return;
        string[] folders = Directory.GetDirectories(path, "*");
        foreach (var folder in folders)
        {
            //Scenes
            if (Filter(folder)) continue;

            string sfolderName = Path.GetFileName(folder);
            string sceneName = string.Empty;

            //Scenes/*.x
            string[] childFiles = Directory.GetFiles(folder, "*");
            foreach (var childFile in childFiles)
            {
                if (!Filter(childFile))
                {
                    if (childFile.EndsWith(".unity"))
                    {
                        sceneName = Path.GetFileNameWithoutExtension(childFile);

                        //如果场景存在流场景则不打包此场景
                        string streamScene = Path.Combine(Path.GetDirectoryName(childFile), sceneName + "_stream.unity");
                        if (File.Exists(streamScene))
                            continue;
                    }
                    CollectionFile(outList, XBuildUtility.GetPorjectPath(childFile));
                }
            }


            //Scenes/../
            string[] childFolders = Directory.GetDirectories(folder, "*");
            foreach (var childFolder in childFolders)
            {
                string folderName = Path.GetFileName(childFolder);
                string[] chils = Directory.GetFiles(childFolder);
                bool isSceneLightData = false;

                //检查这个目录是否是灯光数据目录则不打包
                foreach (var item in chils)
                {
                    if (item.Contains("LightingData.asset") || item.Contains("NavMesh.asset") || item.Contains(".exr"))
                    {
                        isSceneLightData = true;
                        break;
                    }
                }

                if (!Filter(childFolder) && !isSceneLightData)
                {
                    CollectionFolderAll(outList, XBuildUtility.GetPorjectPath(childFolder));
                }
            }
        }
    }

    /// <summary>
    /// 声音
    /// </summary>
    /// <param name="outList"></param>
    static void CollectionAudio(List<AssetBundleBuild> outList)
    {
        CollectionFolderAll(outList, "Assets/Art/Audio", true);
    }

    static void CollectionShaders(List<AssetBundleBuild> outList)
    {
        CollectionFolderAll(outList, "Assets/Shaders");
    }

    static void CollectionAnimation(string moduleName, string materialsFolder, List<AssetBundleBuild> outList)
    {
        XBuildUtility.CollectionFolder(outList, XBuildUtility.GetPorjectPath(materialsFolder));

    }

    /// <summary>
    /// 收集所有功能的脚本对象
    /// </summary>
    static void CollectionScrptObjects(string moduleName, string scriptsFolder, List<AssetBundleBuild> outList)
    {
        string moduleProjectPath = XBuildUtility.GetPorjectPath(Path.GetDirectoryName(scriptsFolder));

        string[] allScripts = Directory.GetFiles(scriptsFolder, "*.asset");

        if (allScripts.Length == 0) return;

        string assetBundleName = string.Format("{0}/ScriptObject_{1}", moduleProjectPath, moduleName);

        string[] addressableNames = new string[0];
        string[] assetNames = new string[0];
        //脚本对象打一个包
        foreach (var path in allScripts)
        {
            string fileName = Path.GetFileName(path);
            string peojectPath = XBuildUtility.GetPorjectPath(path);
            ArrayUtility.Add<string>(ref addressableNames, fileName);
            ArrayUtility.Add<string>(ref assetNames, peojectPath);
        }

        outList.Add(XBuildUtility.CreateAssetBundleBuild(assetBundleName, addressableNames, assetNames));
        //不支持子文件夹
    }

    /// <summary>
    /// 收集所有功能预置件
    /// </summary>
    /// <param name="outList"></param>
    static void CollectionPrefabs(string moduleName, string prefabFolder, List<AssetBundleBuild> outList)
    {

        string moduleProjectPath = XBuildUtility.GetPorjectPath(Path.GetDirectoryName(prefabFolder));

        string[] allPrefabs = Directory.GetFiles(prefabFolder, "*.prefab");
        //模块预置件  单独打
        foreach (var path in allPrefabs)
        {
            string fileName = Path.GetFileName(path);
            string peojectPath = XBuildUtility.GetPorjectPath(path);
            string assetBundleName = string.Format("{0}/Prefab_{1}", moduleProjectPath, Path.GetFileNameWithoutExtension(path));
            outList.Add(XBuildUtility.CreateAssetBundleBuild(assetBundleName, new string[] { fileName }, new string[] { peojectPath }));
        }



        string[] childFolders = Directory.GetDirectories(prefabFolder);
        if (childFolders != null && childFolders.Length > 0)
        {
            foreach (var folder in childFolders)
            {
                if (moduleName == "Common")
                {
                    //公共模块单独打
                    XBuildUtility.CollectionFolder(outList, XBuildUtility.GetPorjectPath(folder), true);
                }
                else
                {
                    //模块下的子目录一个目录打一个
                    XBuildUtility.CollectionFolder(outList, XBuildUtility.GetPorjectPath(folder));
                }
            }
        }
    }

    /// <summary>
    /// 收集所有图集  图片全是 png
    /// </summary>
    /// <param name="outList"></param>
    static void CollectionAtlas(string moduleName, string imagesFolder, List<AssetBundleBuild> outList, bool notContainAtlas = false)
    {
        string moduleProjectPath = XBuildUtility.GetPorjectPath(Path.GetDirectoryName(imagesFolder));
        //string imagesProjectPath = GetPorjectPath(imagesFolder);

        //Images 下的图片打成一个包 不包含子目录
        string[] topImgs = Directory.GetFiles(imagesFolder, "*.png");
        if (topImgs.Length > 0)
        {
            string[] addressableNames = new string[0];
            string[] assetNames = new string[0];
            string assetBundleName = string.Format("{0}/Atlas_{1}", moduleProjectPath, moduleName);

            //string spriteAtlasPath = string.Format("{0}/Atlas_{1}.spriteatlas", moduleProjectPath, moduleName);
            //string spriteAtlasFileName = Path.GetFileName(spriteAtlasPath);

            foreach (var imgPath in topImgs)
            {
                string fileName = Path.GetFileName(imgPath);
                string peojectPath = XBuildUtility.GetPorjectPath(imgPath);
                ArrayUtility.Add<string>(ref addressableNames, fileName);
                ArrayUtility.Add<string>(ref assetNames, peojectPath);
            }

            if (!notContainAtlas)
            {
                string spriteAtlasPath = string.Format("{0}/Atlas_{1}.spriteatlas", moduleProjectPath, moduleName);
                string spriteAtlasFileName = Path.GetFileName(spriteAtlasPath);

                ArrayUtility.Add<string>(ref addressableNames, spriteAtlasFileName);
                ArrayUtility.Add<string>(ref assetNames, spriteAtlasPath);
            }
            outList.Add(XBuildUtility.CreateAssetBundleBuild(assetBundleName, addressableNames, assetNames));
        }


        //Images 下的子目录
        string[] childFolders = Directory.GetDirectories(imagesFolder);
        foreach (var childFolder in childFolders)
        {
            string[] childImgs = Directory.GetFiles(childFolder, "*.png");
            string folderName = Path.GetFileName(childFolder);
            if (folderName.Contains("Single"))
            {
                //单张图片一个包
                foreach (var childImg in childImgs)
                {
                    string fileName = Path.GetFileName(childImg);
                    string assetBundleName = string.Format("{0}/Atlas_{1}_Single_{2}", moduleProjectPath, moduleName, Path.GetFileNameWithoutExtension(childImg));
                    string peojectPath = XBuildUtility.GetPorjectPath(childImg);
                    string spriteAtlasPath = string.Format("{0}/Atlas_{1}_Single_{2}.spriteatlas", Path.GetDirectoryName(peojectPath), moduleName, Path.GetFileNameWithoutExtension(childImg));
                    string spriteAtlasFileName = Path.GetFileName(spriteAtlasPath);
                    outList.Add(XBuildUtility.CreateAssetBundleBuild(assetBundleName, new string[] { fileName, spriteAtlasFileName }, new string[] { peojectPath, spriteAtlasPath }));
                }
            }
            else if (folderName.Contains("Sequence"))
            {
                //序列帧图集
                foreach (var childImg in childImgs)
                {
                    string fileName = Path.GetFileName(childImg);
                    string assetBundleName = string.Format("{0}/Atlas_{1}_Sequence_{2}", moduleProjectPath, moduleName, Path.GetFileNameWithoutExtension(childImg));
                    string peojectPath = XBuildUtility.GetPorjectPath(childImg);
                    outList.Add(XBuildUtility.CreateAssetBundleBuild(assetBundleName, new string[] { fileName }, new string[] { peojectPath }));
                }
            }
            else if (folderName.Contains("Split") && childImgs.Length > 0)
            {
                //全部碎图片一个包
                string[] addressableNames = new string[0];
                string[] assetNames = new string[0];
                string assetBundleName = string.Format("{0}/Split_{1}_{2}", moduleProjectPath, moduleName, folderName);
                foreach (var childImg in childImgs)
                {
                    string fileName = Path.GetFileName(childImg);
                    string peojectPath = XBuildUtility.GetPorjectPath(childImg);
                    ArrayUtility.Add<string>(ref addressableNames, fileName);
                    ArrayUtility.Add<string>(ref assetNames, peojectPath);
                }

                outList.Add(XBuildUtility.CreateAssetBundleBuild(assetBundleName, addressableNames, assetNames));
            }
            else if (childImgs.Length > 0)
            {
                //一个目录一个包
                string[] addressableNames = new string[0];
                string[] assetNames = new string[0];
                string assetBundleName = string.Format("{0}/Atlas_{1}_{2}", moduleProjectPath, moduleName, folderName);
                string spriteAtlasPath = string.Format("{0}/Atlas_{1}_{2}.spriteatlas", moduleProjectPath, moduleName, folderName);
                string spriteAtlasFileName = Path.GetFileName(spriteAtlasPath);

                foreach (var childImg in childImgs)
                {
                    string fileName = Path.GetFileName(childImg);
                    string peojectPath = XBuildUtility.GetPorjectPath(childImg);
                    ArrayUtility.Add<string>(ref addressableNames, fileName);
                    ArrayUtility.Add<string>(ref assetNames, peojectPath);
                }

                ArrayUtility.Add<string>(ref addressableNames, spriteAtlasFileName);
                ArrayUtility.Add<string>(ref assetNames, spriteAtlasPath);

                outList.Add(XBuildUtility.CreateAssetBundleBuild(assetBundleName, addressableNames, assetNames));
            }

            string[] gradsons = Directory.GetDirectories(childFolder);
            if (gradsons.Length > 0)
            {
                string lastFloaderName = Path.GetFileNameWithoutExtension(childFolder);
                for (int i = 0;i< gradsons.Length;i++)
                {
                    string gradsonPath = gradsons[i];
                    string gradSonFloader = Path.GetFileNameWithoutExtension(gradsonPath);
                    string ggMName = string.Format("{0}_{1}_{2}", moduleName, lastFloaderName, gradSonFloader);
                    CollectionAtlas(ggMName, gradsonPath, outList, notContainAtlas);//继续递归
                }
                
            }
        }
    }

    /// <summary>
    /// 特效
    /// </summary>
    /// <param name="outList"></param>
    /// <param name="buildin"></param>
    static void CollectionEffect(List<AssetBundleBuild> outList, string buildin = "Effect")
    {
        string path = XBuildUtility.GetFullPath("Assets/Art/" + buildin);
        if (!Directory.Exists(path))
            return;
        string[] folders = Directory.GetDirectories(path, "*");

        foreach (var folder in folders)
        {
            if (Filter(folder)) continue;

            //Effect/../
            string[] childFolders = Directory.GetDirectories(folder, "*");
            foreach (var childFolder in childFolders)
            {
                string[] tfolder = Directory.GetDirectories(childFolder, "*");
                //Effect/../../
                if (tfolder.Length > 0)
                {
                    foreach (var item in tfolder)
                    {
                        CollectionFolderAll(outList, XBuildUtility.GetPorjectPath(item));
                    }
                }
                else
                {
                    CollectionFolderAll(outList, XBuildUtility.GetPorjectPath(childFolder));
                }
            }
        }
    }

    static void CollectionModule(List<AssetBundleBuild> outList, bool notContainAtlas)
    {
        string moduleRoot = XBuildUtility.GetFullPath("Assets/GUI/Modules");
        if (!Directory.Exists(moduleRoot))
            return;
        
        string[] allModules = Directory.GetDirectories(XBuildUtility.GetFullPath("Assets/GUI/Modules"));
        foreach (var moduleFolder in allModules)
        {
            string moduleName = Path.GetFileNameWithoutExtension(moduleFolder);
            string imagesFolder = Path.Combine(moduleFolder, "Images");
            string prefabsFolder = Path.Combine(moduleFolder, "Prefabs");
            string scriptObjFolder = Path.Combine(moduleFolder, "ScriptObjs");
			string AnimationFolder = Path.Combine(moduleFolder, "Animation");

            if (Directory.Exists(imagesFolder))
                CollectionAtlas(moduleName, imagesFolder, outList, notContainAtlas);

            if (Directory.Exists(prefabsFolder))
                CollectionPrefabs(moduleName, prefabsFolder, outList);

            if (Directory.Exists(scriptObjFolder))
                CollectionScrptObjects(moduleName, scriptObjFolder, outList);
				
			if (Directory.Exists(AnimationFolder))
                CollectionAnimation(moduleName, AnimationFolder, outList);
				
        }
    }

    static void CollectionFont(List<AssetBundleBuild> outList, string folder = "Fonts")
    {
        string fontPath = "Assets/GUI/" + folder;
        string fontRoot = XBuildUtility.GetFullPath(fontPath);
        if (!Directory.Exists(fontRoot))
            return;
        XBuildUtility.CollectionFolder(outList, fontPath);
    }

    public static bool Build(BuildResourceParameter parameter)
    {
        bool notContainAtlas = false;
        if (parameter.buildTarget == BuildTarget.iOS)
        {
            EditorSettings.spritePackerMode = SpritePackerMode.Disabled;
            notContainAtlas = true;
        }
        else
        	EditorSettings.spritePackerMode = SpritePackerMode.BuildTimeOnlyAtlas;

        //获取本地svn库版本
        //string version = XBuildUtility.GetSvnVersion(parameter.version);

        //获取本地git版本
        string version = XBuildUtility.GetGitCommitID();


        List<AssetBundleBuild> list = new List<AssetBundleBuild>();

        //收集打包文件
        CollectionModule(list, notContainAtlas);
        //字体
        CollectionFont(list, "Fonts");
        //着色器
        CollectionShaders(list);
        //角色
        CollectionCharactars(list);
        //场景
        CollectionEnvironment(list);
        //UI模型
        CollectionUIAvatar(list);
        //声音
        CollectionAudio(list);
        //特效
        CollectionEffect(list);
        //视频
        CollectionVideo(list);

        if (list.Count < 1)
        {
            Debug.LogWarning("AssetBundleBuild list Count 0");
            return false;
        }
        //资源包存放路径
        string outPath = !string.IsNullOrEmpty(parameter.outputPath) ? parameter.outputPath : Path.Combine(Application.dataPath, "../A_Build/");
        string manifestPath = Path.Combine(outPath, XBuildUtility.GetPlatformAtBuildTarget(parameter.buildTarget));

        outPath = Path.Combine(outPath, string.Format("{0}/01", XBuildUtility.GetPlatformAtBuildTarget(parameter.buildTarget)));
        parameter.outputPath = outPath;

        parameter.buildBundleName = BuildResourceParameter.NameType.NONE;
//#if UNITY_IOS
//        parameter.buildBundleName = BuildResourceParameter.NameType.HASH;
//#endif
        bool result = XBuildUtility.BuildWriteInfo(list, outPath, parameter.buildAssetBundleOptions, parameter.buildTarget,
                                            parameter.isClearFolder, parameter.buildBundleName, version);

        //XBuildUtility.BuildByteList(list, outPath);

        XBuildUtility.BuildAssetManifest(manifestPath, parameter.buildTarget);

        return result;
    }
}