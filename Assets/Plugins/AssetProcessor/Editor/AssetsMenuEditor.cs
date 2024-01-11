using UnityEngine;
using UnityEditor;

public class AssetsMenuEditor : ScriptableObject
{
    [MenuItem("Assets/Refresh AssetsManifest")]
    static void RefreshAssetsManifest()
    {
        AssetManifestProcessor.RefreshAll();
    }





    [MenuItem("Assets/Copy Selected Assets Path")]
    static void CopySelectedAssetsPath()
    {
        string[] guids = Selection.assetGUIDs;
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            if (i == guids.Length - 1)
                sb.Append(path);
            else
                sb.AppendLine(path);
        }


        TextEditor textEditor = new TextEditor();
        textEditor.text = sb.ToString();
        textEditor.OnFocus();
        textEditor.Copy();
    }




    [MenuItem("GameObject/XScene", priority = -1)]
    static public void AddXEnvironment3D(MenuCommand menuCommand)
    {
        Transform ts = new GameObject("XScene").transform;
        new GameObject("Navigation").transform.SetParent(ts);
        new GameObject("Background").transform.SetParent(ts);
        new GameObject("Grounds").transform.SetParent(ts);
        new GameObject("Buildings").transform.SetParent(ts);
        new GameObject("Plants").transform.SetParent(ts);
        new GameObject("Details").transform.SetParent(ts);
        new GameObject("Effects").transform.SetParent(ts);
        new GameObject("Lights").transform.SetParent(ts);
        new GameObject("Cameras").transform.SetParent(ts);
        new GameObject("Animation").transform.SetParent(ts);
        new GameObject("Procedure").transform.SetParent(ts);
    }

    [MenuItem("Assets/Packet Animations")]
    static void CreateAnimPack()
    {
        string[] objs = Selection.assetGUIDs;
        if (objs.Length > 0)
        {
            string floadPath = "";
            string fileName = "";
            AnimationPack pack = AnimationPack.CreateInstance<AnimationPack>();
            for (int i = 0; i < objs.Length; i++)
            {
                floadPath = AssetDatabase.GUIDToAssetPath(objs[i]);
                if (fileName == "")
                {
                    string[] tts = floadPath.Split('/');
                    fileName = tts[tts.Length - 2];
                }

                ExExecuteBuildAnimRes(floadPath, pack);

            }

            AssetDatabase.CreateAsset(pack, floadPath + "/" + fileName + "_Anims.asset");
        }

    }

    public static void ExExecuteBuildAnimRes(string floadPath, AnimationPack pack)
    {

        if (!floadPath.Contains("Anim")) return;
        string[] files = System.IO.Directory.GetFiles(floadPath);
        for (int j = 0; j < files.Length; j++)
        {
            if (!files[j].Contains(".anim")) continue;
            AnimationClip clip = AssetDatabase.LoadAssetAtPath(files[j], typeof(AnimationClip)) as AnimationClip;
            if (clip != null)
            {

                pack.AddClip(clip.name, clip);
            }
        }
    }
}