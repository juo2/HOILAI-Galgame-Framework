using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XGUI;
using XNode.Examples.MathNodes;

namespace XNode.Story
{
	public class UGUIAddCharacter : UGUIBaseNode {
		
		public InputField ID;
		public XButton imageBtn;
		public InputField p_name;

		public Toggle isSelf;

		private StoryAddCharacterNode addCharacterNode;

		public override void Start() {
			base.Start();
			addCharacterNode = node as StoryAddCharacterNode;

			ID.onValueChanged.AddListener(OnChangeID);
			imageBtn.onClick.AddListener(OnChangeImage);
			p_name.onValueChanged.AddListener(OnChangeName);
			UpdateGUI();
		}

		public override void OnCreate()
		{
			base.OnCreate();

			addCharacterNode = node as StoryAddCharacterNode;
			UpdateGUI();
		}

		public override void UpdateGUI() {
			
			ID.text = addCharacterNode.ID;
			p_name.text = addCharacterNode.p_name;

			if (string.IsNullOrEmpty(addCharacterNode.image))
            {
				imageBtn.label = "please select image";
			}
			else
            {
				imageBtn.label = addCharacterNode.image;
			}

		}

		private void OnChangeID(string val) {
			addCharacterNode.ID = ID.text;
		}

		private void OnChangeImage() {

			graph.ShowImage((string _imageName) => 
			{
				addCharacterNode.image = _imageName;
				if (string.IsNullOrEmpty(_imageName))
				{
					imageBtn.label = "please select image";
				}
				else
				{
					imageBtn.label = _imageName;
				}
			});
		}

		private void OnChangeName(string val) {
			addCharacterNode.p_name = p_name.text;
		}
	}
}