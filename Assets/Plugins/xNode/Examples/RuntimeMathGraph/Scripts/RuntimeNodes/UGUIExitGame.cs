using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XNode.Examples.MathNodes;

namespace XNode.Story
{
	public class UGUIExitGame : UGUIMathBaseNode {
		
		private StoryExitGameNode exitGameNode;

		public override void Start() {
			base.Start();
			exitGameNode = node as StoryExitGameNode;

			
			UpdateGUI();
		}

		public override void UpdateGUI() {
			NodePort portX = node.GetInputPort("x");
			NodePort portY = node.GetInputPort("y");
			NodePort portZ = node.GetInputPort("z");
			//ID.gameObject.SetActive(!portX.IsConnected);
			//image.gameObject.SetActive(!portY.IsConnected);

		}

	}
}