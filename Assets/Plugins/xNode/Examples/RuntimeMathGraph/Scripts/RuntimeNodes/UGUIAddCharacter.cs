using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XNode.Examples.MathNodes;

namespace XNode.Story
{
	public class UGUIAddCharacter : UGUIMathBaseNode {
		
		public InputField ID;
		public InputField image;
		public InputField p_name;

		public Toggle isSelf;

		private StoryAddCharacterNode addCharacterNode;

		public override void Start() {
			base.Start();
			addCharacterNode = node as StoryAddCharacterNode;

			ID.onValueChanged.AddListener(OnChangeID);
			image.onValueChanged.AddListener(OnChangeImage);
			p_name.onValueChanged.AddListener(OnChangeName);
			UpdateGUI();
		}

		public override void UpdateGUI() {
			NodePort portX = node.GetInputPort("x");
			NodePort portY = node.GetInputPort("y");
			NodePort portZ = node.GetInputPort("z");
			//ID.gameObject.SetActive(!portX.IsConnected);
			//image.gameObject.SetActive(!portY.IsConnected);
			//p_name.gameObject.SetActive(!portZ.IsConnected);

			//Vector vectorNode = node as Vector;
			ID.text = addCharacterNode.ID;
			image.text = addCharacterNode.image;
			p_name.text = addCharacterNode.p_name;
		}

		private void OnChangeID(string val) {
			addCharacterNode.ID = ID.text;
		}

		private void OnChangeImage(string val) {
			addCharacterNode.image = image.text;
		}

		private void OnChangeName(string val) {
			addCharacterNode.p_name = p_name.text;
		}
	}
}