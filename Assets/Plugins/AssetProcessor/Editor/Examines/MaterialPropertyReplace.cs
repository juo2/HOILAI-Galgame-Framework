using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

public class MaterialPropertyReplace : EditorWindow
{
    class MaterialInfo
    {
        public Material material;
    }


    private MTreeView m_MTreeView;
    private TreeViewState m_TreeViewState;
    private List<string> m_AllPaths;
    private List<MaterialInfo> m_MaterialDoneInfos;
    void OnEnable()
    {
        m_TreeViewState = new TreeViewState();
        MultiColumnHeader header = new MultiColumnHeader(MTreeView.GetColumnHeaderState());
        m_MTreeView = new MTreeView(m_TreeViewState, header);
        m_MTreeView.RefreshData(null);
    }

    void RefreshTree()
    {
        if (m_MTreeView != null) m_MTreeView.RefreshData(m_MaterialDoneInfos);
    }

    private void OnGUI()
    {
        string[] ids = Selection.assetGUIDs;

        for (int i = 0; i < ids.Length; i++)
        {
            ids[i] = AssetDatabase.GUIDToAssetPath(ids[i]);
        }


        EditorGUI.BeginDisabledGroup(ids.Length == 0);
        if (GUILayout.Button("选中目录（搜索）"))
        {
            SearchMaterial(ids);
        }
        EditorGUI.EndDisabledGroup();



        Rect rect = EditorGUILayout.GetControlRect();
        rect.y += 5;
        rect.height = position.height - rect.y - 5;
        m_MTreeView.OnGUI(rect);
    }


    void SearchMaterial(string[] paths)
    {
        string[] materials = AssetDatabase.FindAssets("t:Material", paths);
        if (materials.Length <= 0) return;
        m_MaterialDoneInfos = new List<MaterialInfo>(materials.Length);
        m_AllPaths = new List<string>(materials.Length);

        foreach (var mat in materials)
            m_AllPaths.Add(AssetDatabase.GUIDToAssetPath(mat));

        EditorApplication.update -= AsyncLoad;
        EditorApplication.update += AsyncLoad;
    }


    void AsyncLoad()
    {
        RefreshTree();
        if (m_AllPaths.Count <= 0)
        {
            EditorApplication.update -= AsyncLoad;
            return;
        }

        Material mat = AssetDatabase.LoadAssetAtPath<Material>(m_AllPaths[0]);
        m_AllPaths.RemoveAt(0);
        if (mat && mat.shader)
        {
            MaterialInfo info = new MaterialInfo();
            info.material = mat;
            m_MaterialDoneInfos.Add(info);
        }


    }



    class MTreeView : TreeView
    {
        private List<MaterialInfo> m_MaterialInfos;

        public MTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader) : base(state, multiColumnHeader)
        {
            showBorder = true;
            showAlternatingRowBackgrounds = true;
            rowHeight = 22;
            customFoldoutYOffset = 10;
        }

        public void RefreshData(List<MaterialInfo> infos)
        {
            m_MaterialInfos = infos;
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            TreeViewItem root = new TreeViewItem(0, -1, "root");
            List<TreeViewItem> items = new List<TreeViewItem>();

            if (m_MaterialInfos == null || m_MaterialInfos.Count <= 0)
            {
                SetupParentsAndChildrenFromDepths(root, items);
                return root;
            }


            Dictionary<string, MTreeViewItem> shaderGroup = new Dictionary<string, MTreeViewItem>();

            int countId = 1;
            foreach (MaterialInfo materialInfo in m_MaterialInfos)
            {
                if (!materialInfo.material.shader) continue;

                MTreeViewItem sg;
                if (!shaderGroup.TryGetValue(materialInfo.material.shader.name, out sg))
                {

                    sg = new MTreeViewItem(countId++, materialInfo);
                    sg.isShaderNode = true;
                    sg.children = new List<TreeViewItem>();
                    shaderGroup.Add(materialInfo.material.shader.name, sg);
                }

                MTreeViewItem ttiem = new MTreeViewItem(countId++, materialInfo);
                ttiem.parent = sg;
                sg.children.Add(ttiem);

            }

            List<TreeViewItem> list = new List<TreeViewItem>();
            foreach (var value in shaderGroup.Values) list.Add(value);
            SetupParentsAndChildrenFromDepths(root, list);
            return root;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            base.RowGUI(args);
            TreeViewItem item = args.item;
            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(args.GetCellRect(i), item, args.GetColumn(i), ref args);
            }
        }

        void SelectedItemsForChilds(MTreeViewItem item, bool value)
        {
            if (item.children != null && item.children.Count > 0)
            {
                foreach (var btiem in item.children)
                {
                    (btiem as MTreeViewItem).isSelected = value;
                }
            }
            else
            {
                item.isSelected = value;
            }
        }

        void CellGUI(Rect cellRect, TreeViewItem item, int column, ref RowGUIArgs args)
        {
            CenterRectUsingSingleLineHeight(ref cellRect);

            MTreeViewItem mtreeItem = item as MTreeViewItem;

            //if (mtreeItem.serializedObject == null)
            //    mtreeItem.serializedObject = new SerializedObject(mtreeItem.materialInfo.material);

            Rect rect = cellRect;
            switch (column)
            {
                case 0:
                    cellRect.x += 16;

                    Rect toggleRect = cellRect;
                    toggleRect.width = 18;
                    toggleRect.height = 18;

                    EditorGUI.BeginChangeCheck();
                    mtreeItem.isSelected = EditorGUI.Toggle(toggleRect, mtreeItem.isSelected);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (mtreeItem.isShaderNode)
                            SelectedItemsForChilds(mtreeItem, mtreeItem.isSelected);
                        else
                        {
                            IList<int> selection = GetSelection();
                            if (selection.Count > 1)
                            {
                                foreach (var rolwItem in FindRows(GetSelection()))
                                {
                                    SelectedItemsForChilds(rolwItem as MTreeViewItem, mtreeItem.isSelected);
                                }
                            }
                        }
                    }

                    cellRect.x += 20;
                    cellRect.width -= 40;
                    EditorGUI.BeginDisabledGroup(true);
                    if (mtreeItem.isShaderNode)
                        EditorGUI.ObjectField(cellRect, mtreeItem.materialInfo.material.shader, typeof(Shader), false);
                    else
                        EditorGUI.ObjectField(cellRect, mtreeItem.materialInfo.material, typeof(Material), false);
                    EditorGUI.EndDisabledGroup();
                    break;
                case 1:
                    rect.width = 18;
                    if (mtreeItem.isShaderNode)
                    {
                        foreach (var proinfo in mtreeItem.shaderProInfos)
                        {
                            if (proinfo.type != ShaderUtil.ShaderPropertyType.TexEnv) continue;
                            if (!mtreeItem.materialInfo.material.HasProperty(proinfo.name)) continue;
                            Texture tex = mtreeItem.GetTempTex(proinfo.name);

                            float oy = rect.y;

                            rect.y -= 12;
                            rect.width = cellRect.width;
                            GUI.Label(rect, proinfo.name);

                            rect.y += 17;
                            rect.width = 65;
                            EditorGUI.BeginChangeCheck();
                            tex = (Texture)EditorGUI.ObjectField(rect, tex, typeof(Texture), false);
                            if (EditorGUI.EndChangeCheck())
                            {
                                if (RepAllChildTex(mtreeItem, proinfo.name, tex))
                                    mtreeItem.SetTempTex(proinfo.name, tex);

                            }

                            rect.x += 80;
                            rect.y = oy;
                        }
                    }
                    else
                    {
                        MTreeViewItem parenTreeViewItem = mtreeItem.parent as MTreeViewItem;
                        foreach (var proinfo in parenTreeViewItem.shaderProInfos)
                        {
                            if (proinfo.type != ShaderUtil.ShaderPropertyType.TexEnv) continue;
                            if (!mtreeItem.materialInfo.material.HasProperty(proinfo.name)) continue;

                            float oy = rect.y;

                            rect.y -= 12;
                            rect.width = cellRect.width;
                            GUI.Label(rect, proinfo.name);

                            rect.y += 17;
                            rect.width = 65;

                            Texture tex = mtreeItem.materialInfo.material.GetTexture(proinfo.name);
                            EditorGUI.BeginChangeCheck();
                            tex = (Texture)EditorGUI.ObjectField(rect, tex, typeof(Texture), false);
                            if (EditorGUI.EndChangeCheck())
                            {
                                mtreeItem.materialInfo.material.SetTexture(proinfo.name, tex);
                            }
                            rect.x += 80;
                            rect.y = oy;
                        }
                    }
                    break;
                case 2:
                    if (mtreeItem.isShaderNode)
                    {
                        foreach (var proinfo in mtreeItem.shaderProInfos)
                        {
                            if (proinfo.type != ShaderUtil.ShaderPropertyType.Color) continue;
                            if (!mtreeItem.materialInfo.material.HasProperty(proinfo.name)) continue;

                            Color color = mtreeItem.GetTempColor(proinfo.name);
                            float oy = rect.y;

                            rect.y -= 12;
                            rect.width = cellRect.width;
                            GUI.Label(rect, proinfo.name);

                            rect.y += 17;
                            rect.width = 65;
                            EditorGUI.BeginChangeCheck();
                            color = EditorGUI.ColorField(rect, color);
                            if (EditorGUI.EndChangeCheck())
                            {
                                if (RepAllChildColor(mtreeItem, proinfo.name, color))
                                    mtreeItem.SetTempColor(proinfo.name, color);

                            }
                            rect.x += 80;
                            rect.y = oy;
                        }
                    }
                    else
                    {
                        MTreeViewItem parenTreeViewItem = mtreeItem.parent as MTreeViewItem;
                        foreach (var proinfo in parenTreeViewItem.shaderProInfos)
                        {
                            if (proinfo.type != ShaderUtil.ShaderPropertyType.Color) continue;
                            if (!mtreeItem.materialInfo.material.HasProperty(proinfo.name)) continue;

                            float oy = rect.y;

                            rect.y -= 12;
                            rect.width = cellRect.width;
                            GUI.Label(rect, proinfo.name);

                            rect.y += 17;
                            rect.width = 65;

                            Color color = mtreeItem.materialInfo.material.GetColor(proinfo.name);
                            EditorGUI.BeginChangeCheck();
                            color = EditorGUI.ColorField(rect, color);
                            if (EditorGUI.EndChangeCheck())
                            {
                                mtreeItem.materialInfo.material.SetColor(proinfo.name, color);
                            }
                            rect.x += 80;
                            rect.y = oy;
                        }
                    }
                    break;
                case 3:
                    if (mtreeItem.isShaderNode)
                    {
                        foreach (var proinfo in mtreeItem.shaderProInfos)
                        {
                            if (proinfo.type != ShaderUtil.ShaderPropertyType.Float &&
                                proinfo.type != ShaderUtil.ShaderPropertyType.Range) continue;
                            if (!mtreeItem.materialInfo.material.HasProperty(proinfo.name)) continue;

                            float value = mtreeItem.GetTempFolatRange(proinfo.name);

                            value = value == -99999999 ? proinfo.defaultValue : value;

                            float oy = rect.y;

                            rect.y -= 12;
                            rect.width = cellRect.width;
                            GUI.Label(rect, proinfo.name);

                            rect.y += 17;

                            EditorGUI.BeginChangeCheck();
                            if (proinfo.type == ShaderUtil.ShaderPropertyType.Range)
                            {
                                rect.width = 150;
                                value = EditorGUI.Slider(rect, value, proinfo.minValue, proinfo.maxValue);
                            }
                            else
                            {
                                rect.width = 65;
                                value = EditorGUI.FloatField(rect, value);
                            }
                            if (EditorGUI.EndChangeCheck())
                            {
                                if (RepAllChildFloat(mtreeItem, proinfo.name, value))
                                    mtreeItem.SetTempFolatRange(proinfo.name, value);

                            }
                            rect.x += rect.width + 20;
                            rect.y = oy;
                        }
                    }
                    else
                    {
                        MTreeViewItem parenTreeViewItem = mtreeItem.parent as MTreeViewItem;
                        foreach (var proinfo in parenTreeViewItem.shaderProInfos)
                        {
                            if (proinfo.type != ShaderUtil.ShaderPropertyType.Float &&
                                proinfo.type != ShaderUtil.ShaderPropertyType.Range) continue;
                            if (!mtreeItem.materialInfo.material.HasProperty(proinfo.name)) continue;

                            float value = mtreeItem.materialInfo.material.GetFloat(proinfo.name);

                            value = value == -99999999 ? proinfo.defaultValue : value;

                            float oy = rect.y;

                            rect.y -= 12;
                            rect.width = cellRect.width;
                            GUI.Label(rect, proinfo.name);

                            rect.y += 17;

                            EditorGUI.BeginChangeCheck();
                            if (proinfo.type == ShaderUtil.ShaderPropertyType.Range)
                            {
                                rect.width = 150;
                                value = EditorGUI.Slider(rect, value, proinfo.minValue, proinfo.maxValue);
                            }
                            else
                            {
                                rect.width = 65;
                                value = EditorGUI.FloatField(rect, value);
                            }

                            if (EditorGUI.EndChangeCheck())
                            {
                                mtreeItem.materialInfo.material.SetFloat(proinfo.name, value);
                            }
                            rect.x += rect.width + 20;
                            rect.y = oy;
                        }
                    }
                    break;
                default:
                    break;
            }
        }




        bool RepAllChildTex(MTreeViewItem item, string proName, Texture tex)
        {
            List<MTreeViewItem> toggleSelect = new List<MTreeViewItem>();
            foreach (MTreeViewItem child in item.children)
            {
                if (child.isSelected)
                    toggleSelect.Add(child);
            }

            if (toggleSelect.Count <= 0)
            {
                EditorUtility.DisplayDialog("提示", "没有勾选材质!将没有任何效果", "确定");
                return false;
            }


            bool result = EditorUtility.DisplayDialog("提示", string.Format("确定为{0}个材质应用此操作！注意此操作不可逆", toggleSelect.Count), "确定", "不敢试");

            if (result)
            {
                foreach (var childItem in toggleSelect)
                {
                    if (childItem.materialInfo.material.HasProperty(proName))
                    {
                        childItem.materialInfo.material.SetTexture(proName, tex);
                    }
                }
                AssetDatabase.SaveAssets();
            }

            return result;
        }

        bool RepAllChildColor(MTreeViewItem item, string proName, Color color)
        {
            List<MTreeViewItem> toggleSelect = new List<MTreeViewItem>();
            foreach (MTreeViewItem child in item.children)
            {
                if (child.isSelected)
                    toggleSelect.Add(child);
            }

            if (toggleSelect.Count <= 0)
            {
                EditorUtility.DisplayDialog("提示", "没有勾选材质!将没有任何效果", "确定");
                return false;
            }


            //bool result = EditorUtility.DisplayDialog("提示", string.Format("确定为{0}个材质应用此操作！注意此操作不可逆", toggleSelect.Count), "确定", "不敢试");

            //if (true)
            {
                foreach (var childItem in toggleSelect)
                {
                    if (childItem.materialInfo.material.HasProperty(proName))
                    {
                        childItem.materialInfo.material.SetColor(proName, color);
                    }
                }
            }

            return true;
        }


        bool RepAllChildFloat(MTreeViewItem item, string proName, float value)
        {
            List<MTreeViewItem> toggleSelect = new List<MTreeViewItem>();
            foreach (MTreeViewItem child in item.children)
            {
                if (child.isSelected)
                    toggleSelect.Add(child);
            }

            if (toggleSelect.Count <= 0)
            {
                EditorUtility.DisplayDialog("提示", "没有勾选材质!将没有任何效果", "确定");
                return false;
            }


            //bool result = EditorUtility.DisplayDialog("提示", string.Format("确定为{0}个材质应用此操作！注意此操作不可逆", toggleSelect.Count), "确定", "不敢试");

            //if (true)
            {
                foreach (var childItem in toggleSelect)
                {
                    if (childItem.materialInfo.material.HasProperty(proName))
                    {
                        childItem.materialInfo.material.SetFloat(proName, value);
                    }
                }
            }

            return true;
        }

        protected override float GetCustomRowHeight(int row, TreeViewItem item)
        {
            //MTreeViewItem mitem = item as MTreeViewItem;
            //if (mitem.isShaderNode) return 35;
            return 35;
            //return base.GetCustomRowHeight(row, item);
        }


        public static MultiColumnHeaderState GetColumnHeaderState()
        {
            MultiColumnHeaderState state = new MultiColumnHeaderState(new MultiColumnHeaderState.Column[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Object"),
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 320,
                    minWidth = 150,
                    autoResize = false,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("TexEnvs"),
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 320,
                    minWidth = 150,
                    autoResize = false,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Colors"),
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 450,
                    minWidth = 150,
                    autoResize = false,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Float"),
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 500,
                    minWidth = 150,
                    autoResize = false,
                    allowToggleVisibility = false
                },
            });
            return state;
        }





        class MTreeViewItem : TreeViewItem
        {
            public class ShaderPropertyInfo
            {
                public string name;
                public ShaderUtil.ShaderPropertyType type;

                public float defaultValue;
                public float minValue;
                public float maxValue;
            }


            public MaterialInfo materialInfo;
            public bool isShaderNode;
            public bool isSelected;
            private List<ShaderPropertyInfo> m_ShaderProInfos;
            private Dictionary<string, Texture> m_TempRepTex;
            private Dictionary<string, Color> m_TempRepColor;
            private Dictionary<string, float> m_TempRepFolatRange;

            public MTreeViewItem(int id, MaterialInfo info) : base(id)
            {
                materialInfo = info;
            }


            public List<ShaderPropertyInfo> shaderProInfos
            {
                get
                {
                    if (!isShaderNode) return null;
                    if (m_ShaderProInfos == null)
                    {
                        m_ShaderProInfos = new List<ShaderPropertyInfo>();
                        for (int i = 0; i < ShaderUtil.GetPropertyCount(materialInfo.material.shader); i++)
                        {

                            string pname = ShaderUtil.GetPropertyName(materialInfo.material.shader, i);
                            ShaderUtil.ShaderPropertyType ptype = ShaderUtil.GetPropertyType(materialInfo.material.shader, i);
                            if (!string.IsNullOrEmpty(pname))
                            {
                                ShaderPropertyInfo info = new ShaderPropertyInfo { name = pname, type = ptype };
                                if (ptype == ShaderUtil.ShaderPropertyType.Range)
                                {
                                    info.defaultValue = ShaderUtil.GetRangeLimits(materialInfo.material.shader, i, 0);
                                    info.minValue = ShaderUtil.GetRangeLimits(materialInfo.material.shader, i, 1);
                                    info.maxValue = ShaderUtil.GetRangeLimits(materialInfo.material.shader, i, 2);
                                }
                                m_ShaderProInfos.Add(info);
                            }

                        }
                    }

                    return m_ShaderProInfos;
                }
            }

            public void SetTempTex(string key, Texture tex)
            {
                if (m_TempRepTex == null)
                    m_TempRepTex = new Dictionary<string, Texture>();
                if (!m_TempRepTex.ContainsKey(key))
                    m_TempRepTex.Add(key, tex);
                else
                    m_TempRepTex[key] = tex;
            }

            public Texture GetTempTex(string key)
            {
                return m_TempRepTex != null && m_TempRepTex.ContainsKey(key) ? m_TempRepTex[key] : null;
            }


            public void SetTempColor(string key, Color color)
            {
                if (m_TempRepColor == null)
                    m_TempRepColor = new Dictionary<string, Color>();
                if (!m_TempRepColor.ContainsKey(key))
                    m_TempRepColor.Add(key, color);
                else
                    m_TempRepColor[key] = color;
            }

            public Color GetTempColor(string key)
            {
                return m_TempRepColor != null && m_TempRepColor.ContainsKey(key) ? m_TempRepColor[key] : Color.white;
            }



            public void SetTempFolatRange(string key, float value)
            {
                if (m_TempRepFolatRange == null)
                    m_TempRepFolatRange = new Dictionary<string, float>();
                if (!m_TempRepFolatRange.ContainsKey(key))
                    m_TempRepFolatRange.Add(key, value);
                else
                    m_TempRepFolatRange[key] = value;
            }

            public float GetTempFolatRange(string key)
            {
                return m_TempRepFolatRange != null && m_TempRepFolatRange.ContainsKey(key) ? m_TempRepFolatRange[key] : -99999999;
            }
        }
    }


    [MenuItem("XGame/Examine/MaterialPropertyReplace")]
    static void Open()
    {
        MaterialPropertyReplace se = EditorWindow.GetWindow<MaterialPropertyReplace>();
        se.Show();
    }
}