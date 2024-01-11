using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AssetManagement;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine.Events;
using XAssetBundleInfo = AssetProfilerDetail.XAssetBundleInfo;
using XRawObjectInfo = AssetProfilerDetail.XRawObjectInfo;

public class XAssetBundleTreeView : TreeView
{
    public class XAssetBundleTreeItem : TreeViewItem
    {
        public enum ItemNodeType { Bundle, RawObject, InstanceObject }
        public XAssetBundleInfo bundleInfo;
        public XRawObjectInfo rawObjectInfo;
        public Object go;
        public ItemNodeType nodeType;
        public XAssetBundleTreeItem(int id, int depth, string displayName) : base(id, depth, displayName)
        {

        }
    }

    private List<XAssetBundleInfo> m_Datas;
    private Dictionary<XRawObjectInfo, int> m_RawInfoToId;
    public UnityAction callRefresh;
    public UnityAction<XRawObjectInfo> onItemClick;
    public XAssetBundleTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader) : base(state, multiColumnHeader)
    {
        showBorder = true;
        showAlternatingRowBackgrounds = true;
    }

    public void Refresh(List<XAssetBundleInfo> data)
    {
        m_Datas = data;
        Debug.LogWarningFormat("List<XAssetBundleInfo> 数量：{0}", m_Datas.Count);
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
        root.children = new List<TreeViewItem>();
        List<TreeViewItem> list = new List<TreeViewItem>();
        m_RawInfoToId = new Dictionary<XRawObjectInfo, int>();
        m_Datas.Sort((XAssetBundleInfo a, XAssetBundleInfo b) => { return b.bundleName.CompareTo(a.bundleName); });
        int id = 0;
        foreach (var item in m_Datas)
        {
            XAssetBundleTreeItem bitem = new XAssetBundleTreeItem(++id, 0, item.bundleName);
            bitem.bundleInfo = item;
            bitem.nodeType = XAssetBundleTreeItem.ItemNodeType.Bundle;
            list.Add(bitem);

            if (item.rawObjects != null && item.rawObjects.Count > 0)
            {
                bitem.children = new List<TreeViewItem>();
                foreach (var ritem in item.rawObjects)
                {
                    XAssetBundleTreeItem rawitem = new XAssetBundleTreeItem(++id, 1, ritem.assetName);
                    m_RawInfoToId.Add(ritem, rawitem.id);
                    rawitem.nodeType = XAssetBundleTreeItem.ItemNodeType.RawObject;
                    rawitem.parent = bitem;
                    rawitem.rawObjectInfo = ritem;
                    bitem.children.Add(rawitem);

                    if (ritem.instanceObjects != null && ritem.instanceObjects.Count > 0)
                    {
                        rawitem.children = new List<TreeViewItem>();
                        foreach (var go in ritem.instanceObjects)
                        {
                            XAssetBundleTreeItem instanceitem = new XAssetBundleTreeItem(++id, 2, go != null ? go.name : "-");
                            instanceitem.go = go;
                            instanceitem.nodeType = XAssetBundleTreeItem.ItemNodeType.InstanceObject;
                            rawitem.children.Add(instanceitem);
                        }
                    }

                    if (ritem.instanceObjectNames != null && ritem.instanceObjectNames.Count > 0)
                    {
                        rawitem.children = new List<TreeViewItem>();
                        foreach (var gname in ritem.instanceObjectNames)
                        {
                            XAssetBundleTreeItem instanceitem = new XAssetBundleTreeItem(++id, 2, gname);
                            instanceitem.go = null;
                            instanceitem.nodeType = XAssetBundleTreeItem.ItemNodeType.InstanceObject;
                            rawitem.children.Add(instanceitem);
                        }
                    }
                }
            }
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
                    headerContent = new GUIContent("Name"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 600,
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
                    width = 100,
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
                    width = 100,
                    maxWidth = 100,
                    autoResize = true,
                    allowToggleVisibility = true
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("--"),
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Left,
                    //width = 50,
                    //maxWidth = 2,
                    autoResize = true,
                    allowToggleVisibility = true
                },

            };
        var state = new MultiColumnHeaderState(columns);
        return state;
    }

    protected override void RowGUI(RowGUIArgs args)
    {
        XAssetBundleTreeItem item = args.item as XAssetBundleTreeItem;
        base.RowGUI(args);
        if (item != null)
        {
            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(args.GetCellRect(i), item, args.GetColumn(i), ref args);
            }
        }

    }

    void CellGUI(Rect cellRect, XAssetBundleTreeItem item, int column, ref RowGUIArgs args)
    {
        CenterRectUsingSingleLineHeight(ref cellRect);
        switch (column)
        {
            case 0:
                break;
            case 1:
                if (item.nodeType == XAssetBundleTreeItem.ItemNodeType.Bundle)
                {
                    EditorGUI.LabelField(cellRect, string.Format("XAssetBundle [{0}]", item.bundleInfo.doneFrame));
                }
                else if (item.nodeType == XAssetBundleTreeItem.ItemNodeType.RawObject)
                {
                    EditorGUI.ObjectField(cellRect, item.rawObjectInfo.rawObject, typeof(Object), true);
                }
                else if (item.nodeType == XAssetBundleTreeItem.ItemNodeType.InstanceObject)
                {
                    EditorGUI.ObjectField(cellRect, item.go, typeof(Object), true);
                }
                break;
            case 2:

                if (item.nodeType == XAssetBundleTreeItem.ItemNodeType.Bundle)
                {
                    string countStr = string.Empty;
                    string root = item.bundleInfo.rootLoad ? "root" : "";
                    string parentNum = item.bundleInfo.parentNum > 0 ? item.bundleInfo.parentNum.ToString() : "";
                    if (item.hasChildren)
                    {
                        countStr = string.Format("{0}    ({1})   |  {2}  {3}", item.bundleInfo.newReferenceCount, item.bundleInfo.rawReferenceCount, root, parentNum);
                    }
                    else
                    {
                        countStr = string.Format("{0}  {1}  {2}", item.bundleInfo.newReferenceCount, root, parentNum);
                    }

                    EditorGUI.LabelField(cellRect, countStr);
                }
                else if (item.nodeType == XAssetBundleTreeItem.ItemNodeType.RawObject)
                {
                    if (item.hasChildren)
                    {
                        EditorGUI.LabelField(cellRect, string.Format("{0} / ({1})", item.rawObjectInfo.referenceCount.ToString(), item.children.Count));
                    }
                    else
                    {
                        EditorGUI.LabelField(cellRect, string.Format("{0}", item.rawObjectInfo.referenceCount.ToString()));
                    }
                }
                else if (item.nodeType == XAssetBundleTreeItem.ItemNodeType.InstanceObject)
                {

                }
                break;
            case 3:
                if (item.nodeType == XAssetBundleTreeItem.ItemNodeType.Bundle)
                {
                    string countStr = string.Empty;
                    if (item.bundleInfo.beginDestoryTime != -1 && item.bundleInfo.destoryTime != -1)
                    {
                        countStr = ((int)(Time.realtimeSinceStartup - item.bundleInfo.beginDestoryTime)).ToString();
                    }
                    EditorGUI.LabelField(cellRect, countStr);
                }
                break;
            default:
                break;
        }
    }



    protected override void ContextClickedItem(int id)
    {
        XAssetBundleTreeItem item = (XAssetBundleTreeItem)FindItem(id, rootItem);

        GenericMenu menu = new GenericMenu();
        if (item.nodeType == XAssetBundleTreeItem.ItemNodeType.Bundle)
        {
            menu.AddItem(new GUIContent("卸载"), false, () =>
            {
                //AssetBundleManager.Instance.UnloadAssetBundle(item.bundleInfo.bundleName);
                CallRefresh();
            });

            menu.AddItem(new GUIContent("反引用"), false, () =>
            {
                List<string> revRef;
                if (AssetBundleManager.Instance.loadReverseRefMap.TryGetValue(item.bundleInfo.bundleName, out revRef))
                {
                    Debug.LogWarningFormat("反引用：{0}  {1}=============", item.bundleInfo.bundleName, revRef.Count);
                    foreach (var refName in revRef)
                    {
                        Debug.LogWarningFormat("{0}", refName);
                    }
                }
            });
        }
        else if (item.nodeType == XAssetBundleTreeItem.ItemNodeType.RawObject)
        {
            if (!item.hasChildren && item.rawObjectInfo != null)
            {
                if (item.rawObjectInfo.rawObject)
                {
                    menu.AddItem(new GUIContent("卸载源对象"), false, () =>
                    {
                        AssetUtility.DestroyAsset(item.rawObjectInfo.rawObject);
                        CallRefresh();
                    });
                }
            }
        }
        else if (item.nodeType == XAssetBundleTreeItem.ItemNodeType.InstanceObject)
        {
            if (item.go)
            {
                menu.AddItem(new GUIContent("卸载实例化对象"), false, () =>
                {
                    AssetUtility.DestroyAsset(item.go);
                    CallRefresh();
                });
            }
        }

        menu.ShowAsContext();
    }

    protected override void DoubleClickedItem(int id)
    {

        XAssetBundleTreeItem item = FindItem(id, rootItem) as XAssetBundleTreeItem;
        if (item == null || item.rawObjectInfo == null) return;
        if (onItemClick != null) onItemClick.Invoke(item.rawObjectInfo);
    }

    protected override bool CanRename(TreeViewItem item)
    {
        return true;
    }

    protected override void RenameEnded(RenameEndedArgs args)
    {
        base.RenameEnded(args);
    }


    private SearchField m_SearchField;
    private string m_SearchString;
    public override void OnGUI(Rect rect)
    {
        if (m_SearchField == null)
            m_SearchField = new SearchField();

        Rect search = rect;
        search.height = 20;
        string lastStr = m_SearchString;
        m_SearchString = m_SearchField.OnGUI(search, m_SearchString);
        this.searchString = m_SearchString;

        rect.y += 20;
        rect.height -= 20;
        base.OnGUI(rect);
    }
}
