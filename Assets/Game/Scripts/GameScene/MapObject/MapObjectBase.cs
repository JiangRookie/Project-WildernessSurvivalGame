using Project_WildernessSurvivalGame;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

public enum MapObjectType
{
    Tree, Bush, Stone, Material, Consumable, Weapon, Building
}

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

    public virtual void Init(MapChunkController chunk, ulong objectId, bool isFromBuild)
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

    #region Editor

#if UNITY_EDITOR

    [Button]
    public void AddNavMeshObstacle()
    {
        NavMeshObstacle navMeshObstacle = transform.gameObject.AddComponent<NavMeshObstacle>();

        BoxCollider boxCollider = GetComponent<BoxCollider>();
        CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();

        if (boxCollider != null)
        {
            navMeshObstacle.shape = NavMeshObstacleShape.Box;
            navMeshObstacle.center = boxCollider.center;
            navMeshObstacle.size = boxCollider.size;

            // navMeshObstacle.carving = true;
        }
        else if (capsuleCollider != null)
        {
            navMeshObstacle.shape = NavMeshObstacleShape.Capsule;
            navMeshObstacle.center = capsuleCollider.center;
            navMeshObstacle.height = capsuleCollider.height;
            navMeshObstacle.radius = capsuleCollider.radius;

            // navMeshObstacle.carving = true;
        }
        navMeshObstacle.carving = true;
    }

#endif

    #endregion
}