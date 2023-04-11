using JKFrame;
using Project_WildernessSurvivalGame;
using UnityEngine;

/// <summary>
/// 篝火控制器
/// </summary>
public class CampfireController : BuildingBase
{
    [SerializeField] Light m_Light;
    [SerializeField] GameObject m_Fire;
    CampfireConfig m_CampfireConfig;
    CampfireData m_CampfireData;
    bool m_IsOnGround;

    void Update()
    {
        if (GameSceneManager.Instance.IsInitialized == false) return;
        if (m_IsOnGround)
        {
            UpdateFuel();
        }
    }

    public override void OnPreview()
    {
        m_IsOnGround = false;

        // 关闭粒子和火焰效果
        SetLight(0);
    }

    public override void Init(MapChunkController chunk, ulong objectId, bool isFromBuild)
    {
        base.Init(chunk, objectId, isFromBuild);
        m_CampfireConfig = ConfigManager.Instance.GetConfig<CampfireConfig>(ConfigName.Campfire);

        if (isFromBuild)
        {
            m_CampfireData = new CampfireData();
            m_CampfireData.CurrentFuel = m_CampfireConfig.DefaultFuelValue;
            ArchiveManager.Instance.AddMapObjectTypeData(objectId, m_CampfireData);
        }
        else
        {
            m_CampfireData = ArchiveManager.Instance.GetMapObjectTypeData(objectId) as CampfireData;
        }
        SetLight(m_CampfireData.CurrentFuel);
        m_IsOnGround = true;
    }

    void UpdateFuel()
    {
        if (m_CampfireData.CurrentFuel == 0) return;

        m_CampfireData.CurrentFuel = Mathf.Clamp(
            m_CampfireData.CurrentFuel - Time.deltaTime * m_CampfireConfig.BurningSpeed * TimeManager.Instance.TimeScale
          , 0
          , m_CampfireConfig.MaxFuelValue);

        SetLight(m_CampfireData.CurrentFuel);
    }

    /// <summary>
    /// 根据燃料设置燃料
    /// </summary>
    /// <param name="fuelValue"></param>
    void SetLight(float fuelValue)
    {
        m_Light.gameObject.SetActive(fuelValue != 0);
        m_Fire.gameObject.SetActive(fuelValue != 0);
        if (fuelValue != 0)
        {
            float value = fuelValue / m_CampfireConfig.MaxFuelValue;
            m_Light.intensity = Mathf.Lerp(0, m_CampfireConfig.MaxLightIntensity, value);
            m_Light.range = Mathf.Lerp(0, m_CampfireConfig.MaxLightRange, value);
        }
    }

    public override bool OnSlotEndDragSelect(int itemID)
    {
        // 木材、燃料等作为燃料物品
        if (m_CampfireConfig.TryGetFuelValueByItemID(itemID, out float fuelValue))
        {
            m_CampfireData.CurrentFuel = Mathf.Clamp(m_CampfireData.CurrentFuel + fuelValue, 0, m_CampfireConfig.MaxFuelValue);
            SetLight(m_CampfireData.CurrentFuel);
            return true;
        }

        // 烘焙检查
        if (m_CampfireConfig.TryGetBakedValueByItemID(itemID, out int bakedItemID))
        {
            // 当前有没有燃料
            if (m_CampfireData.CurrentFuel <= 0)
            {
                UIManager.Instance.AddTips("需要点燃篝火！");
                return false;
            }

            // 给玩家添加一个物品
            InventoryManager.Instance.AddItemAndPlayAudioToMainInventoryWindow(bakedItemID);
            return true;
        }

        // 烧烤食物
        return false;
    }
}