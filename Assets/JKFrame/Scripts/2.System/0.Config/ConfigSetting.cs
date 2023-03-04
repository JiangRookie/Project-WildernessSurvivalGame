using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace JKFrame
{
    /// <summary>
    /// 所有游戏中（非框架）配置，游戏运行时只有一个<br/>
    /// 包含所有的配置文件
    /// </summary>
    [CreateAssetMenu(fileName = "ConfigSetting", menuName = "JKFrame/ConfigSetting")]
    public class ConfigSetting : ConfigBase
    {
        /// <summary>
        /// 所有配置的容器
        /// (配置类型的名称，(id，具体配置))
        /// </summary>
        [DictionaryDrawerSettings(KeyLabel = "类型", ValueLabel = "列表")]
        public Dictionary<string, Dictionary<int, ConfigBase>> ConfigDic;

        /// <summary>
        /// 获取<paramref name="configTypeName"/>下的所有配置
        /// </summary>
        /// <param name="configTypeName">配置类型名称</param>
        /// <returns></returns>
        public Dictionary<int, ConfigBase> GetConfigs(string configTypeName)
        {
            if (!ConfigDic.TryGetValue(configTypeName, out Dictionary<int, ConfigBase> configBaseDict))
            {
                throw new Exception("JK:配置设置中不包含这个Key:" + configTypeName);
            }
            return configBaseDict;
        }

        /// <summary>
        /// 获取配置
        /// </summary>
        /// <typeparam name="T">具体的配置类型</typeparam>
        /// <param name="configTypeName">配置类型名称</param>
        /// <param name="id">id</param>
        public T GetConfig<T>(string configTypeName, int id) where T : ConfigBase
        {
            // 检查类型
            if (!ConfigDic.TryGetValue(configTypeName, out Dictionary<int, ConfigBase> configBaseDict))
            {
                throw new Exception("JK:配置设置中不包含这个Key:" + configTypeName);
            }

            // 检查ID
            if (!configBaseDict.TryGetValue(id, out ConfigBase configBase))
            {
                throw new Exception($"JK:配置设置中{configTypeName}不包含这个ID:{id}");
            }

            // 说明一切正常
            return configBase as T;
        }
    }
}