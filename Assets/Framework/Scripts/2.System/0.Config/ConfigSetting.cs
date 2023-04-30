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
        /// 通过指定配置类型名称获取该类型下的所有的具体配置文件字典
        /// </summary>
        /// <param name="configTypeName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public Dictionary<int, ConfigBase> GetConfigs(string configTypeName)
        {
            if (!ConfigDic.TryGetValue(configTypeName, out Dictionary<int, ConfigBase> configBaseDict))
            {
                throw new Exception("配置设置中不包含这个Key:" + configTypeName);
            }
            return configBaseDict;
        }

        /// <summary>
        /// 通过指定配置类型名称和ID获取具体的配置文件。其中T为具体的配置文件类型
        /// 该方法内部判断传入的类型和配置文件所属的类型是否相同，如果不同则会抛出异常。
        /// </summary>
        public T GetConfig<T>(string configTypeName, int id) where T : ConfigBase
        {
            // 检查类型
            if (!ConfigDic.TryGetValue(configTypeName, out Dictionary<int, ConfigBase> configBaseDict))
            {
                throw new Exception("配置设置中不包含这个Key:" + configTypeName);
            }

            // 检查ID
            if (!configBaseDict.TryGetValue(id, out ConfigBase configBase))
            {
                throw new Exception($"配置设置中{configTypeName}不包含这个ID:{id}");
            }

            // 说明一切正常
            return configBase as T;
        }
    }
}