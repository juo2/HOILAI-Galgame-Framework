using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XNode.Examples.MathNodes;

namespace XNode.Story
{
	public class UGUIDeleteCharacter : UGUIMathBaseNode {
		
		public InputField ID;

		private StoryDeleteCharacterNode deleteCharacterNode;

		public override void Start() {
			base.Start();
			deleteCharacterNode = node as StoryDeleteCharacterNode;

			ID.onValueChanged.AddListener(OnChangeID);
			UpdateGUI();
		}

		public override void UpdateGUI() {
			NodePort portX = node.GetInputPort("x");
			NodePort portY = node.GetInputPort("y");
			NodePort portZ = node.GetInputPort("z");
			//ID.gameObject.SetActive(!portX.IsConnected);
			//image.gameObject.SetActive(!portY.IsConnected);
			//p_name.gameObject.SetActive(!portZ.IsConnected);

			ID.text = deleteCharacterNode.ToString();
		}

		private void OnChangeID(string val) {
			deleteCharacterNode.ID = ID.text;
		}

	}
}