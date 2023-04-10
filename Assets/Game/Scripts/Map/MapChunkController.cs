using System.Collections.Generic;
using JKFrame;
using UnityEngine;

namespace Project_WildernessSurvivalGame
{
    public class MapChunkController : MonoBehaviour
    {
        bool m_IsActive = false;
        Dictionary<ulong, MapObjectBase> m_MapObjectDict;
        Dictionary<ulong, MapObjectData> m_WantToDestroyMapObjectDict;
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
            m_WantToDestroyMapObjectDict = new Dictionary<ulong, MapObjectData>();
            foreach (MapObjectData data in MapChunkData.MapObjectDict.Dictionary.Values)
            {
                if (data.DestroyDays > 0) m_WantToDestroyMapObjectDict.Add(data.ID, data);
            }
            EventManager.AddEventListener(EventName.OnMorning, OnMorning);
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
                    foreach (var mapObjectData in MapChunkData.MapObjectDict.Dictionary.Values)
                    {
                        InstantiateMapObject(mapObjectData, false);
                    }
                }
                else // 如果当前地图块为失活状态，则把所有物体放回对象池
                {
                    foreach (var mapObject in m_MapObjectDict.Values)
                    {
                        mapObject.JKGameObjectPushPool();
                    }
                    m_MapObjectDict.Clear();
                }
            }
        }

        void InstantiateMapObject(MapObjectData mapObjectData, bool isFromBuild)
        {
            MapObjectConfig config = ConfigManager.Instance.GetConfig<MapObjectConfig>(ConfigName.MapObject, mapObjectData.ConfigID);
            MapObjectBase mapObj = PoolManager.Instance.GetGameObject(config.Prefab, transform).GetComponent<MapObjectBase>();
            mapObj.transform.position = mapObjectData.Position;
            mapObj.Init(this, mapObjectData.ID, isFromBuild);
            m_MapObjectDict.Add(mapObjectData.ID, mapObj);
        }

        public void AddMapObject(MapObjectData mapObjectData, bool isFromBuild)
        {
            // 添加存档数据
            MapChunkData.MapObjectDict.Dictionary.Add(mapObjectData.ID, mapObjectData);
            if (mapObjectData.DestroyDays > 0) m_WantToDestroyMapObjectDict.Add(mapObjectData.ID, mapObjectData);

            // 实例化物体
            if (m_IsActive)
            {
                InstantiateMapObject(mapObjectData, isFromBuild);
            }
        }

        public void RemoveMapObject(ulong mapObjectID)
        {
            // 数据层面移除
            MapChunkData.MapObjectDict.Dictionary.Remove(mapObjectID, out MapObjectData mapObjectData);

            // 数据放进对象池
            mapObjectData.JKObjectPushPool();

            // 自身显示层面移除
            if (m_MapObjectDict.TryGetValue(mapObjectID, out MapObjectBase mapObject))
            {
                // 把游戏物体放进对象池
                mapObject.JKGameObjectPushPool();
                m_MapObjectDict.Remove(mapObjectID);
            }

            // UI地图层面移除
            MapManager.Instance.RemoveMapObject(mapObjectID);
        }

        static List<ulong> s_ExecuteDestroyMapObjectList = new List<ulong>(20); // 执行销毁的地图对象列表

        /// <summary>
        /// 当早晨时，刷新地图对象
        /// </summary>
        void OnMorning()
        {
            // 遍历可能要销毁的地图对象，做事件计算
            foreach (var mapObjectData in m_WantToDestroyMapObjectDict.Values)
            {
                mapObjectData.DestroyDays -= 1;
                if (mapObjectData.DestroyDays == 0)
                {
                    s_ExecuteDestroyMapObjectList.Add(mapObjectData.ID);
                }
            }
            foreach (ulong mapObjectID in s_ExecuteDestroyMapObjectList)
            {
                RemoveMapObject(mapObjectID);
            }
            s_ExecuteDestroyMapObjectList.Clear();

            // 得到新增的地图对象数据
            List<MapObjectData> mapObjectDatas = MapManager.Instance.SpawnMapObjectDataOnMapChunkRefresh(ChunkIndex);
            foreach (var mapObjectData in mapObjectDatas)
            {
                AddMapObject(mapObjectData, false);
            }
        }
    }
}