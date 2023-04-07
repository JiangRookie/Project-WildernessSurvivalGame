using JKFrame;
using Project_WildernessSurvivalGame;
using UnityEngine;
using UnityEngine.UI;

public class UI_BuildWindow_BuildPanel : MonoBehaviour
{
    [SerializeField] UI_BuildWindow_BuildPanelItem[] m_BuildPanelItems;
    [SerializeField] Text m_DescriptionText;
    [SerializeField] Button m_BuildButton;
    BuildConfig m_BuildConfig;
    UI_BuildWindow_SecondaryMenu m_OwnerWindow;

    public void Init(UI_BuildWindow_SecondaryMenu ownerWindow)
    {
        m_OwnerWindow = ownerWindow;
        m_BuildButton.onClick.AddListener(BuildButtonClick);
        Close();
    }

    void BuildButtonClick()
    {
        if (m_BuildConfig.BuildType == BuildType.Weapon)
        {
            if (UI_InventoryWindow.Instance.AddItemAndPlayAudio(m_BuildConfig.TargetID))
            {
                // 根据建造配置减少材料
                UI_InventoryWindow.Instance.UpdateItemsForBuild(m_BuildConfig);

                // 刷新当前界面状态
                RefreshView();
            }
            else
            {
                UIManager.Instance.AddTips("背包已满，无法建造");
            }
        }
        else
        {
            // 进入建造模式
            EventManager.EventTrigger<BuildConfig>(EventName.BuildBuilding, m_BuildConfig);
            m_OwnerWindow.Close();
        }
    }

    public void Show(BuildConfig buildConfig)
    {
        m_BuildConfig = buildConfig;

        // 显示正确的合成需要的物品
        for (var i = 0; i < buildConfig.BuildConfigConditionList.Count; i++)
        {
            int id = buildConfig.BuildConfigConditionList[i].ItemID;
            int currCount = UI_InventoryWindow.Instance.GetItemCount(id);
            int needCount = buildConfig.BuildConfigConditionList[i].Count;
            m_BuildPanelItems[i].Show(id, currCount, needCount);
        }
        if (buildConfig.BuildType == BuildType.Weapon)
        {
            m_DescriptionText.text = ConfigManager.Instance.GetConfig<ItemConfig>(ConfigName.ITEM, buildConfig.TargetID).Description;
        }
        else
        {
            m_DescriptionText.text = ConfigManager.Instance.GetConfig<MapObjectConfig>(ConfigName.MapObject, buildConfig.TargetID).Description;
        }

        m_BuildButton.interactable = buildConfig.CheckBuildConfigCondition();
        gameObject.SetActive(true);
    }

    public void RefreshView()
    {
        Show(m_BuildConfig);
        m_OwnerWindow.RefreshView();
    }

    public void Close()
    {
        foreach (var buildPanelItem in m_BuildPanelItems)
        {
            buildPanelItem.Close();
        }
        gameObject.SetActive(false);
    }
}