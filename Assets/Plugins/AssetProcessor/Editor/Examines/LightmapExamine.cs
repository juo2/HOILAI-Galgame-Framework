using UnityEngine;
using UnityEditor;

//[CustomEditor(typeof(LightingDataAsset))]
public class LightmapExamine : EditorWindow
{

    //void OnEnable()
    //{
        //Debug.Log(serializedObject);
        //SerializedProperty sp = serializedObject.GetIterator();
        //SerializedProperty sp = serializedObject.FindProperty("m_LightMaps");
        //Debug.Log("->" + sp);
        //while (sp.Next(true))
        //{
        //    Debug.Log(sp.name + "  " + sp.propertyPath + "  " + sp.propertyType);
        //}

        //SerializedObject so = new SerializedObject(LightmapSettings.lightmaps);

    //}

    //public override void OnInspectorGUI()
    //{
    //    base.OnInspectorGUI();

    //    //EditorGUILayout.PropertyField(LightmapEditorSettings.GetLightmapSettings());
    //    //LightmapEditorSettings.GetLightmapSettings();
    //}



    Vector2 scrollRct = Vector2.zero;
    void OnGUI()
    {
        EditorGUILayout.HelpBox("直接将贴图拖上来便可替换烘焙贴图", MessageType.Info);
        scrollRct = EditorGUILayout.BeginScrollView(scrollRct);
        EditorGUILayout.Space();
        int index = 0;
        LightmapData[] datas = LightmapSettings.lightmaps;
        for (int i = 0; i < datas.Length; i++)
        {
            LightmapData item = datas[i];
            if (item.lightmapColor == null)
                continue;
            EditorGUILayout.BeginHorizontal();

            datas[i].lightmapColor = EditorGUILayout.ObjectField(item.lightmapColor, typeof(Texture2D), false, GUILayout.Width(100), GUILayout.Height(100)) as Texture2D;

            if (item.lightmapDir != null)
            {
                EditorGUILayout.ObjectField(item.lightmapDir, typeof(Texture2D), false, GUILayout.Width(100), GUILayout.Height(100));
            }

            EditorGUILayout.LabelField("index: " + index++);
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
        LightmapSettings.lightmaps = datas;

        //if (GUI.changed)
        //{

        //}
    }




    [MenuItem("XGame/Examine/Lightmap")]
    static void Open()
    {
        LightmapExamine se = EditorWindow.GetWindow<LightmapExamine>();
        se.Show();
    }
}