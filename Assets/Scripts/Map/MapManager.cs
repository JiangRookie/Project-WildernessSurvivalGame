using System.Collections.Generic;
using JKFrame;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    #region Field

    #region 地图美术资源

    public Texture2D ForestTexture;
    public Texture2D[] MarshTextures;
    public Material MapMaterial;
    public MapConfig MapConfig;

    #endregion

    #region 地图尺寸

    [Tooltip("一行/列地图块数量")] public int MapSize;       //  m_MapSize -> m_MapChunkNum
    [Tooltip("一个地图块的格子数量")] public int MapChunkSize; //  m_MapChunkSize -> m_CellNum
    public float CellSize;
    float m_ChunkSizeOnWorld; // 在世界中实际的地图块尺寸 MapChunkSize * CellSize
    float m_MapSizeOnWorld;   //在世界中实际的地图尺寸

    #endregion

    #region 地图的随机参数

    public float NoiseLacunarity;
    public int MapGenerationSeed;
    public int MapRandomObjectSpawnSeed;
    public float MarshLimit;

    #endregion

    MapGenerator m_MapGenerator;

    public Transform Viewer;
    public int ViewDistance; // 单位：地图块（Chunk）
    Vector3 m_LastViewerPos = Vector3.one * -1;

    public Dictionary<Vector2Int, MapChunkController> MapChunkDict; // 全部已有的地图块
    public float UpdateVisibleChunkTime = 1f;
    bool m_CanUpdateChunk = true;
    List<MapChunkController> m_FinallyDisplayChunkList = new(); // 最终显示出来的地图块

    Dictionary<MapVertexType, List<int>> m_SpawnConfigDict; // 某个类型可以生成哪些地图对象配置的ID

    #endregion

    void Start()
    {
        // 获取地图物品配置，初始化地图生成对象配置字典
        Dictionary<int, ConfigBase> mapConfigDict = ConfigManager.Instance.GetConfigs(ConfigName.MapObject);
        m_SpawnConfigDict = new Dictionary<MapVertexType, List<int>>();
        m_SpawnConfigDict.Add(MapVertexType.Forest, new List<int>());
        m_SpawnConfigDict.Add(MapVertexType.Marsh, new List<int>());
        foreach ((int id, ConfigBase configBase) in mapConfigDict)
        {
            MapVertexType mapVertexType = ((MapObjectConfig)configBase).MapVertexType;
            m_SpawnConfigDict[mapVertexType].Add(id); // 将相同的顶点类型的Id放在同一个列表中
        }

        // 初始化地图生成器
        m_MapGenerator = new MapGenerator(ForestTexture, MarshTextures, MapMaterial, m_SpawnConfigDict, MapSize
                                        , MapChunkSize, CellSize, NoiseLacunarity, MapGenerationSeed
                                        , MapRandomObjectSpawnSeed, MarshLimit);
        m_MapGenerator.GenerateMapData();

        // 初始化地图块字典
        MapChunkDict = new Dictionary<Vector2Int, MapChunkController>();
        m_ChunkSizeOnWorld = MapChunkSize * CellSize;
        m_MapSizeOnWorld = m_ChunkSizeOnWorld * MapSize;
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

        if (m_IsShowingMap)
        {
            UpdateMapUI();
        }
    }

    /// <summary>
    /// 根据观察者的位置更新可视地图块
    /// </summary>
    void UpdateVisibleChunk()
    {
        // 如果观察者没有移动过，不需要刷新
        if (Viewer.position == m_LastViewerPos) return;
        m_LastViewerPos = Viewer.position;

        // 更新地图UI
        if (m_IsShowingMap) m_MapUI.UpdatePivot(Viewer.position);

        if (m_CanUpdateChunk == false) return;

        // 获取当前观察者所在的地图块
        var currChunkIndex = GetMapChunkIndex(Viewer.position);

        // 关闭全部不需要显示的地图块
        for (int i = m_FinallyDisplayChunkList.Count - 1; i >= 0; i--)
        {
            var chunkIndex = m_FinallyDisplayChunkList[i].ChunkIndex;

            if (Mathf.Abs(chunkIndex.x - currChunkIndex.x) <= ViewDistance
             && Mathf.Abs(chunkIndex.y - currChunkIndex.y) <= ViewDistance)
                continue;
            m_FinallyDisplayChunkList[i].SetActive(false);
            m_FinallyDisplayChunkList.RemoveAt(i);
        }

        // 从左下角开始遍历地图块
        int startX = currChunkIndex.x - ViewDistance;
        int startY = currChunkIndex.y - ViewDistance;
        int length = 2 * ViewDistance + 1;

        // 开启需要显示的地图块
        for (int x = 0; x < length; x++)
        {
            for (int y = 0; y < length; y++)
            {
                m_CanUpdateChunk = false;
                Invoke(nameof(ResetCanUpdateChunkFlag), UpdateVisibleChunkTime);
                var chunkIndex = new Vector2Int(startX + x, startY + y);

                // 在地图字典中，也就是之前加载过，但是不一定加载完成了，因为贴图会在协程中执行，执行完成后才算初始化完毕
                if (MapChunkDict.TryGetValue(chunkIndex, out MapChunkController chunk) && chunk.IsInitializedMapUI)
                {
                    // 上一次显示的地图列表中并不包含这个地图块 && 同时它已经完成了初始化
                    if (m_FinallyDisplayChunkList.Contains(chunk) == false && chunk.IsInitializedMapUI)
                    {
                        m_FinallyDisplayChunkList.Add(chunk);
                        chunk.SetActive(true);
                    }
                }

                // 之前未加载过
                else
                {
                    chunk = GenerateMapChunk(chunkIndex);
                }
            }
        }
    }

    /// <summary>
    /// 根据<paramref name="worldPos"/>获取地图块的索引
    /// </summary>
    /// <param name="worldPos">世界坐标</param>
    /// <returns>返回地图块的索引</returns>
    Vector2Int GetMapChunkIndex(Vector3 worldPos)
    {
        int x = Mathf.Clamp(value: Mathf.RoundToInt(worldPos.x / m_ChunkSizeOnWorld), 1, MapSize);
        int z = Mathf.Clamp(value: Mathf.RoundToInt(worldPos.z / m_ChunkSizeOnWorld), 1, MapSize);
        return new Vector2Int(x, z);
    }

    /// <summary>
    /// 生成地图块
    /// </summary>
    /// <returns></returns>
    MapChunkController GenerateMapChunk(Vector2Int index)
    {
        // 检查坐标的合法性
        if (index.x > MapSize - 1 || index.y > MapSize - 1) return null;
        if (index.x < 0 || index.y < 0) return null;

        MapChunkController chunk =
            m_MapGenerator.GenerateMapChunk(index, transform, () =>
            {
                m_WaitForUIUpdateMapChunkList.Add(index);
            });
        MapChunkDict.Add(index, chunk);
        return chunk;
    }

    void ResetCanUpdateChunkFlag() => m_CanUpdateChunk = true;

    #region MapUI

    bool m_IsInitializedMapUI = false;
    bool m_IsShowingMap = false;
    List<Vector2Int> m_WaitForUIUpdateMapChunkList = new(); // 等待UI更新的地图块列表
    UI_MapWindow m_MapUI;

    void ShowMapUI()
    {
        m_MapUI = UIManager.Instance.Show<UI_MapWindow>();

        if (m_IsInitializedMapUI == false)
        {
            m_MapUI.InitMap(MapSize, MapChunkSize, m_MapSizeOnWorld, ForestTexture);
            m_IsInitializedMapUI = true;
        }

        // 需要更新
        UpdateMapUI();
    }

    void UpdateMapUI()
    {
        for (int i = 0; i < m_WaitForUIUpdateMapChunkList.Count; i++)
        {
            var chunkIndex = m_WaitForUIUpdateMapChunkList[i];
            Texture2D texture = null;
            var mapChunk = MapChunkDict[chunkIndex];
            if (mapChunk.IsAllForest == false)
            {
                texture = (Texture2D)mapChunk.GetComponent<MeshRenderer>().material.mainTexture;
            }
            m_MapUI.AddMapChunk(chunkIndex, mapChunk.m_MapChunkData.MapObjectList, texture);
        }
        m_WaitForUIUpdateMapChunkList.Clear();

        // Content的坐标
        m_MapUI.UpdatePivot(Viewer.position);
    }

    void CloseMapUI() => UIManager.Instance.Close<UI_MapWindow>();

    #endregion
}