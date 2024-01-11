using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.IO;

public class XBuildWindow : EditorWindow
{
    public abstract class XBuildPage
    {
        protected EditorWindow m_Window;
        public void Init(EditorWindow window) { m_Window = window; }
        public abstract string GetName();
        public abstract void OnEnable();
        public abstract void OnDisable();
        public abstract void OnGUI();
        public abstract void Update();

    }

    int m_ActiveIndex;
    XBuildPage m_ActivePage;
    string[] m_PageNames;
    XBuildPage[] pages = new XBuildPage[] 
    { 
        new XAssetsBundlesPage(),
        //new CheckManifestFilePage(),
        new BuildAssetsPage(),
    };

    void OnEnable()
    {
        m_ActiveIndex = 0;
        m_ActivePage = pages[0];
        m_PageNames = new string[0];
        foreach (var item in pages)
        {
            item.Init(this);
            ArrayUtility.Add<string>(ref m_PageNames, item.GetName());
        }
        m_ActivePage.OnEnable();
    }

    void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space();

        EditorGUI.BeginChangeCheck();
        m_ActiveIndex = GUILayout.Toolbar(m_ActiveIndex, m_PageNames, GUILayout.Height(25));
        if (EditorGUI.EndChangeCheck())
        {
            m_ActivePage.OnDisable();
            m_ActivePage = pages[m_ActiveIndex];
            m_ActivePage.OnEnable();
        }
        EditorGUILayout.Space();
        EditorGUILayout.EndHorizontal();

        m_ActivePage.OnGUI();
    }


    void Update()
    {
        if (m_ActivePage != null) m_ActivePage.Update();
    }


    [MenuItem("XGame/XBuild")]
    static void Open()
    {
        EditorWindow.GetWindow<XBuildWindow>().Show();
    }
}