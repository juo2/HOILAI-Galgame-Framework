
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Text;

public class UpdateUtility
{
    //解析版本文件
    public static XVersionFile DeVersion(AssetBundle assetBundle, string path)
    {
        XVersionFile version = null;
        TextAsset textAsset = assetBundle.LoadAsset<TextAsset>("version");
        if (textAsset == null || string.IsNullOrEmpty(textAsset.text))
        {
            XLogger.ERROR(string.Format("LaunchUpdate::CheckVersion() 版本文件损坏 ! path={0}", path));
        }
        else
        {
            version = JsonUtility.FromJson<XVersionFile>(textAsset.text);
            Debug.Log(textAsset.text);
            Resources.UnloadAsset(textAsset);
        }

        assetBundle.Unload(true);
        return version;
    }

    public static XVersionFile DeVersion(string data, string path)
    {
        XVersionFile version = null;
        version = JsonUtility.FromJson<XVersionFile>(data);
        return version;
    }

    public static XAssetsFiles DeFileList(string data)
    {
        XAssetsFiles assetFiles = null;
        assetFiles = JsonUtility.FromJson<XAssetsFiles>(data);
        return assetFiles;
    }

    public static string ReadAssetList(AssetBundle ab)
    {
        if (ab == null)
            return null;

        string data = string.Empty;
        if (ab != null)
        {
            TextAsset textAsset = ab.LoadAsset<TextAsset>(ab.GetAllAssetNames()[0]);
            if (textAsset != null)
            {
                data = textAsset.text;
                Resources.UnloadAsset(textAsset);
            }
            ab.Unload(true);
        }
        return data;
    }

    public static IEnumerator ReadAssetListAsync(AssetBundle ab, Action<string> callback)
    {
        if (ab == null)
            yield break;

        string data = string.Empty;
        if (ab != null)
        {
            AssetBundleRequest abr = ab.LoadAssetAsync<TextAsset>(ab.GetAllAssetNames()[0]);
            yield return abr;
            TextAsset textAsset = abr.asset as TextAsset;
            if (textAsset != null)
            {
                data = textAsset.text;
                Resources.UnloadAsset(textAsset);
            }
            ab.Unload(true);
            callback.Invoke(data);
            yield return null;
        }
    }

    public static string GetVersionStrInfo(XVersionFile version)
    {
        return string.Format("Dev:{0} files:{1} manifest:{2}",
            version.p_DevVersion.gitVer,
            version.p_files_md5, version.p_manifest_md5);
    }


    //初始化Dll
    public static void InitDll()
    {

//#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_STANDALONE_WIN)
//        string path = "00/00000000000000000000000000000001.asset";
//        path = Path.Combine(AssetManagement.AssetDefine.ExternalSDCardsPath, path);
//        if (File.Exists(path))
//        {
//            try
//            {
//                AssetBundle ab = AssetBundle.LoadFromFile(path);
//                TextAsset[] texts = ab.LoadAllAssets<TextAsset>();

//                string dllPath = AssetManagement.AssetDefine.DataDataPath;

//#if UNITY_STANDALONE_WIN
//                dllPath = Path.Combine(dllPath, "Managed");
//#endif
//                foreach (var text in texts)
//                {
//                    string savePath = Path.Combine(dllPath, string.Format("{0}.dll", text.name));
//                    XFileUtility.WriteBytes(savePath, text.bytes);
//                    Resources.UnloadAsset(text);
//                }
//                Debug.Log("正在使用mono");
//                ab.Unload(true);

//                XMobileUtility.RestartApplication();
//            }
//            catch (System.Exception e)
//            {
//                XLogger.ERROR(string.Format("UpdateUtility::InitDll error:{0}", e.ToString()));
//            }
//        }
//#endif
    }

#if ENABLE_IL2CPP && UNITY_ANDROID
    //初始化il2cpp
    public static bool InitIl2cpp()
    {
//        string debugStr = "";
//#if DEVELOPMENT_BUILD
//        debugStr = "_debug";
//#endif
//        string path = string.Format("00/00000000000000000000000000000002_{0}{1}.asset", Bootstrap.get_arch_abi(), debugStr);
//        path = Path.Combine(AssetManagement.AssetDefine.ExternalSDCardsPath, path);
//        if (File.Exists(path))
//        {
//            try
//            {
//                AssetBundle ab = AssetBundle.LoadFromFile(path);
//                TextAsset[] texts = ab.LoadAllAssets<TextAsset>();

//                string dllPath = AssetManagement.AssetDefine.DataDataPath;

//                foreach (var text in texts)
//                {
//                    Debug.Log(string.Format("AssetManagement.AssetDefine.RuntimePatchPathFormat={0}", AssetManagement.AssetDefine.RuntimePatchPath));
                    
//                    Debug.Log("解压第一步 111111111111111111");
//                    try
//                    {
//                        ZipHelper.UnZipBytes(text.bytes, AssetManagement.AssetDefine.RuntimePatchPath, "", true);
//                    }
//                    catch (Exception e)
//                    {
//                        Debug.LogError(e.Message);

//                        TimerManager.AddCoroutine(ResetIl2cpp(e.Message));
//                        return true;
//                    }

//                    string zipLibil2cppPath = AssetManagement.AssetDefine.RuntimePatchPath + "/lib_" + Bootstrap.get_arch_abi() + "_libil2cpp.so.zip";

//                    Debug.Log(string.Format("zipLibil2cppPath={0}", zipLibil2cppPath));

//                    Debug.Log("解压第二步 22222222222222222");
//                    try
//                    {
//                        ZipHelper.UnZip(zipLibil2cppPath, AssetManagement.AssetDefine.RuntimePatchPath, "", true);
//                    }
//                    catch (Exception e)
//                    {
//                        Debug.LogError(e.Message);

//                        TimerManager.AddCoroutine(ResetIl2cpp(e.Message));

//                        return true;
//                    }

//                    //重启框提前，小米11pro会报错
//                    if (SystemInfo.deviceModel == "Xiaomi M2012K11AC" || SystemInfo.deviceModel == "Xiaomi M2102K1AC")
//                    {
//                        TimerManager.AddCoroutine(CloseApplication());
//                    }

//                    string apkPath = "";
//                    string error = Bootstrap.use_data_dir(AssetManagement.AssetDefine.RuntimePatchPath, apkPath);
//                    Debug.Log("use_data_dir第三步 33333333333333333");
//                    if (!string.IsNullOrEmpty(error))
//                    {
//                        Debug.LogError(error);

//                        //TimerManager.AddCoroutine(ResetIl2cpp(error));
//                        //要是没有il2cpp java代码，也别卡住
//                        return false;
//                    }

//                    string cacheDir = Application.persistentDataPath + "/il2cpp";

//                    Debug.Log(string.Format("cacheDir={0}", cacheDir));

//                    if (Directory.Exists(cacheDir))
//                    {
//                        DeleteDirectory(cacheDir);
//                    }
//                    else
//                    {
//                        Debug.LogError("pre Unity cached file not found.path:" + cacheDir);
//                        return true;
//                    }
//                }
//                Debug.Log("正在使用il2cpp");

//                ab.Unload(true);

//                //重启框提前，小米11pro会报错
//                if (SystemInfo.deviceModel == "Xiaomi M2012K11AC" || SystemInfo.deviceModel == "Xiaomi M2102K1AC")
//                {
//                    //TimerManager.AddCoroutine(CloseApplication());
//                }
//                else
//                {
//                    XMobileUtility.RestartApplication();
//                }

//                return false;
//            }
//            catch (System.Exception e)
//            {
//                XLogger.ERROR(string.Format("UpdateUtility::InitDll error:{0}", e.ToString()));
//            }
//        }
//        else
//        {
//            Debug.LogError(path + " -- 不存在 InitIl2cpp");
//        }

        return true;
    }
#endif

    public static void CheckIl2cpp() 
    {
//#if ENABLE_IL2CPP && UNITY_ANDROID
//        string debugStr = "";
//#if DEVELOPMENT_BUILD
//        debugStr = "_debug";
//#endif
//        string path = string.Format("00/00000000000000000000000000000002_{0}{1}.asset", Bootstrap.get_arch_abi(), debugStr);
//        path = Path.Combine(AssetManagement.AssetDefine.ExternalSDCardsPath, path);
//        if (!File.Exists(path))
//        {
//            Debug.Log("没有目录  00/00000000000000000000000000000002.asset");
//            return;
//        }
        
//        //检测文件完整性
//        if (!Directory.Exists(AssetManagement.AssetDefine.RuntimePatchPath))
//        {
//            Debug.Log("il2cpp AssetManagement.AssetDefine.RuntimePatchPath目录为空");
//            //重新解压
//            TimerManager.AddCoroutine(ResetIl2cpp("il2cpp AssetManagement.AssetDefine.RuntimePatchPath目录为空"));

//            return;
//        }
//        else
//        {
//            string il2cppPath = Path.Combine(AssetManagement.AssetDefine.RuntimePatchPath, "libil2cpp.so");
//            if (File.Exists(il2cppPath))
//            {
//                //旧版本
//                //string versionPath = Path.Combine(AssetManagement.AssetDefine.RuntimePatchPath, "version.txt");
//                //if(File.Exists(versionPath))
//                //{
//                //    string version = File.ReadAllText(versionPath);

//                //    string []strArray1 = version.Split('\n');
//                //    string []strArray2 = XAssetsFiles.s_00Version.Split('\n');

//                //    Debug.Log("version:" + version);
//                //    Debug.Log("XAssetsFiles.s_00Version:" + XAssetsFiles.s_00Version);

//                //    if (strArray1.Length > 1 && strArray2.Length > 1)
//                //    {
//                //        if (strArray1[0] == strArray2[0])
//                //        {
//                //            Debug.Log("il2cpp 版本号对上了,没问题！！！！");
//                //        }
//                //        else
//                //        {
//                //            Debug.LogError("il2cpp 版本号对不上");

//                //            TimerManager.AddCoroutine(ResetIl2cpp("il2cpp 版本号对不上"));

//                //            return;
//                //        }
//                //    }
//                //    else
//                //    {
//                //        Debug.LogError("il2cpp version格式不对");

//                //        TimerManager.AddCoroutine(ResetIl2cpp("il2cpp version格式不对"));

//                //        return;
//                //    }

//                //}
//                //else
//                //{
//                //    Debug.LogError("il2cpp version 文件不存在");

//                //    TimerManager.AddCoroutine(ResetIl2cpp("il2cpp version 文件不存在"));

//                //    return;
//                //}

//                //新版本
//                string []tempStrs = XAssetsFiles.s_il2cppMd5.Split('\n');
//                foreach(string str in tempStrs)
//                {
//                    string []strArray = str.Split('=');
//                    string md5Path = Path.Combine(AssetManagement.AssetDefine.RuntimePatchPath, strArray[0]);
//                    if (File.Exists(md5Path))
//                    {
//                        if(XFileUtility.FileMd5(md5Path).Equals(strArray[1]))
//                        {
//                            //pass
//                        }
//                        else
//                        {
//                            string errstr = string.Format("md5对不上: path = {0},fileMd5 = {1},md5 = {2}", md5Path, XFileUtility.FileMd5(md5Path), strArray[1]);
//                            Debug.LogError(errstr);
//                            TimerManager.AddCoroutine(ResetIl2cpp(errstr));
//                            break;
//                        }
//                    }
//                    else
//                    {
//                        string errstr = "路径不存在:" + md5Path;
//                        Debug.LogError(errstr);
//                        TimerManager.AddCoroutine(ResetIl2cpp(errstr));
//                        break;
//                    }
//                }
//            }
//            else
//            {
//                Debug.Log("il2cpp il2cppPath目录为空");
//                TimerManager.AddCoroutine(ResetIl2cpp("il2cpp il2cppPath目录为空"));
//                return;
//            }
//        }
//#endif
    }

    static IEnumerator ResetIl2cpp(string msg)
    {
        string content = string.Format(UpdateConst.GetLanguage(13013), msg);
        string title = UpdateConst.GetLanguage(11301);
        string sureStr = UpdateConst.GetLanguage(11304);
        DefaultAlertGUI alert = DefaultAlertGUI.Open(title, content, sureStr, "", DefaultAlertGUI.ButtonOpt.Sure);
        
        yield return alert.Wait();

        //删掉
        DeleteDirectory(AssetManagement.AssetDefine.RuntimePatchPath);

#if ENABLE_IL2CPP && UNITY_ANDROID
        //重新解压
        UpdateUtility.InitIl2cpp();
#endif
    }

    public static void DeleteDirectory(string target_dir)
    {
        try
        {
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(target_dir, false);
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
    }
    public static IEnumerator CloseApplication()
    {
        string title = UpdateConst.GetLanguage(11301);
        string content = string.Format(UpdateConst.GetLanguage(13012));

        DefaultAlertGUI alert = DefaultAlertGUI.Open(title, content, UpdateConst.GetLanguage(11206), "", DefaultAlertGUI.ButtonOpt.Sure);
        yield return alert.Wait();

        //Application.Quit();
        Quit();
    }

    public static void Quit()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        using (AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity"))
        {
            activity.Call("finish");
            using (AndroidJavaClass process = new AndroidJavaClass("android.os.Process"))
            {
                process.CallStatic("killProcess", process.CallStatic<int>("myPid"));
                process.Dispose();
            }

            activity.Dispose();
        }
#endif
    }

    public static string ReadBuildinAssetList(string path)
    {
        string error = string.Empty;
        return ReadAssetList(XFileUtility.ReadStreamingAssetBundle(path, out error));
    }


    public static void SetUIVersionInfo(XVersionFile local, XVersionFile server)
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendFormat("Dev   {0}", Application.version);
        if (local != null)
        {
            sb.AppendFormat(".{0}", local.p_DevVersion.gitVer);
            if (server != null && server.p_DevVersion.gitVer != local.p_DevVersion.gitVer)
                sb.AppendFormat("<color=#00ff00> - {0}</color>", server.p_DevVersion.gitVer);
        }

        //if (XConfig.defaultConfig != null && !XConfig.defaultConfig.isGetUrlByPHP)
        //{
        //    string curVer = "";
        //    string url = XConfig.defaultConfig.testDownloadUrls;
        //    sb.Append("\n");
        //    sb.AppendFormat("Ver    {0}版本", curVer);
        //}

        DefaultLoaderGUI.SetVerText(sb.ToString());
    }

    public static void ShowTextTip(string str)
    {
        SystemTipGUI.ShowTip(str);
    }
}
