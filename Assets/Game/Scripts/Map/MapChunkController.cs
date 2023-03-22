using System.Collections.Generic;
using JKFrame;
using UnityEngine;

namespace Project_WildernessSurvivalGame
{
    public class MapChunkController : MonoBehaviour
    {
        public Vector2Int ChunkIndex { get; private set; }
        public MapChunkData MapChunkData { get; private set; }
        public bool IsAllForest { get; private set; }
        public bool IsInitializedMapUI { get; private set; } = false;

        bool m_IsActive = false;
        List<GameObject> m_MapObjectList;

        /// <summary>
        /// 初始化地图块
        /// </summary>
        /// <param name="chunkIndex">地图块索引</param>
        /// <param name="isAllForest">地图块是否完全是森林</param>
        /// <param name="mapChunkData">地图块中的各种地图对象组合成的列表</param>
        public void Init(Vector2Int chunkIndex, bool isAllForest, MapChunkData mapChunkData)
        {
            ChunkIndex = chunkIndex;
            IsAllForest = isAllForest;
            MapChunkData = mapChunkData;

            m_MapObjectList = new List<GameObject>(MapChunkData.MapObjectList.Count);
            IsInitializedMapUI = true;
        }

        /// <param name="active">是否激活</param>
        public void SetActive(bool active)
        {
            if (m_IsActive != active)
            {
                m_IsActive = active;
                gameObject.SetActive(m_IsActive);
                var mapObjectList = MapChunkData.MapObjectList;
                if (m_IsActive) // 如果当前地图块为激活状态，则从对象池中获取所有物体
                {
                    foreach (var mapObject in mapObjectList)
                    {
                        var config = ConfigManager.Instance.GetConfig<MapObjectConfig>(
                            ConfigName.MapObject, mapObject.ConfigID);
                        var gameObj = PoolManager.Instance.GetGameObject(config.Prefab, transform);
                        gameObj.transform.position = mapObject.Position;
                        m_MapObjectList.Add(gameObj);
                    }
                }
                else // 如果当前地图块为失活状态，则把所有物体放回对象池
                {
                    for (int i = 0; i < mapObjectList.Count; i++)
                    {
                        PoolManager.Instance.PushGameObject(m_MapObjectList[i]);
                    }
                    m_MapObjectList.Clear();
                }
            }
        }
    }
}