using System.Collections;
using System.Collections.Generic;
using JKFrame;
using UnityEngine;

namespace Project_WildernessSurvivalGame
{
    public class MapManager : SingletonMono<MapManager>
    {
        #region FIELD

        #region 运行时逻辑变量

        [SerializeField] MeshCollider m_MeshCollider;
        Transform m_Viewer; // 观察者 -> Player
        MapGenerator m_MapGenerator;
        float m_MapSizeOnWorld;
        float m_ChunkSizeOnWorld;
        const float UPDATE_VISIBLE_CHUNK_TIME = 1f; // 刷新可视地图块时间间隔
        bool m_CanUpdateChunk = true;
        Vector3 m_LastViewerPos = Vector3.one * -1;
        [Tooltip("全部已有地图块字典")] Dictionary<Vector2Int, MapChunkController> m_MapChunkDict;
        [Tooltip("最终显示出来的地图块")] List<MapChunkController> m_FinallyDisplayChunkList = new List<MapChunkController>();

        #endregion

        #region 配置

        MapConfig m_MapConfig;
        Dictionary<MapVertexType, List<int>> m_SpawnConfigDict; // 某个类型可以生成哪些地图对象配置的ID

        #endregion

        #region 存档

        MapInitData m_MapInitData;
        MapData m_MapData;

        #endregion

        #endregion

        void Update()
        {
            if (GameSceneManager.Instance.IsInitialized == false) return;
            UpdateVisibleChunk();
            if (Input.GetKeyDown(KeyCode.M))
            {
                if (m_IsShowingMap)
                    CloseMapUI();
                else
                    ShowMapUI();
                m_IsShowingMap = !m_IsShowingMap;
            }

            if (m_IsShowingMap) UpdateMapUI();
        }

        void OnDestroy()
        {
            ArchiveManager.Instance.SaveMapData();
        }

        public void Init()
        {
            StartCoroutine(DoInit());
        }

        IEnumerator DoInit()
        {
            // 确定存档
            m_MapInitData = ArchiveManager.Instance.MapInitData;
            m_MapData = ArchiveManager.Instance.MapData;

            // 确定配置 获取地图物品配置，初始化地图生成对象配置字典
            m_MapConfig = ConfigManager.Instance.GetConfig<MapConfig>(ConfigName.MAP);
            Dictionary<int, ConfigBase> mapConfigDict = ConfigManager.Instance.GetConfigs(ConfigName.MapObject);
            m_SpawnConfigDict = new Dictionary<MapVertexType, List<int>>();
            m_SpawnConfigDict.Add(MapVertexType.Forest, new List<int>());
            m_SpawnConfigDict.Add(MapVertexType.Marsh, new List<int>());
            foreach ((int id, ConfigBase config) in mapConfigDict)
            {
                MapVertexType mapVertexType = ((MapObjectConfig)config).MapVertexType;
                if (mapVertexType == MapVertexType.None) continue;
                m_SpawnConfigDict[mapVertexType].Add(id); // 将相同的顶点类型的Id放在同一个列表中
            }

            // 初始化地图生成器
            m_MapGenerator = new MapGenerator(m_MapConfig, m_MapInitData, m_MapData, m_SpawnConfigDict);
            m_MapGenerator.GenerateMapData();

            // 初始化地图块字典
            m_MapChunkDict = new Dictionary<Vector2Int, MapChunkController>();

            m_ChunkSizeOnWorld = m_MapConfig.MapChunkSize * m_MapConfig.CellSize;
            m_MapSizeOnWorld = m_ChunkSizeOnWorld * m_MapInitData.MapSize;

            // 生成地面碰撞体
            m_MeshCollider.sharedMesh = GenerateGroundMesh(m_MapSizeOnWorld, m_MapSizeOnWorld);

            int mapChunkCount = m_MapData.MapChunkIndexList.Count;
            if (mapChunkCount > 0) // 旧存档
            {
                // 根据存档去恢复整个地图的状态
                for (int i = 0; i < mapChunkCount; i++)
                {
                    SerializableVector2 chunkIndex = m_MapData.MapChunkIndexList[i];
                    MapChunkData mapChunkData = ArchiveManager.Instance.GetMapChunkData(chunkIndex);
                    GenerateMapChunk(chunkIndex.Convert2Vector2Int(), mapChunkData).gameObject.SetActive(false);
                }
                DoUpdateVisibleChunk();

                // 进度条的时间要跟地图块的数量关联
                for (int i = 1; i <= mapChunkCount; i++)
                {
                    yield return new WaitForSeconds(0.01f);
                    GameSceneManager.Instance.UpdateMapProgress(i, mapChunkCount);
                }
            }
            else // 新存档
            {
                DoUpdateVisibleChunk();
                for (int i = 1; i <= 10; i++) // 加载九宫格
                {
                    yield return new WaitForSeconds(0.01f);
                    GameSceneManager.Instance.UpdateMapProgress(i, 10);
                }
            }
            // 显示一次MapUI，做好初始化后再关闭掉
            ShowMapUI();
            CloseMapUI();
        }

        static Mesh GenerateGroundMesh(float width, float height)
        {
            Mesh mesh = new Mesh();

            // 确定顶点在哪里
            mesh.vertices = new[]
            {
                new Vector3(0, 0, 0)
              , new Vector3(0, 0, height)
              , new Vector3(width, 0, height)
              , new Vector3(width, 0, 0)
            };

            // 确定哪些点形成三角形
            mesh.triangles = new[]
            {
                0, 1, 2
              , 0, 2, 3
            };

            // 设置 UV
            mesh.uv = new Vector2[]
            {
                new Vector3(0, 0)
              , new Vector3(0, 1)
              , new Vector3(1, 1)
              , new Vector3(1, 0)
            };

            return mesh;
        }

        #region MapChunk

        public void UpdateViewer(Transform player)
        {
            m_Viewer = player;
        }

        /// <summary>
        /// 根据观察者的位置更新可视地图块
        /// </summary>
        void UpdateVisibleChunk()
        {
            // 如果观察者没有移动过，不需要刷新
            if (m_Viewer.position == m_LastViewerPos) return;
            m_LastViewerPos = m_Viewer.position;

            // 更新地图 UI
            if (m_IsShowingMap) m_MapUI.UpdatePivot(m_Viewer.position);

            if (m_CanUpdateChunk == false) return;

            DoUpdateVisibleChunk();
        }

        void DoUpdateVisibleChunk()
        {
            // 获取当前观察者所在的地图块
            Vector2Int currViewerChunkIndex = GetMapChunkIndex(m_Viewer.position);

            #region 关闭全部不需要显示的地图块

            for (int i = m_FinallyDisplayChunkList.Count - 1; i >= 0; i--)
            {
                Vector2Int chunkIndex = m_FinallyDisplayChunkList[i].ChunkIndex;

                if (Mathf.Abs(chunkIndex.x - currViewerChunkIndex.x) > m_MapConfig.ViewDistance
                 || Mathf.Abs(chunkIndex.y - currViewerChunkIndex.y) > m_MapConfig.ViewDistance)
                {
                    m_FinallyDisplayChunkList[i].SetActive(false);
                    m_FinallyDisplayChunkList.RemoveAt(i);
                }
            }

            #endregion

            #region 开启需要显示的地图块

            // 从左下角开始遍历地图块
            int startX = currViewerChunkIndex.x - m_MapConfig.ViewDistance;
            int startY = currViewerChunkIndex.y - m_MapConfig.ViewDistance;
            int count = 2 * m_MapConfig.ViewDistance + 1;

            for (int x = 0; x < count; x++)
            {
                for (int y = 0; y < count; y++)
                {
                    Vector2Int chunkIndex = new Vector2Int(startX + x, startY + y);

                    // 在地图字典中，也就是之前加载过，但是不一定加载完成了，因为贴图会在协程中执行，执行完成后才算初始化完毕
                    if (m_MapChunkDict.TryGetValue(chunkIndex, out MapChunkController chunk))
                    {
                        // 上一次显示的地图列表中并不包含这个地图块 && 同时它已经完成了初始化
                        if (m_FinallyDisplayChunkList.Contains(chunk) == false && chunk.IsInitializedMapUI)
                        {
                            m_FinallyDisplayChunkList.Add(chunk);
                            chunk.SetActive(true);
                        }
                    }
                    else // MapChunkDict.TryGetValue(chunkIndex, out var chunk) == false 之前没有加载过
                    {
                        GenerateMapChunk(chunkIndex);
                    }
                }
            }
            m_CanUpdateChunk = false;
            Invoke(nameof(ResetCanUpdateChunkFlag), UPDATE_VISIBLE_CHUNK_TIME);

            #endregion
        }

        /// <summary>
        /// 根据<paramref name="worldPos"/>获取地图块的索引
        /// </summary>
        /// <param name="worldPos">世界坐标</param>
        /// <returns>返回地图块的索引</returns>
        Vector2Int GetMapChunkIndex(Vector3 worldPos)
        {
            int x = Mathf.Clamp(value: Mathf.RoundToInt(worldPos.x / m_ChunkSizeOnWorld), 1, m_MapInitData.MapSize);
            int z = Mathf.Clamp(value: Mathf.RoundToInt(worldPos.z / m_ChunkSizeOnWorld), 1, m_MapInitData.MapSize);
            return new Vector2Int(x, z);
        }

        /// <summary>
        /// 生成地图块
        /// </summary>
        /// <returns></returns>
        MapChunkController GenerateMapChunk(Vector2Int index, MapChunkData mapChunkData = null)
        {
            // 检查坐标的合法性，限制坐标在第一象限
            if (index.x > m_MapInitData.MapSize - 1 || index.y > m_MapInitData.MapSize - 1) return null;
            if (index.x < 0 || index.y < 0) return null;

            MapChunkController chunk
                = m_MapGenerator.GenerateMapChunk(index, transform, mapChunkData, () =>
                {
                    m_WaitForUpdateMapChunkUIList.Add(index); // 加入到待更新的地图块UI列表
                });
            m_MapChunkDict.Add(index, chunk); // 加入到地图块列表

            return chunk;
        }

        void ResetCanUpdateChunkFlag()
        {
            m_CanUpdateChunk = true;
        }

        #endregion

        #region Map UI

        UI_MapWindow m_MapUI;
        bool m_IsInitializedMapUI = false;
        bool m_IsShowingMap = false;
        List<Vector2Int> m_WaitForUpdateMapChunkUIList = new(); // 等待更新的地图块UI列表

        void ShowMapUI()
        {
            m_MapUI = UIManager.Instance.Show<UI_MapWindow>();
            if (m_IsInitializedMapUI == false)
            {
                m_MapUI.InitMap(m_MapInitData.MapSize, m_MapConfig.MapChunkSize, m_MapSizeOnWorld, m_MapConfig.ForestTexture);
                m_IsInitializedMapUI = true;
            }
            UpdateMapUI();
        }

        static void CloseMapUI()
        {
            UIManager.Instance.Close<UI_MapWindow>();
        }

        void UpdateMapUI()
        {
            foreach (var chunkIndex in m_WaitForUpdateMapChunkUIList)
            {
                Texture2D texture = null;
                MapChunkController mapChunk = m_MapChunkDict[chunkIndex];
                if (mapChunk.IsAllForest == false)
                {
                    texture = (Texture2D)mapChunk.GetComponent<MeshRenderer>().material.mainTexture;
                }
                m_MapUI.AddMapChunk(chunkIndex, mapChunk.MapChunkData.MapObjectDict, texture);
            }
            m_WaitForUpdateMapChunkUIList.Clear();
            m_MapUI.UpdatePivot(m_Viewer.position);
        }

        #endregion

        #region MapObject

        /// <summary>
        /// 移除一个地图对象
        /// </summary>
        /// <param name="mapObjectID">地图对象ID</param>
        public void RemoveMapObject(ulong mapObjectID)
        {
            if (m_MapUI != null) m_MapUI.RemoveMapObjectIcon(mapObjectID);
        }

        /// <summary>
        /// 生成一个地图对象
        /// </summary>
        /// <param name="mapChunkController"></param>
        /// <param name="mapObjectConfigID"></param>
        /// <param name="spawnPos"></param>
        /// <param name="isFromBuild"></param>
        public void SpawnMapObject(MapChunkController mapChunkController, int mapObjectConfigID, Vector3 spawnPos, bool isFromBuild)
        {
            // 生成数据
            MapObjectData mapObjectData = m_MapGenerator.GenerateMapObjectData(mapObjectConfigID, spawnPos);
            if (mapObjectData == null) return;

            // 交给地图块
            mapChunkController.AddMapObject(mapObjectData, isFromBuild);

            // 处理Icon
            if (m_MapUI != null)
            {
                m_MapUI.AddMapObjectIcon(mapObjectData);
            }
        }

        /// <summary>
        /// 生成一个地图对象
        /// </summary>
        /// <param name="mapObjectConfigID"></param>
        /// <param name="spawnPos"></param>
        /// <param name="isFromBuild"></param>
        public void SpawnMapObject(int mapObjectConfigID, Vector3 spawnPos, bool isFromBuild)
        {
            Vector2Int chunkIndex = GetMapChunkIndex(spawnPos);
            SpawnMapObject(m_MapChunkDict[chunkIndex], mapObjectConfigID, spawnPos, isFromBuild);
        }

        public List<MapObjectData> SpawnMapObjectDataOnMapChunkRefresh(Vector2Int chunkIndex)
        {
            return m_MapGenerator.GenerateMapObjectDataListOnMapChunkRefresh(chunkIndex);
        }

        #endregion
    }
}