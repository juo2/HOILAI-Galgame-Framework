using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XNode.Examples.MathNodes;

namespace XNode.Story
{
	public class UGUIDeleteCharacter : UGUIBaseNode {
		
		public InputField ID;

		private StoryDeleteCharacterNode deleteCharacterNode;

		public override void Start() {
			base.Start();
			deleteCharacterNode = node as StoryDeleteCharacterNode;

			ID.onValueChanged.AddListener(OnChangeID);
			UpdateGUI();
		}

		public override void UpdateGUI() {
			ID.text = deleteCharacterNode.ToString();
		}

		private void OnChangeID(string val) {
			deleteCharacterNode.ID = ID.text;
		}

	}
}