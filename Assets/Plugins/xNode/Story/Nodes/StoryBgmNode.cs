using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace XNode.Story
{
    public class StoryBgmNode : StoryBaseNode
    {
        [SerializeField]
        public string bgm;

        public override object GetValue(NodePort port)
        {
            //if (port.fieldName == "b") return GetInputValue<float>("a", a);
            //else return null;
            return null;
        }

        
    }
}
