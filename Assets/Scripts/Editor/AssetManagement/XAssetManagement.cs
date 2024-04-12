using UnityEngine;
using UnityEditor;

public class XAssetManagement : EditorWindow
{
    private string[] m_Menus = new[] { "AssetCache", "AssetDownlaoder" };
    private int m_MenuSelectedIndex = 0;

    private RuntimeCacheAssetView m_RuntimeCacheAssetView;



    RuntimeCacheAssetView GetRuntimeCacheAssetView()
    {
        return m_RuntimeCacheAssetView ?? (m_RuntimeCacheAssetView = new RuntimeCacheAssetView());
    }

    private void OnEnable()
    {

    }

    private void OnDisable()
    {

    }


    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal("Toolbar");
        EditorGUILayout.BeginHorizontal();
        m_MenuSelectedIndex = GUILayout.Toolbar(m_MenuSelectedIndex, m_Menus, "ToolbarButton");
        GUILayout.FlexibleSpace();

        EditorGUI.BeginDisabledGroup(m_MenuSelectedIndex != 0);

        if (GUILayout.Button("Import", "ToolbarButton"))
        {
            GetRuntimeCacheAssetView().Import();
        }

        if (GUILayout.Button("Export", "ToolbarButton"))
        {
            GetRuntimeCacheAssetView().Export();
        }

        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        if (GUILayout.Button("Refresh", "ToolbarButton"))
        {
            if (m_MenuSelectedIndex == 0)
            {
                GetRuntimeCacheAssetView().Refresh();
            }
        }



        if (GUILayout.Button("UnloadUnusedObject", "ToolbarButton"))
        {
            if (AssetManagement.AssetBundleManager.Instance != null)
            {
                AssetManagement.AssetBundleManager.Instance.UnloadUnusedObject();
            }

        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndHorizontal();

        if (m_MenuSelectedIndex == 0)
        {
            GetRuntimeCacheAssetView().OnGUI(new Rect(0, 20, position.width, position.height - 20));
        }

    }


    [MenuItem("Window/XAssetManagement")]
    static void Open()
    {
        EditorWindow.GetWindow<XAssetManagement>().Show();
    }
}