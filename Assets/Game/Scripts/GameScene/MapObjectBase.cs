using JKFrame;
using Project_WildernessSurvivalGame;
using UnityEngine;

public enum MapObjectType
{
    Tree
  , Stone
  , SmallStone
}

/// <summary>
/// 地图对象基类
/// </summary>
public abstract class MapObjectBase : MonoBehaviour
{
    [SerializeField] MapObjectType m_MapObjectType;
    public MapObjectType MapObjectType => m_MapObjectType;

    protected MapChunkController MapChunkController;
    protected ulong ID;

    public virtual void Init(MapChunkController mapChunkController, ulong id)
    {
        MapChunkController = mapChunkController;
        ID = id;
    }

    public virtual void RemoveOnMap()
    {
        MapChunkController.RemoveMapObject(ID);

        // 把自己扔回对象池
        this.JKGameObjectPushPool();
    }
}