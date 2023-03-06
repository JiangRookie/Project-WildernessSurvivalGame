using System.Collections.Generic;
using UnityEngine;

namespace JKFrame
{
    public class ConfigManager : ManagerBase<ConfigManager>
    {
        [SerializeField] ConfigSetting ConfigSetting;

        /// <summary>
        /// 获取配置
        /// </summary>
        /// <typeparam name="T">具体的配置类型</typeparam>
        /// <param name="configTypeName">配置类型名称</param>
        /// <param name="id">id</param>
        public T GetConfig<T>(string configTypeName, int id = 0) where T : ConfigBase
        {
            return ConfigSetting.GetConfig<T>(configTypeName, id);
        }

        /// <summary>
        /// 获取<paramref name="configTypeName"/>下的所有配置
        /// </summary>
        /// <param name="configTypeName">配置类型名称</param>
        /// <returns></returns>
        public Dictionary<int, ConfigBase> GetConfigs(string configTypeName)
        {
            return ConfigSetting.GetConfigs(configTypeName);
        }
    }
}