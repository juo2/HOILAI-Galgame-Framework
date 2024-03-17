﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XNode.Examples.MathNodes;

namespace XNode.Story
{
	public class UGUIMessage : UGUIBaseNode {
		
		public InputField image;
		public InputField opt1;
		public InputField opt2;
		public InputField opt3;
		public InputField opt4;


		private StoryMessageNode messageNode;

		public override void Start() {
			base.Start();
			messageNode = node as StoryMessageNode;

			image.onValueChanged.AddListener(OnChangeImage);
			opt1.onValueChanged.AddListener(OnChangeOpt1);
			opt2.onValueChanged.AddListener(OnChangeOpt2);
			opt3.onValueChanged.AddListener(OnChangeOpt3);
			opt4.onValueChanged.AddListener(OnChangeOpt4);
			UpdateGUI();
		}

		public override void UpdateGUI() {
			image.text = messageNode.image;
			opt1.text = messageNode.opt1;
			opt2.text = messageNode.opt2;
			opt3.text = messageNode.opt3;
			opt4.text = messageNode.opt4;
		}


		private void OnChangeImage(string val) {
			messageNode.image = image.text;
		}

		private void OnChangeOpt1(string val) {
			messageNode.opt1 = opt1.text;
		}

		private void OnChangeOpt2(string val)
		{
			messageNode.opt2 = opt2.text;
		}

		private void OnChangeOpt3(string val)
		{
			messageNode.opt3 = opt3.text;
		}

		private void OnChangeOpt4(string val)
		{
			messageNode.opt4 = opt4.text;
		}
	}
}