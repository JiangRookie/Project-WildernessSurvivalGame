using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace JKFrame
{
    /// <summary>
    /// JKFrame 框架主要的拓展方法
    /// </summary>
    public static class JExtension
    {
        #region 通用

        public static T GetAttribute<T>(this object obj) where T : Attribute
        {
            return obj.GetType().GetCustomAttribute<T>();
        }

        public static T GetAttribute<T>(this object obj, Type type) where T : Attribute
        {
            return type.GetCustomAttribute<T>();
        }

        /// <summary>
        /// 数组相等对比
        /// </summary>
        public static bool ArrayEquals(this object[] objs, object[] other)
        {
            if (other == null || objs.GetType() != other.GetType())
            {
                return false;
            }

            if (objs.Length == other.Length)
            {
                for (int i = 0; i < objs.Length; i++)
                {
                    if (!objs[i].Equals(other[i]))
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        #endregion

        #region 资源管理

        /// <summary>
        /// GameObject放入对象池
        /// </summary>
        public static void PushGameObj2Pool(this GameObject gameObj)
        {
            PoolManager.Instance.Push(gameObj);
        }

        /// <summary>
        /// 将GameObject放入对象池
        /// </summary>
        public static void PushGameObj2Pool(this Component component)
        {
            PushGameObj2Pool(component.gameObject);
        }

        /// <summary>
        /// 普通类放进池子
        /// </summary>
        /// <param name="obj"></param>
        public static void PushObj2Pool(this object obj)
        {
            PoolManager.Instance.Push(obj);
        }

        #endregion

        #region 本地化

        /// <summary>
        /// 从本地化系统中修改内容
        /// </summary>
        /// <param name="text"></param>
        /// <param name="packName"></param>
        /// <param name="contentKey"></param>
        public static void LocalSet(this Text text, string packName, string contentKey)
        {
            text.text = LocalizationManager.Instance.GetContent<L_Text>(packName, contentKey).content;
        }

        /// <summary>
        /// 从本地化系统中修改内容
        /// </summary>
        /// <param name="image"></param>
        /// <param name="packName"></param>
        /// <param name="contentKey"></param>
        public static void LocalSet(this Image image, string packName, string contentKey)
        {
            image.sprite = LocalizationManager.Instance.GetContent<L_Image>(packName, contentKey).content;
        }

        /// <summary>
        /// 从本地化系统中修改内容
        /// </summary>
        /// <param name="audioSource"></param>
        /// <param name="packName"></param>
        /// <param name="contentKey"></param>
        public static void LocalSet(this AudioSource audioSource, string packName, string contentKey)
        {
            audioSource.clip = LocalizationManager.Instance.GetContent<L_Audio>(packName, contentKey).content;
        }

        /// <summary>
        /// 从本地化系统中修改内容
        /// </summary>
        /// <param name="videoPlayer"></param>
        /// <param name="packName"></param>
        /// <param name="contentKey"></param>
        public static void LocalSet(this VideoPlayer videoPlayer, string packName, string contentKey)
        {
            videoPlayer.clip = LocalizationManager.Instance.GetContent<L_Video>(packName, contentKey).content;
        }

        #endregion

        #region MonoBehaviour

        /// <summary>
        /// 添加Update监听
        /// </summary>
        public static void OnUpdate(this object obj, Action action)
        {
            MonoManager.Instance.AddUpdateListener(action);
        }

        /// <summary>
        /// 移除Update监听
        /// </summary>
        public static void RemoveUpdate(this object obj, Action action)
        {
            MonoManager.Instance.RemoveUpdateListener(action);
        }

        /// <summary>
        /// 添加LateUpdate监听
        /// </summary>
        public static void OnLateUpdate(this object obj, Action action)
        {
            MonoManager.Instance.AddLateUpdateListener(action);
        }

        /// <summary>
        /// 移除LateUpdate监听
        /// </summary>
        public static void RemoveLateUpdate(this object obj, Action action)
        {
            MonoManager.Instance.RemoveLateUpdateListener(action);
        }

        /// <summary>
        /// 添加FixedUpdate监听
        /// </summary>
        public static void OnFixedUpdate(this object obj, Action action)
        {
            MonoManager.Instance.AddFixedUpdateListener(action);
        }

        /// <summary>
        /// 移除Update监听
        /// </summary>
        public static void RemoveFixedUpdate(this object obj, Action action)
        {
            MonoManager.Instance.RemoveFixedUpdateListener(action);
        }

        public static Coroutine StartCoroutine(this object obj, IEnumerator routine)
        {
            return MonoManager.Instance.StartCoroutine(routine);
        }

        public static void StopCoroutine(this object obj, Coroutine routine)
        {
            MonoManager.Instance.StopCoroutine(routine);
        }

        public static void StopAllCoroutines(this object obj)
        {
            MonoManager.Instance.StopAllCoroutines();
        }

        #endregion
    }

    public static class GameObjectExtension
    {
        public static GameObject Show(this GameObject selfObj)
        {
            selfObj.SetActive(true);
            return selfObj;
        }

        public static T Show<T>(this T selfComponent) where T : Component
        {
            selfComponent.gameObject.SetActive(true);
            return selfComponent;
        }

        public static GameObject Hide(this GameObject selfObj)
        {
            selfObj.SetActive(false);
            return selfObj;
        }

        public static T Hide<T>(this T selfComponent) where T : Component
        {
            selfComponent.gameObject.SetActive(false);
            return selfComponent;
        }
    }

    public static class BehaviourExtension
    {
        public static T Enable<T>(this T selfBehaviour) where T : Behaviour
        {
            selfBehaviour.enabled = true;
            return selfBehaviour;
        }

        public static T Disable<T>(this T selfBehaviour) where T : Behaviour
        {
            selfBehaviour.enabled = false;
            return selfBehaviour;
        }
    }

    public static class ColliderExtension
    {
        public static T Enable<T>(this T selfCollider) where T : Collider
        {
            selfCollider.enabled = true;
            return selfCollider;
        }

        public static T Disable<T>(this T selfCollider) where T : Collider
        {
            selfCollider.enabled = false;
            return selfCollider;
        }
    }
}