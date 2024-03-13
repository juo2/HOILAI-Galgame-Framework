using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XNode.Examples.MathNodes;

namespace XNode.Story
{
	public class UGUISpeak : UGUIMathBaseNode {
		
		public InputField ID;
		public InputField image;
		public InputField content;
		public InputField p_audio;

		public Toggle isJump;

		public InputField opt1;
		public InputField opt2;
		public InputField opt3;
		public InputField opt4;

		public GameObject panel;

		private StorySpeakNode speakNode;

		public override void Start() {
			base.Start();
			speakNode = node as StorySpeakNode;

			ID.onValueChanged.AddListener(OnChangeID);
			image.onValueChanged.AddListener(OnChangeImage);
			content.onValueChanged.AddListener(OnChangeContent);
			p_audio.onValueChanged.AddListener(OnChangeAudio);
			opt1.onValueChanged.AddListener(OnChangeOpt1);
			opt2.onValueChanged.AddListener(OnChangeOpt2);
			opt3.onValueChanged.AddListener(OnChangeOpt3);
			opt4.onValueChanged.AddListener(OnChangeOpt4);
			isJump.onValueChanged.AddListener(OnChangeJump);

			UpdateGUI();
		}

		public override void UpdateGUI() {
			NodePort portX = node.GetInputPort("x");
			NodePort portY = node.GetInputPort("y");
			NodePort portZ = node.GetInputPort("z");
			//ID.gameObject.SetActive(!portX.IsConnected);
			//image.gameObject.SetActive(!portY.IsConnected);
			//p_name.gameObject.SetActive(!portZ.IsConnected);

			Vector vectorNode = node as Vector;
			ID.text = speakNode.ID;
			image.text = speakNode.image;
			content.text = speakNode.content;
			p_audio.text = speakNode.audio;
			opt1.text = speakNode.opt1;
			opt2.text = speakNode.opt2;
			opt3.text = speakNode.opt3;
			opt4.text = speakNode.opt4;
			isJump.isOn = speakNode.isJump;
		}

		private void OnChangeID(string val) {
			speakNode.ID = ID.text;
		}

		private void OnChangeImage(string val) {
			speakNode.image = image.text;
		}

		private void OnChangeContent(string val) {
			speakNode.content = content.text;
		}

		private void OnChangeAudio(string val)
		{
			speakNode.audio = p_audio.text;
		}

		private void OnChangeJump(bool isJump)
        {
			panel.SetActive(isJump);
			speakNode.isJump = isJump;
		}

		private void OnChangeOpt1(string val)
		{
			speakNode.opt1 = opt1.text;
		}

		private void OnChangeOpt2(string val)
		{
			speakNode.opt2 = opt2.text;
		}

		private void OnChangeOpt3(string val)
		{
			speakNode.opt3 = opt3.text;
		}

		private void OnChangeOpt4(string val)
		{
			speakNode.opt4 = opt4.text;
		}
	}
}