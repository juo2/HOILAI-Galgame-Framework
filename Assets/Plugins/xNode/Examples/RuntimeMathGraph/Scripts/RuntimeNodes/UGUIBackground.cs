using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XNode.Examples.MathNodes;

namespace XNode.Story
{
	public class UGUIBackground : UGUIMathBaseNode {
		
		public InputField BG;

		private StoryBackgroundNode backgroundNode;

		public override void Start() {
			base.Start();
			backgroundNode = node as StoryBackgroundNode;

			BG.onValueChanged.AddListener(OnChangeBG);
			UpdateGUI();
		}

		public override void UpdateGUI() {
			NodePort portX = node.GetInputPort("x");
			NodePort portY = node.GetInputPort("y");
			NodePort portZ = node.GetInputPort("z");
			//ID.gameObject.SetActive(!portX.IsConnected);
			//image.gameObject.SetActive(!portY.IsConnected);
			//p_name.gameObject.SetActive(!portZ.IsConnected);
			BG.text = backgroundNode.background;
		}

		private void OnChangeBG(string val) {
			backgroundNode.background = BG.text;
		}
		
	}
}