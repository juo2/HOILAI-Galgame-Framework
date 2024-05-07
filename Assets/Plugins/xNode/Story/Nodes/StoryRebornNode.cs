using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace XNode.Story
{
    public class StoryRebornNode : StoryBaseNode
    {
        [SerializeField]
        public string storyGraphicName;

        public override object GetValue(NodePort port)
        {
            //if (port.fieldName == "b") return GetInputValue<float>("a", a);
            //else return null;
            return null;
        }

        
    }
}
