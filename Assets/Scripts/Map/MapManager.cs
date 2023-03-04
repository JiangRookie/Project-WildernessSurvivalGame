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
    float m_ChunkLengthOnWorld; // 在世界中实际的地图块尺寸 单位是米

    #endregion

    #region 地图的随机参数

    public float NoiseLacunarity;
    public int MapGenerationSeed;
    public int MapRandomObjectSpawnSeed;
    public float MarshLimit;

    #endregion

    MapGenerator m_MapGenerator;
    public int VisualDistance; // 玩家可视距离，单位是 Chunk（地图块）
    public Transform Watcher;
    Vector3 m_LastWatcherPos = Vector3.one * -1;
    public Dictionary<Vector2Int, MapChunkController> MapChunkDict; // 全部已有的地图块

    public float UpdateVisibleChunkTime = 1f; // 更新可视地图块最小时间
    bool m_CanUpdateChunk = true;

    List<MapChunkController> m_FinallyDisplayChunkList = new(); // 最终显示出来的地图块

    // 某个类型可以生成哪些配置的ID
    Dictionary<MapVertexType, List<int>> m_SpawnConfigDict;

    #endregion

    void Start()
    {
        // 确定配置
        Dictionary<int, ConfigBase> mapConfigDict = ConfigManager.Instance.GetConfigs(ConfigName.MapObject);
        m_SpawnConfigDict = new Dictionary<MapVertexType, List<int>>();
        m_SpawnConfigDict.Add(MapVertexType.Forest, new List<int>());
        m_SpawnConfigDict.Add(MapVertexType.Marsh, new List<int>());

        foreach ((int id, ConfigBase configBase) in mapConfigDict)
        {
            MapVertexType mapVertexType = ((MapObjectConfig)configBase).MapVertexType;
            m_SpawnConfigDict[mapVertexType].Add(id);
        }

        // 初始化地图生成器
        m_MapGenerator = new MapGenerator(ForestTexture, MarshTextures, MapMaterial, m_SpawnConfigDict, MapSize
                                        , MapChunkSize, CellSize, NoiseLacunarity, MapGenerationSeed
                                        , MapRandomObjectSpawnSeed, MarshLimit);
        m_MapGenerator.GenerateMapData();
        MapChunkDict = new Dictionary<Vector2Int, MapChunkController>();
        m_ChunkLengthOnWorld = MapChunkSize * CellSize;
    }

    void Update()
    {
        UpdateVisibleChunk();
    }

    /// <summary>
    /// 根据观察者的位置更新可视地图块
    /// </summary>
    void UpdateVisibleChunk()
    {
        // 如果观察者没有移动过，不需要刷新
        if (Watcher.position == m_LastWatcherPos) return;
        if (m_CanUpdateChunk == false) return;

        // 当前 Watcher 所在的地图块
        var currChunkIndex = GetMapChunkIndex(Watcher.position);

        // 关闭全部不需要显示的地图块
        for (int i = m_FinallyDisplayChunkList.Count - 1; i >= 0; i--)
        {
            var chunkIndex = m_FinallyDisplayChunkList[i].ChunkIndex;

            if (Mathf.Abs(chunkIndex.x - currChunkIndex.x) > VisualDistance
             || Mathf.Abs(chunkIndex.y - currChunkIndex.y) > VisualDistance)
            {
                m_FinallyDisplayChunkList[i].SetActive(false);
                m_FinallyDisplayChunkList.RemoveAt(i);
            }
        }

        int startX = currChunkIndex.x - VisualDistance;
        int startY = currChunkIndex.y - VisualDistance;

        // 开启需要显示的地图块
        for (int x = 0; x < 2 * VisualDistance + 1; x++)
        {
            for (int y = 0; y < 2 * VisualDistance + 1; y++)
            {
                m_CanUpdateChunk = false;
                Invoke(nameof(ResetCanUpdateChunkFlag), UpdateVisibleChunkTime);
                var chunkIndex = new Vector2Int(startX + x, startY + y);

                // 之前加载过
                if (MapChunkDict.TryGetValue(chunkIndex, out MapChunkController chunk))
                {
                    // 这个地图是不是在显示列表
                    if (m_FinallyDisplayChunkList.Contains(chunk) == false)
                    {
                        m_FinallyDisplayChunkList.Add(chunk);
                        chunk.SetActive(true);
                    }
                }

                // 之前未加载过
                else
                {
                    chunk = GenerateMapChunk(chunkIndex);

                    if (chunk != null)
                    {
                        chunk.SetActive(true);
                        m_FinallyDisplayChunkList.Add(chunk);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 根据<paramref name="worldPosition"/>获取地图块的索引
    /// </summary>
    /// <param name="worldPosition">世界坐标</param>
    /// <returns>返回地图块的索引</returns>
    Vector2Int GetMapChunkIndex(Vector3 worldPosition)
    {
        int x = Mathf.Clamp(value: Mathf.RoundToInt(worldPosition.x / m_ChunkLengthOnWorld), min: 1, max: MapSize);
        int z = Mathf.Clamp(value: Mathf.RoundToInt(worldPosition.z / m_ChunkLengthOnWorld), min: 1, max: MapSize);
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

        MapChunkController chunk = m_MapGenerator.GenerateMapChunk(index, transform);
        MapChunkDict.Add(index, chunk);
        return chunk;
    }

    void ResetCanUpdateChunkFlag() => m_CanUpdateChunk = true;
}