using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XGUI;
using XNode.Examples.MathNodes;

namespace XNode.Story
{
	public class UGUIVideo : UGUIBaseNode {
		
		public XButton videoBtn;

		private StoryVideoNode videoNode;

		public override void Start() {
			base.Start();
			videoNode = node as StoryVideoNode;

			videoBtn.onClick.AddListener(OnChangeVideo);
			UpdateGUI();
		}

		public override void OnCreate()
		{
			base.OnCreate();
			videoNode = node as StoryVideoNode;

			UpdateGUI();
		}

		public override void UpdateGUI() {

			if (string.IsNullOrEmpty(videoNode.video))
			{
				videoBtn.label = "please select video";
			}
			else
			{
				videoBtn.label = videoNode.video;
			}
		}

		private void OnChangeVideo() {
			graph.ShowVideo((string _video) =>
			{
				videoNode.video = _video;

				if (string.IsNullOrEmpty(_video))
				{
					videoBtn.label = "please select video";
				}
				else
				{
					videoBtn.label = _video;
				}
			});
		}

	}
}