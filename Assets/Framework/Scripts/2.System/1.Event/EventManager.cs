using System;
using System.Collections.Generic;

namespace JKFrame
{
    /// <summary>
    /// 事件系统管理器
    /// </summary>
    public static class EventManager
    {
        #region 内部接口、内部类

        /// <summary>
        /// 事件信息接口
        /// </summary>
        interface IEventInfo { void Destroy(); };

        /// <summary>
        /// 无参-事件信息
        /// </summary>
        class EventInfo : IEventInfo
        {
            public Action Action;

            public void Init(Action action) => Action = action;

            public void Destroy()
            {
                Action = null;
                this.PushObj2Pool();
            }
        }

        /// <summary>
        /// 1个参数-事件信息
        /// </summary>
        class EventInfo<T> : IEventInfo
        {
            public Action<T> Action;

            public void Init(Action<T> action) => Action = action;

            public void Destroy()
            {
                Action = null;
                this.PushObj2Pool();
            }
        }

        /// <summary>
        /// 2个参数-事件信息
        /// </summary>
        class EventInfo<T, K> : IEventInfo
        {
            public Action<T, K> Action;

            public void Init(Action<T, K> action) => Action = action;

            public void Destroy()
            {
                Action = null;
                this.PushObj2Pool();
            }
        }

        /// <summary>
        /// 3个参数-事件信息
        /// </summary>
        class EventInfo<T, K, L> : IEventInfo
        {
            public Action<T, K, L> Action;

            public void Init(Action<T, K, L> action) => Action = action;

            public void Destroy()
            {
                Action = null;
                this.PushObj2Pool();
            }
        }

        #endregion

        static Dictionary<string, IEventInfo> s_EventInfoDic = new Dictionary<string, IEventInfo>();

        #region 添加事件的监听，你想要关心某个事件，当这个事件触时，会执行你传递过来的Action

        /// <summary>
        /// 添加无参事件
        /// </summary>
        public static void AddEventListener(string eventName, Action action)
        {
            // 有没有对应的事件可以监听
            if (s_EventInfoDic.ContainsKey(eventName))
            {
                (s_EventInfoDic[eventName] as EventInfo).Action += action;
            }

            // 没有的话，需要新增 到字典中，并添加对应的Action
            else
            {
                EventInfo eventInfo = PoolManager.Instance.Get<EventInfo>();
                eventInfo.Init(action);
                s_EventInfoDic.Add(eventName, eventInfo);
            }
        }

        /// <summary>
        /// 添加1个参数事件
        /// </summary>
        public static void AddEventListener<T>(string eventName, Action<T> action)
        {
            // 有没有对应的事件可以监听
            if (s_EventInfoDic.ContainsKey(eventName))
            {
                (s_EventInfoDic[eventName] as EventInfo<T>).Action += action;
            }

            // 没有的话，需要新增 到字典中，并添加对应的Action
            else
            {
                EventInfo<T> eventInfo = PoolManager.Instance.Get<EventInfo<T>>();
                eventInfo.Init(action);
                s_EventInfoDic.Add(eventName, eventInfo);
            }
        }

        /// <summary>
        /// 添加2个参数事件
        /// </summary>
        public static void AddEventListener<T, K>(string eventName, Action<T, K> action)
        {
            // 有没有对应的事件可以监听
            if (s_EventInfoDic.ContainsKey(eventName))
            {
                (s_EventInfoDic[eventName] as EventInfo<T, K>).Action += action;
            }

            // 没有的话，需要新增 到字典中，并添加对应的Action
            else
            {
                EventInfo<T, K> eventInfo = PoolManager.Instance.Get<EventInfo<T, K>>();
                eventInfo.Init(action);
                s_EventInfoDic.Add(eventName, eventInfo);
            }
        }

        /// <summary>
        /// 添加3个参数事件
        /// </summary>
        public static void AddEventListener<T, K, L>(string eventName, Action<T, K, L> action)
        {
            // 有没有对应的事件可以监听
            if (s_EventInfoDic.ContainsKey(eventName))
            {
                (s_EventInfoDic[eventName] as EventInfo<T, K, L>).Action += action;
            }

            // 没有的话，需要新增 到字典中，并添加对应的Action
            else
            {
                EventInfo<T, K, L> eventInfo = PoolManager.Instance.Get<EventInfo<T, K, L>>();
                eventInfo.Init(action);
                s_EventInfoDic.Add(eventName, eventInfo);
            }
        }

        #endregion

        #region 触发事件

        /// <summary>
        /// 触发无参的事件
        /// </summary>
        public static void EventTrigger(string eventName)
        {
            if (s_EventInfoDic.ContainsKey(eventName))
            {
                (s_EventInfoDic[eventName] as EventInfo).Action?.Invoke();
            }
        }

        /// <summary>
        /// 触发1个参数的事件
        /// </summary>
        public static void EventTrigger<T>(string eventName, T arg)
        {
            if (s_EventInfoDic.ContainsKey(eventName))
            {
                (s_EventInfoDic[eventName] as EventInfo<T>).Action?.Invoke(arg);
            }
        }

        /// <summary>
        /// 触发2个参数的事件
        /// </summary>
        public static void EventTrigger<T, K>(string eventName, T arg1, K arg2)
        {
            if (s_EventInfoDic.ContainsKey(eventName))
            {
                (s_EventInfoDic[eventName] as EventInfo<T, K>).Action?.Invoke(arg1, arg2);
            }
        }

        /// <summary>
        /// 触发3个参数的事件
        /// </summary>
        public static void EventTrigger<T, K, L>(string eventName, T arg1, K arg2, L arg3)
        {
            if (s_EventInfoDic.ContainsKey(eventName))
            {
                (s_EventInfoDic[eventName] as EventInfo<T, K, L>).Action?.Invoke(arg1, arg2, arg3);
            }
        }

        #endregion

        #region 取消事件的监听

        /// <summary>
        /// 移除无参的事件监听
        /// </summary>
        public static void RemoveEventListener(string eventName, Action action)
        {
            if (s_EventInfoDic.ContainsKey(eventName))
            {
                (s_EventInfoDic[eventName] as EventInfo).Action -= action;
            }
        }

        /// <summary>
        /// 移除1个参数的事件监听
        /// </summary>
        public static void RemoveEventListener<T>(string eventName, Action<T> action)
        {
            if (s_EventInfoDic.ContainsKey(eventName))
            {
                (s_EventInfoDic[eventName] as EventInfo<T>).Action -= action;
            }
        }

        /// <summary>
        /// 移除2个参数的事件监听
        /// </summary>
        public static void RemoveEventListener<T, K>(string eventName, Action<T, K> action)
        {
            if (s_EventInfoDic.ContainsKey(eventName))
            {
                (s_EventInfoDic[eventName] as EventInfo<T, K>).Action -= action;
            }
        }

        /// <summary>
        /// 移除3个参数的事件监听
        /// </summary>
        public static void RemoveEventListener<T, K, L>(string eventName, Action<T, K, L> action)
        {
            if (s_EventInfoDic.ContainsKey(eventName))
            {
                (s_EventInfoDic[eventName] as EventInfo<T, K, L>).Action -= action;
            }
        }

        #endregion

        #region 移除事件

        /// <summary>
        /// 移除/删除一个事件
        /// </summary>
        public static void RemoveEventListener(string eventName)
        {
            if (s_EventInfoDic.ContainsKey(eventName))
            {
                s_EventInfoDic[eventName].Destroy();
                s_EventInfoDic.Remove(eventName);
            }
        }

        /// <summary>
        /// 清空事件中心
        /// </summary>
        public static void Clear()
        {
            foreach (string eventName in s_EventInfoDic.Keys)
            {
                s_EventInfoDic[eventName].Destroy();
            }
            s_EventInfoDic.Clear();
        }

        #endregion
    }
}