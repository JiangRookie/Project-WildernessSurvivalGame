using System.Collections.Generic;
using JKFrame;
using UnityEngine;

/// <summary>
/// 地图块数据
/// </summary>
public class MapChunkData
{
    public List<MapChunkMapObjectModel> MapObjectList = new();
}

/// <summary>
/// 地图块中的各种地图对象 MapObjectModelInMapChunk
/// </summary>
public class MapChunkMapObjectModel
{
    public GameObject Prefab;
    public Vector3 Position;
}

public class MapChunkController : MonoBehaviour
{
    public Vector2Int ChunkIndex { get; private set; }
    public Vector3 MapChunkCenterPos { get; private set; }

    MapChunkData m_MapChunkData;
    List<GameObject> m_MapObjectList;
    bool m_IsActive = false;

    public void InitCenter(Vector2Int chunkIndex, Vector3 centerPosition, List<MapChunkMapObjectModel> mapObjectList)
    {
        ChunkIndex = chunkIndex;
        MapChunkCenterPos = centerPosition;
        m_MapChunkData = new MapChunkData();
        m_MapChunkData.MapObjectList = mapObjectList;
        m_MapObjectList = new List<GameObject>(mapObjectList.Count);
    }

    public void SetActive(bool active)
    {
        if (m_IsActive != active)
        {
            m_IsActive = active;
            gameObject.SetActive(active);
            List<MapChunkMapObjectModel> objectList = m_MapChunkData.MapObjectList;

            // TODO: 基于对象池去生成所有的地图对象，花草树木之类的
            if (m_IsActive) // 从对象池中获取所有物体
            {
                for (int i = 0; i < objectList.Count; i++)
                {
                    var gameObj = PoolManager.Instance.GetGameObject(objectList[i].Prefab, transform);
                    gameObj.transform.position = objectList[i].Position;
                    m_MapObjectList.Add(gameObj);
                }
            }
            else // 把所有物体放回对象池
            {
                for (int i = 0; i < objectList.Count; i++)
                {
                    PoolManager.Instance.PushGameObject(m_MapObjectList[i]);
                }

                m_MapObjectList.Clear();
            }
        }
    }
}