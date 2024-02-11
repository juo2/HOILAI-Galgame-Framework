using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace XNode.Story
{
    public class StoryBackgroundNode : Node
    {

        [Input] public Empty In;
        [Output] public Empty Out;

        [SerializeField]
        public string background;

        public override object GetValue(NodePort port)
        {
            //if (port.fieldName == "b") return GetInputValue<float>("a", a);
            //else return null;
            return null;
        }

        
    }
}
