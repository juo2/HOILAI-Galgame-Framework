using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AssetManagement;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine.Events;
using XRawObjectInfo = AssetProfilerDetail.XRawObjectInfo;

public class CacheRawObjectTreeView : TreeView
{

    public class CacheRawObjectTreeItem : TreeViewItem
    {
        public XRawObjectInfo objectInfo;
        public Object go;
        public bool isChild;
        public CacheRawObjectTreeItem(int id, int depth, string displayName) : base(id, depth, displayName)
        {

        }
    }


    private List<XRawObjectInfo> m_Datas;
    private Dictionary<XRawObjectInfo, int> m_RawInfoToId;
    public UnityAction callRefresh;
    public UnityAction<XRawObjectInfo> onItemClick;
    public CacheRawObjectTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader) : base(state, multiColumnHeader)
    {
        showBorder = true;
        showAlternatingRowBackgrounds = true;
    }

    public void Refresh(List<XRawObjectInfo> data)
    {
        m_Datas = data;
        Reload();
    }

    public void SelectedRawObject(XRawObjectInfo rawInfo)
    {
        int id;
        if (m_RawInfoToId.TryGetValue(rawInfo, out id))
        {
            List<int> ids = new List<int>();
            TreeViewItem item = FindItem(id, rootItem);
            while (item != null && item != rootItem)
            {
                ids.Add(item.id);
                item = item.parent != null ? FindItem(item.parent.id, rootItem) : null;
            }
            SetFocus();
            SetExpandedRecursive(ids[ids.Count - 1], true);
            SetSelection(new List<int>() { id }, TreeViewSelectionOptions.FireSelectionChanged | TreeViewSelectionOptions.RevealAndFrame);
        }
    }

    void CallRefresh()
    {
        if (callRefresh != null) callRefresh.Invoke();
    }

    protected override TreeViewItem BuildRoot()
    {
        var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
        m_RawInfoToId = new Dictionary<XRawObjectInfo, int>();
        int id = 0;
        List<TreeViewItem> list = new List<TreeViewItem>();
        m_Datas.Sort((XRawObjectInfo a, XRawObjectInfo b) => { return b.assetName.CompareTo(a.assetName); });
        foreach (var item in m_Datas)
        {
            CacheRawObjectTreeItem titem = new CacheRawObjectTreeItem(++id, 0, item.assetName);
            m_RawInfoToId.Add(item, titem.id);
            titem.objectInfo = item;
            if (item.instanceObjects != null && item.instanceObjects.Count > 0)
            {
                titem.children = new List<TreeViewItem>();
                foreach (var citem in item.instanceObjects)
                {
                    CacheRawObjectTreeItem child_titem = new CacheRawObjectTreeItem(++id, 1, citem != null ? citem.name : "-");
                    child_titem.go = citem;
                    child_titem.isChild = true;
                    titem.children.Add(child_titem);
                }

            }

            if (item.instanceObjectNames != null && item.instanceObjectNames.Count > 0)
            {
                titem.children = new List<TreeViewItem>();
                foreach (var citem in item.instanceObjectNames)
                {
                    CacheRawObjectTreeItem child_titem = new CacheRawObjectTreeItem(++id, 1, citem);
                    child_titem.go = null;
                    child_titem.isChild = true;
                    titem.children.Add(child_titem);
                }
            }

            list.Add(titem);
        }

        SetupParentsAndChildrenFromDepths(root, list);
        return root;
    }

    public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState()
    {
        var columns = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("AssetName"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 200,
                    //minWidth = 320,
                    //maxWidth = 320,
                    autoResize = false,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Object"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Left,
                    width = 200,
                    //minWidth = 280,
                    maxWidth = 280,
                    autoResize = true
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("ReferenceCount"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Left,
                    width = 50,
                    minWidth = 130,
                    maxWidth = 130,
                    autoResize = true,
                    allowToggleVisibility = true
                },
                //new MultiColumnHeaderState.Column
                //{
                //    headerContent = new GUIContent("--"),
                //    headerTextAlignment = TextAlignment.Center,
                //    sortedAscending = true,
                //    sortingArrowAlignment = TextAlignment.Left,
                //    width = 40,
                //    autoResize = true,
                //    allowToggleVisibility = true
                //},
            };
        var state = new MultiColumnHeaderState(columns);
        return state;
    }

    protected override void RowGUI(RowGUIArgs args)
    {
        CacheRawObjectTreeItem item = args.item as CacheRawObjectTreeItem;
        base.RowGUI(args);
        if (item != null)
        {
            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(args.GetCellRect(i), item, args.GetColumn(i), ref args);
            }
        }

    }

    void CellGUI(Rect cellRect, CacheRawObjectTreeItem item, int column, ref RowGUIArgs args)
    {
        CenterRectUsingSingleLineHeight(ref cellRect);
        switch (column)
        {
            case 0:
                break;
            case 1:
                if (item.isChild)
                {
                    EditorGUI.ObjectField(cellRect, item.go, typeof(Object), true);
                }
                else
                {
                    EditorGUI.ObjectField(cellRect, item.objectInfo.rawObject, typeof(Object), true);
                }


                break;
            case 2:
                if (item.isChild)
                {

                }
                else
                {
                    if (item.hasChildren)
                    {
                        EditorGUI.LabelField(cellRect, string.Format("{0} / ({1})", item.objectInfo.referenceCount.ToString(), item.children.Count));
                    }
                    else
                    {
                        EditorGUI.LabelField(cellRect, item.objectInfo.referenceCount.ToString());
                    }

                }
                break;
            //case 3:
            //    //if (item.isChild)
            //    {
            //        if (!item.hasChildren && item.objectInfo != null)
            //        {
            //            if (item.objectInfo.rawObject && GUI.Button(cellRect, "del"))
            //            {
            //                AssetUtility.DestroyAsset(item.objectInfo.rawObject);
            //            }
            //        }
            //        else if (item.go)
            //        {
            //            if (GUI.Button(cellRect, "del"))
            //            {
            //                AssetUtility.DestroyAsset(item.go);
            //            }
            //        }
            //    }
            //break;
            default:
                break;
        }
    }

    protected override bool CanRename(TreeViewItem item)
    {
        return true;
    }

    protected override void RenameEnded(RenameEndedArgs args)
    {
        base.RenameEnded(args);
    }

    protected override void ContextClickedItem(int id)
    {
        CacheRawObjectTreeItem item = (CacheRawObjectTreeItem)FindItem(id, rootItem);

        GenericMenu menu = new GenericMenu();

        if (!item.hasChildren && item.objectInfo != null)
        {
            if (item.objectInfo.rawObject)
            {
                menu.AddItem(new GUIContent("卸载源对象"), false, () =>
                 {
                     AssetUtility.DestroyAsset(item.objectInfo.rawObject);
                     CallRefresh();
                 });
            }
        }
        else if (item.go)
        {
            menu.AddItem(new GUIContent("卸载实例化对象"), false, () =>
            {
                AssetUtility.DestroyAsset(item.go);
                CallRefresh();
            });
        }

        menu.ShowAsContext();
    }

    protected override void DoubleClickedItem(int id)
    {
        CacheRawObjectTreeItem item = FindItem(id, rootItem) as CacheRawObjectTreeItem;
        if (item == null || item.objectInfo == null) return;
        if (onItemClick != null) onItemClick.Invoke(item.objectInfo);
    }
}
