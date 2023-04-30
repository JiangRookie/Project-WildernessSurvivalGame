using System;

namespace JKFrame
{
    public class MonoManager : ManagerBase<MonoManager>
    {
        Action m_UpdateEvent;
        Action m_LateUpdateEvent;
        Action m_FixedUpdateEvent;

        /// <summary>
        /// 添加Update监听
        /// </summary>
        /// <param name="action"></param>
        public void AddUpdateListener(Action action) => m_UpdateEvent += action;

        /// <summary>
        /// 移除Update监听
        /// </summary>
        /// <param name="action"></param>
        public void RemoveUpdateListener(Action action) => m_UpdateEvent -= action;

        /// <summary>
        /// 添加LateUpdate监听
        /// </summary>
        /// <param name="action"></param>
        public void AddLateUpdateListener(Action action) => m_LateUpdateEvent += action;

        /// <summary>
        /// 移除LateUpdate监听
        /// </summary>
        /// <param name="action"></param>
        public void RemoveLateUpdateListener(Action action) => m_LateUpdateEvent -= action;

        /// <summary>
        /// 添加FixedUpdate监听
        /// </summary>
        /// <param name="action"></param>
        public void AddFixedUpdateListener(Action action) => m_FixedUpdateEvent += action;

        /// <summary>
        /// 移除FixedUpdate监听
        /// </summary>
        /// <param name="action"></param>
        public void RemoveFixedUpdateListener(Action action) => m_FixedUpdateEvent -= action;

        void Update() => m_UpdateEvent?.Invoke();

        void LateUpdate() => m_LateUpdateEvent?.Invoke();

        void FixedUpdate() => m_FixedUpdateEvent?.Invoke();
    }
}