using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace XGUI
{
    public class XView : MonoBehaviour, XIEvent
    {
        [SerializeField]
        private UnityObjectStructure m_Objects;
        [SerializeField]
        public class ViewEvent : UnityEvent { }
        protected ViewEvent m_OnDestroy = new ViewEvent();
        public XView.ViewEvent onDestroy { get { return m_OnDestroy; } }

        //private XLua.LuaTable m_InjectLuaTable;
        public virtual void Start()
        { }

        public Object Get(string name)
        {
            foreach (var item in m_Objects.unityObjects)
                if (item.name == name)
                    return item.component;
            return null;
        }

        public List<UnityObjectStructure.UnityObject> GetUnityObjects()
        {
            return m_Objects.unityObjects;
        }

        //public void InitInject(XLua.LuaTable tab)
        //{
        //    m_InjectLuaTable = tab;
        //    UnityEngine.Profiling.Profiler.BeginSample("XView.InitInject");
        //    foreach (var item in m_Objects.unityObjects)
        //        tab.Set<string, Object>(item.name, item.component);
        //    UnityEngine.Profiling.Profiler.EndSample();
        //}

        public virtual void ClearEvent()
        {
            if (this.m_OnDestroy != null)
            {
                this.m_OnDestroy.Invoke();
                this.m_OnDestroy.RemoveAllListeners();
                this.m_OnDestroy = null;
            }
        }

        public virtual void OnDestroy()
        {
            //if (m_InjectLuaTable != null)
            //{
            //    m_InjectLuaTable.Dispose();
            //    m_InjectLuaTable = null;
            //}

            ClearEvent();
            //#if UNITY_EDITOR
            if (m_Objects != null && m_Objects.unityObjects != null)
            {
                foreach (var item in m_Objects.unityObjects)
                {
                    if (item.component is XIEvent)
                        ((XIEvent)item.component).ClearEvent();
                }
                m_Objects.unityObjects.Clear();
            }
            //#endif
        }
        public void SetFindString(int name)
        {
            m_Objects.searchStringT = name;
        }
    }
}
