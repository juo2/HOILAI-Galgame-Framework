//#define LEGACYATLAS
using UnityEngine;
using UnityEngine.U2D;
using UnityEditor;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// 模块图集导入设置
/// </summary>
public class AtlasImportSettings : AssetPostprocessor
{
    struct PlatformSettingCacheData
    {
        public string assetPath;
        public TextureImporter importer;
    }

    /// <summary>
    /// 用于外部暂时关闭设置操作
    /// </summary>
    public static bool isProcessTex = true;

    public static void SetProcessTexState(bool state)
    {
        isProcessTex = state;
    }

    string GetModuleNameAtPath()
    {
        return assetPath.Substring(19, assetPath.IndexOf("/", 19) - 19);
    }

    static string GetFullPath(string projectPath)
    {
        return Path.Combine(Application.dataPath, projectPath.Substring(7)).Replace("\\", "/");
    }


    static string GetPorjectPath(string fullPath)
    {
        return fullPath.Substring(Application.dataPath.Length - 6).Replace("\\", "/");
    }

    static System.Reflection.Assembly s_UnityEditorDll;
    static System.Type s_SpriteAtlasExtensionsType;
    static System.Reflection.MethodInfo s_CopyPlatformSettingsIfAvailable;
    static System.Reflection.MethodInfo s_SetPlatformSettings;

    static void InitReflection()
    {
        if (s_UnityEditorDll == null)
        {
            s_UnityEditorDll = System.Reflection.Assembly.Load("UnityEditor.dll");
            s_SpriteAtlasExtensionsType = s_UnityEditorDll.GetType("UnityEditor.U2D.SpriteAtlasExtensions");
            s_CopyPlatformSettingsIfAvailable = s_SpriteAtlasExtensionsType.GetMethod("GetPlatformSettings");
            s_SetPlatformSettings = s_SpriteAtlasExtensionsType.GetMethod("SetPlatformSettings");
        }
    }


#if LEGACYATLAS
    void OnPostprocessTexture(Texture2D texture)
    {
        TextureImporter import = (TextureImporter)assetImporter;

        if (!Regex.IsMatch(assetPath, "Assets/GUI/Modules/.*/Images"))
            return;

        string packingTag = GetModuleNameAtPath();
        string folderName = Path.GetFileName(Path.GetDirectoryName(assetPath));
        if (folderName != "Images")
            packingTag += "_" + folderName;


        if (import.textureType != TextureImporterType.Sprite)
        {
            import.textureType = TextureImporterType.Sprite;
        }

        if (import.spritePackingTag != packingTag)
        {
            import.spritePackingTag = packingTag;
        }

        if (import.isReadable)
        {
            import.isReadable = false;
        }

        import.ClearPlatformTextureSettings("Android");
        TextureImporterPlatformSettings tips = import.GetPlatformTextureSettings("iPhone");
        if (tips != null)
        {
            tips.overridden = true;
            tips.format = import.DoesSourceTextureHaveAlpha() ? TextureImporterFormat.ASTC_RGBA_6x6 : TextureImporterFormat.ASTC_RGB_6x6;
            import.SetPlatformTextureSettings(tips);
        }
    }
#else
    private static bool isSetPlatformSettings = false;//正在进行平台选项设置


    static Dictionary<string, string> m_WaitUpdate;
    static Dictionary<string, string> m_PreAltasUpdate;

    private static List<PlatformSettingCacheData> m_WaitSetplatformSetting;

    void OnPostprocessTexture(Texture2D texture)
    {
        if (!isProcessTex) return;
        if (isSetPlatformSettings) return;
        TextureImporter import = assetImporter as TextureImporter;
        if (!import) return;
        if (Regex.IsMatch(assetPath, "Assets/GUI/Modules/Chat/Images/EmojiSprite/.*"))
            return;

        bool isModules = Regex.IsMatch(assetPath, "Assets/GUI/Modules/.*/Images");
        bool isFont = Regex.IsMatch(assetPath, "Assets/GUI/Fonts2/.*");
        if (!isModules && !isFont)
        {
            if (!isModules && Regex.IsMatch(assetPath, "Assets/GUI/Modules/.*"))
            {
                Debug.LogError("图片资源须放入模块的Images文件夹或其子文件夹下");
            }
            return;
        }

        bool reimport = false;

        if (!Regex.IsMatch(assetPath, "Assets/GUI/Modules/Chat/Images/EmojiAtlas_Split/.*") && !assetPath.Contains("_readwriteEnable"))
        {
            if (import.isReadable)
            {
                import.isReadable = false;
                reimport = true;
            }
        }

        if (import.textureType != TextureImporterType.Sprite)
        {
            import.textureType = TextureImporterType.Sprite;
            reimport = true;
        }

        TextureImporterSettings ts = new TextureImporterSettings();
        import.ReadTextureSettings(ts);
        if (ts.spriteMeshType != SpriteMeshType.FullRect)
        {
            ts.spriteMeshType = SpriteMeshType.FullRect;
            import.SetTextureSettings(ts);
            reimport = true;
        }

        string moduleName = GetModuleNameAtPath();
        string folderName = Path.GetFileName(Path.GetDirectoryName(assetPath));
        string parentPath = Path.GetDirectoryName(assetPath);
        string atlasAssetName = "Atlas_" + moduleName;

        //XMask  mask贴图将不会打入到大图集
        bool isMaskTex = Path.GetFileNameWithoutExtension(assetPath).EndsWith("mask");

        bool isNeedPlatformSetting = false;

        if (folderName.Contains("Single") || isMaskTex)
            atlasAssetName = string.Empty;
        else if (folderName.Contains("Sequence") || folderName.Contains("Split") || isFont)
        {
            isNeedPlatformSetting = true;
            atlasAssetName = string.Empty;
        }
        else if (folderName != "Images")
            atlasAssetName += "_" + folderName;

        string atlasAssetPath = string.Empty;

        if (!string.IsNullOrEmpty(import.spritePackingTag))
        {
            import.spritePackingTag = string.Empty;
            reimport = true;
        }
        else
            isNeedPlatformSetting = true;

        //if (import.spritePackingTag != atlasAssetName)
        //{
        //    if (!string.IsNullOrEmpty(import.spritePackingTag))
        //    {
        //        if (m_PreAltasUpdate == null)
        //            m_PreAltasUpdate = new Dictionary<string, string>();

        //        string preAtlasAssetPath = import.spritePackingTag + ".spriteatlas";
        //        m_PreAltasUpdate.Add(assetPath, preAtlasAssetPath);
        //    }

        //    import.spritePackingTag = atlasAssetName;
        //    reimport = true;
        //}
#if !UNITY_IOS
        if (!string.IsNullOrEmpty(atlasAssetName))
            atlasAssetPath = parentPath + "/" + atlasAssetName + ".spriteatlas";
#endif

        if (reimport)
        {
            import.SaveAndReimport();
            return;
        }

        if (isNeedPlatformSetting)
        {
            if (m_WaitSetplatformSetting == null) m_WaitSetplatformSetting = new List<PlatformSettingCacheData>();
            PlatformSettingCacheData cacheData = new PlatformSettingCacheData();
            cacheData.importer = import;
            cacheData.assetPath = assetPath;
            m_WaitSetplatformSetting.Add(cacheData);
        }

        if (m_WaitUpdate == null)
        {
            m_WaitUpdate = new Dictionary<string, string>();
            EditorApplication.delayCall += OnDelayCall;
        }

        if (!string.IsNullOrEmpty(atlasAssetPath) && !m_WaitUpdate.ContainsKey(atlasAssetPath))
        {
            m_WaitUpdate.Add(atlasAssetPath, assetPath);
        }

    }

    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromPath)
    {
        foreach (string move in movedAssets)
        {
            //这里重新 import一下
            AssetDatabase.ImportAsset(move);
        }
    }

    //设置单独的（不打入图集）图片的平台设置
    private static void SetTexurePlatformSetting(TextureImporter importer, string assetPath)
    {
        bool isFonts = assetPath.Contains("Fonts2");

        TextureImporterPlatformSettings tips = new TextureImporterPlatformSettings();
        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        bool isAlpha = tex != null ? tex.alphaIsTransparency : true;

        tips.overridden = true;
        if (isFonts)
            tips.format = isAlpha ? TextureImporterFormat.ASTC_4x4 : TextureImporterFormat.ASTC_4x4;
        else
            tips.format = isAlpha ? TextureImporterFormat.ASTC_6x6 : TextureImporterFormat.ASTC_6x6;
        tips.name = "Android";
        importer.SetPlatformTextureSettings(tips);

        tips.overridden = true;
        if (isFonts)
            tips.format = isAlpha ? TextureImporterFormat.ASTC_4x4 : TextureImporterFormat.ASTC_4x4;
        else
            tips.format = isAlpha ? TextureImporterFormat.ASTC_6x6 : TextureImporterFormat.ASTC_6x6;
        tips.name = "iPhone";
        importer.SetPlatformTextureSettings(tips);
        importer.SaveAndReimport();
    }

    public static void RefreshAllAtlas() //一键解决乱引用工具需要
    {
        OnDelayCall();
    }

    private static void OnDelayCall()
    {
        if (m_WaitUpdate == null)
            return;

        System.DateTime stime = System.DateTime.Now;
        try
        {
            foreach (var item in m_WaitUpdate)
            {
                RefreshSpriteAtlas(item.Key, item.Value);
            }

        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
        DelaySetPlatformSetting();

        AssetDatabase.SaveAssets();
        EditorApplication.delayCall -= OnDelayCall;
        m_WaitUpdate.Clear();
        m_WaitUpdate = null;

        Debug.Log("RefreshSpriteAtlas: " + (System.DateTime.Now - stime).TotalSeconds);
    }

    private static void DelaySetPlatformSetting()
    {
        if (m_WaitSetplatformSetting == null)
        {
            return;
        }
        try
        {
            isSetPlatformSettings = true;
            for (int i = 0; i < m_WaitSetplatformSetting.Count; i++)
            {
                var item = m_WaitSetplatformSetting[i];
                SetTexurePlatformSetting(item.importer, item.assetPath);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
        finally
        {
            isSetPlatformSettings = false;
            m_WaitSetplatformSetting.Clear();
        }
    }

    public static void RefreshSpriteAtlas(string atlasAssetPath, string spriteName = "")
    {
        DeletePreAtlasAsset(spriteName);

        SpriteAtlas atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasAssetPath);
        if (atlas == null)
            atlas = CreateSpriteAtlas(atlasAssetPath);

        SerializedObject serializedObject = new SerializedObject(atlas);
        SerializedProperty packables = serializedObject.FindProperty("m_EditorData.packables");
        string parent = Path.GetDirectoryName(GetFullPath(atlasAssetPath));
        string[] images = null;

        if (Path.GetFileName(parent) == "Single" && !string.IsNullOrEmpty(spriteName))
            images = new string[] { GetFullPath(spriteName) };
        else
            images = Directory.GetFiles(parent, "*.png", SearchOption.TopDirectoryOnly);

        bool isAlpha = false;
        HashSet<string> exist = new HashSet<string>();
        string parentProjectPath = GetPorjectPath(parent);
        for (int i = packables.arraySize - 1; i >= 0; i--)
        {
            SerializedProperty texsp = packables.GetArrayElementAtIndex(i);
            string texPath = texsp.objectReferenceValue != null
                ? AssetDatabase.GetAssetPath(texsp.objectReferenceValue)
                : string.Empty;

            string texParent = string.IsNullOrEmpty(texPath) ? string.Empty : Path.GetDirectoryName(texPath);
            if (string.IsNullOrEmpty(texPath) || texParent != parentProjectPath)
            {
                texsp.objectReferenceValue = null;
                packables.DeleteArrayElementAtIndex(i);
            }
            else
            {
                exist.Add(texPath);
                Texture2D tex2d = texsp.objectReferenceValue as Texture2D;
                if (tex2d.alphaIsTransparency && !isAlpha)
                    isAlpha = true;
            }
        }

        foreach (var imagePath in images)
        {
            string imgProjepath = GetPorjectPath(imagePath);
            //XMask  mask贴图将不会打入到大图集
            bool isMaskTex = Path.GetFileNameWithoutExtension(imgProjepath).EndsWith("mask");
            if (isMaskTex) continue;
            if (exist.Contains(imgProjepath)) continue;
            Texture2D obj = AssetDatabase.LoadAssetAtPath<Texture2D>(imgProjepath);
            packables.InsertArrayElementAtIndex(packables.arraySize);
            packables.GetArrayElementAtIndex(packables.arraySize - 1).objectReferenceValue = obj;

            if (obj.alphaIsTransparency && !isAlpha)
                isAlpha = true;
        }

        if (packables.arraySize == 0)
            AssetDatabase.DeleteAsset(atlasAssetPath);
        else
        {
            EditorUtility.SetDirty(atlas);
            serializedObject.ApplyModifiedProperties();
        }

        InitReflection();
        TextureImporterPlatformSettings tips = new TextureImporterPlatformSettings();
        tips = s_CopyPlatformSettingsIfAvailable.Invoke(s_SpriteAtlasExtensionsType, new object[] { atlas, "Android" }) as TextureImporterPlatformSettings;
        tips.overridden = true;
        tips.format = isAlpha ? TextureImporterFormat.ASTC_6x6 : TextureImporterFormat.ASTC_6x6;
        tips.name = "Android";
        s_SetPlatformSettings.Invoke(s_SetPlatformSettings, new object[] { atlas, tips });

        tips.format = isAlpha ? TextureImporterFormat.ASTC_6x6 : TextureImporterFormat.ASTC_6x6;
        tips.name = "iPhone";
        s_SetPlatformSettings.Invoke(s_SetPlatformSettings, new object[] { atlas, tips });
        
        //AssetDatabase.SaveAssets();
    }

    static void DeletePreAtlasAsset(string spriteName)
    {
        string m_PreAltasPath = string.Empty;
        if (m_PreAltasUpdate == null) return;
        if (m_PreAltasUpdate.TryGetValue(spriteName, out m_PreAltasPath))
        {
            AssetManifest assetManifest = UnityEditor.AssetDatabase.LoadAssetAtPath<AssetManifest>(AssetManifest.s_AssetManifestPath);
            AssetManifestProcessor.RefreshAll();
            m_PreAltasUpdate.Remove(spriteName);
            string preFullPath = assetManifest.GetAssetPath(m_PreAltasPath);
            SpriteAtlas preAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(preFullPath);
            if (preAtlas)
            {
                SerializedObject preSerializedObject = new SerializedObject(preAtlas);
                SerializedProperty prePackables = preSerializedObject.FindProperty("m_EditorData.packables");
                string preParent = Path.GetDirectoryName(GetFullPath(preFullPath));
                string preParentProjectPath = GetPorjectPath(preParent);
                for (int i = prePackables.arraySize - 1; i >= 0; i--)
                {
                    SerializedProperty texsp = prePackables.GetArrayElementAtIndex(i);
                    string texPath = texsp.objectReferenceValue != null
                        ? AssetDatabase.GetAssetPath(texsp.objectReferenceValue)
                        : string.Empty;

                    string texParent = string.IsNullOrEmpty(texPath) ? string.Empty : Path.GetDirectoryName(texPath);
                    if (string.IsNullOrEmpty(texPath) || texParent != preParentProjectPath)
                    {
                        texsp.objectReferenceValue = null;
                        prePackables.DeleteArrayElementAtIndex(i);
                    }
                }
                if (prePackables.arraySize == 0)
                    AssetDatabase.DeleteAsset(preFullPath);
                else
                {
                    EditorUtility.SetDirty(preAtlas);
                    preSerializedObject.ApplyModifiedProperties();
                }
            }
        }
    }

    static string opath = Path.Combine(Path.GetDirectoryName(AssetManifest.s_AssetManifestPath), "Editor/atlas_temp.spriteatlas");
    static SpriteAtlas CreateSpriteAtlas(string path)
    {
        AssetDatabase.CopyAsset(opath, path);
        AssetDatabase.Refresh();
        return AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);
    }
#endif
}