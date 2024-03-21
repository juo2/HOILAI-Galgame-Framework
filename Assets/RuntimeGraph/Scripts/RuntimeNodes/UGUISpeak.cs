using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XGUI;

namespace XNode.Story
{
	public class UGUISpeak : UGUIBaseNode {
		
		public XButton IDBtn;
		public XButton imageBtn;
		public InputField content;
		public XButton audioBtn;

		public Toggle isJump;

		public InputField opt1;
		public InputField opt2;
		public InputField opt3;
		public InputField opt4;

		public GameObject panel;

		private StorySpeakNode speakNode;

		public override void Start() {
			base.Start();

			IDBtn.onClick.AddListener(OnChangeID);
			imageBtn.onClick.AddListener(OnChangeImage);
			content.onValueChanged.AddListener(OnChangeContent);
			audioBtn.onClick.AddListener(OnChangeAudio);
			opt1.onValueChanged.AddListener(OnChangeOpt1);
			opt2.onValueChanged.AddListener(OnChangeOpt2);
			opt3.onValueChanged.AddListener(OnChangeOpt3);
			opt4.onValueChanged.AddListener(OnChangeOpt4);
			isJump.onValueChanged.AddListener(OnChangeJump);

			speakNode = node as StorySpeakNode;
			Invoke("startPanel", 0.5f);
			UpdateGUI();
		}

		void startPanel()
        {
			panel.SetActive(speakNode.isJump);
		}

        public override void OnCreate()
        {
            base.OnCreate();
			speakNode = node as StorySpeakNode;
			Invoke("startPanel", 0.5f);
			UpdateGUI();
		}

        public override void UpdateGUI() {
			
			//ID.text = speakNode.ID;
			content.text = speakNode.content;
			opt1.text = speakNode.opt1;
			opt2.text = speakNode.opt2;
			opt3.text = speakNode.opt3;
			opt4.text = speakNode.opt4;
			isJump.isOn = speakNode.isJump;

			if (string.IsNullOrEmpty(speakNode.image))
			{
				imageBtn.label = "please select image";
			}
			else
			{
				imageBtn.label = speakNode.image;
			}

			if (string.IsNullOrEmpty(speakNode.ID))
			{
				IDBtn.label = "please select id";
			}
			else
			{
				IDBtn.label = speakNode.ID;
			}

			if (string.IsNullOrEmpty(speakNode.audio))
			{
				audioBtn.label = "please select audio";
			}
			else
			{
				audioBtn.label = speakNode.audio;
			}
		}

		private void OnChangeID() {

			graph.ShowCharacter((string _id) => 
			{ 
				speakNode.ID = _id;
				IDBtn.label = _id;
			});

		}

		private void OnChangeImage() {
			graph.ShowImage((string _imageName) =>
			{
				speakNode.image = _imageName;
				imageBtn.label = _imageName;
			});
		}

		private void OnChangeContent(string val) {
			speakNode.content = content.text;
		}

		private void OnChangeAudio()
		{
			graph.ShowAudio((string audio) =>
			{
				speakNode.audio = audio;
				audioBtn.label = audio;
			});
		}

		private void OnChangeJump(bool _isJump)
        {
			panel.SetActive(_isJump);
			speakNode.isJump = _isJump;
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