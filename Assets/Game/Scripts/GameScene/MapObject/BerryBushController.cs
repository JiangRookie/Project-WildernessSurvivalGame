using System.Collections.Generic;
using Project_WildernessSurvivalGame;
using UnityEngine;

public class BerryBushController : BushController, IBuilding
{
    [SerializeField] MeshRenderer m_MeshRenderer;
    [SerializeField] Material[] m_Materials; // 0 是有果子，1是没有果子
    [SerializeField] int m_BerryGrowthDays = 2;
    BerryBushTypeData m_TypeData;

    #region IBuilding

    [SerializeField] Collider m_Collider;
    List<Material> m_MaterialList;
    public GameObject GameObject => gameObject;
    public Collider Collider => m_Collider;

    public List<Material> MaterialList
    {
        get => m_MaterialList;
        set => m_MaterialList = value;
    }

    #endregion

    public override void Init(MapChunkController chunk, ulong objectId)
    {
        base.Init(chunk, objectId);

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
        CheckAndSetState();
    }

    public override int OnPickUp()
    {
        // 修改外表
        m_MeshRenderer.sharedMaterial = m_Materials[1];
        canPickUp = false;
        m_TypeData.LastPickUpDayNum = TimeManager.Instance.CurrentDayNum;
        return canPickUpItemConfigID;
    }

    void CheckAndSetState()
    {
        // 有没有采摘过
        if (m_TypeData.LastPickUpDayNum == -1)
        {
            m_MeshRenderer.sharedMaterial = m_Materials[0];
            canPickUp = true;
        }
        else
        {
            // 根据时间决定状态
            if (TimeManager.Instance.CurrentDayNum - m_TypeData.LastPickUpDayNum >= m_BerryGrowthDays)
            {
                m_MeshRenderer.sharedMaterial = m_Materials[0];
                canPickUp = true;
            }
            else
            {
                m_MeshRenderer.sharedMaterial = m_Materials[1];
                canPickUp = false;
            }
        }
    }
}