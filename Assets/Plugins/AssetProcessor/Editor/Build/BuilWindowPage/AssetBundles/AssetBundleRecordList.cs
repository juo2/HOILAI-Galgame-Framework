using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class AssetBundleRecordList
{
    private CustomTreeView m_CustomTreeView;
    private TreeViewState m_TreeViewState;
    private SearchField m_SearchField;
    public AssetBundleRecordList.CustomTreeView treeView { get { return m_CustomTreeView; } }
    public AssetBundleRecordList()
    {
        m_TreeViewState = new TreeViewState();
        m_CustomTreeView = new CustomTreeView(m_TreeViewState);
        m_SearchField = new SearchField();
        m_SearchField.downOrUpArrowKeyPressed += m_CustomTreeView.SetFocusAndEnsureSelectedItem;
    }

    public void OnGUI(Rect rect)
    {
        Rect searchRect = rect;
        searchRect.height = 22;
        searchRect.width = 60;
        if (GUI.Button(searchRect, "保存列表"))
        {
            SaveRecordInfo();
        }
        rect.y += 22;

        searchRect.x += searchRect.width + 2;
        searchRect.width = 60;
        if (GUI.Button(searchRect, "上传列表"))
        {
            UploadRecordInfo();
        }

        searchRect = rect;
        searchRect.height = 22;
        searchRect.width -= 50;
        m_CustomTreeView.searchString = m_SearchField.OnGUI(searchRect, m_CustomTreeView.searchString);
        
        searchRect.x += searchRect.width + 5f;
        searchRect.width = 100;

        EditorGUI.LabelField(searchRect, m_CustomTreeView.dataCount.ToString());

        rect.y += 20;
        rect.height -= 62;
        m_CustomTreeView.OnGUI(rect);
    }

    void SaveRecordInfo()
    {
        if (AssetsRecord.s_CurrentRecord != null && AssetsRecord.s_CurrentRecord.p_Assets != null)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in AssetsRecord.s_CurrentRecord.p_Assets)
            {
                sb.Append(item + "\n");
            }

            string fpath = EditorUtility.SaveFilePanel("保存Record列表", Path.GetDirectoryName(Application.dataPath), "recordList", "txt");
            if (string.IsNullOrEmpty(fpath)) return;

            StreamWriter sw = new StreamWriter(@fpath, false);
            sw.Write(sb.ToString());
            sw.Close();
            sw.Dispose();

            EditorUtility.OpenWithDefaultApp(Path.GetDirectoryName(fpath));
        }
    }
    void UploadRecordInfo()
    {
        string path = UnityEditor.EditorUtility.OpenFilePanel("选择文本", Application.dataPath, "*.*");

        StreamReader sr = new StreamReader(path, false);
        string line;
        List<string> pathNameList = new List<string>();
        while ((line = sr.ReadLine()) != null)
        {
            pathNameList.Insert(0,line.Trim());
        }
        sr.Close();
        sr.Dispose();
        foreach(var item in pathNameList)
        {
            if (!AssetsRecord.s_CurrentRecord.p_Assets.Contains(item))
            {
                AssetsRecord.s_CurrentRecord.p_Assets.Insert(0, item);
            }
        }
    }

    public class CustomTreeView : TreeView
    {
        private List<string> m_Assets;
        public CustomTreeView(TreeViewState state)
            : base(state)
        {
            showBorder = true;
            showAlternatingRowBackgrounds = true;
            Reload();
        }

        public void Refresh(List<string> list)
        {
            this.m_Assets = list;
            Reload();
        }

        public int dataCount { get { return this.m_Assets != null ? m_Assets.Count : 0; } }


        protected override TreeViewItem BuildRoot()
        {
            int id = 0;
            TreeViewItem root = new TreeViewItem(id++, -1, "root");
            root.children = new List<TreeViewItem>();
            if (this.m_Assets != null)
            {
                foreach (var item in this.m_Assets)
                {
                    root.children.Add(new TreeViewItem(id++, 0, item));
                }
            }

            SetupDepthsFromParentsAndChildren(root);
            return root;
        }
    }
}