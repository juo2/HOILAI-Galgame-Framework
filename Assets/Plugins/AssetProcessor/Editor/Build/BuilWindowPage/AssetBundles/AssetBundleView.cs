using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System;
public class AssetBundleView
{
    public UnityAction onChange { get; set; }

    private XAssetsFiles m_RecordFiles;
    public AssetBundleView toCompare { get; private set; }
    private TreeViewState m_TreeViewState;
    private AssetBundleTreeView m_AssetBundleTreeView;
    public AssetBundleTreeView assetBundleTreeView { get { return m_AssetBundleTreeView; } }
    private MultiColumnHeaderState m_MultiColumnHeaderState;
    private AssetBundleManifestDownload m_Download;
    public AssetBundleManifestDownload download { get { return m_Download; } }
    private string m_Path;

    private string m_VerStr;
    public AssetBundleView(string path)
    {
        this.m_Path = path;
        m_TreeViewState = new TreeViewState();
        m_MultiColumnHeaderState = AssetBundleTreeView.CreateDefaultMultiColumnHeaderState();
        MultiColumnHeader header = new AssetBundleTreeView.CustomMultiColumnHeader(m_MultiColumnHeaderState);
        header.height = 20f;
        header.ResizeToFit();
        m_AssetBundleTreeView = new AssetBundleTreeView(m_TreeViewState, header);
        m_Download = new AssetBundleManifestDownload();
        m_Download.onComplete += OnLoadComplete;


    }

    private void OnLoadComplete()
    {
        if (m_Download.version != null)
        {
            m_VerStr = string.Empty;
        }
        m_AssetBundleTreeView.xAssetsFiles = m_Download.files;
        m_AssetBundleTreeView.xAssetManifest = m_Download.manifest;
        m_AssetBundleTreeView.allAssetBundleInfo = m_Download.allBundleInfo;
        m_AssetBundleTreeView.allBundleNameToPath = m_Download.allBundleNameToHash;
        m_AssetBundleTreeView.webUrl = m_Path;
        if (m_RecordFiles != null)
            m_AssetBundleTreeView.recordAssetsFiles = m_RecordFiles;
        m_AssetBundleTreeView.Refresh();

        if (onChange != null)
            onChange.Invoke();
    }

    public void SetCompareView(AssetBundleView abview)
    {
        if (toCompare != null)
            toCompare.onChange = null;

        m_AssetBundleTreeView.toCompareTreeView = null;
        toCompare = abview;
        if (toCompare != null)
        {
            toCompare.onChange = OnCompareChange;
        }
    }

    void OnCompareChange()
    {
        m_AssetBundleTreeView.toCompareTreeView = toCompare.assetBundleTreeView;
        m_AssetBundleTreeView.Refresh();
    }

    public void Reload()
    {
        m_Download.Download(m_Path);
    }



    List<string> m_Temp = new List<string>();
    public void SetAssetList(List<string> assets)
    {
        if (m_Download.manifest != null && m_Download.files != null)
        {
            if (m_RecordFiles == null) 
                m_RecordFiles = new XAssetsFiles();
            m_RecordFiles.Clear();
            foreach (var item in assets)
            {
                string abName = m_Download.manifest.GetAssetBundleNameAtAssetName(item);
                m_Temp.Add(abName);
                m_Download.manifest.GetAllDependencies(abName, m_Temp);
                foreach (var bundleName in m_Temp)
                {
                    XAssetsFiles.FileStruct fs;
                    if (m_Download.files.allFilesMap.TryGetValue(bundleName, out fs))
                    {
                        m_RecordFiles.Add(fs);
                    }
                }
                m_Temp.Clear();
            }
            m_AssetBundleTreeView.recordAssetsFiles = m_RecordFiles;
            m_AssetBundleTreeView.Refresh();
        }
    }


    public void OnGUI(Rect rect)
    {

        Rect r = rect;
        r.x = 2f;
        r.width *= 0.6f;
        r.height = 16f;
        m_Path = EditorGUI.TextField(r, m_Path);
        r.x += r.width + 5;
        r.y += 2;
        r.width = 20f;
        if (GUI.Button(r, "", "Icon.AvatarMaskOn"))
            Reload();


        if (!string.IsNullOrEmpty(m_VerStr))
        {
            r.x += 20f;
            r.y -= 8;
            r.width = rect.width;
            r.height = 40f;
            GUI.Label(r, m_VerStr);
        }


        //GUILayout.Button(r, "保存录制");


        EditorGUI.BeginDisabledGroup(m_Download.isDonwload);
        m_AssetBundleTreeView.OnGUI(new Rect(0, 42, rect.width, rect.height - 62));
        EditorGUI.EndDisabledGroup();

        if (m_Download.isDonwload)
        {
            DoProgress(rect, m_Download.progress, m_Download.progressStr);
        }
    }

    void DoProgress(Rect rect, float v, string desc)
    {
        Rect label = rect;

        const float width = 150f;
        const float height = 8f;
        Rect r = new Rect((rect.width - width * 0.5f) * 0.5f, (rect.height - width * 0.5f) * 0.55f, width, height);
        label.y = r.y + height;
        label.x = r.x;

        EditorGUI.DrawRect(r, Color.black);
        r.width *= v;
        EditorGUI.DrawRect(r, Color.yellow);

        EditorGUI.LabelField(label, desc);

    }
}
