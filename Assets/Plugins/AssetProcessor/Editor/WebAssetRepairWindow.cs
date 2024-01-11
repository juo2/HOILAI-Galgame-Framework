using System;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor.IMGUI.Controls;
using UnityEngine.Networking;
using X_SimpleJSON;
using Object = UnityEngine.Object;

public class WebAssetRepairWindow : EditorWindow
{
    [MenuItem("Window/WebAssetRepairWindow")]
    static void Open() { EditorWindow.GetWindow<WebAssetRepairWindow>().Show(); }
    private Dictionary<string, float> m_ColumsWidth = new Dictionary<string, float>
    {
        {"资源路径",400f},
        {"发射粒子数",70f},
        {"贴图尺寸",50f},
        {"网格面数",50f},
        {"顶点数",70f},
        {"面数",70f},
    };



    private string m_Url;
    private string m_WebErrorStr;
    private JSONNode m_JSONNode;
    private WebAssetRepairTreeView m_WebAssetRepairTreeView;
    private SearchField m_SearchField;
    void OnGUI()
    {
        EditorGUILayout.Space();

        if (!string.IsNullOrEmpty(m_WebErrorStr))
            EditorGUILayout.LabelField(m_WebErrorStr);

        EditorGUILayout.BeginHorizontal();

        EditorGUI.BeginChangeCheck();
        float labelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 90;
        m_Url = EditorGUILayout.TextField("资源网址：", m_Url);
        if (EditorGUI.EndChangeCheck())
        {
            if (!string.IsNullOrEmpty(m_Url))
            {
                m_Url = UnityWebRequest.UnEscapeURL(m_Url);
            }
        }

        if (GUILayout.Button("->", GUILayout.Width(60)))
        {
            if (!string.IsNullOrEmpty(m_Url))
            {
                m_Url = UnityWebRequest.UnEscapeURL(m_Url);
                LoadWWWData(m_Url);
            }
        }

        EditorGUILayout.EndHorizontal();


        EditorGUIUtility.labelWidth = labelWidth;

        if (m_WebAssetRepairTreeView != null)
        {
            Rect rect = EditorGUILayout.GetControlRect();
            m_WebAssetRepairTreeView.searchString = m_SearchField.OnGUI(rect, m_WebAssetRepairTreeView.searchString);

            rect = EditorGUILayout.GetControlRect();
            rect.x = 5;
            rect.y += 5;
            rect.width = position.width - 10;
            rect.height = (position.height - rect.y) - 5;
            m_WebAssetRepairTreeView.OnGUI(rect);
        }
    }


    void LoadWWWData(string url)
    {
        m_WebErrorStr = "";
        string random = DateTime.Now.ToString("yyyymmddhhmmss");
        url += "&api=1&v=" + random;
        Debug.Log(url);
        UnityWebRequest uwr = UnityWebRequest.Get(url);
        UnityWebRequestAsyncOperation uwao = uwr.SendWebRequest();
        uwao.completed += (AsyncOperation ao) =>
        {
            if (!string.IsNullOrEmpty(uwr.error))
            {
                m_WebErrorStr = uwr.error;
                return;
            }

            try
            {
                m_JSONNode = JSON.Parse(uwr.downloadHandler.text);
                Refresh();
            }
            catch (Exception e)
            {
                m_WebErrorStr = e.ToString();
            }
        };

    }


    void Refresh()
    {
        //Debug.Log(m_JSONNode);
        CreateTreeView();
    }





    void CreateTreeView()
    {
        JSONNode tabStyle = m_JSONNode["tabStyle"];
        List<MultiColumnHeaderState.Column> columns = new List<MultiColumnHeaderState.Column>();
        
        for (int i = 0; i < tabStyle.Count; i++)
        {
            columns.Add(new MultiColumnHeaderState.Column
            {
                headerContent = i == 0 ? new GUIContent(tabStyle[i]["title"] + "   " + m_JSONNode["name"]) : new GUIContent(tabStyle[i]["title"]),
                headerTextAlignment = TextAlignment.Left,
                width = GetcolumsWidth(tabStyle[i]["title"]),
            });
        }

        var state = new MultiColumnHeaderState(columns.ToArray());
        m_WebAssetRepairTreeView = new WebAssetRepairTreeView(new TreeViewState(), new MultiColumnHeader(state));
        m_WebAssetRepairTreeView.SetData(m_JSONNode);
        m_WebAssetRepairTreeView.Reload();

        m_SearchField = new SearchField();
        m_SearchField.downOrUpArrowKeyPressed += m_WebAssetRepairTreeView.SetFocusAndEnsureSelectedItem;
    }


    float GetcolumsWidth(string str)
    {
        if (m_ColumsWidth.ContainsKey(str))
            return m_ColumsWidth[str];
        return 180f;
    }


    class WebAssetRepairTreeViewItem : TreeViewItem
    {
        public JSONNode tabStyle;
        public JSONNode data;
        //是否处理失败   -1未处理  0处理失败  1处理成功
        public int handleErr = -1;

        //public override string displayName
        //{
        //    get
        //    {
        //        return base.displayName;
        //    }

        //    set
        //    {
        //        base.displayName = value;
        //    }
        //}

        public WebAssetRepairTreeViewItem(int id, int depth, string displayName) : base(id, depth, displayName)
        {

        }

        public void InitAssetExist()
        {
            //Debug.Log(GetProjectAssetPath("mesh"));
        }


        public string GetProjectAssetPath(string type, string type2 = "")
        {
            string path = GetAssetPath();

            if (type == "skinRenderer" || type == "particle")
            {
                string assetName = GetAssetName();
                if (string.IsNullOrEmpty(assetName)) return "";
                string prefabPath = path + "/" + assetName.Substring(0, assetName.IndexOf("/")) + ".prefab";


                if (type2.StartsWith("没开启Read/Write选项") || type2.Contains("面数网格"))
                {
                    GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    string rePath = assetName.Substring(assetName.IndexOf("/") + 1);
                    Transform ts = go.transform.Find(rePath);
                    ParticleSystemRenderer psr = ts ? ts.gameObject.GetComponent<ParticleSystemRenderer>() : null;
                    if (psr && psr.mesh)
                    {
                        string meshPath = AssetDatabase.GetAssetPath(psr.mesh);
                        return AssetDatabase.AssetPathToGUID(meshPath);
                    }
                }


                return AssetDatabase.AssetPathToGUID(prefabPath);
            }


            string dir = Path.GetDirectoryName(path);
            string fname = Path.GetFileName(path);
            string filter = !string.IsNullOrEmpty(type) ? string.Format("{0} t:{1}", fname, type) : type;
            string[] assets = AssetDatabase.FindAssets(filter, new[] { dir });
            return assets.Length > 0 ? assets[0] : "";
        }

        public string GetAssetName()
        {
            return data["name"];
        }

        public string GetColumName(int idx)
        {
            return data[tabStyle[idx]["key"].Value].Value;
        }

        public string GetAssetPath()
        {
            return data["path"].Value;
        }


    }



    class WebAssetRepairTreeView : TreeView
    {
        private JSONNode m_JSONNode;
        private TreeViewItem m_Root;
        public WebAssetRepairTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader) : base(state, multiColumnHeader)
        {
            showBorder = true;
            showAlternatingRowBackgrounds = true;
            multiColumnHeader.height = 22;
            multiColumnHeader.sortingChanged += OnSortingChanged;
        }


        public void SetData(JSONNode node)
        {
            m_Root = null;
            m_JSONNode = node;
        }

        protected override TreeViewItem BuildRoot()
        {
            if (m_Root != null)
            {
                SetupDepthsFromParentsAndChildren(m_Root);
                return m_Root;
            }


            TreeViewItem root = new TreeViewItem(0, -1);
            root.children = new List<TreeViewItem>();
            int count = m_JSONNode["dlistData"].Count;
            int id = 1;
            for (int i = 0; i < count; i++)
            {
                string key = m_JSONNode["tabStyle"][0]["key"];
                JSONNode item = m_JSONNode["dlistData"][i];
                WebAssetRepairTreeViewItem titem = new WebAssetRepairTreeViewItem(++id, 0, item[key].Value);
                titem.tabStyle = m_JSONNode["tabStyle"];
                titem.data = item;
                titem.InitAssetExist();
                root.children.Add(titem);


                //if (i > 2)
                //    break;
            }

            Debug.Log(root.children.Count);

            SetupDepthsFromParentsAndChildren(root);
            m_Root = root;
            return root;
        }

        protected override bool CanRename(TreeViewItem item)
        {
            return true;
            //return base.CanRename(item);
        }








        protected override void RenameEnded(RenameEndedArgs args)
        {
            //base.RenameEnded(args);
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            base.RowGUI(args);
            WebAssetRepairTreeViewItem item = args.item as WebAssetRepairTreeViewItem;
            if (item != null)
            {
                for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
                {
                    CellGUI(args.GetCellRect(i), item, args.GetColumn(i), ref args);
                }
            }
        }

        void CellGUI(Rect cellRect, WebAssetRepairTreeViewItem item, int column, ref RowGUIArgs args)
        {
            if (column > 0)
            {
                string label = item.GetColumName(column);
                EditorGUI.LabelField(cellRect, label);
            }
        }

        protected override void ContextClicked()
        {
            CreateContextClickedMenu();
        }


        private void OnSortingChanged(MultiColumnHeader multicolumnheader)
        {
            int sortedColumnIndex = multicolumnheader.sortedColumnIndex;
            string key = m_JSONNode["tabStyle"][sortedColumnIndex]["key"].Value;
            bool ascending = multicolumnheader.IsSortedAscending(sortedColumnIndex);

            //int count = m_JSONNode["dlistData"].Count;
            //JSONArray listData = m_JSONNode["dlistData"] as JSONArray;

            //List<JSONNode> list = listData.Linq.OrderByDescending(t => t.Value[key]).ToList();

            //IEnumerable<JSONNode> query = from items in listData.Linq orderby items.Id descending select items;


            //foreach (var item in query)
            //{
            //    Console.WriteLine(item.Id + ":" + item.Name);
            //}

            //listData.Linq.ToList()
            //listData
            //for (int i = 0; i < count - 1; i++)
            //{
            //    for (int j = 0; j < count - 1; j++)
            //    {
            //        JSONNode a = listData[j];
            //        JSONNode b = listData[j + 1];
            //        int result = 0;


            //        JSONNode anode = a[key];
            //        //if (a[key].IsString)
            //        //    result = ascending ? a[key].Value.CompareTo(b[key].Value) : b[key].Value.CompareTo(a[key].Value);
            //        //else if (a[key].IsNumber)
            //        //    result = ascending ? a[key].AsDouble.CompareTo(b[key].AsDouble) : b[key].AsDouble.CompareTo(a[key].AsDouble);

            //        //JSONNode itema = a;
            //        //JSONNode itemb = b;
            //        ////if (result > 0)
            //        ////{
            //        ////    itema = b;
            //        ////    itemb = a;
            //        ////}

            //        //m_JSONNode["dlistData"][j] = itema;
            //        //m_JSONNode["dlistData"][j + 1] = itemb;
            //    }
            //}
            //Reload();
            //m_JSONNode["dlistData"].Linq.OrderByDescending(elm=>elm.Value).Where(n=>n.);
            //Debug.Log(ascending);
            m_Root.children.Sort((TreeViewItem a, TreeViewItem b) =>
            {
                WebAssetRepairTreeViewItem a2 = a as WebAssetRepairTreeViewItem;
                WebAssetRepairTreeViewItem b2 = b as WebAssetRepairTreeViewItem;
                if (a2.data[key].IsString)
                {
                    return ascending ? a2.data[key].Value.CompareTo(b2.data[key].Value) : b2.data[key].Value.CompareTo(a2.data[key].Value);
                }
                else if (a2.data[key].IsNumber)
                {
                    return ascending ? a2.data[key].AsDouble.CompareTo(b2.data[key].AsDouble) : b2.data[key].AsDouble.CompareTo(a2.data[key].AsDouble);
                }
                return 0;
            });
            int id = 0;
            foreach (var item in m_Root.children)
            {
                item.id = ++id;
            }
            Reload();
            //SetupDepthsFromParentsAndChildren(rootItem);
        }

        void OnMenuClick(object cmd)
        {
            int icmd = (int)cmd;


            if (icmd == 0)
            {
                List<int> ids = new List<int>();
                foreach (var child in rootItem.children)
                    ids.Add(child.id);
                SetSelection(ids);
            }
            else if (icmd == 1)
            {
                IList<int> sids = GetSelection();

                List<int> ids = new List<int>();
                foreach (var child in rootItem.children)
                    if (!sids.Contains(child.id))
                        ids.Add(child.id);
                SetSelection(ids);
            }
            else if (icmd == 2)
            {
                //IList<int> sids = GetSelection();
                //if (sids.Count == 1)
                //{
                //    WebAssetRepairTreeViewItem item = (WebAssetRepairTreeViewItem)FindItem(sids[0], rootItem);
                //    string guid = item.GetProjectAssetPath(GetHandleType(), m_JSONNode["name"]);
                //    if (!string.IsNullOrEmpty(guid))
                //    {
                //        string path = AssetDatabase.GUIDToAssetPath(guid);
                //        Object obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                //        EditorGUIUtility.PingObject(obj);
                //    }
                //}
            }
            else if (icmd == 3)
            {
                ExecuteOptimize();
            }
        }

        void CreateContextClickedMenu()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("全选"), false, OnMenuClick, 0);
            menu.AddItem(new GUIContent("反选"), false, OnMenuClick, 1);

            IList<int> sids = GetSelection();
            //if (sids.Count == 1)
            //{
            //    //WebAssetRepairTreeViewItem item = (WebAssetRepairTreeViewItem)FindItem(sids[0], rootItem);
            //    menu.AddItem(new GUIContent("跳转到资源"), false, OnMenuClick, 2);
            //}
            //else
            //{
            //    menu.AddDisabledItem(new GUIContent("跳转到资源"), false);
            //}
            menu.AddSeparator("");
            if (sids.Count > 0)
                menu.AddItem(new GUIContent("优化选中"), false, OnMenuClick, 3);
            else
                menu.AddDisabledItem(new GUIContent("优化选中"), false);

            menu.AddDisabledItem(new GUIContent("优化所有"), false);
            menu.ShowAsContext();
        }

        string GetHandleType()
        {
            int type = m_JSONNode["htype"];
            if (type == 1)
                return "Animation";
            else if (type == 2)
                return "Mesh";
            else if (type == 3)
                return "Texture2D";
            else if (type == 4)
                return "particle";
            else if (type == 5)
                return "skinRenderer";
            return "";
        }

        protected override void DoubleClickedItem(int id)
        {
            WebAssetRepairTreeViewItem item = (WebAssetRepairTreeViewItem)FindItem(id, rootItem);
            string guid = item.GetProjectAssetPath(GetHandleType(), m_JSONNode["name"]);
            if (!string.IsNullOrEmpty(guid))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Object obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                EditorGUIUtility.PingObject(obj);
            }
            base.DoubleClickedItem(id);
        }


        void ExecuteOptimize()
        {
            string type = m_JSONNode["name"];
            if (type.StartsWith("面数超过了") ||
                type.StartsWith("包含Color属性") ||
                type.StartsWith("没有压缩") ||
                type.StartsWith("绝色纹理") ||
                ((type.Contains("超过") && type.Contains("的纹理")) ||
                 (type.Contains("引用超过"))))
            {
                EditorUtility.DisplayDialog(type, "无法通过程序一健处理", "ok");
                return;
            }

            IList<int> ids = GetSelection();

            for (int i = 0; i < ids.Count; i++)
            {

                WebAssetRepairTreeViewItem item = (WebAssetRepairTreeViewItem)FindItem(ids[i], rootItem);
                EditorUtility.DisplayProgressBar(string.Format("{0}/{1}", i, ids.Count), string.Format("正在处理 {0}", item.GetAssetPath()), (float)i / (float)ids.Count);
                Optimize(item);
            }
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
        }





        void Optimize(WebAssetRepairTreeViewItem item)
        {
            string type = m_JSONNode["name"];
            int htype = m_JSONNode["htype"];
            if (type.StartsWith("Compression != Optimal"))
            {
                string guid = item.GetProjectAssetPath(GetHandleType());
                string topath = AssetDatabase.GUIDToAssetPath(guid);
                ModelImporter import = AssetImporter.GetAtPath(topath) as ModelImporter;
                if (import)
                {
                    if (topath.StartsWith("Assets/Art/Charactars/"))
                    {
                        if (import.importAnimation)
                        {
                            import.importAnimation = false;
                            import.SaveAndReimport();
                            item.handleErr = 1;
                        }
                    }
                    else
                    {
                        if (import.animationCompression != ModelImporterAnimationCompression.Optimal)
                        {
                            import.animationCompression = ModelImporterAnimationCompression.Optimal;
                            import.SaveAndReimport();
                            item.handleErr = 1;
                        }
                    }
                }
                else
                {
                    item.handleErr = 0;
                }
            }
            else if (type.StartsWith("精度过高"))
            {
                string guid = item.GetProjectAssetPath(GetHandleType());
                string topath = AssetDatabase.GUIDToAssetPath(guid);
                AssetImporter import = AssetImporter.GetAtPath(topath);
                if (import && Path.GetExtension(topath).ToLower() != ".fbx")
                {
                    AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(topath);
                    OptimizeAnimationClip2(clip, true);
                    item.handleErr = 1;
                }
                else
                {
                    item.handleErr = 0;
                }
            }
            else if (type.StartsWith("包含Scale曲线"))
            {
                string guid = item.GetProjectAssetPath(GetHandleType());
                string topath = AssetDatabase.GUIDToAssetPath(guid);
                AssetImporter import = AssetImporter.GetAtPath(topath);
                if (import && Path.GetExtension(topath).ToLower() != ".fbx")
                {
                    AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(topath);
                    OptimizeAnimationClip(clip, true);
                    item.handleErr = 1;
                }
                else
                {
                    item.handleErr = 0;
                }
            }
            else if (type.StartsWith("开了Read/Write选项") && htype == 2)
            {
                string guid = item.GetProjectAssetPath(GetHandleType());
                string topath = AssetDatabase.GUIDToAssetPath(guid);
                ModelImporter import = AssetImporter.GetAtPath(topath) as ModelImporter;
                if (import)
                {
                    if (import.isReadable)
                    {
                        import.isReadable = false;
                        import.SaveAndReimport();
                    }
                    item.handleErr = 1;
                }
                else
                {
                    item.handleErr = 0;
                }
            }
            else if (type.StartsWith("开了Read/Write选项") && htype == 3)
            {
                string guid = item.GetProjectAssetPath(GetHandleType());
                string topath = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter import = AssetImporter.GetAtPath(topath) as TextureImporter;
                if (import)
                {
                    if (import.isReadable)
                    {
                        import.isReadable = false;
                        import.SaveAndReimport();
                    }
                    item.handleErr = 1;
                }
                else
                {
                    item.handleErr = 0;
                }
            }
            else if (htype == 4)
            {
                if (type.StartsWith("没开启Read/Write选项"))
                {
                    string guid = item.GetProjectAssetPath(GetHandleType(), m_JSONNode["name"]);
                    string topath = AssetDatabase.GUIDToAssetPath(guid);
                    ModelImporter import = AssetImporter.GetAtPath(topath) as ModelImporter;
                    if (import)
                    {
                        if (!import.isReadable)
                        {
                            import.isReadable = true;
                            import.SaveAndReimport();
                        }
                        item.handleErr = 1;
                    }
                    else
                    {
                        item.handleErr = 0;
                    }
                }
                else if (type.StartsWith("超过") && (type.Contains("粒子发射器") || type.Contains("网格发射器")))
                {
                    string guid = item.GetProjectAssetPath(GetHandleType(), m_JSONNode["name"]);
                    string topath = AssetDatabase.GUIDToAssetPath(guid);
                    GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(topath);
                    item.handleErr = 0;
                    if (go)
                    {
                        Transform ts = go.transform.Find(item.GetAssetName().Substring(item.GetAssetName().IndexOf("/") + 1));
                        if (ts)
                        {
                            ParticleSystem ps = ts.GetComponent<ParticleSystem>();
                            if (ps)
                            {
                                ParticleSystem.MainModule mainModule = ps.main;
                                if (type.Contains("网格发射器"))
                                    mainModule.maxParticles = 5;
                                else
                                    mainModule.maxParticles = 30;
                                item.handleErr = 1;
                            }
                        }
                    }
                }
            }
            else if (type.StartsWith("开启了MotionVector"))
            {
                string guid = item.GetProjectAssetPath(GetHandleType(), m_JSONNode["name"]);
                string topath = AssetDatabase.GUIDToAssetPath(guid);
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(topath);
                item.handleErr = 0;
                if (go)
                {
                    SkinnedMeshRenderer[] smrs = go.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                    foreach (var smr in smrs)
                        smr.skinnedMotionVectors = false;
                    item.handleErr = 1;
                }
            }
        }


        //优化动画 精度
        static void OptimizeAnimationClip2(AnimationClip clip, bool save = true)
        {
            try
            {
                //================================================浮点数精度压缩到f4
                Keyframe[] keyFrames = null;
                Keyframe key;
                EditorCurveBinding[] allBinding = AnimationUtility.GetCurveBindings(clip);
                foreach (EditorCurveBinding binding in allBinding)
                {
                    AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);

                    keyFrames = curve != null ? curve.keys : null;

                    if (keyFrames != null)
                    {
                        for (int i = 0; i < keyFrames.Length; i++)
                        {
                            key = keyFrames[i];
                            key.value = float.Parse(key.value.ToString("f4"));
                            key.inTangent = float.Parse(key.inTangent.ToString("f4"));
                            key.outTangent = float.Parse(key.outTangent.ToString("f4"));
                            key.inWeight = float.Parse(key.inWeight.ToString("f4"));
                            key.outWeight = float.Parse(key.outWeight.ToString("f4"));
                            keyFrames[i] = key;
                        }
                        curve.keys = keyFrames;
                        //AnimationUtility.SetEditorCurve(clip, binding, curve);
                        clip.SetCurve(binding.path, binding.type, binding.propertyName, curve);
                    }
                }
                //================================================浮点数精度压缩 end
                if (save)
                    AssetDatabase.SaveAssets();
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }

        }

        //优化动画曲线
        static void OptimizeAnimationClip(AnimationClip clip, bool save = true)
        {
            try
            {
                Keyframe[] keyFrames = null;
                //================================================去除scale曲线
                Dictionary<string, List<EditorCurveBinding>> allScale = new Dictionary<string, List<EditorCurveBinding>>();
                foreach (EditorCurveBinding theCurveBinding in AnimationUtility.GetCurveBindings(clip))
                {

                    string name = theCurveBinding.propertyName.ToLower();
                    if (name.Contains("scale"))
                    {
                        List<EditorCurveBinding> list;
                        if (!allScale.TryGetValue(theCurveBinding.path, out list))
                        {
                            list = new List<EditorCurveBinding>();
                            allScale.Add(theCurveBinding.path, list);
                        }
                        list.Add(theCurveBinding);
                    }
                }


                foreach (var item in allScale)
                {
                    bool canDelete = true;
                    foreach (EditorCurveBinding curveBinding in item.Value)
                    {
                        AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, curveBinding);
                        keyFrames = curve.keys;
                        foreach (Keyframe keyframe in keyFrames)
                        {
                            //如果scale的帧有变化则不能删除
                            if (keyframe.value != 1)
                            {
                                canDelete = false;
                                break;
                            }

                        }

                        if (!canDelete) break;
                    }

                    if (canDelete)
                        foreach (EditorCurveBinding curveBinding in item.Value)
                            AnimationUtility.SetEditorCurve(clip, curveBinding, null);
                }
                //================================================去除scale曲线  end
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
            }

        }
    }
}
