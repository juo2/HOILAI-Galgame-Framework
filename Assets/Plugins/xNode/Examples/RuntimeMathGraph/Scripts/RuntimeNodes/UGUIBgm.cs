using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XNode.Examples.MathNodes;

namespace XNode.Story
{
	public class UGUIBgm : UGUIBaseNode {
		
		public InputField Bgm;

		private StoryBgmNode bgmNode;

		public override void Start() {
			base.Start();
			bgmNode = node as StoryBgmNode;

			Bgm.onValueChanged.AddListener(OnChangeBgm);
			UpdateGUI();
		}

		public override void UpdateGUI() {
			
			Bgm.text = bgmNode.bgm;
		}

		private void OnChangeBgm(string val) {
			bgmNode.bgm = Bgm.text;
		}
		
	}
}