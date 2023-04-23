using System.Collections.Generic;
using UnityEngine;

public abstract class BuildingBase : MapObjectBase, IBuilding
{
    [SerializeField] protected Collider m_Collider;
    [SerializeField] List<int> m_UnlockedScienceIDOnBuild;
    List<Material> m_MaterialList = null;
    public Collider Collider => m_Collider;
    public GameObject GameObject => gameObject;

    public List<Material> MaterialList
    {
        get => m_MaterialList;
        set => m_MaterialList = value;
    }

    public virtual void OnPreview() { }

    public virtual void OnSelect() { }

    public override void Init(MapChunkController chunk, ulong objectId, bool isFromBuild)
    {
        base.Init(chunk, objectId, isFromBuild);
        if (isFromBuild == false) return;
        foreach (int id in m_UnlockedScienceIDOnBuild)
        {
            // 同步科技数据
            ScienceManager.Instance.AddScience(id);
        }
    }

    /// <summary>
    /// 当前物品格子结束拖拽时选中
    /// </summary>
    public virtual bool OnSlotEndDragSelect(int itemID) => false;
}