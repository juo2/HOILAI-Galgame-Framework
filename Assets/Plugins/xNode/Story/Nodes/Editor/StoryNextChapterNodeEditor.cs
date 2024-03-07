using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNode.Story;

namespace XNodeEditor.Story
{
    [CustomNodeEditor(typeof(StoryNextChapterNode))]
    public class StoryNextChapterNodeEditor : StoryBaseNodeEditor
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
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("storyGraphicName"));

            // Apply property modifications
            serializedObject.ApplyModifiedProperties();

        }
    }

}



