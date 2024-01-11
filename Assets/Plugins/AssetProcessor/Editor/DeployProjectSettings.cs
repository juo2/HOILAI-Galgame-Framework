using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.Rendering;
using System.Collections.Generic;
using System.Collections;

public class DeployProjectSettings
{
    static string[] tags = { "MapCamera", "GUICamera", "SkipGate", "SceneTrigger" };
    static string[] layers = { "UIHide", "UIModel", "Wall", "Floor", "Entity", "FightEffect", "WaterSurface", "Particle_High", "Particle_Mid", "Particle_Low", "Particle_SHide", "ShadowsCast", "ShadowsReceive", "TreeModel", "SkyBox", "EntityHide", "GMEntityLayer", "SkyFloor", "JumpPoint", "GUI3DX", "Diamond", "Layer29", "Layer30", "Layer31" };

    static ArrayList navigation_areas = new ArrayList()
    {
        "Walkable",1f,
        "Not Walkable",1f,
        "Jump",2f,
        "Shoal",1f,
        "Water",1f,
        "Roof",1f,
        "Indoor",1f,
        "WalkableLow",2f,
    };

    static string[] sortingLayers;

    [InitializeOnLoadMethod]
    public static void CheckProjectSettings()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        //InitConfig();

        CheckLayer();
        CheckTag();
        CheckSortingLayers();
        UpdateUnitEditor();
        AssetDatabase.SaveAssets();
    }

    [MenuItem("Assets/Refresh ProjectSettings")]
    public static void RefreshProjectSettings()
    {
        CheckProjectSettings();
    }




    static void InitConfig()
    {
        //string pathTagPath = System.IO.Path.Combine(LuaProject.LuaRootPath, "game/defines/TagDefine.lua");
        //if (!System.IO.File.Exists(pathTagPath))
        //{
        //    Debug.LogWarningFormat("DeployProjectSettings.CheckProjectSettings  {0} 不存在", pathTagPath);
        //    return;
        //}

        //string luaCode = System.IO.File.ReadAllText(pathTagPath);

        //System.Reflection.Assembly Assembly = System.Reflection.Assembly.Load("Assembly-CSharp");
        //System.Type luaTab = Assembly.GetType("XLua.LuaTable");

        //System.Type type = Assembly.GetType("XLua.LuaEnv");

        //object luaEnv = System.Activator.CreateInstance(type);

        //object[] objs = (object[])type.GetMethod("DoString", new System.Type[] { typeof(string), typeof(string), luaTab }).Invoke(luaEnv, new object[] { luaCode, "chunk", null });

        //object layersCfg = luaTab.GetMethod("GetInPath").MakeGenericMethod(luaTab).Invoke(objs[0], new string[] { "layers" });
        //object tagsCfg = luaTab.GetMethod("GetInPath").MakeGenericMethod(luaTab).Invoke(objs[0], new string[] { "tags" });
        //object slayersCfg = luaTab.GetMethod("GetInPath").MakeGenericMethod(luaTab).Invoke(objs[0], new string[] { "sortinglayers" });


        //layers = new string[0] { };
        //System.Action<int, string> fun = (int key, string value) => { ArrayUtility.Add<string>(ref layers, value); };
        //luaTab.GetMethod("ForEach").MakeGenericMethod(typeof(int), typeof(string)).Invoke(layersCfg, new object[] { fun });


        //tags = new string[0] { };
        //System.Action<int, string> fun2 = (int key, string value) => { ArrayUtility.Add<string>(ref tags, value); };
        //luaTab.GetMethod("ForEach").MakeGenericMethod(typeof(int), typeof(string)).Invoke(tagsCfg, new object[] { fun2 });


        //sortingLayers = new string[0] { };
        //System.Action<int, string> fun3 = (int key, string value) => { ArrayUtility.Add<string>(ref sortingLayers, value); };
        //luaTab.GetMethod("ForEach").MakeGenericMethod(typeof(int), typeof(string)).Invoke(slayersCfg, new object[] { fun3 });

        //type.GetMethod("Dispose", new System.Type[0]).Invoke(luaEnv, new object[0]);
    }

    static void CheckLayer()
    {
        if (layers == null)
            return;

        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty iter = tagManager.GetIterator();
        while (iter.NextVisible(true))
        {
            if (iter.name == "layers")
            {
                SerializedProperty layer;
                for (int i = 8; i < iter.arraySize; i++)
                {
                    layer = iter.GetArrayElementAtIndex(i);
                    layer.stringValue = string.Empty;
                }


                for (int i = 0; i < layers.Length; i++)
                {
                    layer = iter.GetArrayElementAtIndex(8 + i);
                    layer.stringValue = layers[i];
                }

                tagManager.ApplyModifiedProperties();
                return;
            }
        }
    }

    static void CheckTag()
    {
        if (tags == null)
            return;
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty iter = tagManager.GetIterator();
        while (iter.NextVisible(true))
        {
            if (iter.name == "tags")
            {
                iter.ClearArray();

                for (int i = 0; i < tags.Length; i++)
                {
                    iter.InsertArrayElementAtIndex(i);
                    SerializedProperty tag = iter.GetArrayElementAtIndex(i);
                    tag.stringValue = tags[i];
                }
                tagManager.ApplyModifiedProperties();
                return;
            }
        }
    }


    static void CheckSortingLayers()
    {
        if (sortingLayers == null)
            return;
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty iter = tagManager.GetIterator();
        while (iter.NextVisible(true))
        {
            if (iter.name == "m_SortingLayers")
            {

                for (int i = iter.arraySize - 1; i >= 1; i--)
                    iter.DeleteArrayElementAtIndex(i);



                for (int i = 0; i < sortingLayers.Length; i++)
                {
                    iter.InsertArrayElementAtIndex(i + 1);
                    SerializedProperty layer = iter.GetArrayElementAtIndex(i + 1);
                    layer.FindPropertyRelative("name").stringValue = sortingLayers[i];
                }

                tagManager.ApplyModifiedProperties();
                return;
            }
        }
    }


    static void UpdateUnitEditor()
    {
        //if (UnityEditor.EditorSettings.spritePackerMode != SpritePackerMode.BuildTimeOnlyAtlas)
        //    UnityEditor.EditorSettings.spritePackerMode = SpritePackerMode.BuildTimeOnlyAtlas;


        //GraphicsSettings
        SerializedObject serializedObject = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0]);
        serializedObject.Update();
        SerializedProperty m_LightmapStripping = serializedObject.FindProperty("m_LightmapStripping");
        m_LightmapStripping.intValue = 1;
        SerializedProperty m_LightmapKeepPlain = serializedObject.FindProperty("m_LightmapKeepPlain");
        m_LightmapKeepPlain.boolValue = true;
        SerializedProperty m_LightmapKeepDirCombined = serializedObject.FindProperty("m_LightmapKeepDirCombined");
        m_LightmapKeepDirCombined.boolValue = true;
        SerializedProperty m_LightmapKeepDynamicPlain = serializedObject.FindProperty("m_LightmapKeepDynamicPlain");
        m_LightmapKeepDynamicPlain.boolValue = false;
        SerializedProperty m_LightmapKeepDynamicDirCombined = serializedObject.FindProperty("m_LightmapKeepDynamicDirCombined");
        m_LightmapKeepDynamicDirCombined.boolValue = false;
        SerializedProperty m_LightmapKeepShadowMask = serializedObject.FindProperty("m_LightmapKeepShadowMask");
        m_LightmapKeepShadowMask.boolValue = true;
        SerializedProperty m_LightmapKeepSubtractive = serializedObject.FindProperty("m_LightmapKeepSubtractive");
        m_LightmapKeepSubtractive.boolValue = false;
        SerializedProperty m_FogStripping = serializedObject.FindProperty("m_FogStripping");
        m_FogStripping.intValue = 1;
        SerializedProperty m_FogKeepLinear = serializedObject.FindProperty("m_FogKeepLinear");
        m_FogKeepLinear.boolValue = true;
        SerializedProperty m_FogKeepExp = serializedObject.FindProperty("m_FogKeepExp");
        m_FogKeepExp.boolValue = false;
        SerializedProperty m_FogKeepExp2 = serializedObject.FindProperty("m_FogKeepExp2");
        m_FogKeepExp2.boolValue = false;
        serializedObject.ApplyModifiedProperties();


        QualitySettings.skinWeights = SkinWeights.TwoBones;

        //NavMeshAreas
        serializedObject = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/NavMeshAreas.asset")[0]);
        serializedObject.Update();
        SerializedProperty m_Areas = serializedObject.FindProperty("areas");
        for (int i = 0; i < m_Areas.arraySize; i++)
        {
            int sidx = i * 2;

            if (sidx >= navigation_areas.Count || sidx + 1 >= navigation_areas.Count)
                break;
            SerializedProperty areasData = m_Areas.GetArrayElementAtIndex(i);
            SerializedProperty name = areasData.FindPropertyRelative("name");
            SerializedProperty cost = areasData.FindPropertyRelative("cost");
            if (name.stringValue == "NotWalkable")
                continue;

            if (i > 2)
                name.stringValue = (string)navigation_areas[sidx];

            cost.floatValue = (float)navigation_areas[sidx + 1];
        }
        serializedObject.ApplyModifiedProperties();

#if UNITY_2018_1_OR_NEWER
        UnityEditor.PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
#else
         UnityEditor.PlayerSettings.defaultIsFullScreen = false;
#endif
        UnityEditor.PlayerSettings.defaultScreenWidth = 1280;
        UnityEditor.PlayerSettings.defaultScreenHeight = 720;
        UnityEditor.PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.HiddenByDefault;
        UnityEditor.PlayerSettings.resizableWindow = true;
        UnityEditor.PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows, new GraphicsDeviceType[] { GraphicsDeviceType.Direct3D11 });
        UnityEditor.PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new GraphicsDeviceType[] { GraphicsDeviceType.OpenGLES3 });
        //UnityEditor.PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, new GraphicsDeviceType[] { GraphicsDeviceType.OpenGLES3 });
        UnityEditor.PlayerSettings.bakeCollisionMeshes = true;
    }
}