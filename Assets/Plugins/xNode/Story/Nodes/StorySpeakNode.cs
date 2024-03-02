using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace XNode.Story
{
    public class StorySpeakNode : StoryBaseNode
    {
        [SerializeField]
        public string ID;

        [SerializeField]
        public string image;

        [SerializeField]
        public string content;

        [SerializeField]
        public Animate_type animate;

        [SerializeField]
        public string audio;

        [SerializeField]
        public bool isJump = false;

        [Output] public Choice outOpt1;

        [Output] public Choice outOpt2;

        [Output] public Choice outOpt3;

        [SerializeField]
        public string opt1;

        [SerializeField]
        public string opt2;

        [SerializeField]
        public string opt3;
    }
}
