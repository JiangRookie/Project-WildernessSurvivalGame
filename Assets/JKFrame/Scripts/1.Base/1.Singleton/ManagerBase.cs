using UnityEngine;

namespace JKFrame
{
    public abstract class ManagerBase : MonoBehaviour
    {
        public virtual void Init() { }
    }

    /// <summary>
    /// 基于 MonoBehaviour 的泛型单例基类，非主动激活单例
    /// </summary>
    public abstract class ManagerBase<T> : ManagerBase where T : ManagerBase<T>
    {
        public static T Instance;

        /// <summary>
        /// 管理器的初始化
        /// </summary>
        public override void Init()
        {
            Instance = this as T;
        }
    }
}