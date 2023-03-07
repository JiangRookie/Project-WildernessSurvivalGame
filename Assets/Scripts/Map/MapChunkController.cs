using System.Collections.Generic;
using JKFrame;
using UnityEngine;

/// <summary>
/// 地图块中的各种地图对象个体
/// </summary>
public class MapChunkMapObjectModel
{
    public int ConfigID;
    public Vector3 Position;
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
    List<GameObject> m_MapGameObjList;
    public MapChunkData m_MapChunkData { get; private set; }
    public bool IsAllForest { get; private set; }
    public Vector2Int ChunkIndex { get; private set; }
    public bool IsInitializedMapUI { get; private set; } = false;
    public Vector3 MapChunkCenterPos { get; private set; }

    /// <summary>
    /// 初始化地图块
    /// </summary>
    /// <param name="chunkIndex">初始化地图块索引</param>
    /// <param name="centerPos">初始化地图块中心点</param>
    /// <param name="isAllForest">地图块是否完全是森林</param>
    /// <param name="mapObjectList">地图块中各种对象组合成的列表</param>
    public void Init
        (Vector2Int chunkIndex, Vector3 centerPos, bool isAllForest, List<MapChunkMapObjectModel> mapObjectList)
    {
        ChunkIndex = chunkIndex;
        MapChunkCenterPos = centerPos;

        IsAllForest = isAllForest;

        m_MapChunkData = new MapChunkData();
        m_MapChunkData.MapObjectList = mapObjectList;

        m_MapGameObjList = new List<GameObject>(mapObjectList.Count);
        IsInitializedMapUI = true;
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
                var config = ConfigManager.Instance.GetConfig<MapObjectConfig>
                    (ConfigName.MapObject, mapObject.ConfigID);
                var gameObj = PoolManager.Instance.GetGameObject(config.Prefab, transform);
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