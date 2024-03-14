using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XNode.Examples.MathNodes;

namespace XNode.Story
{
	public class UGUIBackground : UGUIBaseNode {
		
		public InputField BG;

		private StoryBackgroundNode backgroundNode;

		public override void Start() {
			base.Start();
			backgroundNode = node as StoryBackgroundNode;

			BG.onValueChanged.AddListener(OnChangeBG);
			UpdateGUI();
		}

		public override void UpdateGUI() {
			
			BG.text = backgroundNode.background;
		}

		private void OnChangeBG(string val) {
			backgroundNode.background = BG.text;
		}
		
	}
}