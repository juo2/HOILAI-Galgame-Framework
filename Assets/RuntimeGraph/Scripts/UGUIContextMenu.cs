using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace XNode.Story
{
	public class UGUIContextMenu : MonoBehaviour, IPointerExitHandler {

		public Action<Type, Vector2> onClickSpawn;
		public CanvasGroup group;
		[HideInInspector] public Node selectedNode;
		private Vector2 pos;

		public Button addCharacterBtn;
		public Button backgroundBtn;
		public Button deleteCharacterBtn;
		public Button exitGameBtn;
		public Button messageBtn;
		public Button speakAsideBtn;
		public Button videoBtn;
		public Button BgmBtn;
		public Button SpeakBtn;
		public Button NextChapterBtn;

		private void Start() {

			addCharacterBtn.onClick.AddListener(() => 
			{
				SpawnNode(typeof(StoryAddCharacterNode));
			});

			backgroundBtn.onClick.AddListener(() =>
			{
				SpawnNode(typeof(StoryBackgroundNode));
			});

			deleteCharacterBtn.onClick.AddListener(() =>
			{
				SpawnNode(typeof(StoryDeleteCharacterNode));
			});

			exitGameBtn.onClick.AddListener(() =>
			{
				SpawnNode(typeof(StoryExitGameNode));
			});

			messageBtn.onClick.AddListener(() =>
			{
				SpawnNode(typeof(StoryMessageNode));
			});

			speakAsideBtn.onClick.AddListener(() =>
			{
				SpawnNode(typeof(StorySpeakAsideNode));
			});

			videoBtn.onClick.AddListener(() =>
			{
				SpawnNode(typeof(StoryVideoNode));
			});

			BgmBtn.onClick.AddListener(() =>
			{
				SpawnNode(typeof(StoryBgmNode));
			});

			SpeakBtn.onClick.AddListener(() =>
			{
				SpawnNode(typeof(StorySpeakNode));
			});

			NextChapterBtn.onClick.AddListener(() =>
			{
				SpawnNode(typeof(StoryNextChapterNode));
			});

			Close();
		}

		public void OpenAt(Vector2 pos) {
			transform.position = pos;
			group.alpha = 1;
			group.interactable = true;
			group.blocksRaycasts = true;
			transform.SetAsLastSibling();
		}

		public void Close() {
			group.alpha = 0;
			group.interactable = false;
			group.blocksRaycasts = false;
		}

		private void SpawnNode(Type nodeType) {
			Vector2 pos = new Vector2(transform.localPosition.x, -transform.localPosition.y);
			onClickSpawn(nodeType, pos);
		}

		public void RemoveNode() {
			RuntimeStoryGraph runtimeMathGraph = GetComponentInParent<RuntimeStoryGraph>();
			runtimeMathGraph.graph.RemoveNode(selectedNode);
			runtimeMathGraph.Refresh();
			Close();
		}

		public void OnPointerExit(PointerEventData eventData) {
			Close();
		}
	}
}