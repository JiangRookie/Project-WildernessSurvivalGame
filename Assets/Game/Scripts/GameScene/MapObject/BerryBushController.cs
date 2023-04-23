using System.Collections.Generic;
using JKFrame;
using UnityEngine;

public class BerryBushController : BushController, IBuilding
{
    [SerializeField] Collider m_Collider;
    [SerializeField] MeshRenderer m_MeshRenderer;
    [SerializeField] Material[] m_Materials; // 0 是有果子，1是没有果子
    [SerializeField] int m_BerryGrowthDays = 2;
    List<Material> m_MaterialList;
    BerryBushTypeData m_TypeData;
    public GameObject GameObject => gameObject;
    public Collider Collider => m_Collider;

    public List<Material> MaterialList
    {
        get => m_MaterialList;
        set => m_MaterialList = value;
    }

    public void OnPreview() { }

    public override void Init(MapChunkController chunk, ulong objectId, bool isFromBuild)
    {
        base.Init(chunk, objectId, isFromBuild);

        // 可能当前没有这个存档
        if (ArchiveManager.Instance.TryGetMapObjectTypeData(objectId, out IMapObjectTypeData typeData))
        {
            m_TypeData = typeData as BerryBushTypeData;
        }
        else
        {
            m_TypeData = new BerryBushTypeData();
            ArchiveManager.Instance.AddMapObjectTypeData(objectId, m_TypeData);
        }
        if (isFromBuild)
        {
            // 来自建筑物建造的情况下，直接视为刚刚采摘（这件事情也需要持久化）
            if (m_TypeData != null)
            {
                m_TypeData.LastPickUpDayNum = TimeManager.Instance.CurrDayNum;
            }
        }
        CheckAndSetState();
        EventManager.AddEventListener(EventName.OnMorning, OnMorning);
    }

    void OnMorning()
    {
        // 如果已经成熟，无需检测
        if (canPickUp == false) CheckAndSetState();
    }

    public override int OnPickUp()
    {
        SetUnpickableState();
        m_TypeData.LastPickUpDayNum = TimeManager.Instance.CurrDayNum;
        return canPickUpItemConfigID;
    }

    void CheckAndSetState()
    {
        // 有没有采摘过
        if (m_TypeData.LastPickUpDayNum == -1)
        {
            SetPickkableState();
        }
        else
        {
            // 根据时间决定状态
            if (TimeManager.Instance.CurrDayNum - m_TypeData.LastPickUpDayNum >= m_BerryGrowthDays)
            {
                SetPickkableState();
            }
            else
            {
                SetUnpickableState();
            }
        }
    }

    void SetPickkableState()
    {
        m_MeshRenderer.sharedMaterial = m_Materials[0];
        canPickUp = true;
    }

    void SetUnpickableState()
    {
        m_MeshRenderer.sharedMaterial = m_Materials[1];
        canPickUp = false;
    }
}