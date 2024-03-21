using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XGUI;
using XNode.Examples.MathNodes;

namespace XNode.Story
{
	public class UGUISpeakAside : UGUIBaseNode {
		
		public InputField content;
		public XButton audioBtn;

		private StorySpeakAsideNode speakAsideNode;

		public override void Start() {
			base.Start();
			speakAsideNode = node as StorySpeakAsideNode;

			content.onValueChanged.AddListener(OnChangeContent);
			audioBtn.onClick.AddListener(OnChangeAudio);
			UpdateGUI();
		}

		public override void UpdateGUI() {
			
			content.text = speakAsideNode.content;

			if (string.IsNullOrEmpty(speakAsideNode.audio))
			{
				audioBtn.label = "please select audio";
			}
			else
			{
				audioBtn.label = speakAsideNode.audio;
			}
		}

		private void OnChangeContent(string val) {
			speakAsideNode.content = content.text;
		}

		private void OnChangeAudio() {

			graph.ShowAudio((string audio) =>
			{
				speakAsideNode.audio = audio;
				audioBtn.label = audio;
			});
		}

	}
}