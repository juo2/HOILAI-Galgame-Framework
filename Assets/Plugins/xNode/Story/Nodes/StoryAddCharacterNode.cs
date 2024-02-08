using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace XNode.Story
{
    public class StoryAddCharacterNode : Node
    {

        [Input] public Empty In;
        [Output] public Empty Out;

        public AddStoryCharacter addStoryCharacter;

        public override object GetValue(NodePort port)
        {
            //if (port.fieldName == "b") return GetInputValue<float>("a", a);
            //else return null;
            return null;
        }

        [System.Serializable]
        public class AddStoryCharacter
        {
            [SerializeField]
            string ID;

            [SerializeField]
            string Animate;
        }
    }
}
