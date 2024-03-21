using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XGUI;
using XNode.Examples.MathNodes;

namespace XNode.Story
{
	public class UGUIDeleteCharacter : UGUIBaseNode {
		
		public XButton IDBtn;

		private StoryDeleteCharacterNode deleteCharacterNode;

		public override void Start() {
			base.Start();
			deleteCharacterNode = node as StoryDeleteCharacterNode;

			IDBtn.onClick.AddListener(OnChangeID);
			UpdateGUI();
		}

		public override void UpdateGUI() {
			if (string.IsNullOrEmpty(deleteCharacterNode.ID))
			{
				IDBtn.label = "please select id";
			}
			else
			{
				IDBtn.label = deleteCharacterNode.ID;
			}
		}

		private void OnChangeID() {
			graph.ShowCharacter((string _id) =>
			{
				deleteCharacterNode.ID = _id;
				IDBtn.label = _id;
			});
		}

	}
}