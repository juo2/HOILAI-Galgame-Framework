using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace XNode.Story
{

    [CreateAssetMenu]
    public class StoryGraph : NodeGraph
    {

    }


    [System.Serializable]
    public class Empty
    {

    }

    [System.Serializable]
    public class Choice
    {
       
    }


    public enum Animate_StartOrOutside
    {
        None,
        ToShow,
        Outside_ToLeft,
        Outside_ToRight,
    }

    public enum Animate_type
    {
        None,
        Shake,
        Shake_Y_Once,
        ToLeft,
        ToCenter,
        ToRight,
    }
}