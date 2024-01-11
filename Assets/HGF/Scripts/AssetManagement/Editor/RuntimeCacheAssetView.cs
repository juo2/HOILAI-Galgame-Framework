using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AssetManagement;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

public class RuntimeCacheAssetView
{
    private AssetProfilerDetail m_AssetProfilerDetail;
    private CacheRawObjectTreeView m_CacheRawObjectTreeView;
    private TreeViewState m_CacheRawObjectTreeViewState;

    private XAssetBundleTreeView m_XAssetBundleTreeView;
    private TreeViewState m_XAssetBundleTreeViewState;
    public RuntimeCacheAssetView()
    {
        m_CacheRawObjectTreeViewState = new TreeViewState();
        MultiColumnHeader header = new MultiColumnHeader(CacheRawObjectTreeView.CreateDefaultMultiColumnHeaderState());
        header.height = 20;
        m_CacheRawObjectTreeView = new CacheRawObjectTreeView(m_CacheRawObjectTreeViewState, header);
        m_CacheRawObjectTreeView.callRefresh = Refresh;
        m_CacheRawObjectTreeView.onItemClick = OnRawObjectTreeClickItem;
        Refresh();


        m_XAssetBundleTreeViewState = new TreeViewState();
        header = new MultiColumnHeader(XAssetBundleTreeView.CreateDefaultMultiColumnHeaderState());
        header.height = 20;
        m_XAssetBundleTreeView = new XAssetBundleTreeView(m_XAssetBundleTreeViewState, header);
        m_XAssetBundleTreeView.callRefresh = Refresh;
        m_XAssetBundleTreeView.onItemClick = OnAssetBundleTreeClickItem;
        Refresh();
    }




    public void OnGUI(Rect wrect)
    {
        Rect rect = EditorGUILayout.GetControlRect();
        rect.width = wrect.width * 0.5f;
        if (rect.width > 500)
            rect.width = 500;
        rect.height = wrect.height;
        m_CacheRawObjectTreeView.OnGUI(rect);

        rect.x = rect.width + 4;
        rect.width = wrect.width - rect.x - 4;
        m_XAssetBundleTreeView.OnGUI(rect);
    }

    public void Refresh()
    {
        m_AssetProfilerDetail = AssetProfilerDetail.CreateAssetProfilerDetail();

        if (m_CacheRawObjectTreeView != null)
        {
            m_CacheRawObjectTreeView.Refresh(m_AssetProfilerDetail.p_XRawObjectInfos);
        }


        if (m_XAssetBundleTreeView != null)
        {
            m_XAssetBundleTreeView.Refresh(m_AssetProfilerDetail.p_XAssetBundleInfos);
        }
    }

    private void OnRawObjectTreeClickItem(AssetProfilerDetail.XRawObjectInfo rawInfo)
    {
        m_XAssetBundleTreeView.SelectedRawObject(rawInfo);
    }

    private void OnAssetBundleTreeClickItem(AssetProfilerDetail.XRawObjectInfo rawInfo)
    {
        m_CacheRawObjectTreeView.SelectedRawObject(rawInfo);
    }

    public void Export()
    {
        if (m_AssetProfilerDetail == null)
            return;

        string path = EditorUtility.SaveFilePanel("保存", Path.Combine(Application.dataPath, "../"), "data", "assetcache");

        if (string.IsNullOrEmpty(path))
            return;

        if (File.Exists(path))
            File.Delete(path);

        m_AssetProfilerDetail.CSerialize(path);
    }

    public void Import()
    {
        string path = EditorUtility.OpenFilePanel("打开", Path.Combine(Application.dataPath, "../"), "assetcache");

        if (string.IsNullOrEmpty(path))
            return;

        if (!File.Exists(path))
            return;


        m_AssetProfilerDetail = AssetProfilerDetail.Deserialize(path);
        if (m_CacheRawObjectTreeView != null)
        {
            m_CacheRawObjectTreeView.Refresh(m_AssetProfilerDetail.p_XRawObjectInfos);
        }


        if (m_XAssetBundleTreeView != null)
        {
            m_XAssetBundleTreeView.Refresh(m_AssetProfilerDetail.p_XAssetBundleInfos);
        }
    }
}





