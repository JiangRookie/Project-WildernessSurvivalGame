using Project_WildernessSurvivalGame;
using UnityEngine;

public enum MapObjectType { Tree, Bush, Stone, SmallStone, Mushroom, Wood, Twig, Weapon, Berry }

/// <summary>
/// 地图对象基类
/// </summary>
public abstract class MapObjectBase : MonoBehaviour
{
    [SerializeField] MapObjectType m_MapObjectType;
    [SerializeField] protected float touchDistance; // 交互距离
    [SerializeField] protected bool canPickUp;
    [SerializeField] protected int canPickUpItemConfigID = -1;
    protected MapChunkController mapChunkController;
    protected ulong id;

    public MapObjectType MapObjectType => m_MapObjectType;
    public float TouchDistance => touchDistance;
    public bool CanPickUp => canPickUp;
    public int CanPickUpItemConfigID => canPickUpItemConfigID;

    public virtual void Init(MapChunkController chunk, ulong objectId)
    {
        mapChunkController = chunk;
        id = objectId;
    }

    public virtual void RemoveOnMap()
    {
        mapChunkController.RemoveMapObject(id);
    }

    public virtual int OnPickUp()
    {
        RemoveOnMap(); // 从地图上消失
        return canPickUpItemConfigID;
    }
}