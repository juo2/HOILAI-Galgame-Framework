using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNode.Story;

namespace XNodeEditor.Story
{
    [CustomNodeEditor(typeof(StoryBaseNode))]
    public class StoryBaseNodeEditor : NodeEditor
    {
        public override void AddContextMenuItems(GenericMenu menu)
        {
            base.AddContextMenuItems(menu);

            // 添加一个菜单项用于改变颜色
            menu.AddItem(new GUIContent("Change Color"), false, () => ChangeNodeColor());
        }

        public void ChangeNodeColor()
        {
            Debug.Log("Change color");

            StoryBaseNode node = target as StoryBaseNode;
            node.isChangeColor = !node.isChangeColor;
        }

        public override Color GetTint()
        {
            Color color = base.GetTint(); // 默认使用基类的颜色

            StoryBaseNode node = target as StoryBaseNode;
            if (node.isChangeColor)
            {
                // 尝试将字符串转换为颜色值，如果成功，则`color`变量会被更新
                ColorUtility.TryParseHtmlString("#2D6B6BFF", out color);
            }
            return color;
        }
    }

}



