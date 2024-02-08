using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace XNode.Story
{
    public class StoryDeleteCharacter : Node
    {
        [Input] public Empty In;
        [Output] public Empty Out;

        public DelStoryCharacter delStoryCharacter;

        public override object GetValue(NodePort port)
        {
            //if (port.fieldName == "b") return GetInputValue<float>("a", a);
            //else return null;
            return null;
        }

        [System.Serializable]
        public class DelStoryCharacter
        {
            [SerializeField]
            string ID;

            [SerializeField]
            string Animate;
        }
    }

}

