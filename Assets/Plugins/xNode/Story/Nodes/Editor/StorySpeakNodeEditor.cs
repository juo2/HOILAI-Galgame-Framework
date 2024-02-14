using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode.Story;

namespace XNodeEditor.Story
{
    [CustomNodeEditor(typeof(StorySpeakNode))]
    public class StorySpeakNodeEditor : NodeEditor
    {
        public override void OnBodyGUI()
        {
            //base.OnBodyGUI();
            StorySpeakNode node = target as StorySpeakNode;
            //StateGraph graph = node.graph as StateGraph;
            //if (GUILayout.Button("MoveNext Node")) node.MoveNext();
            //if (GUILayout.Button("Continue Graph")) graph.Continue();
            //if (GUILayout.Button("Set as current")) graph.current = node;
            serializedObject.Update();
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("In"));
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("ID"));
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("image"));


            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("content"));
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("animate"));
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("audio"));

            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("isJump"));

            if(node.isJump)
            {
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("outOpt1"));
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("outOpt2"));
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("outOpt3"));
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("opt1"));
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("opt2"));
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("opt3"));
            }
            else
            {
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("Out"));
            }


            // Apply property modifications
            serializedObject.ApplyModifiedProperties();

        }
    }

}



