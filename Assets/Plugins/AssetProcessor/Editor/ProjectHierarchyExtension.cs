using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

/// <summary>
/// 显示目录及场景对象的描述 没别的用处
/// </summary>
[InitializeOnLoad]
public class ProjectHierarchyExtension
{
    static ProjectHierarchyExtension()
    {
        EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemOnGUI;
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
    }

    static string ImageRepeatLabel = "重复名字图片:{0}张";

    static Dictionary<string, GUIContent> s_ProjectFolderDesc = new Dictionary<string, GUIContent>()
    {
        {"Assets/Art",new GUIContent("美术素材总目录")},

        {"Assets/Art/Audio",new GUIContent("声音")},
        {"Assets/Art/Audio/Oggs",new GUIContent("声音ogg文件")},
        {"Assets/Art/Audio/Prefabs",new GUIContent("声音场景放置预置")},
        {"Assets/Art/Story",new GUIContent("剧情相关")},

        {"Assets/Art/Charactars",new GUIContent("游戏实体")},
        {"Assets/Art/Charactars/Collect",new GUIContent("采集物")},
        {"Assets/Art/Charactars/Npc",new GUIContent("Npc")},

        {"Assets/Art/Charactars/Pass",new GUIContent("传送门")},
        {"Assets/Art/Charactars/Drop",new GUIContent("掉落物")},
        {"Assets/Art/Charactars/Monster",new GUIContent("怪物")},
        {"Assets/Art/Charactars/Pet",new GUIContent("宠物")},
        {"Assets/Art/Charactars/PetMount",new GUIContent("宠物(坐骑)")},
        {"Assets/Art/Charactars/Mount",new GUIContent("坐骑")},
        {"Assets/Art/Charactars/UI",new GUIContent("界面")},

        {"Assets/Art/Charactars/Player",new GUIContent("主角 男剑|男枪|女法|女刺")},
        {"Assets/Art/Charactars/Player/Anim",new GUIContent("各职业动作")},
        {"Assets/Art/Charactars/Player/Anim/Assassin",new GUIContent("女刺客")},
        {"Assets/Art/Charactars/Player/Anim/Shooter",new GUIContent("男枪手")},
        {"Assets/Art/Charactars/Player/Anim/Soldier",new GUIContent("男剑客")},
        {"Assets/Art/Charactars/Player/Anim/Wizard",new GUIContent("女法师")},

        {"Assets/Art/Charactars/Player/Body",new GUIContent("各职业身体")},
        {"Assets/Art/Charactars/Player/Body/Male",new GUIContent("男装 z_nz_(男战) z_nq_(男枪)")},
        {"Assets/Art/Charactars/Player/Body/Woman",new GUIContent("女装 z_nf(女法) z_nc_(女刺)")},

        {"Assets/Art/Charactars/Player/Skeleton",new GUIContent("通用骨骼")},
        {"Assets/Art/Charactars/Player/HeadHair",new GUIContent("头")},
        {"Assets/Art/Charactars/Player/HeadHair/Hair",new GUIContent("头发公用")},
        {"Assets/Art/Charactars/Player/HeadHair/Male",new GUIContent("α型头")},
        {"Assets/Art/Charactars/Player/HeadHair/Woman",new GUIContent("β型头")},


        {"Assets/Art/Charactars/PlayerPart",new GUIContent("角色部件")},
        {"Assets/Art/Charactars/PlayerPart/Cloak",new GUIContent("披风")},
        {"Assets/Art/Charactars/PlayerPart/Mount",new GUIContent("坐骑")},
        {"Assets/Art/Charactars/PlayerPart/Weapon",new GUIContent("武器")},
        {"Assets/Art/Charactars/PlayerPart/Ornament",new GUIContent("背饰")},
        {"Assets/Art/Charactars/PlayerPart/Soul",new GUIContent("战魂")},
        {"Assets/Art/Charactars/PlayerPart/Wing",new GUIContent("翅膀")},


        {"Assets/Art/Effect",new GUIContent("特效")},
        {"Assets/Art/Effect/Scene",new GUIContent("场景特效")},
        {"Assets/Art/Effect/UI",new GUIContent("界面特效")},
        {"Assets/Art/Effect/Fight",new GUIContent("战斗特效")},
        {"Assets/Art/Effect/Charactar",new GUIContent("实体身上特效")},
        {"Assets/Art/EffectBuildin",new GUIContent("首包特效[代码抽取]")},

        {"Assets/Art/Env",new GUIContent("场景相关")},
        {"Assets/Art/Env/SkyBox",new GUIContent("天空盒贴图")},
        {"Assets/Art/Env/SAEnv",new GUIContent("标准资产")},
        {"Assets/Art/Env/Scenes",new GUIContent("游戏场景")},
        {"Assets/Art/Env/Scenes/A_scene_dev",new GUIContent("开发测试场景")},
        {"Assets/Art/Env/Scenes/A_scene_share",new GUIContent("场景共享素材")},


    };

    public static Dictionary<string, string> s_AtlasUseRatio;
    public static Dictionary<string, string> s_PrefabReference;
    public static Dictionary<string, string> s_ImageNameRepeat;
    public static string atlasUseRatioPath = Path.Combine(Application.dataPath, "../AtlasUseRatio.txt");
    public static string prefabReferencePath = Path.Combine(Application.dataPath, "../PrefabReference.txt");
    public static string imageNameRepeatPath = Path.Combine(Application.dataPath, "../ImageNameRepeat.txt");
    static string GetFolderNameCfg(string folderName, string fileName, ref Dictionary<string, string> configDic)
    {
        if (configDic == null)
        {
            configDic = new Dictionary<string, string>();
            string path = fileName;
            if (File.Exists(path))
            {
                string[] lines = File.ReadAllLines(path);
                foreach (var line in lines)
                {
                    if (string.IsNullOrEmpty(line)) continue;
                    string[] sp = line.Split('=');
                    if (sp.Length == 2)
                    {
                        string cname = sp[0];
                        string name = sp[1];
                        if (!string.IsNullOrEmpty(cname) && !string.IsNullOrEmpty(name))
                        {
                            if (!configDic.ContainsKey(cname))
                            {
                                configDic.Add(cname, name);
                            }
                        }
                    }
                }
            }
        }

        return (configDic != null && configDic.ContainsKey(folderName))
            ? configDic[folderName]
            : null;
    }


    static GUIStyle label;
    static GUIStyle label2;
    static GUIStyle GetLabelStyle(bool isHierarchy = false)
    {
        GUIStyle style = null;
        if (!isHierarchy)
        {
            if (label == null)
            {
                label = new GUIStyle(EditorStyles.label);
                label.alignment = TextAnchor.MiddleRight;
                label.padding.right = 10;
                label.normal.textColor = Color.gray;
            }
            style = label;
        }
        else
        {
            if (label2 == null)
            {
                label2 = new GUIStyle(EditorStyles.label);
                label2.alignment = TextAnchor.MiddleRight;
                label2.padding.right = 10;
                label2.normal.textColor = Color.gray;
            }
            style = label2;
        }

        return style;
    }

    private static void OnProjectWindowItemOnGUI(string guid, Rect selectionRect)
    {
        string path = AssetDatabase.GUIDToAssetPath(guid);
        if (!AssetDatabase.IsValidFolder(path))
        {
            return;
        }
        GUIContent content;
        if (s_ProjectFolderDesc.TryGetValue(path, out content))
        {
            EditorGUI.LabelField(selectionRect, content, GetLabelStyle());
        }
        else if (Regex.IsMatch(path, "Assets/GUI/Modules/.*/Images/Single"))
        {
            EditorGUI.LabelField(selectionRect, "单张图片图集", GetLabelStyle());
        }
        else if (path.Contains("_Split"))
        {
            EditorGUI.LabelField(selectionRect, "不打图集", GetLabelStyle());
        }
        else
        {
            //string folderName = Path.GetFileNameWithoutExtension(path);
            string atlasUseRatioName = GetFolderNameCfg(path, atlasUseRatioPath, ref s_AtlasUseRatio);
            string prefabRefName = GetFolderNameCfg(path, prefabReferencePath, ref s_PrefabReference);
            string imageNameRepeatName = GetFolderNameCfg(path, imageNameRepeatPath, ref s_ImageNameRepeat);

            if (!string.IsNullOrEmpty(atlasUseRatioName))
            {

                if (string.IsNullOrEmpty(imageNameRepeatName))
                {
                    EditorGUI.LabelField(selectionRect, atlasUseRatioName, GetLabelStyle());
                }
                else
                {
                    EditorGUI.LabelField(selectionRect, string.Format(ImageRepeatLabel, imageNameRepeatName) + "  " + atlasUseRatioName, GetLabelStyle());
                }
            }
            else if (!string.IsNullOrEmpty(imageNameRepeatName))
            {
                EditorGUI.LabelField(selectionRect, string.Format(ImageRepeatLabel, imageNameRepeatName), GetLabelStyle());
            }
            else if (!string.IsNullOrEmpty(prefabRefName))
            {
                EditorGUI.LabelField(selectionRect, prefabRefName, GetLabelStyle());
            }
            else
            {
                TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(path + "/readme.txt");
                if (textAsset != null)
                {
                    EditorGUI.LabelField(selectionRect, textAsset.text, GetLabelStyle());
                }
            }

        }
    }


    static Dictionary<string, GUIContent> s_HierarchyDesc = new Dictionary<string, GUIContent>()
    {
        {"XScene",new GUIContent("场景根对象")},
        {"Navigation",new GUIContent("导航寻路相关")},
        {"Background",new GUIContent("远景天空盒等")},
        {"Grounds",new GUIContent("地面")},
        {"Buildings",new GUIContent("建筑")},
        {"Plants",new GUIContent("植物")},
        {"Details",new GUIContent("细节摆件")},
        {"Effects",new GUIContent("特效")},
        {"Lights",new GUIContent("灯光")},
        {"Cameras",new GUIContent("相机")},
        {"Animation",new GUIContent("动作物件")},
        {"Procedure",new GUIContent("程序控制(不要随意改名)")},
    };

    private static void OnHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
    {
        GameObject go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (go == null)
            return;

        GUIContent content;
        if (s_HierarchyDesc.TryGetValue(go.name, out content))
        {
            EditorGUI.LabelField(selectionRect, content, GetLabelStyle(true));
        }
    }
}