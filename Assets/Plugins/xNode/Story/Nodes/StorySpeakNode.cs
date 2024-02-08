using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace XNode.Story
{
    public class StorySpeakNode : Node
    {
        [Input] public Empty In;
        [Output] public Empty Out;
       

        [SerializeField]
        public string content;

        [SerializeField]
        public string animate;

        [SerializeField]
        public string audio;

        [SerializeField]
        public bool isJump = false;

        [Output] public Choice outOpt1;

        [Output] public Choice outOpt2;

        [SerializeField]
        public string opt1;

        [SerializeField]
        public string opt2;

    }
}
