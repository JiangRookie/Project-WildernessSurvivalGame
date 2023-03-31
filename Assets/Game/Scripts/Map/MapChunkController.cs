﻿using System.Collections.Generic;
using JKFrame;
using UnityEngine;

namespace Project_WildernessSurvivalGame
{
    public class MapChunkController : MonoBehaviour
    {
        bool m_IsActive = false;
        Dictionary<ulong, MapObjectBase> m_MapObjectDict;
        public MapChunkData MapChunkData { get; private set; }
        public Vector2Int ChunkIndex { get; private set; }
        public Vector3 CenterPos { get; private set; }
        public bool IsAllForest { get; private set; }
        public bool IsInitializedMapUI { get; private set; } = false;

        void OnDestroy()
        {
            ArchiveManager.Instance.SaveMapChunkData(ChunkIndex, MapChunkData);
        }

        /// <summary>
        /// 初始化地图块
        /// </summary>
        /// <param name="chunkIndex">地图块索引</param>
        /// <param name="centerPos">地图中心点</param>
        /// <param name="isAllForest">地图块是否完全是森林</param>
        /// <param name="mapChunkData">地图块中的各种地图对象组合成的列表</param>
        public void Init(Vector2Int chunkIndex, Vector3 centerPos, bool isAllForest, MapChunkData mapChunkData)
        {
            ChunkIndex = chunkIndex;
            CenterPos = centerPos;
            IsAllForest = isAllForest;
            MapChunkData = mapChunkData;

            m_MapObjectDict = new Dictionary<ulong, MapObjectBase>(mapChunkData.MapObjectDict.Dictionary.Count);
            IsInitializedMapUI = true;
        }

        /// <param name="active">是否激活</param>
        public void SetActive(bool active)
        {
            if (m_IsActive != active)
            {
                m_IsActive = active;
                gameObject.SetActive(m_IsActive);
                if (m_IsActive) // 如果当前地图块为激活状态，则从对象池中获取所有物体
                {
                    foreach (var mapObjectDict in MapChunkData.MapObjectDict.Dictionary)
                    {
                        var config = ConfigManager.Instance.GetConfig<MapObjectConfig>(ConfigName.MapObject, mapObjectDict.Value.ConfigID);
                        var mapObj = PoolManager.Instance.GetGameObject(config.Prefab, transform).GetComponent<MapObjectBase>();
                        mapObj.transform.position = mapObjectDict.Value.Position;
                        mapObj.Init(this, mapObjectDict.Key);
                        m_MapObjectDict.Add(mapObjectDict.Key, mapObj);
                    }
                }
                else // 如果当前地图块为失活状态，则把所有物体放回对象池
                {
                    foreach (var mapObject in m_MapObjectDict)
                    {
                        mapObject.Value.JKGameObjectPushPool();
                    }
                    m_MapObjectDict.Clear();
                }
            }
        }

        public void RemoveMapObject(ulong mapObjectID)
        {
            // 自身显示层面移除
            m_MapObjectDict.Remove(mapObjectID);

            // 数据层面移除
            MapChunkData.MapObjectDict.Dictionary.Remove(mapObjectID);

            // UI地图层面移除
            MapManager.Instance.RemoveMapObject(mapObjectID);
        }

        public void AddMapObject(MapObjectData mapObjectData)
        {
            // 添加存档数据
            MapChunkData.MapObjectDict.Dictionary.Add(mapObjectData.ID, mapObjectData);

            // 实例化物体
            if (m_IsActive)
            {
                MapObjectConfig mapObjectConfig = ConfigManager.Instance.GetConfig<MapObjectConfig>(ConfigName.MapObject, mapObjectData.ConfigID);
                MapObjectBase mapObj = PoolManager.Instance.GetGameObject<MapObjectBase>(mapObjectConfig.Prefab, transform);
                mapObj.transform.position = mapObjectData.Position;
                mapObj.Init(this, mapObjectData.ID);
                m_MapObjectDict.Add(mapObjectData.ID, mapObj);
            }
        }
    }
}