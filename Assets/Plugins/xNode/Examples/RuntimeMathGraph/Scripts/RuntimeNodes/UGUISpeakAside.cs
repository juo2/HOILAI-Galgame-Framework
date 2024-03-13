using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XNode.Examples.MathNodes;

namespace XNode.Story
{
	public class UGUISpeakAside : UGUIMathBaseNode {
		
		public InputField content;
		public InputField p_audio;

		private StorySpeakAsideNode speakAsideNode;

		public override void Start() {
			base.Start();
			speakAsideNode = node as StorySpeakAsideNode;

			content.onValueChanged.AddListener(OnChangeContent);
			p_audio.onValueChanged.AddListener(OnChangeAudio);
			UpdateGUI();
		}

		public override void UpdateGUI() {
			NodePort portX = node.GetInputPort("x");
			NodePort portY = node.GetInputPort("y");
			NodePort portZ = node.GetInputPort("z");
			//ID.gameObject.SetActive(!portX.IsConnected);
			//image.gameObject.SetActive(!portY.IsConnected);
			//p_name.gameObject.SetActive(!portZ.IsConnected);

			content.text = speakAsideNode.content;
			p_audio.text = speakAsideNode.audio;
		}

		private void OnChangeContent(string val) {
			speakAsideNode.content = content.text;
		}

		private void OnChangeAudio(string val) {
			speakAsideNode.audio = p_audio.text;
		}

	}
}