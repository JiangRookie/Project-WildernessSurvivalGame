using System.Collections.Generic;
using JKFrame;
using UnityEngine;

public class UI_BuildWindow_SecondaryMenu : MonoBehaviour
{
    [SerializeField] Transform m_ItemParent;
    [SerializeField] GameObject m_SecondaryMenuItemPrefab;
    [SerializeField] UI_BuildWindow_BuildPanel m_BuildPanel;
    Dictionary<BuildType, List<BuildConfig>> m_BuildConfigDict;
    List<UI_BuildWindow_SecondaryMenuItem> m_CurrEffectAllSecondaryMenuItemList;
    List<BuildConfig> m_MeetTheConditionConfigList;
    List<BuildConfig> m_NotMeetTheConditionConfigList;

    UI_BuildWindow_SecondaryMenuItem m_CurrSelectedSecondaryMenuItem;
    BuildType m_CurrBuildType;

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

        m_MeetTheConditionConfigList = new List<BuildConfig>();
        m_NotMeetTheConditionConfigList = new List<BuildConfig>();

        m_BuildPanel.Init(this);
        Close();
    }

    public void RefreshView()
    {
        Show(m_CurrBuildType);
        foreach (var secondaryMenuItem in m_CurrEffectAllSecondaryMenuItemList)
        {
            if (secondaryMenuItem.BuildConfig == m_CurrSelectedSecondaryMenuItem.BuildConfig)
            {
                secondaryMenuItem.Select();
            }
        }
    }

    public void Show(BuildType buildType)
    {
        m_CurrBuildType = buildType;

        // 旧列表中的所有选项先放进对象池
        foreach (var secondaryMenuItem in m_CurrEffectAllSecondaryMenuItemList)
        {
            secondaryMenuItem.JKGameObjectPushPool();
        }
        m_CurrEffectAllSecondaryMenuItemList.Clear();

        // 当前类型的配置列表
        List<BuildConfig> buildConfigList = m_BuildConfigDict[buildType];

        m_MeetTheConditionConfigList.Clear();
        m_NotMeetTheConditionConfigList.Clear();

        foreach (BuildConfig buildConfig in buildConfigList)
        {
            // 科技判断
            bool scienceUnlocked = true;
            if (buildConfig.PreconditionScienceIDList != null)
            {
                foreach (int id in buildConfig.PreconditionScienceIDList)
                {
                    if (ScienceManager.Instance.CheckUnlock(id) == false)
                    {
                        scienceUnlocked = false;
                    }
                }
            }

            if (scienceUnlocked)
            {
                bool isMeet = buildConfig.CheckBuildConfigCondition();
                if (isMeet)
                    m_MeetTheConditionConfigList.Add(buildConfig);
                else
                    m_NotMeetTheConditionConfigList.Add(buildConfig);
            }
        }

        // 对配置进行分类，满足条件/不满足条件
        foreach (var buildConfig in m_MeetTheConditionConfigList) AddSecondMenuItem(buildConfig, true);
        foreach (var buildConfig in m_NotMeetTheConditionConfigList) AddSecondMenuItem(buildConfig, false);
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
        m_BuildPanel.Show(newSecondaryMenuItem.BuildConfig);
    }

    public void Close()
    {
        m_BuildPanel.Close();
        gameObject.SetActive(false);
    }
}