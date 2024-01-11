using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor.AssetImporters;

public class ModelImportSettings : AssetPostprocessor
{
    //无效的贴图格式，将强制改成png
    private static HashSet<string> s_InvalidTextureFormat = new HashSet<string> { ".dds" };


    void OnPostprocessTexture(Texture2D texture)
    {
        if (Regex.IsMatch(assetPath, "Assets/GUI/Modules/Chat/Images/EmojiSprite/.*"))
            return;
        string extName = Path.GetExtension(assetImporter.assetPath);

        if (s_InvalidTextureFormat.Contains(extName))
        {
            string path = Path.Combine(Application.dataPath, assetImporter.assetPath.Substring(7));
            FileUtil.MoveFileOrDirectory(path, path.Replace(".", "_") + ".png");
            //AssetDatabase.Refresh();
            return;
        }

        TextureImporter import = assetImporter as TextureImporter;
        if (!import) return;
        bool isReimport = false;


        if (Regex.IsMatch(assetPath, "Assets/Art/Charactars/"))
        {
            if (!import.mipmapEnabled)
            {
                import.mipmapEnabled = true;
                import.anisoLevel = 9;
                isReimport = true;
            }
        }

        //if (import.isReadable)
        //{
        //    import.isReadable = false;
        //    isReimport = true;
        //}

        if (import.maxTextureSize > 2048)
        {
            import.maxTextureSize = 2048;
            isReimport = true;
        }

        if (isReimport)
            import.SaveAndReimport();

    }


    private static Material defaultMat;
    Material OnAssignMaterialModel(Material material, Renderer renderer)
    {

        if (defaultMat == null)
        {
            defaultMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Shaders/M_Materials/XVertexLit.mat");
        }

        return defaultMat;

        
    }


    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
    {
        foreach (string str in importedAssets)
        {
            if(str.Contains(".mat"))
            {
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(str);

                if (mat.shader.name == "X_Shader/C_Charactar/PBR/Standard")
                {
                    Texture mainTex = mat.GetTexture("_MainTex");
                    Texture bumpTex = mat.GetTexture("_BumpSMap");
                    Texture metallicTex = mat.GetTexture("_MetallicGlossMap");

                    Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                    Material material = new Material(shader);

                    material.SetTexture("_BaseMap", mainTex);
                    material.SetTexture("_BumpMap", bumpTex);
                    material.SetTexture("_MetallicGlossMap", metallicTex);

                    AssetDatabase.DeleteAsset(str);
                    AssetDatabase.CreateAsset(material, str);
                }
            }
        }
       
    }

    static Dictionary<string, string> m_WaitUpdate;
    void OnPostprocessModel(GameObject gameObject)
    {

        ModelImporter modelImporter = (ModelImporter)assetImporter;
        if (modelImporter.isReadable)
        {
            if (!assetPath.Contains("dibiao") && !assetPath.Contains("Art/Effect") && !assetPath.Contains("Art/Env"))
            {
                modelImporter.isReadable = false;
                modelImporter.SaveAndReimport();
            }
            return;
        }
        else
        {
            if (assetPath.Contains("Art/Effect") || assetPath.Contains("Art/Env"))
            {
                modelImporter.importCameras = false;
                modelImporter.importLights = false;
                modelImporter.importVisibility = false;
                modelImporter.isReadable = true;
                modelImporter.SaveAndReimport();
                return;
            }
        }


        //string parent = Path.GetFileName(Path.GetDirectoryName(assetPath));
        //if (assetPath.StartsWith("Assets/Art/Charactars/Player/Body") ||
        //    assetPath.StartsWith("Assets/Art/Charactars/Player/HeadHair") && parent == "Models")
        //{
        //    XAvatarUtility.CreateXAvatarBoneData(gameObject,assetPath);
        //}



        if (!assetPath.Contains("Anim"))
            return;

        AnimationClip clip = new AnimationClip();
        AnimationClip nClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
        if (nClip != null)
        {
            EditorUtility.CopySerialized(nClip, clip);
            string path = Path.GetDirectoryName(assetPath);
            path = Path.Combine(Path.GetDirectoryName(assetPath), Path.GetFileNameWithoutExtension(assetPath) + ".anim");
            string tempPath = Path.Combine(Path.GetDirectoryName(assetPath), Path.GetFileNameWithoutExtension(assetPath) + "_temp.anim");
            string fullPath = Path.Combine(Application.dataPath, path.Substring(7));
            string tempFullPath = Path.Combine(Application.dataPath, tempPath.Substring(7));

            if (File.Exists(fullPath))
            {
                AssetDatabase.CreateAsset(clip, tempPath);
                File.Delete(fullPath);
                FileUtil.MoveFileOrDirectory(tempFullPath, fullPath);
            }
            else
            {
                AssetDatabase.CreateAsset(clip, path);
            }

            if (m_WaitUpdate == null)
            {
                m_WaitUpdate = new Dictionary<string, string>();
                EditorApplication.delayCall += OnDelayCall;
            }

            if (!m_WaitUpdate.ContainsKey(assetPath))
                m_WaitUpdate.Add(assetPath, assetPath);
        }
    }

    private void OnDelayCall()
    {
        string fileName = "";
        string[] tts = assetPath.Split('/');
        fileName = tts[tts.Length - 3];
        string floatName = "";
        for (int i = 0; i < tts.Length - 1; i++)
        {
            floatName += tts[i] + '/';
        }

        if (!floatName.Contains("Player"))
        {
            AnimationPack pack = AnimationPack.CreateInstance<AnimationPack>();
            AssetsMenuEditor.ExExecuteBuildAnimRes(floatName, pack);
            AssetDatabase.CreateAsset(pack, floatName + "/" + fileName + "_Anims.asset");
        }

        foreach (var item in m_WaitUpdate)
        {
            AssetDatabase.DeleteAsset(item.Key);
        }

        m_WaitUpdate.Clear();
        m_WaitUpdate = null;

        AssetDatabase.SaveAssets();
        EditorApplication.delayCall -= OnDelayCall;
    }


}
