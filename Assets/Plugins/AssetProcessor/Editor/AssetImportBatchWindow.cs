using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.U2D;
using UnityEngine.Assertions.Must;
using UnityEngine.U2D;
using Object = UnityEngine.Object;

public class AssetImportBatchWindow : EditorWindow
{


    [MenuItem("Window/AssetImportBatchWindow")]
    static void Open() { EditorWindow.GetWindow<AssetImportBatchWindow>().Show(); }


    enum AssetType { Texture, Model, Audio, SpriteAtlas }

    enum TexResolution : int { _32 = 32, _64 = 64, _128 = 128, _256 = 256, _512 = 512, _1024 = 1024, _2048 = 2048 }

    private List<string> m_Folders = new List<string>();
    private AssetType m_AssetType = AssetType.Texture;
    private AssetTreeView m_AssetTreeView;
    private string[] m_AssetGUIDs;
    private string filesStr = "";
    void OnGUI()
    {

        //EditorGUILayout.LabelField(filesStr, "box");
        EditorGUILayout.HelpBox(filesStr, MessageType.Info);
        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        float labelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 80f;
        m_AssetType = (AssetType)EditorGUILayout.EnumPopup("资源类型：", m_AssetType);
        EditorGUIUtility.labelWidth = labelWidth;
        if (EditorGUI.EndChangeCheck())
        {
            RefreshData();
        }

        if (GUILayout.Button("检测选中目录", GUILayout.Width(100)))
        {
            Refresh();
        }

        EditorGUILayout.EndHorizontal();


        if (m_AssetTreeView != null)
        {
            Rect rect = EditorGUILayout.GetControlRect();
            rect.x = 5;
            rect.width = position.width - rect.x * 2f;
            rect.height = (position.height - rect.y) - 5;
            m_AssetTreeView.OnGUI(rect);
        }



    }

    void Update()
    {
        string last = filesStr;
        filesStr = "";
        int count = 0;
        foreach (var guid in Selection.assetGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (AssetDatabase.IsValidFolder(path))
            {
                count++;
                filesStr += path + "\n";
                if (count > 3)
                {
                    filesStr += "...";
                    break;
                }

            }
        }
        if (last != filesStr) Repaint();
    }

    //void OnSelectionChange()
    //{
    //    Refresh();
    //}

    //void OnEnable()
    //{
    //    Refresh();
    //}

    void Refresh()
    {
        m_Folders.Clear();
        m_AssetGUIDs = Selection.assetGUIDs;
        foreach (var guid in m_AssetGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (AssetDatabase.IsValidFolder(path))
            {
                m_Folders.Add(path);
            }
        }

        RefreshData();
    }

    void RefreshData()
    {
        if (m_Folders.Count < 1) return;

        string type = "Texture";
        if (m_AssetType == AssetType.Model)
            type = "Model";
        else if (m_AssetType == AssetType.Audio)
            type = "AudioClip";
        else if (m_AssetType == AssetType.SpriteAtlas)
            type = "SpriteAtlas";
        string filter = string.Format("t:{0}", type);
        double stime = EditorApplication.timeSinceStartup;
        string[] results = AssetDatabase.FindAssets(filter, m_Folders.ToArray());
        List<AssetImporter> imports = new List<AssetImporter>();
        int count = 0;
        foreach (var item in results)
        {
            ++count;
            EditorUtility.DisplayProgressBar(String.Format("[{0}/{1}]", count, results.Length), AssetDatabase.GUIDToAssetPath(item), (float)count / (float)results.Length);
            imports.Add(AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(item)));
        }
        EditorUtility.ClearProgressBar();
        Debug.LogFormat("findTime:{0}, assetCount:{1}", EditorApplication.timeSinceStartup - stime, imports.Count);
        CreateTreeView(imports);
    }

    void CreateTreeView(List<AssetImporter> imports)
    {
        List<MultiColumnHeaderState.Column> columns = new List<MultiColumnHeaderState.Column>();
        columns.Add(new MultiColumnHeaderState.Column { headerContent = new GUIContent("路径"), width = 300f });
        if (m_AssetType == AssetType.Texture)
        {
            columns.Add(new MultiColumnHeaderState.Column { headerContent = new GUIContent("Read/Write Enabled"), minWidth = 120f, width = 120f, maxWidth = 120f });
            columns.Add(new MultiColumnHeaderState.Column { headerContent = new GUIContent("Streaming mipmap"), minWidth = 120f, width = 120f, maxWidth = 120f });
            columns.Add(new MultiColumnHeaderState.Column { headerContent = new GUIContent("Generate mipmap"), minWidth = 120f, width = 120f, maxWidth = 120f });
            columns.Add(new MultiColumnHeaderState.Column { headerContent = new GUIContent("Android Format"), minWidth = 110f, width = 110f, maxWidth = 110f });
            columns.Add(new MultiColumnHeaderState.Column { headerContent = new GUIContent("Android Max Size"), minWidth = 100f, width = 100f, maxWidth = 100f });
        }
        else if (m_AssetType == AssetType.Model)
        {
            columns.Add(new MultiColumnHeaderState.Column { headerContent = new GUIContent("Read/Write Enabled"), minWidth = 120f, width = 120f, maxWidth = 120f });
            columns.Add(new MultiColumnHeaderState.Column { headerContent = new GUIContent("Color"), minWidth = 120f, width = 120f, maxWidth = 120f });
            columns.Add(new MultiColumnHeaderState.Column { headerContent = new GUIContent("UV2"), minWidth = 120f, width = 120f, maxWidth = 120f });
            columns.Add(new MultiColumnHeaderState.Column { headerContent = new GUIContent("UV3-8"), minWidth = 120f, width = 120f, maxWidth = 120f });

        }
        else if (m_AssetType == AssetType.Audio)
        {
            columns.Add(new MultiColumnHeaderState.Column { headerContent = new GUIContent("Length"), minWidth = 120f, width = 120f, maxWidth = 120f });
            columns.Add(new MultiColumnHeaderState.Column { headerContent = new GUIContent("Force To Mono"), minWidth = 120f, width = 120f, maxWidth = 120f });
            columns.Add(new MultiColumnHeaderState.Column { headerContent = new GUIContent("Load In Background"), minWidth = 120f, width = 120f, maxWidth = 120f });
            columns.Add(new MultiColumnHeaderState.Column { headerContent = new GUIContent("Load Type"), minWidth = 120f, width = 120f, maxWidth = 120f });
        }
        else if (m_AssetType == AssetType.SpriteAtlas)
        {
            columns.Add(new MultiColumnHeaderState.Column { headerContent = new GUIContent("Read/Write Enabled"), minWidth = 120f, width = 120f, maxWidth = 120f });
            columns.Add(new MultiColumnHeaderState.Column { headerContent = new GUIContent("Generate mipmap"), minWidth = 120f, width = 120f, maxWidth = 120f });
            columns.Add(new MultiColumnHeaderState.Column { headerContent = new GUIContent("Android Format"), minWidth = 110f, width = 110f, maxWidth = 110f });
        }

        MultiColumnHeader header = new MultiColumnHeader(new MultiColumnHeaderState(columns.ToArray()));
        m_AssetTreeView = new AssetTreeView(new TreeViewState(), header);
        m_AssetTreeView.SetData(imports);
    }



    #region TreeView

    class AssetTreeViewItem : TreeViewItem
    {
        public AssetImporter import;
        public TextureImporter textureImport;
        public ModelImporter modelImport;
        public AudioImporter audioImporter;
        public bool isSpriteAtlas;
        public string name;
        public bool isChange = false;

        public AssetTreeViewItem(int id, int depth, string displayName) : base(id, depth, displayName)
        {

        }

        public override string displayName
        {
            get
            {
                if (string.IsNullOrEmpty(name))
                    name = Path.GetFileName(base.displayName);
                return name;
            }

            set
            {
                base.displayName = value;
            }
        }

        public string assetPath()
        {
            return base.displayName;
        }

        public Mesh GetMesh()
        {
            if (!modelImport) return null;
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(modelImport.assetPath);
            return mesh;
        }

        public AudioClip GetAudio()
        {
            if (!audioImporter) return null;
            return AssetDatabase.LoadAssetAtPath<AudioClip>(audioImporter.assetPath);
        }

        public SpriteAtlas GetSpriteAtlas()
        {
            if (!import) return null;
            return AssetDatabase.LoadAssetAtPath<SpriteAtlas>(import.assetPath);
        }

        public bool ExitsColor()
        {
            return GetMesh().colors.Length > 0;
        }

        public bool ExitsUV2()
        {
            return GetMesh().uv2.Length > 0;
        }

        public bool ExitsUV38()
        {
            return GetMesh().uv3.Length > 0;
        }
    }

    class AssetTreeView : TreeView
    {
        private List<AssetImporter> m_AssetImporterList;
        public AssetTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader) : base(state, multiColumnHeader)
        {
            multiColumnHeader.height = 22;
            showBorder = true;
            showAlternatingRowBackgrounds = true;
            multiColumnHeader.sortingChanged += OnSorting;
        }

        public void SetData(List<AssetImporter> imports)
        {
            m_AssetImporterList = imports;
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            TreeViewItem root = new TreeViewItem(0, -1, "root");
            root.children = new List<TreeViewItem>();

            int id = 0;
            foreach (AssetImporter item in m_AssetImporterList)
            {
                AssetTreeViewItem titem = new AssetTreeViewItem(++id, 0, item.assetPath);
                titem.import = item;
                titem.textureImport = item as TextureImporter;
                titem.modelImport = item as ModelImporter;
                titem.audioImporter = item as AudioImporter;
                titem.isSpriteAtlas = item.assetPath.EndsWith(".spriteatlas");

                root.AddChild(titem);
            }
            SetupDepthsFromParentsAndChildren(root);
            return root;
        }

        protected override void DoubleClickedItem(int id)
        {
            base.DoubleClickedItem(id);

            AssetTreeViewItem item = FindItem(id, rootItem) as AssetTreeViewItem;
            string path = "";
            if (item.textureImport)
            {
                path = item.textureImport.assetPath;
            }
            else if (item.modelImport)
            {
                path = item.modelImport.assetPath;
            }
            else if (item.audioImporter)
            {
                path = item.audioImporter.assetPath;
            }
            else if (item.isSpriteAtlas)
            {
                path = item.import.assetPath;
            }

            Object obj = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (obj) EditorGUIUtility.PingObject(obj);
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            base.RowGUI(args);
            AssetTreeViewItem item = args.item as AssetTreeViewItem;
            if (item != null)
            {
                for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
                {
                    CellGUI(args.GetCellRect(i), item, args.GetColumn(i), ref args);
                }
            }
        }

        void CellGUI(Rect cellRect, AssetTreeViewItem item, int column, ref RowGUIArgs args)
        {
            float labelWidth = EditorGUIUtility.labelWidth;
            if (column == 1)
            {
                EditorGUIUtility.labelWidth = 70f;
                EditorGUI.BeginChangeCheck();
                if (item.textureImport)
                    item.textureImport.isReadable = EditorGUI.Toggle(cellRect, "Read/Write ", item.textureImport.isReadable);
                else if (item.modelImport)
                    item.modelImport.isReadable = EditorGUI.Toggle(cellRect, "Read/Write ", item.modelImport.isReadable);
                else if (item.audioImporter)
                {
                    EditorGUI.LabelField(cellRect, item.GetAudio().length.ToString());
                }
                else if (item.isSpriteAtlas)
                {
                    SpriteAtlasTextureSettings sats = item.GetSpriteAtlas().GetTextureSettings();
                    sats.readable = EditorGUI.Toggle(cellRect, "Read/Write ", sats.readable);
                    item.GetSpriteAtlas().SetTextureSettings(sats);
                }
                if (EditorGUI.EndChangeCheck())
                {
                    item.isChange = true;
                    SaveReload(item);
                    ForEachSelecteds(item, (AssetTreeViewItem fitem) =>
                    {
                        if (fitem.textureImport)
                            fitem.textureImport.isReadable = item.textureImport.isReadable;
                        else if (fitem.modelImport)
                            fitem.modelImport.isReadable = item.modelImport.isReadable;
                        else if (fitem.isSpriteAtlas)
                        {
                            SpriteAtlasTextureSettings sats = item.GetSpriteAtlas().GetTextureSettings();
                            SpriteAtlasTextureSettings fsats = fitem.GetSpriteAtlas().GetTextureSettings();
                            fsats.readable = sats.readable;
                            fitem.GetSpriteAtlas().SetTextureSettings(fsats);
                        }
                        fitem.isChange = true;
                        SaveReload(fitem);
                    });
                }
            }
            else if (column == 2)
            {
                EditorGUIUtility.labelWidth = 40f;
                EditorGUI.BeginChangeCheck();
                if (item.textureImport)
                    item.textureImport.streamingMipmaps = EditorGUI.Toggle(cellRect, "S mipmap ", item.textureImport.streamingMipmaps);
                else if (item.modelImport)
                {
                    EditorGUI.Toggle(cellRect, "Color ", item.ExitsColor());
                }
                else if (item.audioImporter)
                {
                    item.audioImporter.forceToMono = EditorGUI.Toggle(cellRect, "", item.audioImporter.forceToMono);
                }
                else if (item.isSpriteAtlas)
                {
                    EditorGUIUtility.labelWidth = 52f;
                    SpriteAtlasTextureSettings sats = item.GetSpriteAtlas().GetTextureSettings();
                    sats.generateMipMaps = EditorGUI.Toggle(cellRect, "mip map ", sats.generateMipMaps);
                    item.GetSpriteAtlas().SetTextureSettings(sats);
                }
                if (EditorGUI.EndChangeCheck())
                {
                    item.isChange = true;
                    if (item.textureImport)
                        SaveReload(item);
                    ForEachSelecteds(item, (AssetTreeViewItem fitem) =>
                    {
                        if (fitem.textureImport)
                            fitem.textureImport.streamingMipmaps = item.textureImport.streamingMipmaps;
                        else if (fitem.modelImport)
                        {
                            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(fitem.modelImport.assetPath);
                            MeshFilter[] mfs = go.GetComponentsInChildren<MeshFilter>();
                            List<Color> colors = null;
                            foreach (var mf in mfs)
                            {
                                mf.sharedMesh.SetColors(colors);
                            }
                        }
                        else if (fitem.audioImporter)
                        {
                            fitem.audioImporter.forceToMono = item.audioImporter.forceToMono;
                        }
                        else if (fitem.isSpriteAtlas)
                        {
                            SpriteAtlasTextureSettings sats = item.GetSpriteAtlas().GetTextureSettings();
                            SpriteAtlasTextureSettings fsats = fitem.GetSpriteAtlas().GetTextureSettings();
                            fsats.generateMipMaps = sats.generateMipMaps;
                            fitem.GetSpriteAtlas().SetTextureSettings(fsats);
                        }

                        fitem.isChange = true;
                        if (fitem.textureImport || fitem.audioImporter)
                            SaveReload(fitem);
                    });
                }
            }
            else if (column == 3)
            {
                EditorGUIUtility.labelWidth = 32f;
                EditorGUI.BeginChangeCheck();
                if (item.textureImport)
                    item.textureImport.mipmapEnabled = EditorGUI.Toggle(cellRect, "mip map ", item.textureImport.mipmapEnabled);
                else if (item.modelImport)
                {
                    //Mesh mesh = item.GetMesh();
                    EditorGUI.Toggle(cellRect, "UV2 ", item.ExitsUV2());
                }
                else if (item.audioImporter)
                {
                    item.audioImporter.loadInBackground = EditorGUI.Toggle(cellRect, "", item.audioImporter.loadInBackground);
                }
                else if (item.isSpriteAtlas)
                {
                    TextureImporterPlatformSettings tips = item.GetSpriteAtlas().GetPlatformSettings("Android");
                    tips.format = (TextureImporterFormat)EditorGUI.EnumPopup(cellRect, tips.format);
                    tips.overridden = tips.format != TextureImporterFormat.Automatic;
                    item.GetSpriteAtlas().SetPlatformSettings(tips);
                }
                if (EditorGUI.EndChangeCheck())
                {
                    item.isChange = true;
                    if (item.textureImport || item.audioImporter)
                        SaveReload(item);
                    ForEachSelecteds(item, (AssetTreeViewItem fitem) =>
                    {
                        if (fitem.textureImport)
                        {
                            fitem.textureImport.mipmapEnabled = item.textureImport.mipmapEnabled;

                        }
                        else if (fitem.modelImport)
                        {
                            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(fitem.modelImport.assetPath);
                            MeshFilter[] mfs = go.GetComponentsInChildren<MeshFilter>();
                            List<Vector2> uv = null;
                            foreach (var mf in mfs)
                            {

                                mf.sharedMesh.SetUVs(1, uv);
                            }
                        }
                        else if (fitem.audioImporter)
                        {
                            fitem.audioImporter.loadInBackground = item.audioImporter.loadInBackground;
                        }
                        else if (item.isSpriteAtlas)
                        {
                            TextureImporterPlatformSettings tips = item.GetSpriteAtlas().GetPlatformSettings("Android");
                            TextureImporterPlatformSettings ftips = fitem.GetSpriteAtlas().GetPlatformSettings("Android");
                            ftips.overridden = tips.overridden;
                            ftips.format = tips.format;
                            fitem.GetSpriteAtlas().SetPlatformSettings(ftips);
                        }

                        fitem.isChange = true;
                        if (fitem.textureImport || fitem.audioImporter)
                            SaveReload(fitem);
                    });
                }
            }
            else if (column == 4)
            {
                EditorGUI.BeginChangeCheck();
                if (item.textureImport)
                {
                    Color contentColor = GUI.contentColor;
                    if (item.textureImport.DoesSourceTextureHaveAlpha())
                    {
                        GUI.contentColor = new Color(0.1f, 0.9f, 0.98f, 1f);
                    }
                    TextureImporterPlatformSettings tips = item.textureImport.GetPlatformTextureSettings("Android");
                    tips.format = (TextureImporterFormat)EditorGUI.EnumPopup(cellRect, tips.format);
                    item.textureImport.SetPlatformTextureSettings(tips);
                    GUI.contentColor = contentColor;
                }
                else if (item.modelImport)
                {
                    EditorGUIUtility.labelWidth = 40f;
                    Mesh mesh = item.GetMesh();
                    EditorGUI.Toggle(cellRect, "UV3-8 ", item.ExitsUV38());
                }
                else if (item.audioImporter)
                {
                    AudioImporterSampleSettings aiss = item.audioImporter.defaultSampleSettings;
                    aiss.loadType = (AudioClipLoadType)EditorGUI.EnumPopup(cellRect, aiss.loadType);
                    item.audioImporter.defaultSampleSettings = aiss;
                }
                if (EditorGUI.EndChangeCheck())
                {
                    item.isChange = true;
                    if (item.textureImport)
                        SaveReload(item);
                    ForEachSelecteds(item, (AssetTreeViewItem fitem) =>
                    {
                        if (fitem.textureImport)
                        {
                            TextureImporterPlatformSettings tips = item.textureImport.GetPlatformTextureSettings("Android");
                            TextureImporterPlatformSettings ftips = fitem.textureImport.GetPlatformTextureSettings("Android");
                            ftips.format = tips.format;
                            fitem.textureImport.SetPlatformTextureSettings(ftips);
                            SaveReload(fitem);
                        }
                        else if (fitem.modelImport)
                        {
                            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(fitem.modelImport.assetPath);
                            MeshFilter[] mfs = go.GetComponentsInChildren<MeshFilter>();
                            List<Vector2> uv = null;
                            foreach (var mf in mfs)
                            {
                                mf.sharedMesh.SetUVs(2, uv);
                                mf.sharedMesh.SetUVs(3, uv);
                                mf.sharedMesh.SetUVs(4, uv);
                                mf.sharedMesh.SetUVs(5, uv);
                                mf.sharedMesh.SetUVs(6, uv);
                                mf.sharedMesh.SetUVs(7, uv);
                            }
                        }
                        else if (fitem.audioImporter)
                        {
                            AudioImporterSampleSettings aiss = item.audioImporter.defaultSampleSettings;
                            AudioImporterSampleSettings faiss = fitem.audioImporter.defaultSampleSettings;
                            faiss.loadType = aiss.loadType;
                            fitem.audioImporter.defaultSampleSettings = faiss;
                            SaveReload(fitem);
                        }
                        fitem.isChange = true;
                    });
                }
            }

            else if (column == 5)
            {
                EditorGUI.BeginChangeCheck();
                if (item.textureImport)
                {
                    TextureImporterPlatformSettings tips = item.textureImport.GetPlatformTextureSettings("Android");
                    tips.maxTextureSize = (int)((TexResolution)EditorGUI.EnumPopup(cellRect, (TexResolution)tips.maxTextureSize));
                    item.textureImport.SetPlatformTextureSettings(tips);
                }
                if (EditorGUI.EndChangeCheck())
                {
                    item.isChange = true;
                    SaveReload(item);
                    ForEachSelecteds(item, (AssetTreeViewItem fitem) =>
                     {
                         if (fitem.textureImport)
                         {
                             TextureImporterPlatformSettings tips = item.textureImport.GetPlatformTextureSettings("Android");
                             TextureImporterPlatformSettings ftips = fitem.textureImport.GetPlatformTextureSettings("Android");
                             ftips.maxTextureSize = tips.maxTextureSize;
                             fitem.textureImport.SetPlatformTextureSettings(ftips);
                             SaveReload(fitem);
                         }
                         fitem.isChange = true;
                     });
                }
            }

            EditorGUIUtility.labelWidth = labelWidth;
        }

        private void ForEachSelecteds(AssetTreeViewItem clickItem, Action<AssetTreeViewItem> callback)
        {
            IList<int> list = GetSelection();

            if (list.Count < 2)
            {
                callback.Invoke(clickItem);
                return;
            }

            foreach (var id in list)
            {
                callback.Invoke(FindItem(id, rootItem) as AssetTreeViewItem);
            }
        }

        private void SaveReload(AssetTreeViewItem item)
        {
            if (item.textureImport)
                item.textureImport.SaveAndReimport();
            else if (item.modelImport)
                item.modelImport.SaveAndReimport();
            else if (item.audioImporter)
                item.audioImporter.SaveAndReimport();
        }


        private void OnSorting(MultiColumnHeader multicolumnheader)
        {
            int columnIdx = multicolumnheader.sortedColumnIndex;
            bool isAscending = multicolumnheader.IsSortedAscending(columnIdx);


            rootItem.children.Sort((TreeViewItem a, TreeViewItem b) =>
            {
                AssetTreeViewItem ata = (AssetTreeViewItem)a;
                AssetTreeViewItem atb = (AssetTreeViewItem)b;

                if (columnIdx == 0)
                {
                    return isAscending ? ata.import.assetPath.CompareTo(atb.import.assetPath) : atb.import.assetPath.CompareTo(ata.import.assetPath);
                }

                if (columnIdx == 1)
                {
                    if (ata.textureImport)
                    {

                        return isAscending ? ata.textureImport.isReadable.CompareTo(atb.textureImport.isReadable) : atb.textureImport.isReadable.CompareTo(ata.textureImport.isReadable);
                    }
                    else if (ata.modelImport)
                    {

                        return isAscending ? ata.modelImport.isReadable.CompareTo(atb.modelImport.isReadable) : atb.modelImport.isReadable.CompareTo(ata.modelImport.isReadable);
                    }
                    else if (ata.audioImporter)
                    {
                        return isAscending ? ata.GetAudio().length.CompareTo(atb.GetAudio().length) : atb.GetAudio().length.CompareTo(ata.GetAudio().length);
                    }
                    else if (ata.isSpriteAtlas)
                    {
                        return isAscending ? ata.GetSpriteAtlas().GetTextureSettings().readable.CompareTo(atb.GetSpriteAtlas().GetTextureSettings().readable) : 
                            atb.GetSpriteAtlas().GetTextureSettings().readable.CompareTo(ata.GetSpriteAtlas().GetTextureSettings().readable);
                    }
                }

                if (columnIdx == 2)
                {
                    if (ata.textureImport)
                    {
                        return isAscending ? ata.textureImport.streamingMipmaps.CompareTo(atb.textureImport.streamingMipmaps) : atb.textureImport.streamingMipmaps.CompareTo(ata.textureImport.streamingMipmaps);
                    }
                    else if (ata.modelImport)
                    {
                        return isAscending ? ata.ExitsColor().CompareTo(atb.ExitsColor()) : atb.ExitsColor().CompareTo(ata.ExitsColor());
                    }
                    else if (ata.audioImporter)
                    {
                        return isAscending ? ata.audioImporter.forceToMono.CompareTo(atb.audioImporter.forceToMono) : atb.audioImporter.forceToMono.CompareTo(ata.audioImporter.forceToMono);
                    }
                    else if (ata.isSpriteAtlas)
                    {
                        return isAscending ? ata.GetSpriteAtlas().GetTextureSettings().generateMipMaps.CompareTo(atb.GetSpriteAtlas().GetTextureSettings().generateMipMaps) :
                            atb.GetSpriteAtlas().GetTextureSettings().generateMipMaps.CompareTo(ata.GetSpriteAtlas().GetTextureSettings().generateMipMaps);
                    }
                }


                if (columnIdx == 3)
                {
                    if (ata.textureImport)
                    {
                        return isAscending ? ata.textureImport.mipmapEnabled.CompareTo(atb.textureImport.mipmapEnabled) : atb.textureImport.mipmapEnabled.CompareTo(ata.textureImport.mipmapEnabled);
                    }
                    else if (ata.modelImport)
                    {
                        return isAscending ? ata.ExitsUV2().CompareTo(atb.ExitsUV2()) : atb.ExitsUV2().CompareTo(ata.ExitsUV2());

                    }
                    else if (ata.audioImporter)
                    {
                        return isAscending ? ata.audioImporter.loadInBackground.CompareTo(atb.audioImporter.loadInBackground) : atb.audioImporter.loadInBackground.CompareTo(ata.audioImporter.loadInBackground);
                    }
                    else if (ata.isSpriteAtlas)
                    {
                        TextureImporterPlatformSettings tipsa = ata.GetSpriteAtlas().GetPlatformSettings("Android");
                        TextureImporterPlatformSettings tipsb = atb.GetSpriteAtlas().GetPlatformSettings("Android");
                        return isAscending ? tipsa.format.CompareTo(tipsb.format) : tipsb.format.CompareTo(tipsa.format);
                    }
                }

                if (columnIdx == 4)
                {
                    if (ata.textureImport)
                    {
                        TextureImporterPlatformSettings tipsa = ata.textureImport.GetPlatformTextureSettings("Android");
                        TextureImporterPlatformSettings tipsb = atb.textureImport.GetPlatformTextureSettings("Android");

                        return isAscending ? tipsa.format.CompareTo(tipsb.format) : tipsb.format.CompareTo(tipsa.format);
                    }
                    else if (ata.modelImport)
                    {
                        return isAscending ? ata.ExitsUV38().CompareTo(atb.ExitsUV38()) : atb.ExitsUV38().CompareTo(ata.ExitsUV38());
                    }
                    else if (ata.audioImporter)
                    {
                        return isAscending ? ata.audioImporter.defaultSampleSettings.loadType.CompareTo(atb.audioImporter.defaultSampleSettings.loadType) : atb.audioImporter.defaultSampleSettings.loadType.CompareTo(ata.audioImporter.defaultSampleSettings.loadType);
                    }
                }

                if (columnIdx == 5)
                {
                    if (ata.textureImport)
                    {
                        TextureImporterPlatformSettings tipsa = ata.textureImport.GetPlatformTextureSettings("Android");
                        TextureImporterPlatformSettings tipsb = atb.textureImport.GetPlatformTextureSettings("Android");
                        return isAscending ? tipsa.maxTextureSize.CompareTo(tipsb.maxTextureSize) : tipsb.maxTextureSize.CompareTo(tipsa.maxTextureSize);
                    }
                    else
                    {
                    }
                }

                return 1;
            });

            m_AssetImporterList.Clear();
            foreach (var item in rootItem.children)
            {
                m_AssetImporterList.Add((item as AssetTreeViewItem).import);
            }
            Reload();
        }
    }
    #endregion
}