using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace JKFrame
{
    /// <summary>
    /// 事件类型
    /// </summary>
    public enum JKEventType
    {
        OnMouseEnter, OnMouseExit, OnClick, OnClickDown, OnClickUp, OnDrag, OnBeginDrag, OnEndDrag, OnCollisionEnter
      , OnCollisionStay, OnCollisionExit, OnCollisionEnter2D, OnCollisionStay2D, OnCollisionExit2D, OnTriggerEnter
      , OnTriggerStay, OnTriggerExit, OnTriggerEnter2D, OnTriggerStay2D, OnTriggerExit2D
    }

    public interface IMouseEvent : IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerDownHandler
                                 , IPointerUpHandler, IBeginDragHandler, IEndDragHandler, IDragHandler { }

    /// <summary>
    /// 事件工具
    /// 可以添加 鼠标、碰撞、触发等事件
    /// </summary>
    public class JKEventListener : MonoBehaviour, IMouseEvent
    {
        #region 内部类、接口等

        /// <summary>
        /// 某个事件中一个时间的数据包装类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        class JKEventListenerEventInfo<T>
        {
            // T：事件本身的参数（PointerEventData、Collision）
            // object[]:事件的参数
            public Action<T, object[]> Action;
            public object[] Args;

            public void Init(Action<T, object[]> action, object[] args)
            {
                Action = action;
                Args = args;
            }

            public void Destroy()
            {
                Action = null;
                Args = null;
                this.PushObj2Pool();
            }

            public void TriggerEvent(T eventData)
            {
                Action?.Invoke(eventData, Args);
            }
        }

        interface IJKEventListenerEventInfos
        {
            void RemoveAll();
        }

        /// <summary>
        /// 一类事件的数据包装类型：包含多个JKEventListenerEventInfo
        /// </summary>
        /// <typeparam name="T"></typeparam>
        class JKEventListenerEventInfos<T> : IJKEventListenerEventInfos
        {
            // 所有的事件
            List<JKEventListenerEventInfo<T>> m_EventList = new();

            /// <summary>
            /// 添加事件
            /// </summary>
            public void AddListener(Action<T, object[]> action, params object[] args)
            {
                JKEventListenerEventInfo<T> info = PoolManager.Instance.GetObject<JKEventListenerEventInfo<T>>();
                info.Init(action, args);
                m_EventList.Add(info);
            }

            /// <summary>
            /// 移除事件
            /// </summary>
            public void RemoveListener(Action<T, object[]> action, bool checkArgs = false, params object[] args)
            {
                for (int i = 0; i < m_EventList.Count; i++)
                {
                    // 找到这个事件
                    if (m_EventList[i].Action.Equals(action))
                    {
                        // 是否需要检查参数
                        if (checkArgs && args.Length > 0)
                        {
                            // 参数如果相等
                            if (args.ArrayEquals(m_EventList[i].Args))
                            {
                                // 移除
                                m_EventList[i].Destroy();
                                m_EventList.RemoveAt(i);
                                return;
                            }
                        }
                        else
                        {
                            // 移除
                            m_EventList[i].Destroy();
                            m_EventList.RemoveAt(i);
                            return;
                        }
                    }
                }
            }

            /// <summary>
            /// 移除全部，全部放进对象池
            /// </summary>
            public void RemoveAll()
            {
                foreach (var eventInfo in m_EventList)
                {
                    eventInfo.Destroy();
                }
                m_EventList.Clear();
                this.PushObj2Pool();
            }

            public void TriggerEvent(T eventData)
            {
                foreach (var eventInfo in m_EventList)
                {
                    eventInfo.TriggerEvent(eventData);
                }
            }
        }

        /// <summary>
        /// 枚举比较器
        /// </summary>
        class JKEventTypeEnumComparer : Singleton<JKEventTypeEnumComparer>, IEqualityComparer<JKEventType>
        {
            public bool Equals(JKEventType x, JKEventType y)
            {
                return x == y;
            }

            public int GetHashCode(JKEventType obj)
            {
                return (int)obj;
            }
        }

        #endregion

        Dictionary<JKEventType, IJKEventListenerEventInfos> m_EventInfoDict = new(JKEventTypeEnumComparer.Instance);

        #region 外部的访问

        /// <summary>
        /// 添加事件
        /// </summary>
        public void AddListener<T>(JKEventType eventType, Action<T, object[]> action, params object[] args)
        {
            if (m_EventInfoDict.ContainsKey(eventType))
            {
                ((JKEventListenerEventInfos<T>)m_EventInfoDict[eventType]).AddListener(action, args);
            }
            else
            {
                JKEventListenerEventInfos<T> infos = PoolManager.Instance.GetObject<JKEventListenerEventInfos<T>>();
                infos.AddListener(action, args);
                m_EventInfoDict.Add(eventType, infos);
            }
        }

        /// <summary>
        /// 移除事件
        /// </summary>
        public void RemoveListener<T>
            (JKEventType eventType, Action<T, object[]> action, bool checkArgs = false, params object[] args)
        {
            if (m_EventInfoDict.ContainsKey(eventType))
            {
                ((JKEventListenerEventInfos<T>)m_EventInfoDict[eventType]).RemoveListener(action, checkArgs, args);
            }
        }

        /// <summary>
        /// 移除某一个事件类型下的全部事件
        /// </summary>
        /// <param name="eventType"></param>
        public void RemoveAllListener(JKEventType eventType)
        {
            if (m_EventInfoDict.ContainsKey(eventType))
            {
                m_EventInfoDict[eventType].RemoveAll();
                m_EventInfoDict.Remove(eventType);
            }
        }

        /// <summary>
        /// 移除全部事件
        /// </summary>
        public void RemoveAllListener()
        {
            foreach (IJKEventListenerEventInfos infos in m_EventInfoDict.Values)
            {
                infos.RemoveAll();
            }

            m_EventInfoDict.Clear();
        }

        #endregion

        /// <summary>
        /// 触发事件
        /// </summary>
        void TriggerAction<T>(JKEventType eventType, T eventData)
        {
            if (m_EventInfoDict.ContainsKey(eventType))
            {
                (m_EventInfoDict[eventType] as JKEventListenerEventInfos<T>)?.TriggerEvent(eventData);
            }
        }

        #region 鼠标事件

        public void OnPointerEnter(PointerEventData eventData) => TriggerAction(JKEventType.OnMouseEnter, eventData);

        public void OnPointerExit(PointerEventData eventData) => TriggerAction(JKEventType.OnMouseExit, eventData);

        public void OnBeginDrag(PointerEventData eventData) => TriggerAction(JKEventType.OnBeginDrag, eventData);

        public void OnDrag(PointerEventData eventData) => TriggerAction(JKEventType.OnDrag, eventData);

        public void OnEndDrag(PointerEventData eventData) => TriggerAction(JKEventType.OnEndDrag, eventData);

        public void OnPointerUp(PointerEventData eventData) => TriggerAction(JKEventType.OnClickUp, eventData);

        public void OnPointerDown(PointerEventData eventData) => TriggerAction(JKEventType.OnClickDown, eventData);

        public void OnPointerClick(PointerEventData eventData) => TriggerAction(JKEventType.OnClick, eventData);

        #endregion

        #region 碰撞事件

        void OnCollisionEnter(Collision other) => TriggerAction(JKEventType.OnCollisionEnter, other);

        void OnCollisionStay(Collision other) => TriggerAction(JKEventType.OnCollisionStay, other);

        void OnCollisionExit(Collision other) => TriggerAction(JKEventType.OnCollisionExit, other);

        void OnCollisionEnter2D(Collision2D other) => TriggerAction(JKEventType.OnCollisionEnter2D, other);

        void OnCollisionStay2D(Collision2D other) => TriggerAction(JKEventType.OnCollisionStay2D, other);

        void OnCollisionExit2D(Collision2D other) => TriggerAction(JKEventType.OnCollisionExit2D, other);

        #endregion

        #region 触发事件

        void OnTriggerEnter(Collider other) => TriggerAction(JKEventType.OnTriggerEnter, other);

        void OnTriggerStay(Collider other) => TriggerAction(JKEventType.OnTriggerStay, other);

        void OnTriggerExit(Collider other) => TriggerAction(JKEventType.OnTriggerExit, other);

        void OnTriggerEnter2D(Collider2D other) => TriggerAction(JKEventType.OnTriggerEnter2D, other);

        void OnTriggerStay2D(Collider2D other) => TriggerAction(JKEventType.OnTriggerStay2D, other);

        void OnTriggerExit2D(Collider2D other) => TriggerAction(JKEventType.OnTriggerExit2D, other);

        #endregion
    }
}