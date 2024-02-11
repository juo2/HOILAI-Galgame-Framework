﻿using UnityEngine;

namespace XModules.GalManager
{

    public class GalManager_CharacterMessage : MonoBehaviour
    {
        [SerializeField]
        public GalManager_CharacterAnimate Gal_CharacterAnimate;
        public void HandleMessage (string MessageContent)
        {

            Gal_CharacterAnimate.Animate_type = MessageContent;
            Gal_CharacterAnimate.HandleMessgae();
        }
    }
}