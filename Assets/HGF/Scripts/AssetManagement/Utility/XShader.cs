using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Profiling;
using Object = UnityEngine.Object;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class XShader : MonoBehaviour
{
    public const string SHADER_PATH_NAME = "Shaders";
    public const string SHADER_NAME_GREY = "X_Shader/C_Charactar/Grey";
    // (t/20, t, t*2, t*3)
    private static int _XTime_ID = Shader.PropertyToID("_XTime");
    private static Vector4 _XTimeV4 = Vector4.zero;

    private static Dictionary<string, Shader> s_Shaders = new Dictionary<string, Shader>(20);
    private static Dictionary<string, Material> s_Materials = new Dictionary<string, Material>(20);
    private static Dictionary<string, Texture> s_Textures = new Dictionary<string, Texture>(20);
    private static Dictionary<string, Object> s_Other = new Dictionary<string, Object>(20);
    public static bool isInitSuccessful = false;
    public static Action onShaderInitFinish;

    public static Dictionary<string, string> s_IgnoreInit = new Dictionary<string, string>
    {
        { "charactar-pbr-standard-xray.shader", "X_Shader/C_Charactar/PBR/Standard" },
        { "charactar-pbr-standard.shader", "X_Shader/C_Charactar/PBR/StandardXray" },
        { "scene-lit-blinnphong-rsky-weather-wind.shader", "X_Shader/B_Scene/Light BlinnPhong RSKY Weather Wind" },
        { "scene-lit-blinnphong-rsky-weather.shader", "X_Shader/B_Scene/Light BlinnPhong RSKY Weather" },
        { "scene-lit-blinnphong-rsky.shader", "X_Shader/B_Scene/Light BlinnPhong RSKY" },

        { "scene-lit-blinnphong.shader", "X_Shader/B_Scene/Light BlinnPhong" },
        { "scene-lit-blinnphong-rmc.shader", "X_Shader/B_Scene/Light BlinnPhong RMC" },

        { "scene-lit-blinnphong-terrain.shader", "X_Shader/B_Scene/Light BlinnPhong Terrain" },
        { "scene-lit-blinnphong-terrain-weather.shader", "X_Shader/B_Scene/Light BlinnPhong Terrain Weather" },


        { "x_shader_b_scene_light blinnphong rmc.mat", "x" },
        { "x_shader_b_scene_light blinnphong rsky weather wind.mat", "x" },
        { "x_shader_b_scene_light blinnphong rsky weather.mat", "x" },
        { "x_shader_b_scene_light blinnphong rsky.mat", "x" },
        { "x_shader_b_scene_light blinnphong.mat", "x" },

    };

    private static AssetBundle s_AssetBundle;

    IEnumerator Start()
    {
        yield return null;

#if UNITY_EDITOR
        XLogger.INFO("XShader::用本地shader");
        InitLocalShaders();
#else
        Debug.Log("Start");
        AssetManagement.AssetInternalLoader loader = AssetManagement.AssetUtility.LoadAsset<Shader>("Particles.shader");

        Debug.Log("Down load Particles.shader");

        if (loader == null)
        {
            XLogger.INFO("XShader::InitShaders loader is null");
        }
        else
        {

            Debug.Log($"Down load Particles.shader 11111 {loader}");
            
            yield return loader;

            Debug.Log($"Down load Particles.shader 22222 {loader.Error}");

            if (!string.IsNullOrEmpty(loader.Error))
            {
                XLogger.ERROR(string.Format("XShader::InitShaders  {0}", loader.Error));
                yield break;
            }


            yield return InitShaders(loader.XAssetBundle);
        }
#endif
    }

    void InitLocalShaders()
    {
#if UNITY_EDITOR
        string path = Path.Combine(Application.dataPath, SHADER_PATH_NAME);
        List<string> filePathList = new List<string>();
        XFileUtility.TraverseFolder(path, filePathList);

        foreach(string filePath in filePathList)
        {
            string obPath = filePath.Replace("\\","/").Replace(Application.dataPath, "Assets");
            Object ob = AssetDatabase.LoadAssetAtPath<Object>(obPath);
            InitShaders(ob);
        }
#endif
    }

    IEnumerator InitShaders(AssetManagement.XAssetBundle xAssetBundle)
    {

        Debug.Log("InitShaders start");

        if (xAssetBundle == null && xAssetBundle.Bundle == null)
        {
            XLogger.ERROR(string.Format("XShader::InitShaders {0}", xAssetBundle == null ? "xAssetBundle is null" : "xAssetBundle.Bundle is null"));
            yield break;
        }

        s_AssetBundle = xAssetBundle.Bundle;

        xAssetBundle.DestoryTime = -1; //永不卸载

        string[] all = xAssetBundle.Bundle.GetAllAssetNames();
        int finishCount = 0;
        for (int i = 0; i < all.Length; i++)
        {
            Debug.Log($"load all:{i}  ---  {all[i]}");

            if (s_IgnoreInit.ContainsKey(all[i])) { ++finishCount; continue; }
            AssetBundleRequest abr = xAssetBundle.Bundle.LoadAssetAsync<Object>(all[i]);
            abr.completed += (AsyncOperation async) =>
            {

                Debug.Log($"finish all:{i}  ---  {all[i]}");

                Profiler.BeginSample(abr.asset.name);
                InitShaders(abr.asset); ++finishCount;
                Profiler.EndSample();
            };
        }


        while (finishCount < all.Length)
        {
            yield return null;
        }

        isInitSuccessful = true;
        if (onShaderInitFinish != null) onShaderInitFinish.Invoke();

        XLogger.INFO_Format("Shaders finish");
    }

    private int count = 0;
    void InitShaders(Object shader)
    {
        string name = shader.name;
        if (shader is Shader)
        {
            if (s_Shaders.ContainsKey(name))
            {
                XLogger.ERROR(string.Format("XShader::InitShaders() already exist ! name={0}", name));
            }
            s_Shaders.Add(name, (Shader)shader);
        }
        else if (shader is Material)
        {
            if (s_Materials.ContainsKey(name))
            {
                XLogger.ERROR(string.Format("XShader::InitShaders() s_Materials already exist ! name={0}", name));
            }
            s_Materials.Add(name, (Material)shader);
        }
        else if (shader is Texture)
        {
            if (s_Textures.ContainsKey(name))
            {
                XLogger.ERROR(string.Format("XShader::InitShaders() s_Textures already exist ! name={0}", name));
            }
            s_Textures.Add(name, (Texture)shader);
        }
        else
        {
            if (s_Other.ContainsKey(name))
            {
                XLogger.ERROR(string.Format("XShader::InitShaders() s_Other already exist ! name={0}", name));
            }
            s_Other.Add(name, shader);
        }
    }

    public static Shader GetShader(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        if (!s_Shaders.ContainsKey(name) && s_AssetBundle)
        {
            string loadName = string.Empty;
            foreach (var item in s_IgnoreInit)
            {
                if (item.Value == name)
                {
                    loadName = item.Key;
                    break;
                }
            }

            if (string.IsNullOrEmpty(loadName))
            {
                //XLogger.DEBUG_Format("GetShader: {0} Null", name);
                return null;
            }

            Shader shader = s_AssetBundle.LoadAsset<Shader>(loadName);
            if (shader) s_Shaders.Add(name, shader);
        }

        return s_Shaders.ContainsKey(name) ? s_Shaders[name] : null;
    }

    public static Material GetMaterial(string name)
    {
        //XLogger.DEBUG_Format("GetMaterial: {0}", name);
        return !string.IsNullOrEmpty(name) && s_Materials.ContainsKey(name) ? s_Materials[name] : null;
    }

    public static Texture GetTexture(string name)
    {
        //XLogger.DEBUG_Format("GetTexture: {0}", name);
        return !string.IsNullOrEmpty(name) && s_Textures.ContainsKey(name) ? s_Textures[name] : null;
    }

    public static Object GetOther(string name)
    {
        //XLogger.DEBUG_Format("GetTexture: {0}", name);
        return !string.IsNullOrEmpty(name) && s_Other.ContainsKey(name) ? s_Other[name] : null;
    }

    void Update()
    {
        float t = Time.realtimeSinceStartup;
        _XTimeV4.Set(t / 20, t, t * 2, t * 3);
        Shader.SetGlobalVector(_XTime_ID, _XTimeV4);
    }
}
