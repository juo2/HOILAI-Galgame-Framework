using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XGUI;
using XNode.Examples.MathNodes;

namespace XNode.Story
{
	public class UGUIMessageLoop : UGUIBaseNode {
		
		public XButton imageBtn;
		public InputField loop;
		public InputField success;
		public InputField fail;

		private StoryMessageLoopNode messageNode;

		public override void Start() {
			base.Start();
			messageNode = node as StoryMessageLoopNode;

			imageBtn.onClick.AddListener(OnChangeImage);
			loop.onValueChanged.AddListener(OnChangeLoop);
			success.onValueChanged.AddListener(OnChangeSuccess);
			fail.onValueChanged.AddListener(OnChangeFail);
			UpdateGUI();
		}

		public override void OnCreate()
		{
			base.OnCreate();
			messageNode = node as StoryMessageLoopNode;

			UpdateGUI();
		}

		public override void UpdateGUI() {
			//imageBtn.label = messageNode.image;
			loop.text = messageNode.loop;
			success.text = messageNode.success;
			fail.text = messageNode.fail;

			if (string.IsNullOrEmpty(messageNode.image))
			{
				imageBtn.label = "please select image";
			}
			else
			{
				imageBtn.label = messageNode.image;
			}
		}


		private void OnChangeImage() {
			graph.ShowImage((string _imageName) =>
			{
				messageNode.image = _imageName;
				if (string.IsNullOrEmpty(_imageName))
				{
					imageBtn.label = "please select image";
				}
				else
				{
					imageBtn.label = _imageName;
				}
			});
		}

		private void OnChangeLoop(string val) {
			messageNode.loop = loop.text;
		}

		private void OnChangeSuccess(string val)
		{
			messageNode.success = success.text;
		}

		private void OnChangeFail(string val)
		{
			messageNode.fail = fail.text;
		}
	}
}