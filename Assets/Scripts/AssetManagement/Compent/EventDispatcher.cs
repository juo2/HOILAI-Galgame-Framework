using System;
using System.Collections.Generic;

namespace XEvent
{

    public static class EventDispatcher
    {
        // 定义事件监听器结构体
        private struct EventListener
        {
            public Action<string> listener;
            public object listenerCaller;
        }

        // 存储事件监听器的字典
        private static Dictionary<string, List<EventListener>> eventMap = new Dictionary<string, List<EventListener>>();

        // 添加事件监听器
        public static void AddEventListener(string name, Action<string> listener, object listenerCaller)
        {
            if (!eventMap.ContainsKey(name))
                eventMap[name] = new List<EventListener>();

            // 检查是否已存在相同的监听器
            bool isExist = false;
            foreach (EventListener evtListener in eventMap[name])
            {
                if (evtListener.listener == listener && evtListener.listenerCaller == listenerCaller)
                {
                    isExist = true;
                    break;
                }
            }

            if (!isExist)
            {
                EventListener newListener = new EventListener
                {
                    listener = listener,
                    listenerCaller = listenerCaller,
                };
                eventMap[name].Add(newListener);
            }
        }

        // 移除事件监听器
        public static void RemoveEventListener(string name, Action<string> listener, object listenerCaller = null)
        {
            if (!eventMap.ContainsKey(name))
                return;

            eventMap[name].RemoveAll(evtListener =>
                evtListener.listener == listener && (listenerCaller == null || evtListener.listenerCaller == listenerCaller));
        }

        // 触发事件
        public static void DispatchEvent(string name, string args = "")
        {
            if (!eventMap.ContainsKey(name))
                return;

            List<EventListener> listeners = eventMap[name];
            foreach (EventListener evtListener in listeners)
            {
                if (evtListener.listener != null)
                {
                    int startTime = Environment.TickCount;
                    evtListener.listener(args);
                }
            }
        }

    }

}

