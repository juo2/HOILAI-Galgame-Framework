using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace XNode.Story
{
    public class StoryMessageLoopNode : StoryBaseNode
    {

        [SerializeField]
        public string image;

        [SerializeField]
        public string loop = "10";

        [SerializeField]
        public string value = "20";

        [SerializeField]
        public string success = "100";

        [SerializeField]
        public string fail = "0";
    }
}
