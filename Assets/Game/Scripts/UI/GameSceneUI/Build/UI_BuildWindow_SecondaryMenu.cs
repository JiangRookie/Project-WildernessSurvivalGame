using System.Collections.Generic;
using JKFrame;
using UnityEngine;

public class UI_BuildWindow_SecondaryMenu : MonoBehaviour
{
    [SerializeField] Transform m_ItemParent;
    [SerializeField] GameObject m_SecondaryMenuItemPrefab;
    Dictionary<BuildType, List<BuildConfig>> m_BuildConfigDict;
    List<UI_BuildWindow_SecondaryMenuItem> m_CurrEffectAllSecondaryMenuItemList;
    UI_BuildWindow_SecondaryMenuItem m_CurrSelectedSecondaryMenuItem;

    public void Init()
    {
        // 构建自己的配置文件结构
        m_BuildConfigDict = new Dictionary<BuildType, List<BuildConfig>>(3);
        m_BuildConfigDict.Add(BuildType.Weapon, new List<BuildConfig>());
        m_BuildConfigDict.Add(BuildType.Building, new List<BuildConfig>());
        m_BuildConfigDict.Add(BuildType.Crop, new List<BuildConfig>());
        Dictionary<int, ConfigBase> buildConfigs = ConfigManager.Instance.GetConfigs(ConfigName.Build);
        foreach (ConfigBase config in buildConfigs.Values)
        {
            BuildConfig buildConfig = (BuildConfig)config;
            m_BuildConfigDict[buildConfig.BuildType].Add(buildConfig);
        }
        m_CurrEffectAllSecondaryMenuItemList = new List<UI_BuildWindow_SecondaryMenuItem>(10);
        Close();
    }

    public void Show(BuildType buildType)
    {
        // 旧列表中的所有选项先放进对象池
        foreach (var secondaryMenuItem in m_CurrEffectAllSecondaryMenuItemList)
        {
            secondaryMenuItem.JKGameObjectPushPool();
        }
        m_CurrEffectAllSecondaryMenuItemList.Clear();

        // 当前类型的配置列表
        List<BuildConfig> buildConfigList = m_BuildConfigDict[buildType];

        // 对配置进行分类，满足条件/不满足条件
        foreach (var buildConfig in buildConfigList)
        {
            AddSecondMenuItem(buildConfig, true);
        }
        gameObject.SetActive(true);
    }

    void AddSecondMenuItem(BuildConfig buildConfig, bool isMeetCondition)
    {
        // 从对象池中获取菜单选项
        UI_BuildWindow_SecondaryMenuItem secondaryMenuItem
            = PoolManager.Instance.GetGameObject<UI_BuildWindow_SecondaryMenuItem>(m_SecondaryMenuItemPrefab, m_ItemParent);
        m_CurrEffectAllSecondaryMenuItemList.Add(secondaryMenuItem);
        secondaryMenuItem.Init(buildConfig, this, isMeetCondition);
    }

    public void SelectSecondaryMenuItem(UI_BuildWindow_SecondaryMenuItem newSecondaryMenuItem)
    {
        if (m_CurrSelectedSecondaryMenuItem != null)
        {
            m_CurrSelectedSecondaryMenuItem.UnSelect();
        }
        m_CurrSelectedSecondaryMenuItem = newSecondaryMenuItem;
        m_CurrSelectedSecondaryMenuItem.Select();
    }

    void Close()
    {
        gameObject.SetActive(false);
    }
}