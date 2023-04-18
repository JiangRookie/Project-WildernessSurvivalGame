using System;
using System.Collections;
using System.Collections.Generic;
using JKFrame;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// 地图生成工具
/// </summary>
public class MapGenerator
{
    #region FIELD

    #region 运行时逻辑变量

    MapGrid m_MapGrid;
    Material m_MarshMaterial;
    Mesh m_ChunkMesh;
    int m_MapObjectForestWeightTotal; // 森林生成物品的权重总和
    int m_MapObjectMarshWeightTotal;  // 沼泽生成物品的权重总和
    int m_AIForestWeightTotal;        // 森林生成物品的权重总和
    int m_AIMarshWeightTotal;         // 沼泽生成物品的权重总和
    static readonly int s_MainTex = Shader.PropertyToID("_MainTex");

    #endregion

    #region 配置

    Dictionary<MapVertexType, List<int>> m_MapObjectConfigDict;
    Dictionary<MapVertexType, List<int>> m_AIConfigDict;
    MapConfig m_MapConfig;

    #endregion

    #region 存档

    MapInitData m_MapInitData;
    MapData m_MapData;

    #endregion

    #endregion

    public MapGenerator
    (
        MapConfig mapConfig, MapInitData mapInitData, MapData mapData, Dictionary<MapVertexType, List<int>> mapObjectConfigDict
      , Dictionary<MapVertexType, List<int>> aiConfigDict
    )
    {
        m_MapConfig = mapConfig;
        m_MapInitData = mapInitData;
        m_MapData = mapData;
        m_MapObjectConfigDict = mapObjectConfigDict;
        m_AIConfigDict = aiConfigDict;
    }

    /// <summary>
    /// 生成地图数据，主要是所有地图块都通用的数据
    /// </summary>
    public void GenerateMapData()
    {
        // 应用地图随机生成种子
        Random.InitState(m_MapInitData.MapGenerationSeed);

        int rowTotalCellNum = m_MapInitData.MapSize * m_MapConfig.MapChunkSize; // 一行/列总格子数
        float[,] noiseMap = GenerateNoiseMap(rowTotalCellNum, rowTotalCellNum, m_MapConfig.NoiseLacunarity);

        // 生成网格数据
        m_MapGrid = new MapGrid(rowTotalCellNum, rowTotalCellNum, m_MapConfig.CellSize);
        m_MapGrid.CalculateMapVertexType(noiseMap, m_MapInitData.MarshLimit);

        m_MapConfig.MapMaterial.mainTexture = m_MapConfig.ForestTexture;
        float mapChunkLength = m_MapConfig.MapChunkSize * m_MapConfig.CellSize;
        m_MapConfig.MapMaterial.SetTextureScale(s_MainTex, new Vector2(mapChunkLength, mapChunkLength)); // 设置纹理缩放

        // 实例化一个沼泽材质
        m_MarshMaterial = new Material(m_MapConfig.MapMaterial); // 通过复制其他材质的所有属性来创建一个沼泽材质
        m_MarshMaterial.SetTextureScale(s_MainTex, Vector2.one);

        m_ChunkMesh = GenerateMapChunkMesh(m_MapConfig.MapChunkSize, m_MapConfig.MapChunkSize, m_MapConfig.CellSize);

        // 应用地图随机对象（花草树木）生成种子
        Random.InitState(m_MapInitData.MapObjectRandomSpawnSeed);

        List<int> idList = m_MapObjectConfigDict[MapVertexType.Forest];
        foreach (int id in idList)
        {
            m_MapObjectForestWeightTotal += ConfigManager.Instance.GetConfig<MapObjectConfig>(ConfigName.MapObject, id).Probability;
        }

        idList = m_MapObjectConfigDict[MapVertexType.Marsh];
        foreach (int id in idList)
        {
            m_MapObjectMarshWeightTotal += ConfigManager.Instance.GetConfig<MapObjectConfig>(ConfigName.MapObject, id).Probability;
        }

        idList = m_AIConfigDict[MapVertexType.Forest];
        foreach (int id in idList)
        {
            m_AIForestWeightTotal += ConfigManager.Instance.GetConfig<AIConfig>(ConfigName.AI, id).Probability;
        }

        idList = m_AIConfigDict[MapVertexType.Marsh];
        foreach (int id in idList)
        {
            m_AIMarshWeightTotal += ConfigManager.Instance.GetConfig<AIConfig>(ConfigName.AI, id).Probability;
        }
    }

    #region MapChunk

    /// <summary>
    /// 生成地图块
    /// </summary>
    /// <param name="chunkIndex">地图块索引</param>
    /// <param name="parent">父物体</param>
    /// <param name="mapChunkData"></param>
    /// <param name="callBackForMapTexture"></param>
    /// <returns></returns>
    public MapChunkController GenerateMapChunk(Vector2Int chunkIndex, Transform parent, MapChunkData mapChunkData, Action callBackForMapTexture)
    {
        // 生成地图块物体
        GameObject mapChunkGameObj = new GameObject("Chunk_" + chunkIndex.ToString());
        MapChunkController mapChunk = mapChunkGameObj.AddComponent<MapChunkController>();

        // 为地图块生成 Mesh
        mapChunkGameObj.AddComponent<MeshFilter>().mesh = m_ChunkMesh;

        Texture2D mapTexture;
        bool allForest;

        // 生成地图块的贴图
        this.StartCoroutine(GenerateMapTexture(chunkIndex, (texture, isAllForest) =>
        {
            allForest = isAllForest;
            if (isAllForest)
            {
                mapChunkGameObj.AddComponent<MeshRenderer>().sharedMaterial = m_MapConfig.MapMaterial;
            }
            else
            {
                mapTexture = texture;
                Material material = new Material(m_MarshMaterial);
                material.mainTexture = mapTexture;
                mapChunkGameObj.AddComponent<MeshRenderer>().material = material;
            }
            callBackForMapTexture?.Invoke();

            float chunkSize = m_MapConfig.MapChunkSize * m_MapConfig.CellSize; // 地图块大小
            Vector3 position = new Vector3(chunkIndex.x * chunkSize, 0, chunkIndex.y * chunkSize);
            mapChunk.transform.position = position;
            mapChunkGameObj.transform.SetParent(parent);

            // 如果没有指定地图块数据，说明是新建的，需要生成默认数据
            if (mapChunkData == null)
            {
                // 生成场景物体数据
                mapChunkData = GenerateMapChunkData(chunkIndex);

                // 生成后进行持久化保存
                ArchiveManager.Instance.AddAndSaveMapChunkData(chunkIndex, mapChunkData);
            }
            else
            {
                // 恢复VertexList
                RecoverMapChunkData(chunkIndex, mapChunkData);
            }

            // 生成场景物体
            mapChunk.Init(chunkIndex, new Vector3(chunkSize / 2, 0, chunkSize / 2), allForest, mapChunkData);

            // 如果目前游戏没有完成初始化，要告诉GameSceneManager更新进度
            if (GameSceneManager.Instance.IsInitialized == false)
            {
                GameSceneManager.Instance.UpdateGameLoadingProgress();
            }
        }));

        return mapChunk;
    }

    /// <summary>
    /// 生成地图块 Mesh
    /// </summary>
    /// <param name="width">地图 Mesh 的宽</param>
    /// <param name="height">地图 Mesh 的高</param>
    /// <param name="cellSize">格子尺寸</param>
    /// <returns></returns>
    static Mesh GenerateMapChunkMesh(int width, int height, float cellSize)
    {
        Mesh mesh = new Mesh();

        // 确定顶点在哪里
        mesh.vertices = new[]
        {
            new Vector3(0, 0, 0)
          , new Vector3(0, 0, height * cellSize)
          , new Vector3(width * cellSize, 0, height * cellSize)
          , new Vector3(width * cellSize, 0, 0)
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

        // 重新计算法线
        mesh.RecalculateNormals(); // (0, 1, 0)
        return mesh;
    }

    /// <summary>
    /// 生成柏林噪声图
    /// </summary>
    /// <param name="width">宽</param>
    /// <param name="height">高</param>
    /// <param name="lacunarity">间隙，影响平滑度</param>
    /// <returns></returns>
    static float[,] GenerateNoiseMap(int width, int height, float lacunarity)
    {
        lacunarity += 0.1f;

        // 这里的噪声图是为了顶点服务的，而顶点是不包含地图四周的边界的，所以要在原有宽高的基础上 -1
        float[,] noiseMap = new float[width, height];
        float offsetX = Random.Range(-10000f, 10000f);
        float offsetZ = Random.Range(-10000f, 10000f);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                noiseMap[x, y] = Mathf.PerlinNoise(x * lacunarity + offsetX, y * lacunarity + offsetZ);
            }
        }
        return noiseMap;
    }

    /// <summary>
    /// 分帧生成地图块的贴图
    /// </summary>
    /// <param name="chunkIndex">地图块索引</param>
    /// <param name="callBack">Texture2D：贴图，bool：是否全是森林</param>
    /// <returns></returns>
    IEnumerator GenerateMapTexture(Vector2Int chunkIndex, Action<Texture2D, bool> callBack)
    {
        // 当前地图块的偏移量 找到这个地图块具体的每一个格子
        var cellOffsetX = chunkIndex.x * m_MapConfig.MapChunkSize + 1;
        var cellOffsetZ = chunkIndex.y * m_MapConfig.MapChunkSize + 1;
        bool isAllForest = true; // 是不是一张完整的森林地图块

        // 遍历地图快检查是否只有森林类型的格子
        for (int z = 0; z < m_MapConfig.MapChunkSize; z++)
        {
            if (isAllForest == false) break;

            for (int x = 0; x < m_MapConfig.MapChunkSize; x++)
            {
                var cell = m_MapGrid.GetCell(x + cellOffsetX, z + cellOffsetZ);
                if (cell != null && cell.TextureIndex != 0)
                {
                    isAllForest = false;
                    break;
                }
            }
        }

        Texture2D mapTexture = null;

        if (isAllForest == false) // 如果是沼泽
        {
            int textureCellSize = m_MapConfig.ForestTexture.width;           // 贴图都是正方形
            int mapChunkLength = m_MapConfig.MapChunkSize * textureCellSize; // 整个地图块的边长
            mapTexture = new Texture2D(mapChunkLength, mapChunkLength, TextureFormat.RGB24, false);

            // 遍历每一个格子
            for (int y = 0; y < m_MapConfig.MapChunkSize; y++)
            {
                yield return null; // 一帧只绘制一列像素

                int pixelOffsetY = y * textureCellSize; // 像素偏移量

                for (int x = 0; x < m_MapConfig.MapChunkSize; x++)
                {
                    int pixelOffsetX = x * textureCellSize;

                    // <0 是森林，>=0 是 沼泽
                    int textureIndex = m_MapGrid.GetCell(x + cellOffsetX, y + cellOffsetZ).TextureIndex - 1;

                    // 绘制每一个格子内的像素
                    // 访问每一个像素点
                    for (int y1 = 0; y1 < textureCellSize; y1++)
                    {
                        for (int x1 = 0; x1 < textureCellSize; x1++)
                        {
                            // 设置某个像素点的颜色
                            // 确定是森林还是沼泽
                            // 这个地方是森林 || 这个地方是沼泽但是是透明的（这种情况需要绘制 groundTexture 同位置的像素颜色）
                            Color color;

                            if (textureIndex < 0) // <0 是森林
                            {
                                color = m_MapConfig.ForestTexture.GetPixel(x1, y1);
                            }
                            else
                            {
                                color = m_MapConfig.MarshTextures[textureIndex].GetPixel(x1, y1);

                                if (color.a < 1f)
                                {
                                    color = m_MapConfig.ForestTexture.GetPixel(x1, y1);
                                }
                            }

                            mapTexture.SetPixel(x1 + pixelOffsetX, y1 + pixelOffsetY, color);
                        }
                    }
                }
            }

            // 遍历完一个地图块的所有格子后Apply更改
            mapTexture.filterMode = FilterMode.Point;
            mapTexture.wrapMode = TextureWrapMode.Clamp;
            mapTexture.Apply();
        }

        callBack?.Invoke(mapTexture, isAllForest);
    }

    /// <summary>
    /// 生成一个地图对象的数据
    /// </summary>
    /// <param name="mapObjectConfigID"></param>
    /// <param name="spawnPos"></param>
    /// <returns></returns>
    public MapObjectData GenerateMapObjectData(int mapObjectConfigID, Vector3 spawnPos)
    {
        MapObjectData mapObjectData = null;
        MapObjectConfig mapObjectConfig = ConfigManager.Instance.GetConfig<MapObjectConfig>(ConfigName.MapObject, mapObjectConfigID);
        if (mapObjectConfig.IsEmpty == false)
        {
            mapObjectData = GenerateMapObjectData(mapObjectConfigID, spawnPos, mapObjectConfig.DestroyDays);
        }
        return mapObjectData;
    }

    /// <summary>
    /// 生成一个地图对象数据
    /// </summary>
    /// <param name="mapObjectConfigID"></param>
    /// <param name="position"></param>
    /// <param name="destroyDays"></param>
    /// <returns></returns>
    MapObjectData GenerateMapObjectData(int mapObjectConfigID, Vector3 position, int destroyDays)
    {
        MapObjectData mapObjectData = PoolManager.Instance.GetObject<MapObjectData>();
        mapObjectData.ConfigID = mapObjectConfigID;
        mapObjectData.ID = m_MapData.CurrentID;
        m_MapData.CurrentID++;
        mapObjectData.Position = position;
        mapObjectData.DestroyDays = destroyDays;
        return mapObjectData;
    }

    /// <summary>
    /// 通过权重获取一个地图对象的配置ID
    /// </summary>
    /// <returns></returns>
    int GetMapObjectConfigIDForWeight(MapVertexType mapVertexType)
    {
        // 根据概率配置随机
        List<int> spawnConfigIdList = m_MapObjectConfigDict[mapVertexType];

        // 确定权重的总和
        int weightTotal = mapVertexType == MapVertexType.Forest ? m_MapObjectForestWeightTotal : m_MapObjectMarshWeightTotal;

        int randomValue = Random.Range(1, weightTotal + 1);
        float probabilitySum = 0; // 概率和
        foreach (int id in spawnConfigIdList)
        {
            probabilitySum += ConfigManager.Instance.GetConfig<MapObjectConfig>(ConfigName.MapObject, id).Probability;

            if (randomValue < probabilitySum) // 命中
            {
                // 确定到底生成什么地图物品
                return id;
            }
        }
        return 0;
    }

    /// <summary>
    /// 通过权重获取一个AI的配置ID
    /// </summary>
    /// <returns></returns>
    int GetAIConfigIDForWeight(MapVertexType mapVertexType)
    {
        // 根据概率配置随机
        List<int> spawnConfigIdList = m_AIConfigDict[mapVertexType];

        // 确定权重的总和
        int weightTotal = mapVertexType == MapVertexType.Forest ? m_AIForestWeightTotal : m_AIMarshWeightTotal;

        int randomValue = Random.Range(1, weightTotal + 1);
        float probabilitySum = 0; // 概率和
        foreach (int id in spawnConfigIdList)
        {
            probabilitySum += ConfigManager.Instance.GetConfig<AIConfig>(ConfigName.AI, id).Probability;

            if (randomValue < probabilitySum) // 命中
            {
                // 确定到底生成什么地图物品
                return id;
            }
        }
        return 0;
    }

    /// <summary>
    /// 生成地图对象数据，为了地图块初始化准备的
    /// </summary>
    /// <remarks>遍历地图顶点，根据spawnConfig中的配置信息及其概率进行随机生成，并在对应位置实例化物体</remarks>
    MapChunkData GenerateMapChunkData(Vector2Int chunkIndex)
    {
        MapChunkData mapChunkData = new MapChunkData();
        mapChunkData.MapObjectDict = new SerializableDictionary<ulong, MapObjectData>();
        mapChunkData.AIDataDict = new SerializableDictionary<ulong, MapObjectData>();
        mapChunkData.ForestVertexList = new List<MapVertex>(m_MapConfig.MapChunkSize * m_MapConfig.MapChunkSize);
        mapChunkData.MarshVertexList = new List<MapVertex>(m_MapConfig.MapChunkSize * m_MapConfig.MapChunkSize);

        int offsetX = chunkIndex.x * m_MapConfig.MapChunkSize;
        int offsetZ = chunkIndex.y * m_MapConfig.MapChunkSize;

        for (int x = 1; x < m_MapConfig.MapChunkSize; x++)
        {
            for (int y = 1; y < m_MapConfig.MapChunkSize; y++)
            {
                MapVertex mapVertex = m_MapGrid.GetVertex(x + offsetX, y + offsetZ);
                if (mapVertex.VertexType == MapVertexType.Forest)
                {
                    mapChunkData.ForestVertexList.Add(mapVertex);
                }
                else if (mapVertex.VertexType == MapVertexType.Marsh)
                {
                    mapChunkData.MarshVertexList.Add(mapVertex);
                }

                // 通过权重获取一个地图对象的配置ID
                int configID = GetMapObjectConfigIDForWeight(mapVertex.VertexType);
                MapObjectConfig objectConfig = ConfigManager.Instance.GetConfig<MapObjectConfig>(ConfigName.MapObject, configID);
                if (objectConfig.IsEmpty == false)
                {
                    var position = mapVertex.Position + new Vector3(Random.Range(-m_MapGrid.CellSize / 2, m_MapGrid.CellSize / 2), 0
                                                                  , Random.Range(-m_MapGrid.CellSize / 2, m_MapGrid.CellSize / 2));

                    mapVertex.MapObjectID = m_MapData.CurrentID;

                    mapChunkData.MapObjectDict.Dictionary.Add(m_MapData.CurrentID
                                                            , GenerateMapObjectData(configID, position, objectConfig.DestroyDays));
                }
            }
        }

        List<MapObjectData> aiDataList = GenerateAIObjectDataList(mapChunkData);

        foreach (var aiData in aiDataList)
        {
            mapChunkData.AIDataDict.Dictionary.Add(aiData.ID, aiData);
        }

        return mapChunkData;
    }

    void RecoverMapChunkData(Vector2Int chunkIndex, MapChunkData mapChunkData)
    {
        mapChunkData.ForestVertexList = new List<MapVertex>(m_MapConfig.MapChunkSize * m_MapConfig.MapChunkSize);
        mapChunkData.MarshVertexList = new List<MapVertex>(m_MapConfig.MapChunkSize * m_MapConfig.MapChunkSize);

        int offsetX = chunkIndex.x * m_MapConfig.MapChunkSize;
        int offsetZ = chunkIndex.y * m_MapConfig.MapChunkSize;

        for (int x = 1; x < m_MapConfig.MapChunkSize; x++)
        {
            for (int y = 1; y < m_MapConfig.MapChunkSize; y++)
            {
                MapVertex mapVertex = m_MapGrid.GetVertex(x + offsetX, y + offsetZ);
                if (mapVertex.VertexType == MapVertexType.Forest)
                {
                    mapChunkData.ForestVertexList.Add(mapVertex);
                }
                else if (mapVertex.VertexType == MapVertexType.Marsh)
                {
                    mapChunkData.MarshVertexList.Add(mapVertex);
                }
            }
        }
    }

    List<MapObjectData> m_MapObjectDataList = new List<MapObjectData>(); // 用来避免每次都返回一个新的 List 对象

    /// <summary>
    /// 游戏中每天早晨通过地图块索引返回这个地图块多出来（新生成）的物品数据
    /// </summary>
    /// <param name="chunkIndex"></param>
    /// <returns></returns>
    public List<MapObjectData> GenerateMapObjectDataListOnMapChunkRefresh(Vector2Int chunkIndex)
    {
        m_MapObjectDataList.Clear();
        int offsetX = chunkIndex.x * m_MapConfig.MapChunkSize;
        int offsetZ = chunkIndex.y * m_MapConfig.MapChunkSize;
        for (int x = 1; x < m_MapConfig.MapChunkSize; x++)
        {
            for (int y = 1; y < m_MapConfig.MapChunkSize; y++)
            {
                if (Random.Range(0, m_MapConfig.RefreshProbability) != 0) continue; // 如果概率没命中，则这一个顶点不刷新

                MapVertex mapVertex = m_MapGrid.GetVertex(x + offsetX, y + offsetZ);
                if (mapVertex.MapObjectID != 0) continue; // 不为0则不能生成

                // 通过权重获取一个地图对象的配置ID
                int configID = GetMapObjectConfigIDForWeight(mapVertex.VertexType);
                MapObjectConfig objectConfig = ConfigManager.Instance.GetConfig<MapObjectConfig>(ConfigName.MapObject, configID);

                if (objectConfig.IsEmpty == false)
                {
                    var position = mapVertex.Position + new Vector3(Random.Range(-m_MapGrid.CellSize / 2, m_MapGrid.CellSize / 2)
                                                                  , 0
                                                                  , Random.Range(-m_MapGrid.CellSize / 2, m_MapGrid.CellSize / 2));

                    mapVertex.MapObjectID = m_MapData.CurrentID;
                    m_MapObjectDataList.Add(GenerateMapObjectData(configID, position, objectConfig.DestroyDays));
                }
            }
        }
        return m_MapObjectDataList;
    }

    public List<MapObjectData> GenerateAIObjectDataList(MapChunkData mapChunkData)
    {
        m_MapObjectDataList.Clear();

        // 最多生成的数量
        int maxCount = m_MapConfig.MaxAiCountOnChunk - mapChunkData.AIDataDict.Dictionary.Count;

        // 生成Ai数据 一个地图块 森林或沼泽的顶点数要超过配置的才生成
        if (mapChunkData.ForestVertexList.Count > m_MapConfig.GenerateAiMinVertexCountOnChunk)
        {
            for (int i = 0; i < maxCount; i++)
            {
                int configID = GetAIConfigIDForWeight(MapVertexType.Forest);
                AIConfig config = ConfigManager.Instance.GetConfig<AIConfig>(ConfigName.AI, configID);
                if (config.IsEmpty == false)
                {
                    m_MapObjectDataList.Add(GenerateMapObjectData(configID, Vector3.zero, -1));
                    maxCount--;
                }
            }
        }
        if (mapChunkData.MarshVertexList.Count > m_MapConfig.GenerateAiMinVertexCountOnChunk)
        {
            for (int i = 0; i < maxCount; i++)
            {
                int configID = GetAIConfigIDForWeight(MapVertexType.Marsh);
                AIConfig config = ConfigManager.Instance.GetConfig<AIConfig>(ConfigName.AI, configID);
                if (config.IsEmpty == false)
                {
                    m_MapObjectDataList.Add(GenerateMapObjectData(configID, Vector3.zero, -1));
                }
            }
        }

        return m_MapObjectDataList;
    }

    #endregion
}