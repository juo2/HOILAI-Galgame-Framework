using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XNode.Examples.MathNodes;

namespace XNode.Story
{
	public class UGUIExitGame : UGUIBaseNode {
		
		private StoryExitGameNode exitGameNode;

		public override void Start() {
			base.Start();
			exitGameNode = node as StoryExitGameNode;

			
			UpdateGUI();
		}

		public override void OnCreate()
		{
			base.OnCreate();
			exitGameNode = node as StoryExitGameNode;
			UpdateGUI();
		}

		public override void UpdateGUI() {
			

		}

	}
}