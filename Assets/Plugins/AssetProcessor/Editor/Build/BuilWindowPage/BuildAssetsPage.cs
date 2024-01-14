using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

public class BuildAssetsPage : XBuildWindow.XBuildPage
{
    const string NAME = "Build";


    enum BuilPlayerTarget
    {
        Android,
        Windows,
        iOS,
    }

    enum CompressionType
    {
        NONE, LZ4, LZMA
    }


    const string PREFSBUILDOUTPATH = "XBUILDWINDOWPREFSBUILDOUTPATH";
    const string BUILDTARGETINT = "BUILDTARGETINT";
    const string BUILDPLAYERTARGETINT = "BUILDPLAYERTARGETINT";

    const string c_TEMPPATH = "Assets/XAssetManifest.asset";
    private BuildTarget m_BuildTarget = BuildTarget.iOS;
    private BuilPlayerTarget m_BuildPlayerTarget = BuilPlayerTarget.iOS;
    private CompressionType m_CompressionType = CompressionType.LZ4;
    private bool m_IsClearFolder = false;
    private string m_BuildOutPath;
    private bool m_IsOnDisable = false;
    private SerializedObject m_BuildPlayerOptSerializedObject;

    public override string GetName()
    {
        return NAME;
    }

    public override void OnEnable()
    {
        string path = EditorPrefs.GetString(PREFSBUILDOUTPATH);
        path = string.IsNullOrEmpty(path) ? Path.Combine(Application.dataPath, "../A_Build/") : path;
        this.m_BuildOutPath = path;
        m_BuildPlayerOptSerializedObject = new SerializedObject(ScriptableObject.CreateInstance<XBuildPlayer.BuildPlayerOpt>());

        m_BuildTarget = (BuildTarget)EditorPrefs.GetInt(BUILDTARGETINT, 13);
        m_BuildPlayerTarget = (BuilPlayerTarget)EditorPrefs.GetInt(BUILDPLAYERTARGETINT,0);

    }

    public override void OnDisable()
    {
        m_IsOnDisable = true;
    }

    public override void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("编译资源", MessageType.Info);
        m_BuildTarget = (BuildTarget)EditorGUILayout.EnumPopup("平台", m_BuildTarget);
        EditorPrefs.SetInt(BUILDTARGETINT, (int)m_BuildTarget);

        m_CompressionType = (CompressionType)EditorGUILayout.EnumPopup("压缩方式", m_CompressionType);
        
        m_IsClearFolder = EditorGUILayout.Toggle("清除所有资源", m_IsClearFolder);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("打包路径", this.m_BuildOutPath);
        if (GUILayout.Button("选择路径", GUILayout.Width(100)))
        {
            string npath = EditorUtility.OpenFolderPanel("选择目录", "", "");
            if (!string.IsNullOrEmpty(npath))
            {
                this.m_BuildOutPath = npath;
                EditorPrefs.SetString(PREFSBUILDOUTPATH, m_BuildOutPath);
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        //if (GUILayout.Button("Build Lua"))
        //{
        //    BuildLuaParameter parameter = new BuildLuaParameter();
        //    parameter.luaDirectory = LuaProject.LuaRootPath;
        //    parameter.buildAssetBundleOptions = BuildAssetBundleOptions.None |
        //                                  BuildAssetBundleOptions.IgnoreTypeTreeChanges |
        //                                  BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension;
        //    parameter.buildTarget = m_BuildTarget;
        //    parameter.isClearFolder = m_IsClearFolder;
        //    parameter.outputPath = this.m_BuildOutPath;
        //    System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
        //    XBuildLua.Build(parameter);
        //    sw.Stop();
        //    Debug.Log("time: " + sw.ElapsedMilliseconds * 0.001f);
        //    EditorUtility.OpenWithDefaultApp(parameter.outputPath);
        //}

//        if (GUILayout.Button("Build Art"))
//        {
//            BuildResourceParameter parameter = new BuildResourceParameter();

//            if (m_CompressionType == CompressionType.NONE)
//                parameter.buildAssetBundleOptions = BuildAssetBundleOptions.UncompressedAssetBundle;
//            else if (m_CompressionType == CompressionType.LZ4)
//                parameter.buildAssetBundleOptions = BuildAssetBundleOptions.ChunkBasedCompression;
//            else
//                parameter.buildAssetBundleOptions = BuildAssetBundleOptions.None;

//            parameter.buildTarget = m_BuildTarget;
//            parameter.isClearFolder = m_IsClearFolder;
//            parameter.outputPath = this.m_BuildOutPath;
//            //parameter.buildBundleName = BuildResourceParameter.NameType.NONE;
//#if UNITY_IOS
//            parameter.buildBundleName = BuildResourceParameter.NameType.HASH;
//#else
//            parameter.buildBundleName = BuildResourceParameter.NameType.NONE;
//#endif
//            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
//            XBuildArt.Build(parameter);
//            sw.Stop();
//            Debug.Log("time: " + sw.ElapsedMilliseconds * 0.001f);
//            EditorUtility.OpenWithDefaultApp(parameter.outputPath);
//        }

        if (GUILayout.Button("Build Dev"))
        {
            BuildResourceParameter parameter = new BuildResourceParameter();

            if (m_CompressionType == CompressionType.NONE)
                parameter.buildAssetBundleOptions = BuildAssetBundleOptions.UncompressedAssetBundle;
            else if (m_CompressionType == CompressionType.LZ4)
                parameter.buildAssetBundleOptions = BuildAssetBundleOptions.ChunkBasedCompression;
            else
                parameter.buildAssetBundleOptions = BuildAssetBundleOptions.None;

            parameter.buildTarget = m_BuildTarget;
            parameter.isClearFolder = m_IsClearFolder;
            parameter.outputPath = this.m_BuildOutPath;
#if UNITY_IOS
            parameter.buildBundleName = BuildResourceParameter.NameType.HASH;
#else
            parameter.buildBundleName = BuildResourceParameter.NameType.NONE;
#endif
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
            XBuildDevelopment.Build(parameter);

            string manifestPath = Path.Combine(m_BuildOutPath, XBuildUtility.GetPlatformAtBuildTarget(m_BuildTarget));
            XBuildUtility.BuildAssetManifest(manifestPath, m_BuildTarget);

            sw.Stop();
            Debug.Log("time: " + sw.ElapsedMilliseconds * 0.001f);
            EditorUtility.OpenWithDefaultApp(parameter.outputPath);
        }

        if (GUILayout.Button("Refresh ORM"))
        {
            EditorSettings.spritePackerMode = SpritePackerMode.Disabled;
            string manifestPath = Path.Combine(m_BuildOutPath, XBuildUtility.GetPlatformAtBuildTarget(m_BuildTarget));
            XBuildUtility.BuildAssetManifest(manifestPath, m_BuildTarget);


            //string path = Path.Combine(m_BuildOutPath, XBuildUtility.GetPlatformAtBuildTarget(m_BuildTarget));
            ////AssetsFileOrm.OpenAll(path);
            //XAssetManifest manifest = ScriptableObject.CreateInstance<XAssetManifest>();
            //manifest.EditorInitManifest(path);
            //AssetDatabase.CreateAsset(manifest, c_TEMPPATH);
        }

        if (GUILayout.Button("Open Folder"))
        {
            EditorUtility.OpenWithDefaultApp(m_BuildOutPath);
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("编译包【PC、Android、IOS】", MessageType.Info);
        //m_BuildPlayerTarget = (BuilPlayerTarget)EditorGUILayout.EnumPopup("平台", m_BuildPlayerTarget);
        //EditorPrefs.SetInt(BUILDPLAYERTARGETINT, (int)m_BuildPlayerTarget);

        if (m_BuildPlayerOptSerializedObject != null && m_BuildPlayerOptSerializedObject.targetObject)
        {
            m_BuildPlayerOptSerializedObject.Update();

            var iterator = m_BuildPlayerOptSerializedObject.GetIterator();
            iterator.NextVisible(true);
            iterator.Next(false);
            iterator.Next(false);
            while (iterator.Next(false))
            {
                EditorGUILayout.PropertyField(iterator);
            }

            m_BuildPlayerOptSerializedObject.ApplyModifiedProperties();
        }

        if (GUILayout.Button("Build"))
        {

            XBuildPlayer.BuildPlayer(m_BuildPlayerOptSerializedObject.targetObject as XBuildPlayer.BuildPlayerOpt);
            EditorUtility.OpenWithDefaultApp(Path.Combine(Application.dataPath, "../"));
        }
    }

    public override void Update()
    {

    }
}
