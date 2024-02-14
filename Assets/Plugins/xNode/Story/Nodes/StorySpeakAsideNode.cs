using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace XNode.Story
{
    public class StorySpeakAsideNode : Node
    {
        [Input] public Empty In;
        [Output] public Empty Out;

        [SerializeField]
        public string content;

        [SerializeField]
        public string audio;

    }
}
