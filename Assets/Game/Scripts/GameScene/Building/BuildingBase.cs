using System.Collections.Generic;
using Project_WildernessSurvivalGame;
using UnityEngine;

public abstract class BuildingBase : MapObjectBase, IBuilding
{
    [SerializeField] protected Collider m_Collider;
    [SerializeField] List<int> m_UnlockedScienceOnBuild;

    List<Material> m_MaterialList = null;

    #region PreviewMode

    public GameObject GameObject => gameObject;
    public Collider Collider => m_Collider;

    public List<Material> MaterialList
    {
        get => m_MaterialList;
        set => m_MaterialList = value;
    }

    public virtual void OnPreview() { }

    #endregion

    public virtual void OnSelect() { }

    public override void Init(MapChunkController chunk, ulong objectId, bool isFromBuild)
    {
        base.Init(chunk, objectId, isFromBuild);
        if (isFromBuild)
        {
            for (var i = 0; i < m_UnlockedScienceOnBuild.Count; i++)
            {
                // 同步科技数据
                ScienceManager.Instance.AddScience(m_UnlockedScienceOnBuild[i]);
            }
        }
    }

    /// <summary>
    /// 当前物品格子结束拖拽时选中
    /// </summary>
    public virtual bool OnSlotEndDragSelect(int itemID)
    {
        return false;
    }
}