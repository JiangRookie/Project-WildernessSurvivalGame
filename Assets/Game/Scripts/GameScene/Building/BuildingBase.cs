using System.Collections.Generic;
using UnityEngine;

public class BuildingBase : MapObjectBase, IBuilding
{
    [SerializeField] protected Collider m_Collider;

    List<Material> m_MaterialList = null;

    #region PreviewMode

    public GameObject GameObject => gameObject;
    public Collider Collider => m_Collider;

    public List<Material> MaterialList
    {
        get => m_MaterialList;
        set => m_MaterialList = value;
    }

    #endregion

    public virtual void OnSelect() { }
}