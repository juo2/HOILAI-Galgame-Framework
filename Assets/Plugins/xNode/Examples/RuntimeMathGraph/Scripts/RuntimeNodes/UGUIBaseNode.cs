using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XNode.Examples.MathNodes;

namespace XNode.Story
{
	public class UGUIBaseNode : MonoBehaviour, IDragHandler {

		[HideInInspector] public StoryBaseNode node;
		[HideInInspector] public RuntimeStoryGraph graph;
		public Text header;

		private Color cacheColor = Color.white;
		private Color errorColor = Color.red;
		private Image image;
		public Text errorMessage;

		private UGUIPort[] ports;

		public virtual void Start() {
			ports = GetComponentsInChildren<UGUIPort>();
			foreach (UGUIPort port in ports)
			{
				port.node = node;
			}
            header.text = node.name;
			SetPosition(node.position);

			image = GetComponent<Image>();

			cacheColor = image.color;
		}

		public virtual void UpdateGUI() { }
		
		private void LateUpdate() {
			foreach (UGUIPort port in ports)
			{
				port.UpdateConnectionTransforms();
			}

			if(node != null )
            {
				if(node.isError)
                {
					image.color = errorColor;
					errorMessage.gameObject.SetActive(true);
					errorMessage.text = node.errorMessage;
				}
				else
                {
					image.color = cacheColor;
					errorMessage.gameObject.SetActive(false);
				}

			}
		}

		public UGUIPort GetPort(string name) {
			for (int i = 0; i < ports.Length; i++) {
				if (ports[i].name == name) return ports[i];
			}
			return null;
		}

		public void SetPosition(Vector2 pos) {
			pos.y = -pos.y;
			transform.localPosition = pos;
		}

		public void SetName(string name) {
			header.text = name;
		}

		public void OnDrag(PointerEventData eventData) {

		}
	}
}