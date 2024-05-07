using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XNode.Examples.MathNodes;

namespace XNode.Story
{
	public class UGUIReborn : UGUIBaseNode {
		
		private StoryNextChapterNode nextChapterNode;

		public override void Start() {
			base.Start();
			nextChapterNode = node as StoryNextChapterNode;

			UpdateGUI();
		}

		public override void OnCreate()
		{
			base.OnCreate();
			nextChapterNode = node as StoryNextChapterNode;

			UpdateGUI();
		}

		public override void UpdateGUI() {

		}
	}
}