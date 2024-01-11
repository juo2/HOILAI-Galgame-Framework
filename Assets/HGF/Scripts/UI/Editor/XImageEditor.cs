using UnityEngine;
using UnityEditor;
using System.Collections;
using XGUI;
using UnityEditor.UI;
using UnityEngine.UI;

[CustomEditor(typeof(XImage), true)]
[CanEditMultipleObjects]
public class XImageEditor : ImageEditor
{
    SerializedProperty m_SpriteAssetName;
    SerializedProperty m_ImageUrl;
    SerializedProperty m_ChangeClearOld;
    SerializedProperty m_IsColliderRaycast;
    SerializedProperty m_SetNativeSize;
    SerializedProperty m_Visible;
    SerializedProperty m_IsCanSortingMask;
    SerializedProperty m_IgnoreAttachingCanvas;
    SerializedProperty m_IsCheckCrossMod;
    //SerializedProperty m_CacheSpriteName;
    SerializedProperty m_Sprites;
    SerializedProperty m_ErrorSprite;
    SerializedProperty m_xParentMask;


    protected override void OnEnable()
    {
        base.OnEnable();
        m_SpriteAssetName = serializedObject.FindProperty("m_SpriteAssetName");
        m_ImageUrl = serializedObject.FindProperty("m_ImageUrl");

        m_ChangeClearOld = serializedObject.FindProperty("m_ChangeClearOld");
        m_IsColliderRaycast = serializedObject.FindProperty("m_IsColliderRaycast");
        m_SetNativeSize = serializedObject.FindProperty("m_SetNativeSize");
        m_Visible = serializedObject.FindProperty("m_Visible");
        m_IsCanSortingMask = serializedObject.FindProperty("m_IsCanSortingMask");
        m_IgnoreAttachingCanvas = serializedObject.FindProperty("m_IgnoreAttachingCanvas");
        m_IsCheckCrossMod = serializedObject.FindProperty("m_IsCheckCrossMod");
        //m_CacheSpriteName = serializedObject.FindProperty("m_CacheSpriteName");
        m_Sprites = serializedObject.FindProperty("m_Sprites");
        m_ErrorSprite = serializedObject.FindProperty("m_ErrorSprite");
        m_xParentMask = serializedObject.FindProperty("m_xParentMask");
    }

    public override void OnInspectorGUI()
    {

        base.OnInspectorGUI();

        EditorGUILayout.Space();

        serializedObject.Update();

        EditorGUILayout.PropertyField(m_SpriteAssetName);
        EditorGUILayout.PropertyField(m_ImageUrl);
        EditorGUILayout.PropertyField(m_ChangeClearOld);
        EditorGUILayout.PropertyField(m_IsColliderRaycast);
        EditorGUILayout.PropertyField(m_SetNativeSize);
        EditorGUILayout.PropertyField(m_Visible);
        EditorGUILayout.PropertyField(m_IsCanSortingMask);
        EditorGUILayout.PropertyField(m_IgnoreAttachingCanvas);
        EditorGUILayout.PropertyField(m_xParentMask);
        //EditorGUILayout.PropertyField(m_CacheSpriteName);
        EditorGUILayout.PropertyField(m_ErrorSprite);
        EditorGUILayout.PropertyField(m_Sprites, true);
        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        //if (GUILayout.Button("模糊"))
        //{
        //    Graphic graphics = (serializedObject.targetObject as Graphic);
        //    graphics.SetDim(graphics.material == graphics.defaultMaterial);
        //}

        //if (GUILayout.Button("置灰"))
        //{
        //    Graphic graphics = (serializedObject.targetObject as Graphic);
        //    graphics.SetGray(graphics.material == graphics.defaultMaterial);
        //}
        EditorGUILayout.EndHorizontal();
    }
}
