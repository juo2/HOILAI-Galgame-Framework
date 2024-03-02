using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace XNode.Story
{
    public class StorySpeakAsideNode : StoryBaseNode
    {

        [SerializeField]
        public string content;

        [SerializeField]
        public string audio;

    }
}
