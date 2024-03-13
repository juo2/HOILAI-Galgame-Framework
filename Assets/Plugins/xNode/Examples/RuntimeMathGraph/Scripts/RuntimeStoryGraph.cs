using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XNode.Examples.MathNodes;

namespace XNode.Story
{
	public class RuntimeStoryGraph : MonoBehaviour, IPointerClickHandler {
		[Header("Graph")]
		public StoryGraph graph;
		[Header("Prefabs")]
		public UGUIMathNode runtimeMathNodePrefab;
		public UGUIVector runtimeVectorPrefab;
		public UGUIDisplayValue runtimeDisplayValuePrefab;
		public Connection runtimeConnectionPrefab;
		[Header("References")]
		public UGUIContextMenu graphContextMenu;
		public UGUIContextMenu nodeContextMenu;
		public UGUITooltip tooltip;

		public ScrollRect scrollRect { get; private set; }
		private List<UGUIMathBaseNode> nodes;

		private void Awake() {
			// Create a clone so we don't modify the original asset
			graph = graph.Copy() as StoryGraph;
			scrollRect = GetComponentInChildren<ScrollRect>();
			graphContextMenu.onClickSpawn -= SpawnNode;
			graphContextMenu.onClickSpawn += SpawnNode;
		}

		private void Start() {
			SpawnGraph();
		}

		public void Refresh() {
			Clear();
			SpawnGraph();
		}

		public void Clear() {
			for (int i = nodes.Count - 1; i >= 0; i--) {
				Destroy(nodes[i].gameObject);
			}
			nodes.Clear();
		}

		public void SpawnGraph() {
			if (nodes != null) nodes.Clear();
			else nodes = new List<UGUIMathBaseNode>();

			for (int i = 0; i < graph.nodes.Count; i++) {
				Node node = graph.nodes[i];

				UGUIMathBaseNode runtimeNode = null;
				if (node is StoryAddCharacterNode) 
				{
					runtimeNode = Instantiate(runtimeMathNodePrefab);
				} 
				else if (node is StoryBackgroundNode) 
				{
					runtimeNode = Instantiate(runtimeVectorPrefab);
				} 
				else if (node is StoryDeleteCharacterNode) 
				{
					runtimeNode = Instantiate(runtimeDisplayValuePrefab);
				}
				else if (node is StoryExitGameNode)
				{
					runtimeNode = Instantiate(runtimeDisplayValuePrefab);
				}
				else if (node is StoryMessageNode)
				{
					runtimeNode = Instantiate(runtimeDisplayValuePrefab);
				}
				else if (node is StoryNextChapterNode)
				{
					runtimeNode = Instantiate(runtimeDisplayValuePrefab);
				}
				else if (node is StorySpeakAsideNode)
				{
					runtimeNode = Instantiate(runtimeDisplayValuePrefab);
				}
				else if (node is StorySpeakNode)
				{
					runtimeNode = Instantiate(runtimeDisplayValuePrefab);
				}
				else if (node is StoryVideoNode)
				{
					runtimeNode = Instantiate(runtimeDisplayValuePrefab);
				}

				runtimeNode.transform.SetParent(scrollRect.content);
				runtimeNode.node = node;
				runtimeNode.graph = this;
				nodes.Add(runtimeNode);
			}
		}

		public UGUIMathBaseNode GetRuntimeNode(Node node) {
			for (int i = 0; i < nodes.Count; i++) {
				if (nodes[i].node == node) {
					return nodes[i];
				} else { }
			}
			return null;
		}

		public void SpawnNode(Type type, Vector2 position) {
			Node node = graph.AddNode(type);
			node.name = type.Name;
			node.position = position;
			Refresh();
		}

		public void OnPointerClick(PointerEventData eventData) {
			if (eventData.button != PointerEventData.InputButton.Right)
				return;

			graphContextMenu.OpenAt(eventData.position);
		}
	}
}