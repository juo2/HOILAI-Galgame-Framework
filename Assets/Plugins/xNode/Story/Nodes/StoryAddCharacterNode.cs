using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace XNode.Story
{
    public class StoryAddCharacterNode : StoryBaseNode
    {

        [SerializeField]
        public string ID;

        [SerializeField]
        public string image;

        [SerializeField]
        public string p_name;

        [SerializeField]
        public bool isSelf = false;

        [SerializeField]
        public Animate_StartOrOutside animate;

        public override object GetValue(NodePort port)
        {
            //if (port.fieldName == "b") return GetInputValue<float>("a", a);
            //else return null;
            return null;
        }

        
    }
}
