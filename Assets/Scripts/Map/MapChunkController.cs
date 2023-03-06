using System.Collections.Generic;
using JKFrame;
using UnityEngine;

/// <summary>
/// 地图块中的各种地图对象个体
/// </summary>
public class MapChunkMapObjectModel
{
    public Vector3 Position;
    public GameObject Prefab;
}

/// <summary>
/// 地图块数据
/// </summary>
public class MapChunkData
{
    /// <summary>
    /// 地图块中各种对象组合成的列表
    /// </summary>
    public List<MapChunkMapObjectModel> MapObjectList = new();
}

public class MapChunkController : MonoBehaviour
{
    bool m_IsActive = false;
    MapChunkData m_MapChunkData;
    List<GameObject> m_MapGameObjList;

    public Vector2Int ChunkIndex { get; private set; }
    public Vector3 MapChunkCenterPos { get; private set; }

    /// <summary>
    /// 初始化地图块
    /// </summary>
    /// <param name="chunkIndex">初始化地图块索引</param>
    /// <param name="centerPos">初始化地图块中心点</param>
    /// <param name="mapObjectList">地图块中各种对象组合成的列表</param>
    public void Init(Vector2Int chunkIndex, Vector3 centerPos, List<MapChunkMapObjectModel> mapObjectList)
    {
        ChunkIndex = chunkIndex;
        MapChunkCenterPos = centerPos;

        m_MapChunkData = new MapChunkData();
        m_MapChunkData.MapObjectList = mapObjectList;

        m_MapGameObjList = new List<GameObject>(mapObjectList.Count);
    }

    /// <param name="active">是否激活</param>
    public void SetActive(bool active)
    {
        if (m_IsActive == active) return;
        m_IsActive = active;
        gameObject.SetActive(active);

        // 获取地图对象列表
        var mapObjectList = m_MapChunkData.MapObjectList;
        if (m_IsActive) // 如果当前地图块为激活状态，则从对象池中获取所有物体
        {
            foreach (var mapObject in mapObjectList)
            {
                var gameObj = PoolManager.Instance.GetGameObject(mapObject.Prefab, transform);
                gameObj.transform.position = mapObject.Position;
                m_MapGameObjList.Add(gameObj);
            }
        }
        else // 如果当前地图块为失活状态，则把所有物体放回对象池
        {
            for (int i = 0; i < mapObjectList.Count; i++)
            {
                PoolManager.Instance.PushGameObject(m_MapGameObjList[i]);
            }
            m_MapGameObjList.Clear();
        }
    }
}