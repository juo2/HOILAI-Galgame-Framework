using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace XNode.Story
{
	public class UGUINodeContextMenu : MonoBehaviour, IPointerExitHandler {

		public Action<Type, Vector2> onClickSpawn;
		public CanvasGroup group;
		[HideInInspector] public Node selectedNode;
		private Vector2 pos;

		public Button deleteBtn;
		
		private void Start() {

			deleteBtn.onClick.AddListener(() => 
			{
				RemoveNode();
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