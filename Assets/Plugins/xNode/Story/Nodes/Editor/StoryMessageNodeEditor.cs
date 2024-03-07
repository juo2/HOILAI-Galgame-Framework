using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNode.Story;

namespace XNodeEditor.Story
{
    [CustomNodeEditor(typeof(StoryMessageNode))]
    public class StoryMessageNodeEditor : StoryBaseNodeEditor
    {
        public override void OnBodyGUI()
        {
            //base.OnBodyGUI();
            //StateGraph graph = node.graph as StateGraph;
            //if (GUILayout.Button("MoveNext Node")) node.MoveNext();
            //if (GUILayout.Button("Continue Graph")) graph.Continue();
            //if (GUILayout.Button("Set as current")) graph.current = node;
            serializedObject.Update();
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("In"));
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("image"));
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("outOpt1"));
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("outOpt2"));
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("outOpt3"));
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("outOpt4"));
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("opt1"));
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("opt2"));
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("opt3"));
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("opt4"));

            // Apply property modifications
            serializedObject.ApplyModifiedProperties();

        }
    }

}



