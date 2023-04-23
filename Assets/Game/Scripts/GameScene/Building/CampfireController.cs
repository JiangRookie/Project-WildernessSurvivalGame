using JKFrame;
using UnityEngine;

/// <summary>
/// 篝火控制器
/// </summary>
public class CampfireController : BuildingBase
{
    [SerializeField] Light m_Light;
    [SerializeField] GameObject m_Fire;
    [SerializeField] AudioSource m_AudioSource;
    CampfireConfig m_CampfireConfig;
    CampfireData m_CampfireData;
    bool m_IsGeneratedOnGround;

    void Update()
    {
        if (GameSceneManager.Instance.IsInitialized == false) return;
        if (m_IsGeneratedOnGround) UpdateFuel();
    }

    // 当处于预览模式的时候 ->
    public override void OnPreview()
    {
        m_IsGeneratedOnGround = false;
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

        if (m_CampfireData != null)
        {
            SetLight(m_CampfireData.CurrentFuel);
        }
        m_IsGeneratedOnGround = true;
    }

    void UpdateFuel()
    {
        if (m_CampfireData.CurrentFuel == 0) return;

        m_CampfireData.CurrentFuel = Mathf.Clamp
        (
            m_CampfireData.CurrentFuel - Time.deltaTime * m_CampfireConfig.BurningSpeed * TimeManager.Instance.TimeScale
          , 0
          , m_CampfireConfig.MaxFuelValue
        );

        SetLight(m_CampfireData.CurrentFuel);
    }

    void SetLight(float fuelValue)
    {
        m_Light.gameObject.SetActive(fuelValue != 0);
        m_Fire.gameObject.SetActive(fuelValue != 0);
        m_AudioSource.gameObject.SetActive(fuelValue != 0);
        if (fuelValue == 0) return;
        var ratio = fuelValue / m_CampfireConfig.MaxFuelValue;
        m_Light.intensity = Mathf.Lerp(0, m_CampfireConfig.MaxLightIntensity, ratio);
        m_Light.range = Mathf.Lerp(0, m_CampfireConfig.MaxLightRange, ratio);
        m_AudioSource.volume = ratio;
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
        if (m_CampfireConfig.TryGetBakedValueByItemID(itemID, out int bakedItemID) == false) return false;

        // 当前有没有燃料
        if (m_CampfireData.CurrentFuel <= 0)
        {
            UIManager.Instance.AddTips("需要点燃篝火！");
            return false;
        }

        // 给玩家添加一个物品
        InventoryManager.Instance.AddItemAndPlayAudio2MainInventory(bakedItemID);
        return true;
    }
}