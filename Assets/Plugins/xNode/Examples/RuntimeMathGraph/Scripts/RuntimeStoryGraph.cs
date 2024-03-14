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

		public UGUIAddCharacter runtimeAddCharacterPrefab;
		public UGUIBackground runtimeBackgroundPrefab;
		public UGUIBgm runtimeBgmPrefab;
		public UGUIDeleteCharacter runtimeDeleteCharacterPrefab;
		public UGUIExitGame runtimeExitGamePrefab;
		public UGUIMessage runtimeMessagePrefab;
		public UGUINextChapter runtimeNextChapterPrefab;
		public UGUISpeak runtimeSpeakPrefab;
		public UGUISpeakAside runtimeSpeakAsidePrefab;
		public UGUIVideo runtimeVideoPrefab;

		public Connection runtimeConnectionPrefab;


		[Header("References")]
		public UGUIContextMenu graphContextMenu;
		public UGUIContextMenu nodeContextMenu;
		public UGUITooltip tooltip;

		public ScrollRect scrollRect { get; private set; }
		private List<UGUIBaseNode> nodes;

		private void Awake() {
			// Create a clone so we don't modify the original asset
			graph = graph.Copy() as StoryGraph;
			scrollRect = GetComponentInChildren<ScrollRect>();
			graphContextMenu.onClickSpawn -= SpawnNode;
			graphContextMenu.onClickSpawn += SpawnNode;

			SpawnGraph();
		}

		private void Start() {
			
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
			else nodes = new List<UGUIBaseNode>();

			for (int i = 0; i < graph.nodes.Count; i++) {
				Node node = graph.nodes[i];

				UGUIBaseNode runtimeNode = null;
				if (node is StoryAddCharacterNode) 
				{
					runtimeNode = Instantiate(runtimeAddCharacterPrefab);
				} 
				else if (node is StoryBackgroundNode) 
				{
					runtimeNode = Instantiate(runtimeBackgroundPrefab);
				}
				else if (node is StoryBgmNode)
				{
					runtimeNode = Instantiate(runtimeBgmPrefab);
				}
				else if (node is StoryDeleteCharacterNode) 
				{
					runtimeNode = Instantiate(runtimeDeleteCharacterPrefab);
				}
				else if (node is StoryExitGameNode)
				{
					runtimeNode = Instantiate(runtimeExitGamePrefab);
				}
				else if (node is StoryMessageNode)
				{
					runtimeNode = Instantiate(runtimeMessagePrefab);
				}
				else if (node is StoryNextChapterNode)
				{
					runtimeNode = Instantiate(runtimeNextChapterPrefab);
				}
				else if (node is StorySpeakAsideNode)
				{
					runtimeNode = Instantiate(runtimeSpeakAsidePrefab);
				}
				else if (node is StorySpeakNode)
				{
					runtimeNode = Instantiate(runtimeSpeakPrefab);
				}
				else if (node is StoryVideoNode)
				{
					runtimeNode = Instantiate(runtimeVideoPrefab);
				}
				

				runtimeNode.transform.SetParent(scrollRect.content);
				runtimeNode.node = node;
				runtimeNode.graph = this;
				nodes.Add(runtimeNode);
			}
		}

		public UGUIBaseNode GetRuntimeNode(Node node) {
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