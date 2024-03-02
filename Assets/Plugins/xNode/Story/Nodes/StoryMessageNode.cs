using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace XNode.Story
{
    public class StoryMessageNode : StoryBaseNode
    {

        [SerializeField]
        public string image;

        [Output] public Choice outOpt1;

        [Output] public Choice outOpt2;

        [Output] public Choice outOpt3;

        [Output] public Choice outOpt4;


        [SerializeField]
        public string opt1;

        [SerializeField]
        public string opt2;

        [SerializeField]
        public string opt3;

        [SerializeField]
        public string opt4;
    }
}
