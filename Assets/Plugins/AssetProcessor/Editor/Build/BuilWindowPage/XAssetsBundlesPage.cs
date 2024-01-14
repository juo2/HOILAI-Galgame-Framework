using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;

public class XAssetsBundlesPage : XBuildWindow.XBuildPage
{
    public class DefultConfig
    {
        [SerializeField]
        private string[] m_TestDownloadUrls;
        public string testDownloadUrls
        {
            get
            {
                return m_TestDownloadUrls[0] + "Android/";
            }
        }
    }


    const string NAME = "AssetBundles";
    public override string GetName() { return NAME; }
    Rect rect { get { return new Rect(20, 20, m_Window.position.width - 40, m_Window.position.height - 40); } }

    private AssetBundleView m_AssetBundleView;
    private AssetBundleView m_AssetBundleView2;
    private AssetBundleRecordList m_AssetBundleRecordList;
    private DefultConfig m_DefultConfig;
    private bool m_IsRecordAssets;
    private bool m_IsCompareModel;

    public override void OnEnable()
    {
        m_DefultConfig = ReadDefaultConfig();

        string webPath = "";
        if (m_DefultConfig != null)
            webPath = m_DefultConfig.testDownloadUrls;

        m_AssetBundleView = new AssetBundleView(webPath);

        m_AssetBundleView2 = new AssetBundleView(webPath);
        m_AssetBundleView.SetCompareView(m_AssetBundleView2);
        m_AssetBundleView2.SetCompareView(m_AssetBundleView);

        m_AssetBundleRecordList = new AssetBundleRecordList();
        m_IsRecordAssets = EditorPrefs.GetBool("QuickMenuKey_LaunchGameRecordAssets", false);
    }


    DefultConfig ReadDefaultConfig()
    {
        string cfgPath = Path.Combine(Application.streamingAssetsPath, "default.xcfg");
        if (File.Exists(cfgPath))
        {
            DefultConfig cfg = JsonUtility.FromJson<DefultConfig>(File.ReadAllText(cfgPath));
            return cfg;
        }
        return null;
    }

    public override void OnDisable()
    {

    }

    public override void OnGUI()
    {
        float labelWidth = EditorGUIUtility.labelWidth;
        Rect compRect = rect;
        if (m_IsRecordAssets)
        {
            DoRecordAssets(rect);
        }
        else
        {
            compRect.width = 85;
            compRect.height = 20;
            EditorGUIUtility.labelWidth = 60;
            m_IsCompareModel = EditorGUI.Toggle(compRect, "对比模式：", m_IsCompareModel);
            compRect.x += compRect.width + 2;
        }

        compRect.width = 85;
        EditorGUIUtility.labelWidth = 60;
        m_AssetBundleView.assetBundleTreeView.isEditorInput = EditorGUI.Toggle(compRect, "编辑模式：", m_AssetBundleView.assetBundleTreeView.isEditorInput);
        EditorGUIUtility.labelWidth = labelWidth;



        if (m_IsRecordAssets) return;

        Rect r1 = rect;
        if (m_IsCompareModel)
            r1.width *= 0.5f;
        r1.height += 10;
        //EditorGUI.DrawRect(r1, Color.red);

        GUI.BeginGroup(r1);
        m_AssetBundleView.OnGUI(r1);
        GUI.EndGroup();

        if (m_IsCompareModel)
        {
            Rect r2 = rect;
            r2.width *= 0.5f;
            r2.x += r2.width + 2f;
            r2.height = r1.height;

            GUI.BeginGroup(r2);
            m_AssetBundleView2.OnGUI(r2);
            GUI.EndGroup();
        }


    }


    void DoRecordAssets(Rect rect)
    {
        Rect r1 = rect;
        r1.width *= 0.7f;
        r1.height += 10;
        GUI.BeginGroup(r1);
        m_AssetBundleView.OnGUI(r1);
        GUI.EndGroup();


        r1.y += 20;
        r1.x += r1.width + 2;
        r1.width = rect.width * 0.3f;
        m_AssetBundleRecordList.OnGUI(r1);
    }


    public override void Update()
    {
        if (Application.isPlaying && m_IsRecordAssets)
        {
            UpdateRecord();
        }
    }




    int count = 0;
    void UpdateRecord()
    {
        if (Time.frameCount % 100 != 0) return;

        if (AssetsRecord.s_CurrentRecord != null && AssetsRecord.s_CurrentRecord.p_Assets != null)
        {
            if (count == AssetsRecord.s_CurrentRecord.p_Assets.Count) return;
            count = AssetsRecord.s_CurrentRecord.p_Assets.Count;
            m_AssetBundleRecordList.treeView.Refresh(AssetsRecord.s_CurrentRecord.p_Assets);
            m_AssetBundleView.SetAssetList(AssetsRecord.s_CurrentRecord.p_Assets);
        }
    }
}