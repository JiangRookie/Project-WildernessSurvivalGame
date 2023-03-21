using System.Collections.Generic;
using JKFrame;
using UnityEngine;

namespace Project_WildernessSurvivalGame
{
    public class MapManager : SingletonMono<MapManager>
    {
        #region 运行时逻辑变量

        MapGenerator m_MapGenerator;

        public float MapSizeOnWorld; // 在世界中实际的地图尺寸 MapSize * m_ChunkSizeOnWorld
        float m_ChunkSizeOnWorld;    // 在世界中实际的地图块尺寸 MapChunkSize * CellSize

        public Transform Viewer;
        public float UpdateVisibleChunkTime = 1f; // 刷新可视地图块时间间隔
        bool m_CanUpdateChunk = true;
        Vector3 m_LastViewerPos = Vector3.one * -1;

        Dictionary<Vector2Int, MapChunkController> m_MapChunkDict;  // 全部已有的地图块
        List<MapChunkController> m_FinallyDisplayChunkList = new(); // 最终显示出来的地图块

        #endregion

        #region 配置

        public MapConfig MapConfig;
        Dictionary<MapVertexType, List<int>> m_SpawnConfigDict; // 某个类型可以生成哪些地图对象配置的ID

        #endregion

        #region 存档

        public MapInitData MapInitData;

        #endregion

        protected override void Awake()
        {
            base.Awake();
            Init();
        }

        void Init()
        {
            MapConfig = ConfigManager.Instance.GetConfig<MapConfig>(ConfigName.MAP);

            // 获取地图物品配置，初始化地图生成对象配置字典
            Dictionary<int, ConfigBase> mapConfigDict = ConfigManager.Instance.GetConfigs(ConfigName.MapObject);
            m_SpawnConfigDict = new Dictionary<MapVertexType, List<int>>();
            m_SpawnConfigDict.Add(MapVertexType.Forest, new List<int>());
            m_SpawnConfigDict.Add(MapVertexType.Marsh, new List<int>());
            foreach ((int id, ConfigBase configBase) in mapConfigDict)
            {
                var mapVertexType = ((MapObjectConfig)configBase).MapVertexType;
                m_SpawnConfigDict[mapVertexType].Add(id); // 将相同的顶点类型的Id放在同一个列表中
            }

            // 初始化地图生成器
            m_MapGenerator = new MapGenerator(MapConfig, MapInitData, m_SpawnConfigDict);
            m_MapGenerator.GenerateMapData();

            // 初始化地图块字典
            m_MapChunkDict = new Dictionary<Vector2Int, MapChunkController>();
            m_ChunkSizeOnWorld = MapConfig.MapChunkSize * MapConfig.CellSize;
            MapSizeOnWorld = m_ChunkSizeOnWorld * MapInitData.MapSize;
            DoUpdateVisibleChunk();
        }

        void Update()
        {
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

        /// <summary>
        /// 根据观察者的位置更新可视地图块
        /// </summary>
        void UpdateVisibleChunk()
        {
            // 如果观察者没有移动过，不需要刷新
            if (Viewer.position == m_LastViewerPos) return;

            // 更新地图 UI
            if (m_IsShowingMap) m_MapUI.UpdatePivot(Viewer.position);

            if (m_CanUpdateChunk == false) return;

            DoUpdateVisibleChunk();
        }

        void DoUpdateVisibleChunk()
        {
            // 获取当前观察者所在的地图块
            var currChunkIndex = GetMapChunkIndex(Viewer.position);

            #region 关闭全部不需要显示的地图块

            for (int i = m_FinallyDisplayChunkList.Count - 1; i >= 0; i--)
            {
                var chunkIndex = m_FinallyDisplayChunkList[i].ChunkIndex;

                if (Mathf.Abs(chunkIndex.x - currChunkIndex.x) > MapConfig.ViewDistance
                 || Mathf.Abs(chunkIndex.y - currChunkIndex.y) > MapConfig.ViewDistance)
                {
                    m_FinallyDisplayChunkList[i].SetActive(false);
                    m_FinallyDisplayChunkList.RemoveAt(i);
                }
            }

            #endregion

            #region 开启需要显示的地图块

            // Debug.Log("开启需要显示的地图块"); // 代码走到了这里

            // 从左下角开始遍历地图块
            int startX = currChunkIndex.x - MapConfig.ViewDistance;
            int startY = currChunkIndex.y - MapConfig.ViewDistance;

            // int count = 2 * ViewDistance + 1;
            for (int x = 0; x < 2 * MapConfig.ViewDistance + 1; x++)
            {
                for (int y = 0; y < 2 * MapConfig.ViewDistance + 1; y++)
                {
                    m_CanUpdateChunk = false;
                    Invoke(nameof(ResetCanUpdateChunkFlag), UpdateVisibleChunkTime);
                    Vector2Int chunkIndex = new Vector2Int(startX + x, startY + y);

                    // 在地图字典中，也就是之前加载过，但是不一定加载完成了，因为贴图会在协程中执行，执行完成后才算初始化完毕
                    if (m_MapChunkDict.TryGetValue(chunkIndex, out var chunk))
                    {
                        // 上一次显示的地图列表中并不包含这个地图块 && 同时它已经完成了初始化
                        if (m_FinallyDisplayChunkList.Contains(chunk) == false && chunk.IsInitializedMapUI)
                        {
                            m_FinallyDisplayChunkList.Add(chunk);
                            chunk.SetActive(true);
                        }
                    }
                    else // MapChunkDict.TryGetValue(chunkIndex, out var chunk) == false  
                    {
                        GenerateMapChunk(chunkIndex);
                    }
                }
            }

            #endregion
        }

        /// <summary>
        /// 根据<paramref name="worldPos"/>获取地图块的索引
        /// </summary>
        /// <param name="worldPos">世界坐标</param>
        /// <returns>返回地图块的索引</returns>
        Vector2Int GetMapChunkIndex(Vector3 worldPos)
        {
            int x = Mathf.Clamp(value: Mathf.RoundToInt(worldPos.x / m_ChunkSizeOnWorld), 1, MapInitData.MapSize);
            int z = Mathf.Clamp(value: Mathf.RoundToInt(worldPos.z / m_ChunkSizeOnWorld), 1, MapInitData.MapSize);
            return new Vector2Int(x, z);
        }

        /// <summary>
        /// 生成地图块
        /// </summary>
        /// <returns></returns>
        void GenerateMapChunk(Vector2Int index)
        {
            // 检查坐标的合法性，限制坐标在第一象限
            if (index.x > MapInitData.MapSize - 1 || index.y > MapInitData.MapSize - 1) return;
            if (index.x < 0 || index.y < 0) return;

            var chunk = m_MapGenerator.GenerateMapChunk(index, transform, () =>
            {
                m_WaitForUIUpdateMapChunkList.Add(index); // 加入到待更新的地图块UI列表
            });
            m_MapChunkDict.Add(index, chunk); // 加入到地图块列表
        }

        void ResetCanUpdateChunkFlag() => m_CanUpdateChunk = true;

        #region MapUI

        bool m_IsInitializedMapUI = false;
        bool m_IsShowingMap = false;
        List<Vector2Int> m_WaitForUIUpdateMapChunkList = new(); // 等待更新的地图块UI表
        UI_MapWindow m_MapUI;

        void ShowMapUI()
        {
            m_MapUI = UIManager.Instance.Show<UI_MapWindow>();
            Debug.Log(m_MapUI);
            if (m_IsInitializedMapUI == false)
            {
                m_MapUI.InitMap(MapInitData.MapSize, MapConfig.MapChunkSize, MapSizeOnWorld, MapConfig.ForestTexture);
                m_IsInitializedMapUI = true;
            }
            UpdateMapUI();
        }

        static void CloseMapUI() => UIManager.Instance.Close<UI_MapWindow>();

        void UpdateMapUI()
        {
            foreach (var chunkIndex in m_WaitForUIUpdateMapChunkList)
            {
                Texture2D texture = null;
                var mapChunk = m_MapChunkDict[chunkIndex];
                if (mapChunk.IsAllForest == false)
                {
                    texture = (Texture2D)mapChunk.GetComponent<MeshRenderer>().material.mainTexture;
                }
                m_MapUI.AddMapChunk(chunkIndex, mapChunk.MapChunkData.MapObjectList, texture);
            }
            m_WaitForUIUpdateMapChunkList.Clear();
            m_MapUI.UpdatePivot(Viewer.position);
        }

        #endregion
    }
}