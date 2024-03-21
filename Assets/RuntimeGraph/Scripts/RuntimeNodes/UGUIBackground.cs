using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XGUI;
using XNode.Examples.MathNodes;

namespace XNode.Story
{
	public class UGUIBackground : UGUIBaseNode {
		
		public XButton BGBtn;
		private StoryBackgroundNode backgroundNode;

		public override void Start() {
			base.Start();
			backgroundNode = node as StoryBackgroundNode;

			BGBtn.onClick.AddListener(OnChangeBG);
			UpdateGUI();
		}

		public override void OnCreate()
		{
			base.OnCreate();
			backgroundNode = node as StoryBackgroundNode;
			UpdateGUI();
		}

		public override void UpdateGUI() {
			if (string.IsNullOrEmpty(backgroundNode.background))
			{
				BGBtn.label = "please select image";
			}
			else
			{
				BGBtn.label = backgroundNode.background;
			}
		}

		private void OnChangeBG() {
			graph.ShowImage((string _imageName) =>
			{
				backgroundNode.background = _imageName;
				BGBtn.label = _imageName;
			});
		}
		
	}
}