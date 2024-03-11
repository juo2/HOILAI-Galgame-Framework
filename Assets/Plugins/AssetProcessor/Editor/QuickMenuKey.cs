using UnityEngine;
using UnityEditor;


[InitializeOnLoad]
public class QuickMenuKey : ScriptableObject
{
    static string m_LaunchGameTag = "QuickMenuKey_LaunchGameTag";
    static string m_LaunchGameUpdate = "QuickMenuKey_LaunchGameUpdate";
    static string m_LaunchGameAssetBundle = "QuickMenuKey_LaunchGameAssetBundle";
    static string m_LaunchGameRecordAssets = "QuickMenuKey_LaunchGameRecordAssets";

    static QuickMenuKey()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            if (EditorPrefs.GetBool(m_LaunchGameTag))
            {
                EditorApplication.update += Update;
            }
        }


        EditorApplication.playModeStateChanged += (PlayModeStateChange state) =>
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                EditorPrefs.SetBool(m_LaunchGameAssetBundle, false);
                EditorPrefs.SetBool(m_LaunchGameRecordAssets, false);
            }
        };

    }

    static void Update()
    {
        if (EditorApplication.isPlaying)
        {
            EditorPrefs.DeleteKey(m_LaunchGameTag);
            EditorApplication.update -= Update;
            CreateLaunchScene();
        }
    }


    [MenuItem("XGame/Launch本地资源 #F6", false, 50)]
    static void LaunchGameNoUpdate()
    {
        if (EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = false;
            return;
        }

        EditorApplication.ExecuteMenuItem("Assets/Refresh AssetsManifest");
        //if (UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        //{

        EditorPrefs.SetBool(m_LaunchGameAssetBundle, false);
        EditorPrefs.SetBool(m_LaunchGameRecordAssets, false);
        EditorPrefs.SetBool(m_LaunchGameUpdate, false);
        EditorPrefs.SetBool(m_LaunchGameTag, true);
        EditorApplication.isPlaying = true;
        //}
    }

    //[MenuItem("XGame/Launch资源录制 #F6", false, 50)]
    //static void LaunchGameRecordAssets()
    //{
    //    if (EditorApplication.isPlaying)
    //    {
    //        EditorApplication.isPlaying = false;
    //        return;
    //    }

    //    EditorApplication.ExecuteMenuItem("Assets/Refresh AssetsManifest");
    //    //if (UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
    //    //{
    //    EditorPrefs.SetBool(m_LaunchGameAssetBundle, false);
    //    EditorPrefs.SetBool(m_LaunchGameRecordAssets, true);
    //    EditorPrefs.SetBool(m_LaunchGameTag, true);
    //    EditorApplication.isPlaying = true;
    //    //}
    //}


    [MenuItem("XGame/Launch包模式 #%F5", false, 50)]
    static void LaunchGameAssetBundle()
    {
        if (EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = false;
            return;
        }

        UnityEditor.EditorSettings.spritePackerMode = SpritePackerMode.AlwaysOnAtlas;


        EditorApplication.ExecuteMenuItem("Assets/Refresh AssetsManifest");
        //if (UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        //{

        EditorPrefs.SetBool(m_LaunchGameAssetBundle, true);
        EditorPrefs.SetBool(m_LaunchGameRecordAssets, false);
        EditorPrefs.SetBool(m_LaunchGameUpdate, true);
        EditorPrefs.SetBool(m_LaunchGameTag, true);
        EditorApplication.isPlaying = true;
        //}
    }

    [MenuItem("XGame/Launch包模式(离线) #%F5", false, 50)]
    static void LaunchGameAssetBundleLocalCode()
    {
        if (EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = false;
            return;
        }

        UnityEditor.EditorSettings.spritePackerMode = SpritePackerMode.AlwaysOnAtlas;

        EditorApplication.ExecuteMenuItem("Assets/Refresh AssetsManifest");
        EditorPrefs.SetBool(m_LaunchGameAssetBundle, false);
        EditorPrefs.SetBool(m_LaunchGameRecordAssets, false);
        EditorPrefs.SetBool(m_LaunchGameUpdate, false);
        EditorPrefs.SetBool(m_LaunchGameTag, true);
        EditorApplication.isPlaying = true;
    }

    static void CreateLaunchScene()
    {
        UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEngine.SceneManagement.SceneManager.CreateScene("LaunchGame");
        UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene);
        System.Reflection.Assembly Assembly = System.Reflection.Assembly.Load("Assembly-CSharp");
        System.Type type = Assembly.GetType("Launcher");
        GameObject xgame = new GameObject("xgame", type);
        Object.DontDestroyOnLoad(xgame);
    }




    [MenuItem("XGame/PlayerPrefs DeleteAll", false, 10001)]
    static void PlayerPrefsDeleteAll()
    {
        PlayerPrefs.DeleteAll();
    }

    [MenuItem("XGame/Cache DeleteAll", false, 10001)]
    static void CacheDeleteAll()
    {
        bool result = Caching.ClearCache();
        Debug.Log(Caching.currentCacheForWriting.path + "  " + result);
    }
}