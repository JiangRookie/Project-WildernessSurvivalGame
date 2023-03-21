using UnityEngine;

namespace JKFrame
{
    /// <summary>
    /// 基于 MonoBehaviour 的泛型单例模式基类，主动激活单例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SingletonMono<T> : MonoBehaviour where T : SingletonMono<T>
    {
        public static T Instance;

        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this as T;
            }
        }
    }
}