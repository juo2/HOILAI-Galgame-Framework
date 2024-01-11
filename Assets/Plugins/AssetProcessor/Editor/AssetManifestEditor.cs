using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Linq;
using System.Collections.Generic;

[CustomEditor(typeof(AssetManifest))]
public class AssetManifestEditor : Editor
{
    SerializedProperty m_MatchingFolder;
    SerializedProperty m_IgnoreFileType;
    SerializedProperty m_AssetList;

    TreeViewState m_TreeViewState;
    SimpleTreeView m_TreeView;

    SearchField m_SearchField;

    protected virtual void OnEnable()
    {
        m_MatchingFolder = serializedObject.FindProperty("m_MatchingFolder");
        m_IgnoreFileType = serializedObject.FindProperty("m_IgnoreFileType");
        m_AssetList = serializedObject.FindProperty("m_AssetList");



        m_TreeViewState = new TreeViewState();
        m_TreeView = new SimpleTreeView(m_TreeViewState);

        m_SearchField = new SearchField();
        m_SearchField.downOrUpArrowKeyPressed += m_TreeView.SetFocusAndEnsureSelectedItem;
        Refresh();
    }


    void Refresh()
    {
        if (m_TreeView != null)
        {
            AssetManifest am = (AssetManifest)serializedObject.targetObject;
            m_TreeView.SetData(am.AssetList);
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(m_MatchingFolder, true);
        EditorGUILayout.PropertyField(m_IgnoreFileType, true);

        DoToolbar();
        DoTreeView();
        serializedObject.ApplyModifiedProperties();
    }



    void DoToolbar()
    {
        if (m_TreeView == null || m_SearchField == null)
        {
            return;
        }

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        EditorGUILayout.LabelField("资源数量：" + m_AssetList.arraySize,GUILayout.Width(110f));
        m_TreeView.searchString = m_SearchField.OnToolbarGUI(m_TreeView.searchString);
        if (GUILayout.Button("刷新", "miniButton", GUILayout.Width(40f)))
        {
            AssetManifestProcessor.RefreshAll();
            Refresh();
        }
        EditorGUILayout.EndHorizontal();
    }


    void DoTreeView()
    {
        if (m_TreeView != null)
        {
            Rect rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);
            m_TreeView.OnGUI(rect);
        }

    }

    public class SimpleTreeView : TreeView
    {

        class AssetTreeViewItem : TreeViewItem
        {
            AssetManifest.AssetInfo m_Info;
            public AssetManifest.AssetInfo Info { get { return m_Info; } }
            public AssetTreeViewItem(AssetManifest.AssetInfo info, int id)
                : base(id, 0, info != null ? info.m_AssetName : "root")
            {
                this.m_Info = info;
            }
        }


        List<AssetManifest.AssetInfo> m_Datas;
        public SimpleTreeView(TreeViewState treeViewState)
            : base(treeViewState)
        {
            showBorder = true;
            showAlternatingRowBackgrounds = true;
        }


        public void SetData(List<AssetManifest.AssetInfo> data)
        {
            m_Datas = data;
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
            var allItems = new List<TreeViewItem>();

            
            for (int i = 0; i < m_Datas.Count; i++)
                allItems.Add(new AssetTreeViewItem(m_Datas[i], i));
            SetupParentsAndChildrenFromDepths(root, allItems);
            return root;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
           

            Rect rect = args.rowRect;
            rect.x += 200f;
            AssetTreeViewItem item = (AssetTreeViewItem)args.item;

            EditorGUI.LabelField(rect, item.Info.m_AssetPath);

            base.RowGUI(args);
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            IList<TreeViewItem> list = FindRows(selectedIds);
            int[] ids = selectedIds.ToArray();
            Object[] objs = new Object[ids.Length];
            for (int i = 0; i < ids.Length; )
            {
                string path = ((AssetTreeViewItem)list[i]).Info.m_AssetPath;
                objs[i] = AssetDatabase.LoadAssetAtPath<Object>(path);
                ids[i] = objs[i].GetInstanceID();
                break;
            }


            EditorGUIUtility.PingObject(ids[0]);
        }
    }


}