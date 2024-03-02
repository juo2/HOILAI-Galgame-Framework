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

            // ���һ���˵������ڸı���ɫ
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
            Color color = base.GetTint(); // Ĭ��ʹ�û������ɫ

            StoryBaseNode node = target as StoryBaseNode;
            if (node.isChangeColor)
            {
                // ���Խ��ַ���ת��Ϊ��ɫֵ������ɹ�����`color`�����ᱻ����
                ColorUtility.TryParseHtmlString("#2D6B6BFF", out color);
            }
            return color;
        }
    }

}



