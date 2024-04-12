using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XModules
{
    public class XBaseView : MonoBehaviour
    {
        public object[] viewArgs;

        public Action finishAction;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public virtual void OnEnableView()
        {
            
        }

        public virtual void OnDisableView()
        {
            finishAction?.Invoke();
        }
    }
}


