using System.Collections.Generic;
using JKFrame;
using UnityEngine;
using UnityEngine.AI;

namespace Project_WildernessSurvivalGame
{
    public class MapChunkController : MonoBehaviour
    {
        bool m_IsActive = false;
        Dictionary<ulong, MapObjectBase> m_MapObjectDict;
        Dictionary<ulong, AIBase> m_AIObjectDict;
        Dictionary<ulong, MapObjectData> m_WantToDestroyMapObjectDict;
        public MapChunkData MapChunkData { get; private set; }
        public Vector2Int ChunkIndex { get; private set; }
        public Vector3 CenterPos { get; private set; }
        public bool IsAllForest { get; private set; }
        public bool IsInitializedMapUI { get; private set; } = false;

        void OnGameSave()
        {
            ArchiveManager.Instance.SaveMapChunkData(ChunkIndex, MapChunkData);
        }

        public void OnCloseGameScene()
        {
            SetActive(false);
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
            m_AIObjectDict = new Dictionary<ulong, AIBase>(mapChunkData.AIDataDict.Dictionary.Count);
            IsInitializedMapUI = true;
            m_WantToDestroyMapObjectDict = new Dictionary<ulong, MapObjectData>();
            foreach (MapObjectData data in MapChunkData.MapObjectDict.Dictionary.Values)
            {
                if (data.DestroyDays > 0) m_WantToDestroyMapObjectDict.Add(data.ID, data);
            }
            EventManager.AddEventListener(EventName.OnMorning, OnMorning);
            EventManager.AddEventListener(EventName.SaveGame, OnGameSave);
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
                    // 处理地图对象
                    foreach (MapObjectData mapObjectData in MapChunkData.MapObjectDict.Dictionary.Values)
                    {
                        InstantiateMapObject(mapObjectData, false);
                    }

                    // 处理AI对象
                    foreach (MapObjectData aiData in MapChunkData.AIDataDict.Dictionary.Values)
                    {
                        InstantiateAIObject(aiData);
                    }
                }
                else // 如果当前地图块为失活状态，则把所有物体放回对象池
                {
                    foreach (MapObjectBase mapObject in m_MapObjectDict.Values)
                    {
                        mapObject.JKGameObjectPushPool();
                    }
                    foreach (AIBase ai in m_AIObjectDict.Values)
                    {
                        ai.Destroy();
                    }
                    m_MapObjectDict.Clear();
                    m_AIObjectDict.Clear();
                }
            }
        }

        #region MapObject

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

        #endregion

        #region AI

        public void AddAIObject(MapObjectData aiData)
        {
            // 添加存档数据
            MapChunkData.AIDataDict.Dictionary.Add(aiData.ID, aiData);

            // 实例化物体
            if (m_IsActive) InstantiateAIObject(aiData);
        }

        void InstantiateAIObject(MapObjectData aiData)
        {
            AIConfig aiConfig = ConfigManager.Instance.GetConfig<AIConfig>(ConfigName.AI, aiData.ConfigID);
            AIBase aiObj = PoolManager.Instance.GetGameObject(aiConfig.Prefab, transform).GetComponent<AIBase>();
            if (aiData.Position == Vector3.zero)
            {
                aiData.Position = GetAIRandomPoint(aiConfig.MapVertexType);
            }
            aiObj.Init(this, aiData);
            m_AIObjectDict.Add(aiData.ID, aiObj);
        }

        public Vector3 GetAIRandomPoint(MapVertexType vertexType)
        {
            // if (vertexType == MapVertexType.Forest)
            // {
            //     // 如果格子不够，依然用另外一个类型
            //     vertexList = MapChunkData.ForestVertexList.Count < MapManager.Instance.MapConfig.GenerateAiMinVertexCountOnChunk
            //         ? MapChunkData.MarshVertexList
            //         : MapChunkData.ForestVertexList;
            // }
            // else if (vertexType == MapVertexType.Marsh)
            // {
            //     vertexList = MapChunkData.ForestVertexList.Count < MapManager.Instance.MapConfig.GenerateAiMinVertexCountOnChunk
            //         ? MapChunkData.ForestVertexList
            //         : MapChunkData.MarshVertexList;
            // }

            List<MapVertex> vertexList = vertexType switch
            {
                MapVertexType.Forest => MapChunkData.ForestVertexList.Count < MapManager.Instance.MapConfig.GenerateAiMinVertexCountOnChunk
                    ? MapChunkData.MarshVertexList
                    : MapChunkData.ForestVertexList
              , MapVertexType.Marsh => MapChunkData.ForestVertexList.Count < MapManager.Instance.MapConfig.GenerateAiMinVertexCountOnChunk
                    ? MapChunkData.ForestVertexList
                    : MapChunkData.MarshVertexList
              , _ => null
            };

            int index = Random.Range(0, vertexList.Count);
            if (NavMesh.SamplePosition(vertexList[index].Position, out NavMeshHit hitInfo, 1, NavMesh.AllAreas))
            {
                return hitInfo.position;
            }

            return GetAIRandomPoint(vertexType);
        }

        public void RemoveAIObjectOnTransfer(ulong aiObjectID)
        {
            MapChunkData.AIDataDict.Dictionary.Remove(aiObjectID);
            m_AIObjectDict.Remove(aiObjectID);
        }

        public void RemoveAIObject(ulong aiObjectID)
        {
            MapChunkData.AIDataDict.Dictionary.Remove(aiObjectID, out MapObjectData aiData);
            aiData.JKObjectPushPool();
            if (m_AIObjectDict.Remove(aiObjectID, out AIBase aiObject))
            {
                aiObject.Destroy();
            }
        }

        public void AddAIObjectOnTransfer(MapObjectData aiObjectData, AIBase aiObject)
        {
            MapChunkData.AIDataDict.Dictionary.Add(aiObjectData.ID, aiObjectData);
            m_AIObjectDict.Add(aiObjectData.ID, aiObject);
            aiObject.transform.SetParent(transform);
            aiObject.InitOnTransfer(this);
        }

        #endregion

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
            List<MapObjectData> mapObjectDataList = MapManager.Instance.SpawnMapObjectDataOnMapChunkRefresh(ChunkIndex);
            foreach (var mapObjectData in mapObjectDataList)
            {
                AddMapObject(mapObjectData, false);
            }

            // 每三天刷新一次AI
            if (TimeManager.Instance.CurrentDayNum % 3 == 0)
            {
                mapObjectDataList = MapManager.Instance.SpawnMapObjectDataOnMapChunkRefresh(MapChunkData);
                foreach (var mapObjectData in mapObjectDataList)
                {
                    AddAIObject(mapObjectData);
                }
            }
        }
    }
}