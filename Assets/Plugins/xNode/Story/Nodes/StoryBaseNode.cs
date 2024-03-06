using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XNode.Story
{
    public class StoryBaseNode : Node
    {

        [Input] public Empty In;
        [Output] public Empty Out;

        [HideInInspector]
        [SerializeField]
        public bool isChangeColor = false;

        [HideInInspector]
        [SerializeField]
        public bool isError = false;
    }
}
