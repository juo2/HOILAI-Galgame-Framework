using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XGUI;
using XNode.Examples.MathNodes;

namespace XNode.Story
{
	public class UGUIBgm : UGUIBaseNode {
		
		public XButton BgmBtn;

		private StoryBgmNode bgmNode;

		public override void Start() {
			base.Start();
			bgmNode = node as StoryBgmNode;

			BgmBtn.onClick.AddListener(OnChangeBgm);
			UpdateGUI();
		}

		public override void OnCreate()
		{
			base.OnCreate();
			bgmNode = node as StoryBgmNode;
			UpdateGUI();
		}

		public override void UpdateGUI() {
			
			if (string.IsNullOrEmpty(bgmNode.bgm))
			{
				BgmBtn.label = "please select bgm";
			}
			else
			{
				BgmBtn.label = bgmNode.bgm;
			}
		}

		private void OnChangeBgm() {

			graph.ShowBgm((string _bgm) =>
			{
				bgmNode.bgm = _bgm;
				if (string.IsNullOrEmpty(_bgm))
				{
					BgmBtn.label = "please select bgm";
				}
				else
				{
					BgmBtn.label = _bgm;
				}
			});

		}
		
	}
}