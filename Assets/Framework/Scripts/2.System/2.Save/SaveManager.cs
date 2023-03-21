using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace JKFrame
{
    /// <summary>
    /// 一个存档的数据
    /// </summary>
    [Serializable]
    public class SaveItem
    {
        public int SaveID { get; private set; }
        public DateTime LastSaveTime { get; private set; }

        public SaveItem(int saveID, DateTime lastSaveTime)
        {
            SaveID = saveID;
            LastSaveTime = lastSaveTime;
        }

        public void UpdateTime(DateTime lastSaveTime)
        {
            LastSaveTime = lastSaveTime;
        }
    }

    /// <summary>
    /// 存档管理器
    /// </summary>
    public static class SaveManager
    {
        /// <summary>
        /// 存档管理器的设置数据
        /// </summary>
        [Serializable]
        class SaveManagerData
        {
            // 当前的存档ID
            public int CurrID = 0;

            // 所有存档的列表
            public List<SaveItem> SaveItemList = new();
        }

        static SaveManagerData s_SaveManagerData;

        // 存档的保存
        const string SAVE_DIR_NAME = "saveData";

        // 设置的保存：1.全局数据的保存（分辨率、按键设置） 2.存档的设置保存。
        // 常规情况下，存档管理器自行维护
        const string SETTING_DIR_NAME = "setting";

        // 存档文件夹路径
        static readonly string saveDirPath;
        static readonly string settingDirPath;

        // 存档中对象的缓存字典 
        // <存档ID,<文件名称，实际的对象>>
        static Dictionary<int, Dictionary<string, object>> s_CacheDict = new();

        // 初始化的事情
        static SaveManager()
        {
            saveDirPath = Application.persistentDataPath + "/" + SAVE_DIR_NAME;
            settingDirPath = Application.persistentDataPath + "/" + SETTING_DIR_NAME;

            // 确保路径的存在
            if (Directory.Exists(saveDirPath) == false)
            {
                Directory.CreateDirectory(saveDirPath);
            }
            if (Directory.Exists(settingDirPath) == false)
            {
                Directory.CreateDirectory(settingDirPath);
            }

            // 初始化SaveManagerData
            InitSaveManagerData();
        }

        public static void Clear()
        {
            foreach (var saveItem in s_SaveManagerData.SaveItemList)
            {
                Directory.Delete(saveDirPath + "/" + saveItem.SaveID, true);
            }
            s_SaveManagerData = new SaveManagerData();
            s_CacheDict.Clear();
            UpdateSaveManagerData();
        }

        #region 存档设置

        /// <summary>
        /// 获取存档管理器数据
        /// </summary>
        /// <returns></returns>
        static void InitSaveManagerData()
        {
            s_SaveManagerData = LoadFile<SaveManagerData>(saveDirPath + "/SaveMangerData");
            if (s_SaveManagerData == null)
            {
                s_SaveManagerData = new SaveManagerData();
                UpdateSaveManagerData();
            }
        }

        /// <summary>
        /// 更新存档管理器数据
        /// </summary>
        public static void UpdateSaveManagerData()
        {
            SaveFile(s_SaveManagerData, saveDirPath + "/SaveMangerData");
        }

        /// <summary>
        /// 获取所有存档
        /// 最新的在最后面
        /// </summary>
        /// <returns></returns>
        public static List<SaveItem> GetAllSaveItem()
        {
            return s_SaveManagerData.SaveItemList;
        }

        /// <summary>
        /// 获取所有存档
        /// 创建最新的在最前面
        /// </summary>
        /// <returns></returns>
        public static List<SaveItem> GetAllSaveItemByCreatTime()
        {
            List<SaveItem> saveItems = new List<SaveItem>(s_SaveManagerData.SaveItemList.Count);

            for (int i = 0; i < s_SaveManagerData.SaveItemList.Count; i++)
            {
                saveItems.Add(s_SaveManagerData.SaveItemList[^(i + 1)]);
            }
            return saveItems;
        }

        /// <summary>
        /// 获取所有存档
        /// 最新更新的在最上面
        /// </summary>
        /// <returns></returns>
        public static List<SaveItem> GetAllSaveItemByUpdateTime()
        {
            List<SaveItem> saveItems = new List<SaveItem>(s_SaveManagerData.SaveItemList.Count);
            foreach (var saveItem in s_SaveManagerData.SaveItemList)
            {
                saveItems.Add(saveItem);
            }
            OrderByUpdateTimeComparer orderBy = new OrderByUpdateTimeComparer();
            saveItems.Sort(orderBy);
            return saveItems;
        }

        class OrderByUpdateTimeComparer : IComparer<SaveItem>
        {
            public int Compare(SaveItem x, SaveItem y)
            {
                if (x.LastSaveTime > y.LastSaveTime)
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }
        }

        /// <summary>
        /// 获取所有存档
        /// 万能解决方案
        /// </summary>
        public static List<SaveItem> GetAllSaveItem<T>(Func<SaveItem, T> orderFunc, bool isDescending = false)
        {
            if (isDescending)
            {
                return s_SaveManagerData.SaveItemList.OrderByDescending(orderFunc).ToList();
            }
            else
            {
                return s_SaveManagerData.SaveItemList.OrderBy(orderFunc).ToList();
            }
        }

        #endregion

        #region 关于存档

        /// <summary>
        /// 获取SaveItem
        /// </summary>
        public static SaveItem GetSaveItem(int id)
        {
            foreach (var saveItem in s_SaveManagerData.SaveItemList)
            {
                if (saveItem.SaveID == id)
                {
                    return saveItem;
                }
            }
            return null;
        }

        /// <summary>
        /// 添加一个存档
        /// </summary>
        /// <returns></returns>
        public static SaveItem CreateSaveItem()
        {
            SaveItem saveItem = new SaveItem(s_SaveManagerData.CurrID, DateTime.Now);
            s_SaveManagerData.SaveItemList.Add(saveItem);
            s_SaveManagerData.CurrID += 1;

            // 更新SaveManagerData 写入磁盘
            UpdateSaveManagerData();
            return saveItem;
        }

        /// <summary>
        /// 删除存档
        /// </summary>
        /// <param name="saveID">存档的ID</param>
        public static void DeleteSaveItem(int saveID)
        {
            string itemDir = GetSavePath(saveID, false);

            // 如果路径存在 且 有效
            if (itemDir != null)
            {
                // 把这个存档下的文件递归删除
                Directory.Delete(itemDir, true);
            }
            s_SaveManagerData.SaveItemList.Remove(GetSaveItem(saveID));

            // 移除缓存
            RemoveCache(saveID);

            // 更新SaveManagerData 写入磁盘
            UpdateSaveManagerData();
        }

        /// <summary>
        /// 删除存档
        /// </summary>
        public static void DeleteSaveItem(SaveItem saveItem)
        {
            string itemDir = GetSavePath(saveItem.SaveID, false);

            // 如果路径存在 且 有效
            if (itemDir != null)
            {
                // 把这个存档下的文件递归删除
                Directory.Delete(itemDir, true);
            }
            s_SaveManagerData.SaveItemList.Remove(saveItem);

            // 移除缓存
            RemoveCache(saveItem.SaveID);

            // 更新SaveManagerData 写入磁盘
            UpdateSaveManagerData();
        }

        #endregion

        #region 关于缓存

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="saveID">存档ID</param>
        /// <param name="fileName">文件名称</param>
        /// <param name="saveObject">要缓存的对象</param>
        static void SetCache(int saveID, string fileName, object saveObject)
        {
            // 缓存字典中是否有这个SaveID
            if (s_CacheDict.ContainsKey(saveID))
            {
                // 这个存档中有没有这个文件
                if (s_CacheDict[saveID].ContainsKey(fileName))
                {
                    s_CacheDict[saveID][fileName] = saveObject;
                }
                else
                {
                    s_CacheDict[saveID].Add(fileName, saveObject);
                }
            }
            else
            {
                s_CacheDict.Add(saveID, new Dictionary<string, object>() { { fileName, saveObject } });
            }
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="saveID">存档ID</param>
        /// <param name="fileName"></param>
        static T GetCache<T>(int saveID, string fileName) where T : class
        {
            // 缓存字典中是否有这个SaveID
            if (s_CacheDict.ContainsKey(saveID))
            {
                // 这个存档中有没有这个文件
                if (s_CacheDict[saveID].ContainsKey(fileName))
                {
                    return s_CacheDict[saveID][fileName] as T;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 移除缓存
        /// </summary>
        static void RemoveCache(int saveID)
        {
            s_CacheDict.Remove(saveID);
        }

        #endregion

        #region 关于对象

        /// <summary>
        /// 保存对象至某个存档中
        /// </summary>
        /// <param name="saveObject">要保存的对象</param>
        /// <param name="saveFileName">保存的文件名称</param>
        /// <param name="saveID">存档的ID</param>
        public static void SaveObject(object saveObject, string saveFileName, int saveID = 0)
        {
            // 存档所在的文件夹路径
            string dirPath = GetSavePath(saveID);

            // 具体的对象要保存的路径
            string savePath = dirPath + "/" + saveFileName;

            // 具体的保存
            SaveFile(saveObject, savePath);

            // 更新存档时间
            GetSaveItem(saveID).UpdateTime(DateTime.Now);

            // 更新SaveManagerData 写入磁盘
            UpdateSaveManagerData();

            // 更新缓存
            SetCache(saveID, saveFileName, saveObject);
        }

        /// <summary>
        /// 保存对象至某个存档中
        /// </summary>
        /// <param name="saveObject">要保存的对象</param>
        /// <param name="saveFileName">保存的文件名称</param>
        /// <param name="saveItem"></param>
        public static void SaveObject(object saveObject, string saveFileName, SaveItem saveItem)
        {
            // 存档所在的文件夹路径
            string dirPath = GetSavePath(saveItem.SaveID);

            // 具体的对象要保存的路径
            string savePath = dirPath + "/" + saveFileName;

            // 具体的保存
            SaveFile(saveObject, savePath);

            // 更新存档时间
            saveItem.UpdateTime(DateTime.Now);

            // 更新SaveManagerData 写入磁盘
            UpdateSaveManagerData();

            // 更新缓存
            SetCache(saveItem.SaveID, saveFileName, saveObject);
        }

        /// <summary>
        /// 保存对象至某个存档中
        /// </summary>
        /// <param name="saveObject">要保存的对象</param>
        /// <param name="saveID">存档的ID</param>
        public static void SaveObject(object saveObject, int saveID = 0)
        {
            SaveObject(saveObject, saveObject.GetType().Name, saveID);
        }

        /// <summary>
        /// 保存对象至某个存档中
        /// </summary>
        /// <param name="saveObject">要保存的对象</param>
        /// <param name="saveItem"></param>
        public static void SaveObject(object saveObject, SaveItem saveItem)
        {
            SaveObject(saveObject, saveObject.GetType().Name, saveItem);
        }

        /// <summary>
        /// 从某个具体的存档中加载某个对象
        /// </summary>
        /// <typeparam name="T">要返回的实际类型</typeparam>
        /// <param name="saveFileName">文件名称</param>
        /// <param name="saveID">存档ID</param>
        public static T LoadObject<T>(string saveFileName, int saveID = 0) where T : class
        {
            T obj = GetCache<T>(saveID, saveFileName);
            if (obj == null)
            {
                // 存档所在的文件夹路径
                string dirPath = GetSavePath(saveID);
                if (dirPath == null) return null;

                // 具体的对象要保存的路径
                string savePath = dirPath + "/" + saveFileName;
                obj = LoadFile<T>(savePath);
                SetCache(saveID, saveFileName, obj);
            }
            return obj;
        }

        /// <summary>
        /// 从某个具体的存档中加载某个对象
        /// </summary>
        /// <typeparam name="T">要返回的实际类型</typeparam>
        /// <param name="saveFileName">文件名称</param>
        /// <param name="saveItem"></param>
        public static T LoadObject<T>(string saveFileName, SaveItem saveItem) where T : class
        {
            return LoadObject<T>(saveFileName, saveItem.SaveID);
        }

        /// <summary>
        /// 从某个具体的存档中加载某个对象
        /// </summary>
        /// <typeparam name="T">要返回的实际类型</typeparam>
        public static T LoadObject<T>(int saveID = 0) where T : class
        {
            return LoadObject<T>(typeof(T).Name, saveID);
        }

        /// <summary>
        /// 从某个具体的存档中加载某个对象
        /// </summary>
        /// <typeparam name="T">要返回的实际类型</typeparam>
        /// <returns></returns>
        public static T LoadObject<T>(SaveItem saveItem) where T : class
        {
            return LoadObject<T>(typeof(T).Name, saveItem.SaveID);
        }

        #endregion

        #region 全局数据

        /// <summary>
        /// 加载设置，全局生效，不关乎任何一个存档
        /// </summary>
        public static T LoadSetting<T>(string fileName) where T : class
        {
            return LoadFile<T>(settingDirPath + "/" + fileName);
        }

        /// <summary>
        /// 加载设置，全局生效，不关乎任何一个存档
        /// </summary>
        public static T LoadSetting<T>() where T : class
        {
            return LoadSetting<T>(typeof(T).Name);
        }

        /// <summary>
        /// 保存设置，全局生效，不关乎任何一个存档
        /// </summary>
        public static void SaveSetting(object saveObject, string fileName)
        {
            SaveFile(saveObject, settingDirPath + "/" + fileName);
        }

        /// <summary>
        /// 保存设置，全局生效，不关乎任何一个存档
        /// </summary>
        public static void SaveSetting(object saveObject)
        {
            SaveSetting(saveObject, saveObject.GetType().Name);
        }

        #endregion

        #region 工具函数

        static BinaryFormatter s_BinaryFormatter = new();

        /// <summary>
        /// 获取某个存档的路径
        /// </summary>
        /// <param name="saveID">存档ID</param>
        /// <param name="createDir">如果不存在这个路径，是否需要创建</param>
        /// <returns></returns>
        static string GetSavePath(int saveID, bool createDir = true)
        {
            // 验证是否有某个存档
            if (GetSaveItem(saveID) == null) throw new Exception("JK:saveID 存档不存在！");

            string saveDir = saveDirPath + "/" + saveID;

            // 确定文件夹是否存在
            if (Directory.Exists(saveDir) == false)
            {
                if (createDir)
                {
                    Directory.CreateDirectory(saveDir);
                }
                else
                {
                    return null;
                }
            }

            return saveDir;
        }

        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="saveObject">保存的对象</param>
        /// <param name="path">保存的路径</param>
        static void SaveFile(object saveObject, string path)
        {
            FileStream f = new FileStream(path, FileMode.OpenOrCreate);

            // 二进制的方式把对象写进文件
            s_BinaryFormatter.Serialize(f, saveObject);
            f.Dispose();
        }

        /// <summary>
        /// 加载文件
        /// </summary>
        /// <typeparam name="T">加载后要转为的类型</typeparam>
        /// <param name="path">加载路径</param>
        static T LoadFile<T>(string path) where T : class
        {
            if (!File.Exists(path))
            {
                return null;
            }
            FileStream file = new FileStream(path, FileMode.Open);

            // 将内容解码成对象
            T obj = (T)s_BinaryFormatter.Deserialize(file);
            file.Dispose();
            return obj;
        }

        #endregion
    }
}