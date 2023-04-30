using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace JKFrame
{
    public class PoolManager : ManagerBase<PoolManager>
    {
        [SerializeField] GameObject poolRootObj; // 对象池根节点

        public Dictionary<string, GameObjectPoolData> GameObjectPoolDict = new Dictionary<string, GameObjectPoolData>(); // GameObject对象容器

        public Dictionary<string, ObjectPoolData> ObjectPoolDict = new Dictionary<string, ObjectPoolData>(); // 普通类对象容器

        #region GameObject

        /// <summary>
        /// 获取GameObject对象上的T组件
        /// </summary>
        /// <typeparam name="T">需要获取的组件</typeparam>
        public T Get<T>(GameObject prefab, Transform parent = null) where T : Object
        {
            GameObject obj = Get(prefab, parent);
            if (obj != null) return obj.GetComponent<T>();
            return null;
        }

        /// <summary>
        /// 获取GameObject对象
        /// </summary>
        public GameObject Get(GameObject prefab, Transform parent = null)
        {
            GameObject obj;
            string prefabName = prefab.name;
            if (CheckGameObjectCache(prefab))
            {
                obj = GameObjectPoolDict[prefabName].Get(parent); // 从对象池获取对象
            }
            else
            {
                obj = Instantiate(prefab, parent); // 创建新对象
                obj.name = prefabName;
            }
            return obj;
        }

        /// <summary>
        /// 将GameObject放进对象池
        /// </summary>
        public void Push(GameObject gameObj)
        {
            string gameObjName = gameObj.name;

            // 现在有没有这一层
            if (GameObjectPoolDict.ContainsKey(gameObjName))
            {
                GameObjectPoolDict[gameObjName].Push(gameObj); // 放进已有的对象池
            }
            else
            {
                GameObjectPoolDict.Add(gameObjName, new GameObjectPoolData(gameObj, poolRootObj)); // 创建新的对象池
            }
        }

        /// <summary>
        /// 检查有没有某一层对象池数据
        /// </summary>
        bool CheckGameObjectCache(GameObject prefab)
        {
            string prefabName = prefab.name;
            return GameObjectPoolDict.ContainsKey(prefabName) && GameObjectPoolDict[prefabName].PoolQueue.Count > 0;
        }

        /// <summary>
        /// 检查缓存，如果成功则加载游戏物体，不成功返回Null
        /// </summary>
        /// <returns></returns>
        public GameObject CheckCacheAndLoadGameObject(string path, Transform parent = null)
        {
            // 通过路径获取最终预制体的名称 "UI/LoginWindow"
            string[] pathSplit = path.Split('/');
            string prefabName = pathSplit[^1];

            // 对象池有数据
            if (GameObjectPoolDict.ContainsKey(prefabName) && GameObjectPoolDict[prefabName].PoolQueue.Count > 0)
            {
                return GameObjectPoolDict[prefabName].Get(parent);
            }

            return null;
        }

        #endregion

        #region 普通对象相关操作

        /// <summary>
        /// 获取普通类对象
        /// </summary>
        public T Get<T>() where T : class, new()
        {
            if (CheckObjectCache<T>())
            {
                string fullName = typeof(T).FullName;
                var obj = (T)ObjectPoolDict[fullName].Get();
                return obj;
            }

            return new T();
        }

        /// <summary>
        /// 将普通类对象放进对象池
        /// </summary>
        /// <param name="obj">需要放进对象池的对象</param>
        public void Push(object obj)
        {
            string fullName = obj.GetType().FullName;

            // 现在有没有这一层
            if (ObjectPoolDict.ContainsKey(fullName))
            {
                ObjectPoolDict[fullName].Push(obj);
            }
            else
            {
                ObjectPoolDict.Add(fullName, new ObjectPoolData(obj));
            }
        }

        /// <summary>
        /// 检查有没有某一层对象池数据
        /// </summary>
        bool CheckObjectCache<T>()
        {
            string fullName = typeof(T).FullName;
            return ObjectPoolDict.ContainsKey(fullName) && ObjectPoolDict[fullName].poolQueue.Count > 0;
        }

        #endregion

        #region 删除

        /// <summary>
        /// 删除全部
        /// </summary>
        /// <param name="clearGameObject">是否删除游戏物体</param>
        /// <param name="clearCObject">是否删除普通C#对象</param>
        public void Clear(bool clearGameObject = true, bool clearCObject = true)
        {
            if (clearGameObject)
            {
                for (int i = 0; i < poolRootObj.transform.childCount; i++)
                {
                    Destroy(poolRootObj.transform.GetChild(i).gameObject);
                }
                GameObjectPoolDict.Clear();
            }
            if (clearCObject) ObjectPoolDict.Clear();
        }

        public void ClearAllGameObject() => Clear(true, false);

        public void ClearGameObject(string prefabName)
        {
            GameObject go = poolRootObj.transform.Find(prefabName).gameObject;
            if (go != null)
            {
                Destroy(go);
                GameObjectPoolDict.Remove(prefabName);
            }
        }

        public void ClearGameObject(GameObject prefab) => ClearGameObject(prefab.name);
        public void ClearAllObject() => Clear(false, true);
        public void ClearObject<T>() => ObjectPoolDict.Remove(typeof(T).FullName);
        public void ClearObject(Type type) => ObjectPoolDict.Remove(type.FullName);

        #endregion
    }
}