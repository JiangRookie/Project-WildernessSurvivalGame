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
}