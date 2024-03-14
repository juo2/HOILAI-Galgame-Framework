using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XNode.Examples.MathNodes;

namespace XNode.Story
{
	public class UGUIVideo : UGUIBaseNode {
		
		public InputField video;

		private StoryVideoNode videoNode;

		public override void Start() {
			base.Start();
			videoNode = node as StoryVideoNode;

			video.onValueChanged.AddListener(OnChangeVideo);
			UpdateGUI();
		}

		public override void UpdateGUI() {
			NodePort portX = node.GetInputPort("x");
			NodePort portY = node.GetInputPort("y");
			NodePort portZ = node.GetInputPort("z");
			//ID.gameObject.SetActive(!portX.IsConnected);
			//image.gameObject.SetActive(!portY.IsConnected);
			//p_name.gameObject.SetActive(!portZ.IsConnected);

			video.text = videoNode.video;
		}

		private void OnChangeVideo(string val) {
			videoNode.video = video.text;
		}

	}
}