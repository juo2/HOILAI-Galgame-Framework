using UnityEngine;
using System.Collections;
using UnityEditor.IMGUI.Controls;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using AssetBundleInfo = AssetsFileOrm.FileOrm.AssetBundleInfo;
using FileStruct = XAssetsFiles.FileStruct;
using System.Text;

public class AssetBundleTreeView : TreeView
{
    string focusName = string.Empty;
    bool isSearchSelect = false;
    int focusId = -99999;
    public enum NameShow
    {
        BundleName,
        FileStruct,
    }

    public enum CompareType
    {
        None,
        Change,
        New,
    }

    static int[] tags = new int[] { -2, -1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110 };
    static string[] tagsStr = new string[] { "All", "-1", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "101", "102", "103", "104", "105", "106", "107", "108", "109", "110" };

    public static List<FileStruct> currentShowFiles { get; private set; }
    public static long totalSize { get; private set; }
    public static NameShow nameShowType = NameShow.FileStruct;
    public static XAssetsFiles.FileOptions showOptions = XAssetsFiles.FileOptions.All;
    public static int showTags = -2;


    public class CustomMultiColumnHeader : MultiColumnHeader
    {
        public CustomMultiColumnHeader(MultiColumnHeaderState state) : base(state) { }
        protected override void ColumnHeaderGUI(MultiColumnHeaderState.Column column, Rect headerRect, int columnIndex)
        {
            string content = column.headerContent.text;
            if (content == "Name")
            {
                if (currentShowFiles != null)
                {
                    column.headerContent.text += " (" + currentShowFiles.Count + ")";
                }
            }
            else if (content == "Size")
            {
                column.headerContent.text += " (" + EditorUtility.FormatBytes(totalSize) + ")";
            }

            base.ColumnHeaderGUI(column, headerRect, columnIndex);




            column.headerContent.text = content;
        }
    }


    class CustomTreeViewItem : TreeViewItem
    {
        private long m_Size = -1;
        public AssetBundleInfo assetBundleInfo { get; private set; }
        public FileStruct fileStruct { get; private set; }
        public bool isFolder { get; private set; }
        public CompareType compareType { get; set; }
        public CustomTreeViewItem(int id, int depth, string displayName, XAssetsFiles.FileStruct fStruct, AssetBundleInfo info, bool folder = false)
            : base(id, depth, displayName)
        {
            //+ (info != null ? "\n" + info.p_AssetBundleName : ""
            isFolder = folder;
            assetBundleInfo = info;
            fileStruct = fStruct;
        }

        public override string displayName
        {
            get
            {
                string result = base.displayName;
                if (nameShowType == NameShow.BundleName || nameShowType == NameShow.FileStruct)
                    result = base.displayName;
                else
                    result = assetBundleInfo != null ? assetBundleInfo.p_AssetBundleName : base.displayName;
                return result;
                //if (children != null && children.Count > 0)
                //    return string.Format("{0} ({1})", base.displayName, children.Count);

            }
            set
            {
                base.displayName = value;
            }
        }


        public string md5
        {
            get { return fileStruct != null ? fileStruct.md5 : ""; }
        }

        //public FileOptions options { get { return fileStruct != null ? fileStruct.options :; } }

        public long size
        {
            get
            {
                if (m_Size == -1)
                {
                    if (isFolder && children != null && children.Count > 0)
                    {
                        m_Size = 0;
                        foreach (var item in children)
                            m_Size += ((CustomTreeViewItem)item).size;
                    }
                    else
                    {
                        m_Size = fileStruct != null ? fileStruct.size : 0;
                    }
                }
                return m_Size;
            }
        }
    }


    public XAssetsFiles xAssetsFiles { get; set; }
    public XAssetsFiles recordAssetsFiles { get; set; }
    public XAssetManifest xAssetManifest { get; set; }
    public Dictionary<string, AssetBundleInfo> allAssetBundleInfo { get; set; }
    public Dictionary<string, string> allBundleNameToPath { get; set; }
    public string webUrl { get; set; }

    public AssetBundleTreeView toCompareTreeView { get; set; }
    public bool isEditorInput { get; set; }
    public int gid { get; private set; }
    public bool onlyShowRecord { get; set; }
    private bool onlyChange = false;
    private SearchField m_SearchField;
    private string m_SearchString;

    int m_SortedColumnIndex;
    bool m_Ascending;

    public AssetBundleTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader)
        : base(state, multiColumnHeader)
    {
        //TreeView.DefaultStyles.label = new GUIStyle(EditorStyles.label);
        //TreeView.DefaultStyles.label.richText = true;
        showBorder = true;
        showAlternatingRowBackgrounds = true;
        rowHeight = 22;
        multiColumnHeader.sortingChanged += OnSortingChanged;


        m_SearchField = new SearchField();
        //m_SearchField.downOrUpArrowKeyPressed += SetFocusAndEnsureSelectedItem;

        Reload();
    }

    public void Refresh()
    {
        Reload();
    }



    protected override TreeViewItem BuildRoot()
    {
        FilterFiles();
        var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
        List<TreeViewItem> items = new List<TreeViewItem>();
        gid = 0;
        if (currentShowFiles != null && allAssetBundleInfo != null)
        {
            Dictionary<string, TreeViewItem> titems = new Dictionary<string, TreeViewItem>();
            foreach (var item in currentShowFiles)
            {
                DisplayFileStruct(titems, item, nameShowType == NameShow.FileStruct);
            }


            items = titems.Values.ToList<TreeViewItem>();

            root.children = new List<TreeViewItem>();
            foreach (var item in items)
            {
                if (item.parent == null)
                    root.children.Add(item);
            }

            foreach (var item in titems)
            {
                CustomTreeViewItem ctvi = (CustomTreeViewItem)item.Value;
                if (ctvi.assetBundleInfo != null)
                {
                    if (ctvi.children == null)
                        ctvi.children = new List<TreeViewItem>();
                    foreach (var asset in ctvi.assetBundleInfo.p_Assets)
                    {

                        ctvi.children.Add(new CustomTreeViewItem(++gid, 0, asset.p_AssetName, null, null, false));


                    }
                }

                if (ctvi.isFolder)
                    continue;

                if (toCompareTreeView != null && toCompareTreeView.xAssetsFiles != null && ctvi.fileStruct != null)
                {
                    FileStruct fs;
                    if (!toCompareTreeView.xAssetsFiles.allFilesMap.TryGetValue(ctvi.fileStruct.path, out fs))
                    {
                        ctvi.compareType = CompareType.New;
                    }
                    else if (fs.md5 != ctvi.fileStruct.md5)
                    {
                        ctvi.compareType = CompareType.Change;
                    }
                }
            }

            SetupDepthsFromParentsAndChildren(root);
        }
        else
            SetupParentsAndChildrenFromDepths(root, items);

        SortItems(root.children);
        return root;
    }

    //protected override void SearchChanged(string newSearch)
    //{
    //    if (string.IsNullOrEmpty(newSearch) || newSearch.StartsWith("dep:"))
    //    {
    //        Refresh();
    //    }
    //}

    void FilterFiles()
    {
        if (xAssetsFiles == null)
            return;

        if (currentShowFiles == null)
            currentShowFiles = new List<FileStruct>();
        else
            currentShowFiles.Clear();

        totalSize = 0;
        List<FileStruct> files = xAssetsFiles.p_AllFiles;

        HashSet<string> listSet = null;
        if (!string.IsNullOrEmpty(m_SearchString) && m_SearchString.StartsWith("list:"))
        {
            listSet = new HashSet<string>();
            string[] listdata = m_SearchString.Replace("list:", "").Split('|');
            foreach (var item in listdata)
            {
                string abname = item.Trim();
                if (!listSet.Contains(abname))
                    listSet.Add(abname);
            }

        }

        if (!string.IsNullOrEmpty(m_SearchString) && m_SearchString.StartsWith("rdep:"))
        {
            //反引用
            string abName = m_SearchString.Replace("rdep:", "");
            files = new List<FileStruct>();
            if (!string.IsNullOrEmpty(abName))
            {
                foreach (var item in xAssetsFiles.p_AllFiles)
                {
                    string[] deps = xAssetManifest.GetAllDependencies(item.path);
                    foreach (var dep in deps)
                    {
                        if (dep == abName)
                        {
                            if (xAssetsFiles.allFilesMap.ContainsKey(item.path))
                                files.Add(item);
                            break;
                        }

                    }
                }
            }
        }
        else if (!string.IsNullOrEmpty(m_SearchString) && m_SearchString.StartsWith("dep:"))
        {
            //搜索依赖
            string abName = m_SearchString.Replace("dep:", "");
            files = new List<FileStruct>();
            if (!string.IsNullOrEmpty(abName))
            {
                string[] deps = xAssetManifest.GetAllDependencies(abName);
                if (deps.Length > 0)
                    ArrayUtility.Add(ref deps, abName);
                foreach (var dep in deps)
                {
                    if (xAssetsFiles.allFilesMap.ContainsKey(dep))
                        files.Add(xAssetsFiles.allFilesMap[dep]);
                }
            }
        }


        foreach (var item in files)
        {
            if (listSet != null)
            {
                if (!listSet.Contains(item.path)) continue;
            }
            else if (!string.IsNullOrEmpty(m_SearchString) && !m_SearchString.StartsWith("dep:") && !m_SearchString.StartsWith("rdep:"))
            {
                if (!item.path.Contains(m_SearchString)) continue;
            }



            if (onlyChange)
            {
                FileStruct fs;
                if (toCompareTreeView != null && toCompareTreeView.xAssetsFiles != null)
                {
                    if (!toCompareTreeView.xAssetsFiles.allFilesMap.TryGetValue(item.path, out fs)) { }
                    else if (fs.md5 != item.md5) { }
                    else
                        continue;
                }
            }

            if (showTags != -2 && showTags != item.tag) continue;

            if (onlyShowRecord && recordAssetsFiles != null)
            {
                if (!recordAssetsFiles.allFilesMap.ContainsKey(item.path))
                    continue;
            }


            if (showOptions == XAssetsFiles.FileOptions.All
                || (showOptions == XAssetsFiles.FileOptions.NONE && item.options == XAssetsFiles.FileOptions.NONE)
                || (showOptions & item.options) != XAssetsFiles.FileOptions.NONE
                || showOptions == item.options)
            {
                totalSize += item.size;
                currentShowFiles.Add(item);
            }
        }
    }


    void DisplayFileStruct(Dictionary<string, TreeViewItem> dic, FileStruct fs, bool useFileStructDisplay, string ppath = "", bool isFolder = false)
    {
        AssetBundleInfo info;
        allAssetBundleInfo.TryGetValue(fs.path, out info);

        string path = string.IsNullOrEmpty(ppath) ? fs.path : ppath;

        if (string.IsNullOrEmpty(path))
            return;


        string parent = Path.GetDirectoryName(path);

        if (useFileStructDisplay)
        {
            if (!string.IsNullOrEmpty(parent) && !dic.ContainsKey(parent))
            {
                DisplayFileStruct(dic, fs, useFileStructDisplay, parent, true);
            }
        }

        TreeViewItem item;
        dic.TryGetValue(parent, out item);
        if (item != null)
        {
            if (!isFolder)
            {
                CustomTreeViewItem ctvi = new CustomTreeViewItem(++gid, 0, path, fs, info);
                item.AddChild(ctvi);
                //if (info.p_Assets != null && info.p_Assets.Count > 0)
                //{
                //    if (ctvi.children == null)
                //        ctvi.children = new List<TreeViewItem>();
                //    foreach (var asset in info.p_Assets)
                //    {
                //        ctvi.children.Add(new CustomTreeViewItem(++gid, 0, asset.p_AssetName, null, null, true));
                //    }
                //}
                dic.Add(path, ctvi);
            }
            else
            {
                CustomTreeViewItem ctvi = new CustomTreeViewItem(++gid, 0, path, fs, null, true);
                item.AddChild(ctvi);
                dic.Add(path, ctvi);
            }
        }
        else
            dic.Add(path, new CustomTreeViewItem(++gid, 0, path, fs, null, useFileStructDisplay));
    }


    public override void OnGUI(Rect rect)
    {
        float labelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 80F;

        Rect r = rect;

        r.x = rect.x;
        EditorGUI.BeginChangeCheck();
        NameShow last = nameShowType;
        r.height = 20f;
        r.width = 160f;
        nameShowType = (NameShow)EditorGUI.EnumPopup(r, "文件名显示：", nameShowType);
        if (EditorGUI.EndChangeCheck())
        {
            if (last == NameShow.FileStruct || nameShowType == NameShow.FileStruct)
                Refresh();
        }

        EditorGUIUtility.labelWidth = 55F;
        EditorGUI.BeginChangeCheck();
        r.x += r.width + 10;
        r.width = 150f;
        showOptions = (XAssetsFiles.FileOptions)EditorGUI.EnumFlagsField(r, "类型过滤：", showOptions);
        if (EditorGUI.EndChangeCheck())
        {
            Refresh();
        }


        EditorGUI.BeginChangeCheck();
        r.x += r.width + 10;
        showTags = EditorGUI.IntPopup(r, "Tag过滤：", showTags, tagsStr, tags);
        if (EditorGUI.EndChangeCheck())
        {
            Refresh();
        }



        r.x += r.width + 10;
        r.width = 40;
        Rect brect = r;
        Rect brect2 = r;
        brect.y -= 2;
        EditorGUI.BeginDisabledGroup(xAssetsFiles == null);
        {
            if (GUI.Button(brect, "保存"))
            {
                SaveFilesInfo();
            }

            brect.x += brect.width + 2;
            if (GUI.Button(brect, "导入"))
            {
                ImportFilesInfo();
            }

            brect.x += brect.width + 2;
            brect.width = 50;
            if (GUI.Button(brect, "下首包"))
            {
                DownloadBuildingAssets(webUrl);
            }
        }

        EditorGUIUtility.labelWidth = 55f;
        r.x += r.width * 10 + 80;
        r.width = 70;
        EditorGUI.BeginDisabledGroup(recordAssetsFiles == null);

        brect2.y -= -20f;
        brect2.width = 100;
        onlyShowRecord = EditorGUI.Toggle(brect2, "显示录制：", onlyShowRecord);
        if (EditorGUI.EndChangeCheck())
        {
            Refresh();
        }
        EditorGUI.EndDisabledGroup();

        brect2.x += brect2.width;
        EditorGUI.BeginDisabledGroup(xAssetsFiles == null || toCompareTreeView == null || toCompareTreeView.xAssetsFiles == null);
        EditorGUI.BeginChangeCheck();
        onlyChange = EditorGUI.Toggle(brect2, "显示改变：", onlyChange);
        if (EditorGUI.EndChangeCheck())
        {
            Refresh();
        }
        EditorGUI.EndDisabledGroup();
        EditorGUIUtility.labelWidth = labelWidth;



        r.x = rect.x;
        r.width = rect.width;
        r.height = 22;
        r.y = rect.y + 40;
        EditorGUI.BeginChangeCheck();

        string lastStr = m_SearchString;
        m_SearchString = m_SearchField.OnGUI(r, m_SearchString);
        if (EditorGUI.EndChangeCheck())
        {
            //searchString = m_SearchString;
            //if (!string.IsNullOrEmpty(lastStr) && lastStr.StartsWith("dep:") && !string.IsNullOrEmpty(m_SearchString) && !m_SearchString.StartsWith("dep:"))
            if (!string.IsNullOrEmpty(lastStr) && string.IsNullOrEmpty(m_SearchString))
            {
                isSearchSelect = true;
                //Debug.Log("取消了搜索。。。。。。。。。。。。。。。。");

            }

            Refresh();
        }


        rect.y += 60f;
        rect.height -= 20;
        //rect.height -= 50f;
        base.OnGUI(rect);
    }

    void SaveFilesInfo()
    {
        string fpath = EditorUtility.SaveFilePanel("保存列表设置", Path.GetDirectoryName(Application.dataPath), "files", "setting");
        if (string.IsNullOrEmpty(fpath)) return;
        XAssetsFiles files = new XAssetsFiles();
        files.Clear();
        foreach (var item in xAssetsFiles.p_AllFiles)
        {
            AssetBundleInfo info;
            if (allAssetBundleInfo.TryGetValue(item.path, out info))
            {
                FileStruct fs = new FileStruct();
                fs.path = info.p_AssetBundleName;
                string bpath;
                if (!allBundleNameToPath.TryGetValue(info.p_AssetBundleName, out bpath))
                {
                    Debug.LogWarning("AssetBundleTreeView::ImportFilesInfo   allBundleNameToPath not exist  name=" + item.path);
                }
                else
                {
                    if (info.p_AssetHashBundleName == info.p_AssetBundleHash)
                        fs.path = bpath;
                }
                fs.md5 = item.md5;
                fs.options = item.options;
                fs.tag = item.tag;
                fs.size = item.size;
                fs.priority = item.priority;
                files.Add(fs);
            }
        }
        string json = JsonUtility.ToJson(files, true);
        File.WriteAllText(fpath, json);

        //录制的所有资源
        string recordPath = Path.Combine(Path.GetDirectoryName(fpath), "filesRecord.setting");
        if (AssetsRecord.s_CurrentRecord != null && AssetsRecord.s_CurrentRecord.p_Assets != null)
        {
            using (StreamWriter sw = File.AppendText(recordPath))
            {
                foreach (var asset in AssetsRecord.s_CurrentRecord.p_Assets)
                {
                    sw.WriteLine(asset);
                }
                sw.Close();
                sw.Dispose();
            }
        }
        EditorUtility.OpenWithDefaultApp(Path.GetDirectoryName(fpath));
    }


    void ImportFilesInfo()
    {
        string fpath = EditorUtility.OpenFilePanel("导入列表设置", Path.GetDirectoryName(Application.dataPath), "setting");
        if (string.IsNullOrEmpty(fpath)) return;
        try
        {
            XAssetsFiles files = JsonUtility.FromJson<XAssetsFiles>(File.ReadAllText(fpath));
            foreach (var item in files.p_AllFiles)
            {
                string bpath;
                if (!allBundleNameToPath.TryGetValue(item.path, out bpath))
                {
                    Debug.LogWarning("AssetBundleTreeView::ImportFilesInfo   allBundleNameToPath not exist  name=" + item.path);
                }
                else
                {
                    FileStruct ofs;
                    if (xAssetsFiles.allFilesMap.TryGetValue(bpath, out ofs))
                    {
                        ofs.options = item.options;
                        ofs.priority = item.priority;
                        ofs.tag = item.tag;
                    }
                }

            }
            Refresh();
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("导入发生错误", e.ToString(), "知道了");
        }
    }

    Dictionary<string, string> extInputInfo = new Dictionary<string, string>();
    public struct extSetting
    {
        public sbyte tag;
        public short priority;
        public XAssetsFiles.FileOptions options;
    }
    void AddManualSetting()
    {
        if (xAssetsFiles == null) return;
        string fpath = EditorUtility.OpenFilePanel("选择配置文件", Path.GetDirectoryName(Application.dataPath), "txt");
        if (string.IsNullOrEmpty(fpath))
            return;
        extInputInfo.Clear();
        string[] strList = File.ReadAllLines(fpath);
        string setting = "";
        foreach (string strPtah in strList)
        {
            if (strPtah.Contains("{") && !strPtah.Contains("END"))
            {
                setting = strPtah;
                continue;
            }
            else if (strPtah == null || strPtah == "" || strPtah.Contains("END"))
            {
                continue;
            }
            extInputInfo[strPtah] = setting;
        }

        if (xAssetsFiles == null) return;
        foreach (var item in xAssetsFiles.p_AllFiles)
        {
            foreach (KeyValuePair<string, string> dic in extInputInfo)
            {
                if (item.path.Contains(dic.Key))
                {
                    FileStruct ofs;
                    if (xAssetsFiles.allFilesMap.TryGetValue(item.path, out ofs))
                    {
                        extSetting extData = JsonUtility.FromJson<extSetting>(dic.Value);
                        ofs.options = extData.options;
                        ofs.priority = extData.priority;
                        ofs.tag = extData.tag;
                    }
                    Debug.LogWarning(item.path + " ***已修改***");
                    break;
                }
            }
        }
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
                    width = 320,
                    minWidth = 320,
                    //maxWidth = 320,
                    autoResize = false,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Md5"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Left,
                    width = 280,
                    //minWidth = 280,
                    maxWidth = 280,
                    autoResize = true
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Size"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Left,
                    width = 130,
                    minWidth = 130,
                    maxWidth = 130,
                    autoResize = true,
                    allowToggleVisibility = true
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Options"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Left,
                    width = 130,
                    minWidth = 130,
                    maxWidth = 130,
                    autoResize = true
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Tag(包标记,分包依据)"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Left,
                    width = 130,
                    minWidth = 130,
                    maxWidth = 130,
                    autoResize = true
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("priority(优先级)"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Left,
                    width = 130,
                    minWidth = 130,
                    maxWidth = 130,
                    autoResize = true
                }
            };
        var state = new MultiColumnHeaderState(columns);
        return state;
    }

    protected override void RowGUI(RowGUIArgs args)
    {
        base.RowGUI(args);
        CustomTreeViewItem item = args.item as CustomTreeViewItem;
        if (item != null)
        {
            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(args.GetCellRect(i), item, args.GetColumn(i), ref args);
            }
        }

    }

    void CellGUI(Rect cellRect, CustomTreeViewItem item, int column, ref RowGUIArgs args)
    {
        var current = Event.current;
        if (current.type == EventType.MouseDown)
        {
            if (cellRect.Contains(current.mousePosition))
            {
                if (!string.IsNullOrEmpty(m_SearchString))
                {
                    //Debug.Log("拿到了focusName："+ item.displayName);
                    focusName = item.displayName;
                }
            }
        }

        CenterRectUsingSingleLineHeight(ref cellRect);
        switch (column)
        {
            case 0:
                //EditorGUI.LabelField(cellRect, item.fileStruct.path);
                Rect rect = cellRect;
                rect.x = cellRect.width - 20;
                //rect.y += 2;
                rect.width = 100;
                rect.height = 15;
                Color c = GUI.contentColor;
                GUI.contentColor = Color.green;
                if (item.compareType == CompareType.Change)
                {
                    GUI.Label(rect, "❂");
                }
                else if (item.compareType == CompareType.New)
                {
                    GUI.Label(rect, "✚");
                }
                GUI.contentColor = c;

                break;
            case 1:
                if (!item.isFolder)
                    EditorGUI.LabelField(cellRect, item.md5);
                break;
            case 2:
                EditorGUI.LabelField(cellRect, EditorUtility.FormatBytes(item.size));
                break;
            case 3:
                if (!item.isFolder && item.fileStruct != null)
                {
                    EditorGUI.BeginDisabledGroup(!isEditorInput);
                    EditorGUI.BeginChangeCheck();
                    XAssetsFiles.FileOptions optValue = (XAssetsFiles.FileOptions)EditorGUI.EnumFlagsField(cellRect, item.fileStruct.options);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (isEditorInput)
                        {
                            IList<TreeViewItem> tvis = FindRows(GetSelection());
                            if (tvis.Count > 1)
                            {
                                foreach (var itemRenderer in tvis)
                                {
                                    CustomTreeViewItem ctiv = itemRenderer as CustomTreeViewItem;
                                    if (ctiv.fileStruct != null)
                                    {
                                        ctiv.fileStruct.options = optValue;
                                    }
                                }
                            }
                            else
                            {
                                if (item.fileStruct != null)
                                    item.fileStruct.options = optValue;
                            }
                        }
                    }

                    EditorGUI.EndDisabledGroup();
                }

                break;
            case 4:
                if (!item.isFolder && item.fileStruct != null)
                {
                    EditorGUI.BeginDisabledGroup(!isEditorInput);
                    EditorGUI.BeginChangeCheck();
                    int tagValue = EditorGUI.IntPopup(cellRect, item.fileStruct.tag, tagsStr, tags);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (isEditorInput)
                        {
                            IList<TreeViewItem> tvis = FindRows(GetSelection());
                            if (tvis.Count > 1)
                            {
                                foreach (var itemRenderer in tvis)
                                {
                                    CustomTreeViewItem ctiv = itemRenderer as CustomTreeViewItem;
                                    if (ctiv.fileStruct != null)
                                    {
                                        ctiv.fileStruct.tag = (sbyte)tagValue;
                                    }
                                }
                            }
                            else
                            {
                                if (item.fileStruct != null)
                                    item.fileStruct.tag = (sbyte)tagValue;
                            }
                        }
                    }
                    EditorGUI.EndDisabledGroup();
                }
                break;
            case 5:
                if (!item.isFolder && item.fileStruct != null)
                {
                    EditorGUI.BeginDisabledGroup(!isEditorInput);
                    EditorGUI.BeginChangeCheck();

                    short value = short.Parse(EditorGUI.TextField(cellRect, item.fileStruct.priority.ToString()));
                    item.fileStruct.priority = (short)Mathf.Max(sbyte.MinValue, Mathf.Min(value, short.MaxValue));
                    //int value = EditorGUI.IntSlider(cellRect, item.fileStruct.priority, sbyte.MinValue, sbyte.MaxValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        //if (isEditorInput)
                        //{
                        //    IList<TreeViewItem> tvis = FindRows(GetSelection());
                        //    if (tvis.Count > 1)
                        //    {
                        //        foreach (var itemRenderer in tvis)
                        //        {
                        //            CustomTreeViewItem ctiv = itemRenderer as CustomTreeViewItem;
                        //            if (ctiv.fileStruct != null)
                        //            {
                        //                ctiv.fileStruct.priority = (sbyte)value;
                        //            }
                        //        }
                        //    }
                        //    else
                        //    {
                        //        if (item.fileStruct != null)
                        //            item.fileStruct.priority = (sbyte)value;
                        //    }
                        //}
                    }
                    EditorGUI.EndDisabledGroup();
                }
                break;
        }
    }

    //protected override void SearchChanged(string newSearch)
    //{
    //    base.SearchChanged(newSearch);
    //    if (string.IsNullOrEmpty(newSearch))
    //    {
    //        //foreach (var item in GetSelection())
    //        //{
    //        //    FrameItem(item);
    //        //}


    //        SetFocusAndEnsureSelectedItem();
    //        //IList<TreeViewItem> items = FindRows(GetSelection());
    //        //foreach (var item in items)
    //        //{
    //        //    if (item.parent != null)
    //        //    {
    //        //        SetExpanded(item.parent.id, true);
    //        //    }
    //        //}
    //    }
    //}

    void OnSortingChanged(MultiColumnHeader multiColumnHeader)
    {
        if (xAssetsFiles == null)
            return;

        m_SortedColumnIndex = multiColumnHeader.sortedColumnIndex;
        m_Ascending = multiColumnHeader.IsSortedAscending(m_SortedColumnIndex);
        xAssetsFiles.p_AllFiles.Sort(OnSort);
        Reload();
    }


    private void SortItems(List<TreeViewItem> items)
    {
        items.Sort(OnSort2);
        foreach (var item in items)
        {
            CustomTreeViewItem titem = item as CustomTreeViewItem;
            if (item.children != null && item.children.Count > 0 && titem.isFolder)
                SortItems(item.children);

            if (titem.displayName == focusName && isSearchSelect)
            {
                focusId = titem.id;


                isSearchSelect = false;
                //Debug.Log("SortItems");
                //Debug.Log(focusId);
            }
        }
    }

    protected override void AfterRowsGUI()
    {
        base.AfterRowsGUI();


        if (focusId != -99999)
        {
            //Debug.Log("AfterRowsGUI");
            List<int> ids = new List<int>();
            ids.Add(focusId);
            this.SetSelection(ids, TreeViewSelectionOptions.FireSelectionChanged | TreeViewSelectionOptions.RevealAndFrame);

            focusId = -99999;
        }
    }

    private int OnSort2(TreeViewItem xt, TreeViewItem yt)
    {
        CustomTreeViewItem x = xt as CustomTreeViewItem;
        CustomTreeViewItem y = yt as CustomTreeViewItem;

        switch (m_SortedColumnIndex)
        {
            case 0:
                return m_Ascending ? y.fileStruct.path.CompareTo(x.fileStruct.path) : x.fileStruct.path.CompareTo(y.fileStruct.path);
            case 1:
                return m_Ascending ? y.md5.CompareTo(x.md5) : x.md5.CompareTo(y.md5);
            case 2:
                return m_Ascending ? y.size.CompareTo(x.size) : x.size.CompareTo(y.size);
            case 3:
                return m_Ascending ? y.fileStruct.options.CompareTo(x.fileStruct.options) : x.fileStruct.options.CompareTo(y.fileStruct.options);
            default:
                return m_Ascending ? y.fileStruct.priority.CompareTo(x.fileStruct.priority) : x.fileStruct.priority.CompareTo(y.fileStruct.priority);
        }
    }


    private int OnSort(XAssetsFiles.FileStruct x, XAssetsFiles.FileStruct y)
    {
        switch (m_SortedColumnIndex)
        {
            case 0:
                return m_Ascending ? y.path.CompareTo(x.path) : x.path.CompareTo(y.path);
            case 1:
                return m_Ascending ? y.md5.CompareTo(x.md5) : x.md5.CompareTo(y.md5);
            case 2:
                return m_Ascending ? y.size.CompareTo(x.size) : x.size.CompareTo(y.size);
            case 3:
                return m_Ascending ? y.options.CompareTo(x.options) : x.options.CompareTo(y.options);
            default:
                return m_Ascending ? y.priority.CompareTo(x.priority) : x.priority.CompareTo(y.priority);
        }
    }

    private void DownloadBuildingAssets(string url)
    {
        string path = EditorUtility.OpenFolderPanel("存放路径", "", "");
        if (string.IsNullOrEmpty(path)) return;
        List<FileStruct> building = new List<FileStruct>();
        foreach (var item in xAssetsFiles.p_AllFiles)
        {
            if ((item.options & XAssetsFiles.FileOptions.BUILDING) == XAssetsFiles.FileOptions.BUILDING)
            {
                building.Add(item);
            }
            else if ((item.options & XAssetsFiles.FileOptions.LUA) == XAssetsFiles.FileOptions.LUA)
            {
                building.Add(item);
            }
        }

        if (building.Count < 1)
        {
            EditorUtility.DisplayDialog("下载首包文件", "没有需要下载的首包文件是否有生成！", "我知道了");
            return;
        }

        DownloadBuilding.StartDownload(url, path, building);
    }

    void OnAutoPiority()
    {
        short maxPiority = short.MaxValue;
        for (int i = recordAssetsFiles.p_AllFiles.Count - 1; i >= 0; i--)
        {
            foreach (var item in xAssetsFiles.p_AllFiles)
            {
                if (item.path == recordAssetsFiles.p_AllFiles[i].path && item.priority == -1)
                {
                    item.priority = maxPiority;
                    maxPiority--;
                }
            }
        }

        Refresh();
    }


    void OnCopyBuffFilter()
    {
        string data = GUIUtility.systemCopyBuffer;
        if (string.IsNullOrEmpty(data))
        {
            EditorUtility.DisplayDialog("提示", "错误剪切版数据为空", "知道了");
            return;
        }

        if (!(data.Contains("00/") || data.Contains("01/") || data.Contains("02/")))
        {
            EditorUtility.DisplayDialog("提示", "错误剪切版数据异常", "知道了");
            return;
        }


        string[] arr = data.Split('\n');
        for (int i = 0; i < arr.Length; i++)
        {
            if (arr[i].Contains("00/"))
            {
                arr[i] = arr[i].Substring(arr[i].IndexOf("00/"));
            }
            else if (arr[i].Contains("01/"))
            {
                arr[i] = arr[i].Substring(arr[i].IndexOf("01/"));
            }
            else if (arr[i].Contains("02/"))
            {
                arr[i] = arr[i].Substring(arr[i].IndexOf("02/"));
            }

        }
        StringBuilder sb = new StringBuilder();
        sb.Append("list:");
        foreach (var item in arr)
        {
            sb.AppendFormat("{0}|", item);
        }
        m_SearchString = sb.ToString();
        searchString = string.Empty;
        Refresh();
    }

    protected override void ContextClickedItem(int id)
    {
        CustomTreeViewItem item = (CustomTreeViewItem)FindItem(id, rootItem);
        if (!item.isFolder)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("查看依赖"), false, () =>
            {
                m_SearchString = "dep:" + item.displayName;
                searchString = string.Empty;
                Refresh();
            });

            menu.AddItem(new GUIContent("反引用"), false, () =>
            {

                m_SearchString = "rdep:" + item.displayName;
                searchString = string.Empty;
                Refresh();
            });
            menu.ShowAsContext();
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
}
